using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Xbim.Common;
using Xbim.Common.Geometry;

using Xbim.Ifc4.Interfaces;

using Bitub.Dto.Scene;

namespace Bitub.Ifc.Export
{
    public abstract class IfcExportResult<TExport, TSettings> where TSettings: IfcExportSettings
    {
        public IModel Model { get; protected set; }
        public IIfcProject Project { get; protected set; }
        public TExport ExportedModel { get; protected set; }
        public TSettings Preferences { get; protected set; }

        public float Scale { get; protected set; }

        #region Internals

        private IDictionary<int, Tuple<SceneContext, XbimMatrix3D>> contextCache;

        protected IfcExportResult(IModel sourceModel, TSettings preferences)
        {
            Init(sourceModel, preferences);
        }

        protected void Init(IModel model, TSettings settings)
        {
            Model = model;
            contextCache = new SortedDictionary<int, Tuple<SceneContext, XbimMatrix3D>>();
            Preferences = settings;

            Scale = settings.UnitsPerMeter / (float)model.ModelFactors.OneMeter;

            Project = model.Instances.OfType<IIfcProject>().First();

            var instanceContexts = settings.UserRepresentationContext.Select(c => new SceneContext
            {
                Name = c.Name,
                // Given in DEG => use as it is
                FDeflection = Model.ModelFactors.DeflectionAngle,
                // Given internally in model units => convert to meter
                FTolerance = model.ModelFactors.LengthToMetresConversionFactor * Model.ModelFactors.DeflectionTolerance,
            }).ToArray();

            Preferences.UserRepresentationContext = instanceContexts;
        }

        internal void SetRepresentationContext(int contextLabel, SceneContext sc, XbimMatrix3D wcs)
        {
            contextCache[contextLabel] = new Tuple<SceneContext, XbimMatrix3D>(sc, wcs);
        }

        #endregion

        public Exception FailureReason { get; protected set; }

        public bool IsFailure { get => null != FailureReason; }

        public Tuple<SceneContext, XbimMatrix3D> RepresentationContext(int contextLabel)
        {
            Tuple<SceneContext, XbimMatrix3D> sc;
            if (contextCache.TryGetValue(contextLabel, out sc))
                return sc;
            else
                return null;
        }

        public int[] ContextLabels() => contextCache.Keys.ToArray();

        public SceneContext ContextOf(int contextLabel) => RepresentationContext(contextLabel)?.Item1;

        public XbimMatrix3D? TransformOf(int contextLabel) => RepresentationContext(contextLabel)?.Item2;

        public bool IsInContext(int contextLabel) => contextCache.ContainsKey(contextLabel);

    }
}
