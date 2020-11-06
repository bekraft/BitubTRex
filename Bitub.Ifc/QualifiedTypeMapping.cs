using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using Bitub.Dto;
using Xbim.Common;

namespace Bitub.Ifc
{    
    public sealed class QualifiedTypePair
    {
        public Qualifier FromQualifiedType { set; get; }
        public Qualifier ToQualifiedType { set; get; }

        public QualifiedTypePair(Qualifier s, Qualifier t)
        {
            FromQualifiedType = s;
            ToQualifiedType = t;
        }
    }

    public class QualifiedTypeMapping
    {

    }
}
