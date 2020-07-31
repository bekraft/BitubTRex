using System;
using System.Collections.Generic;
using System.Linq;

using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

using Bitub.Transfer;
using Microsoft.Extensions.Logging;


namespace Bitub.Ifc.Transform.Requests
{
    /// <summary>
    /// Placement strategy options. Exclusive alternatives of either adding a correction as a new
    /// placement or to change existing placements.
    /// </summary>
    public enum IfcPlacementStrategy
    {
        /// <summary>
        /// A new root placement wich adapts the correction. Only available for IFC4. Coordinate values
        /// might exceed precision thresholds of consumer platforms, since two subsequent transformations with
        /// (potentially) large values are concatenated.
        /// 
        /// Since no changes (except for parent transform) are made to existing instances, 
        /// it is a strategy with minimal footprint.
        /// </summary>
        NewRootPlacement,
        /// <summary>
        /// Adjust existing root placements. Ideally, there's only one placement per model. In this case
        /// the precision will be best. If there is more than one root placement, a mean shift vector will be
        /// computed to minimize precision error.
        /// </summary>
        ChangeRootPlacements
    }

    /// <summary>
    /// IFC placement transforming package bundling needed information per request.
    /// </summary>
    public sealed class IfcPlacementTransformPackage : TransformPackage
    {
        public IfcPlacementStrategy AppliedPlacementStrategy { get; private set; }
        public IfcAxisAlignment AppliedAxisAlignment { get; private set; }
        public int[] SourceRootPlacementsLabels { get; private set; }

        private IIfcLocalPlacement _newRootPlacement;


        public IfcPlacementTransformPackage(IModel aSource, IModel aTarget, 
            IfcPlacementStrategy placementStrategy, IfcAxisAlignment axisAlignment) : base(aSource, aTarget)
        {
            AppliedPlacementStrategy = placementStrategy;
            UnitsPerMeterSource = aSource.ModelFactors.OneMeter;
            UnitsPerMeterTarget = aTarget.ModelFactors.OneMeter;
            AppliedAxisAlignment = new IfcAxisAlignment(axisAlignment);
        }

        internal void Prepare(CancelableProgressing cancelableProgress)
        {
            PlacementTree = new XbimPlacementTree(Source, false);
            SourceRootPlacementsLabels = Source.Instances
                .OfType<IIfcLocalPlacement>()
                .Where(p => p.PlacementRelTo == null).Select(p => p.EntityLabel).ToArray();
            Array.Sort(SourceRootPlacementsLabels);

            switch (AppliedPlacementStrategy)
            {
                case IfcPlacementStrategy.NewRootPlacement:
                    if (Source.SchemaVersion == Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3)
                        throw new NotSupportedException("IFC2x3 doesn't support new placements without object context. Consider using 'IfcPlacementStrategy.ChangeRootPlacements'");
                    // Shift is absorbed by new placement
                    SingletonShift = XbimVector3D.Zero;
                    break;
                case IfcPlacementStrategy.ChangeRootPlacements:                    
                    if (SourceRootPlacementsLabels.Length > 1)
                    {   // Mean, if more than one
                        var entireTranslation = SourceRootPlacementsLabels.Select(l => PlacementTree[l].Translation).Aggregate((a, b) => a + b);
                        SingletonShift = entireTranslation * (1.0 / SourceRootPlacementsLabels.Length);
                    }
                    else
                    {   // Precise offset of singleton root
                        SingletonShift = PlacementTree[SourceRootPlacementsLabels[0]].Translation;
                    }

                    AppliedAxisAlignment.SourceReferenceAxis.Translate(SingletonShift, - UnitsPerMeterSource);

                    break;
                default:
                    throw new NotImplementedException($"Missing '{AppliedPlacementStrategy}'");
            }
        }

        internal bool IsAffected(IPersistEntity p) => -1 < Array.BinarySearch(SourceRootPlacementsLabels, p.EntityLabel);

        internal XbimPlacementTree PlacementTree { get; private set; }

        internal double UnitsPerMeterSource { get; set; }

        internal double UnitsPerMeterTarget { get; set; }

        internal IEnumerable<IIfcLocalPlacement> Placements 
        { 
            get => SourceRootPlacementsLabels.Select(i => Source.Instances[i]).OfType<IIfcLocalPlacement>(); 
        }

        internal XbimVector3D SingletonShift { get; private set; }

