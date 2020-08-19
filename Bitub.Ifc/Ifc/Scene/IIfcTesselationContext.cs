using Bitub.Transfer;
using Bitub.Transfer.Scene;

using System.Collections.Generic;

using Xbim.Common;

namespace Bitub.Ifc.Scene
{
    /// <summary>
    /// A tuple of model entity label and an enumeration of scene represenation messages.
    /// </summary>
    public sealed class IfcProductSceneRepresentation 
    {
        public readonly int EntityLabel;
        public readonly IEnumerable<Representation> Representations;

        public IfcProductSceneRepresentation(int entityLabel, IEnumerable<Representation> representations)
        {
            EntityLabel = entityLabel;
            Representations = representations;
        }
    }

    /// <summary>
    /// A tesselation context provider contract.
    /// </summary>
    public interface IIfcTesselationContext 
    {        
        IEnumerable<IfcProductSceneRepresentation> Tesselate(IModel m, IfcSceneExportSummary summary, CancelableProgressing progressing);
    }
}
