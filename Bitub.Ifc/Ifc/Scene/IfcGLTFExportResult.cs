using System;
using SharpGLTF.Schema2;
using Xbim.Common;

namespace Bitub.Ifc.Export
{
    public class IfcGLTFExportResult : IfcExportResult<ModelRoot, IfcExportSettings>
    {
        #region Internals

        internal IfcGLTFExportResult(ModelRoot scene, IModel sourceModel, IfcExportSettings preferences) 
            : base(sourceModel, preferences)
        {
            ExportedModel = scene;
        }

        internal IfcGLTFExportResult(Exception failure, IModel sourceModel, IfcExportSettings preferences)
            : base(sourceModel, preferences)
        {
            FailureReason = failure;
        }

        #endregion
    }
}
