using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xbim.Common;
using Xbim.Common.Metadata;

namespace Bitub.Ifc
{
    public static class IPersistEntityExtension
    {
        public static XName XLabel(this IPersistEntity e)
        {
            return $"{{{e?.Model.SchemaVersion.ToString().ToUpper()}}}{e.ExpressType.Name.ToUpper()}";
        }
    }
}
