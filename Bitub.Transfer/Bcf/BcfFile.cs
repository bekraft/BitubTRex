using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitub.Transfer.Bcf
{
    public class BcfEntry
    {
        public Guid ID { get; private set; }
        public BcfMarkup Markup { get; internal set; }        
    }

    public class BcfFile
    {
    }
}