        internal void HandlePlacementCopy(IIfcLocalPlacement sourcePlacement, IIfcLocalPlacement targetPlacement)
        {
            switch(AppliedPlacementStrategy)
            {
                case IfcPlacementStrategy.NewRootPlacement:
                    if (null == _newRootPlacement)
                    {
                        _newRootPlacement = AppliedAxisAlignment.NewRootIfcLocalPlacement(Target);
                        Log?.Add(new TransformLogEntry(new XbimInstanceHandle(_newRootPlacement), TransformAction.Added));
                    }
                    Log?.Add(new TransformLogEntry(new XbimInstanceHandle(sourcePlacement), TransformAction.Modified));
                    targetPlacement.PlacementRelTo = _newRootPlacement;
                    break;

                case IfcPlacementStrategy.ChangeRootPlacements:
                    Log?.Add(new TransformLogEntry(new XbimInstanceHandle(sourcePlacement), TransformAction.Modified));
                    ChangePlacement(sourcePlacement, targetPlacement);
                    break;

                default:
                    throw new NotImplementedException($"Misses '{AppliedAxisAlignment}'");
            }
        }

        private void ChangePlacement(IIfcLocalPlacement sourcePlacement, IIfcLocalPlacement targetPlacement)
        {
            var t1 = PlacementTree[sourcePlacement.EntityLabel];
            var offset = t1.Translation - SingletonShift;
            // Clamp final offset if less than precision
            var prec = Target.ModelFactors.Precision;
            var clamped = offset.ToDouble().Select(v => Math.Abs(v) < prec ? 0 : v).ToArray();

            var t2 = new XbimMatrix3D
            (
                t1.M11, t1.M12, t1.M13, 
                t1.M14, t1.M21, t1.M22, 
                t1.M23, t1.M24, t1.M31, 
                t1.M32, t1.M33, t1.M34, 
                clamped[0], clamped[1], clamped[2], t1.M44
            );
            AppliedAxisAlignment.ChangeIfcLocalPlacement(targetPlacement, t2);
        }
    }

    /// <summary>
    /// IFC placement transformation request.
    /// </summary>
    public class IfcPlacementTransformRequest : IfcTransformRequestTemplate<IfcPlacementTransformPackage>
    {
        #region Internals

        private readonly object _monitor = new object();
        private IfcPlacementStrategy _placementStrategy = IfcPlacementStrategy.ChangeRootPlacements;
        private IfcAxisAlignment _axisAlignment = new IfcAxisAlignment();

        #endregion

        /// <summary>
        /// The logger.
        /// </summary>
        public override ILogger Log { get; protected set; }

        public override string Name { get => "IFC Model Placement Transform"; }

        // TODO Check alignment for identity transform
        protected override bool IsNoopTransform { get => false; }

        public override bool IsInplaceTransform { get => false; }

        /// <summary>
        /// Default strategy is <see cref="IfcPlacementStrategy.AdjustRootPlacements"/>.
        /// </summary>
        public IfcPlacementStrategy PlacementStrategy
        {
            get {
                return _placementStrategy;
            }
            set {
                lock(_monitor)
                {
                    _placementStrategy = value;
                }
            }
        }

        /// <summary>
        /// The current axis alignment specification.
        /// </summary>
        public IfcAxisAlignment AxisAlignment
        {
            get {
                lock (_monitor)
                    return new IfcAxisAlignment(_axisAlignment);
            }
            set {
                lock (_monitor)
                    _axisAlignment = new IfcAxisAlignment(value);
            }
        }

        /// <summary>
        /// The model axis alignment transformation request.
        /// </summary>
        /// <param name="loggerFactory">Logger factory</param>
        public IfcPlacementTransformRequest(ILoggerFactory loggerFactory = null)
        {
            Log = loggerFactory?.CreateLogger<IfcPlacementTransformRequest>();
        }

        protected override IfcPlacementTransformPackage CreateTransformPackage(IModel aSource, IModel aTarget)
        {
            lock (_monitor)
                return new IfcPlacementTransformPackage(aSource, aTarget, _placementStrategy, _axisAlignment);
        }

        protected override TransformResult.Code DoPreprocessTransform(IfcPlacementTransformPackage package, CancelableProgressing progress)
        {
            Log?.LogInformation("({0}) Applying '{1}' strategy to model.", Name, PlacementStrategy);
            package.Prepare(progress);
            Log?.LogInformation("({0}) Got singleton offset shift of {1} [m].", Name, package.SingletonShift * (1/package.UnitsPerMeterSource));
            return TransformResult.Code.Finished;
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, IfcPlacementTransformPackage package)
        {
            if (package.IsAffected(instance))
                return TransformActionType.Delegate;
            else
                return TransformActionType.CopyWithInverse;
        }

        protected override IPersistEntity DelegateCopy(IPersistEntity instance, IfcPlacementTransformPackage package)
        {
            if (instance is IIfcLocalPlacement p)
            {
                Log?.LogInformation($"Changing placement '{p}'");
                // Don't copy inverse references (products and children)
                var targetPlacement = Copy(instance, package, false) as IIfcLocalPlacement;
                package.HandlePlacementCopy(p, targetPlacement);
                return targetPlacement;
            }
            else
            {
                throw new NotSupportedException($"Illegal handling of '{instance}' requested.");
            }
        }
    }
}
