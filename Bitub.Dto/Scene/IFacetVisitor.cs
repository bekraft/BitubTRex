using System.Collections.Generic;

namespace Bitub.Dto.Scene
{
    public interface IFacetVisitor : IEnumerable<Facet>
    {
        bool IsNewFace { get; }

        uint? PivotVertex { get; }
    }
}
