using System;

using Google.Protobuf.WellKnownTypes;

using Xbim.Common;
using Xbim.Ifc4.Interfaces;

using Bitub.Ifc;
using Bitub.Dto;
using Bitub.Dto.Scene;

namespace Bitub.Ifc.Export
{
    public sealed class IfcSceneExportResult : IfcExportResult<ComponentModel, IfcExportSettings>
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

        private ComponentModel CreateNew(IIfcProject p, IfcExportSettings settings)
        {
            return new ComponentModel()
            {
                Metadata = new MetaData
                {
                    Name = p?.Name,
                    Stamp = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
                },                
                Id = p?.GlobalId.ToGlobalUniqueId().ToQualifier(),
                UnitsPerMeter = settings.UnitsPerMeter               
            };
        }

        #endregion
    }
}
