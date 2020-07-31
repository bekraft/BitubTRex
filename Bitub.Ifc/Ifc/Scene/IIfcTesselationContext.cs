using Bitub.Transfer;
using Bitub.Transfer.Scene;

using System.Collections.Generic;

using Xbim.Common;

namespace Bitub.Ifc.Scene
{
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

    public interface IIfcTesselationContext : ICancelableProgressing<ICancelableProgressState>
    {
        IEnumerable<IfcProductSceneRepresentation> Tesselate(IModel m, IfcSceneExportSummary summary, CancelableProgressStateToken progressState);
    }
}
