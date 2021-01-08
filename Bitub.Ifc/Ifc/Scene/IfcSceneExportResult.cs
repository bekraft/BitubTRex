using System;

using Google.Protobuf.WellKnownTypes;

using Xbim.Common;
using Xbim.Ifc4.Interfaces;

using Bitub.Dto.Scene;

namespace Bitub.Ifc.Export
{
    public sealed class IfcSceneExportResult : IfcExportResult<SceneModel, IfcExportSettings>
    {
        #region Internals

        internal IfcSceneExportResult(IModel model, IfcExportSettings preferences)
            : base(model, preferences)
        {
            ExportedModel = CreateNew(Project, preferences);
            ExportedModel.Contexts.AddRange(preferences.UserRepresentationContext);
        }

        internal IfcSceneExportResult(Exception exception, IModel model, IfcExportSettings preferences)
            : base(model, preferences)
        {
            FailureReason = exception;
        }

        private SceneModel CreateNew(IIfcProject p, IfcExportSettings settings)
        {
            return new SceneModel()
            {
                Name = p?.Name,
                Id = p?.GlobalId.ToGlobalUniqueId(),
                UnitsPerMeter = settings.UnitsPerMeter,
                Stamp = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            };
        }

        #endregion
    }
}
