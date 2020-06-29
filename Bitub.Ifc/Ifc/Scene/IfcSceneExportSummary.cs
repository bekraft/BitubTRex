using Bitub.Transfer.Scene;

using System;
using System.Collections.Generic;
using System.Linq;

using Google.Protobuf.WellKnownTypes;

using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc.Scene
{
    public sealed class IfcSceneExportSummary
    {
        public readonly IModel Model;
        public readonly SceneModel Scene;
        public readonly IfcSceneExportSettings AppliedSettings;
        public readonly IDictionary<int, Component> ComponentCache;
        public readonly double Scale;

        internal readonly IDictionary<int, Tuple<SceneContext, XbimMatrix3D>> Context;

        internal IfcSceneExportSummary(IModel model, IfcSceneExportSettings settings)
        {
            Model = model;
            AppliedSettings = settings;
            Context = new SortedDictionary<int, Tuple<SceneContext, XbimMatrix3D>>();
            ComponentCache = new Dictionary<int, Component>();
            Scale = settings.UnitsPerMeter / model.ModelFactors.OneMeter;

            var p = model.Instances.OfType<IIfcProject>().First();
            Scene = new SceneModel()
            {
                Name = p?.Name,
                Id = p?.GlobalId.ToGlobalUniqueId(),
                UnitsPerMeter = settings.UnitsPerMeter,
                Stamp = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            };

            var instanceContexts = settings.UserRepresentationContext.Select(c => new SceneContext
            {
                Name = c.Name,
                // Given in DEG => use as it is
                FDeflection = p.Model.ModelFactors.DeflectionAngle,
                // Given internally in model units => convert to meter
                FTolerance = model.ModelFactors.LengthToMetresConversionFactor * p.Model.ModelFactors.DeflectionTolerance,
            }).ToArray();

            // TODO Apply custom tolerances
            Scene.Contexts.AddRange(instanceContexts);
            settings.UserRepresentationContext = instanceContexts;
        }


        public int[] ExportedContextLabels => Context.Keys.ToArray();

        public SceneContext ContextOf(int contextLabel) => Context[contextLabel].Item1;

        public XbimMatrix3D TransformOf(int contextLabel) => Context[contextLabel].Item2;

        public bool IsInContext(int contextLabel) => Context.ContainsKey(contextLabel);
    }
}
