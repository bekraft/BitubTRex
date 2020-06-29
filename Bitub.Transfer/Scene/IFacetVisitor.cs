﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitub.Transfer.Scene
{
    public interface IFacetVisitor : IEnumerable<Facet>
    {
        bool IsNewFace { get; }

        uint? PivotVertex { get; }
    }
}
