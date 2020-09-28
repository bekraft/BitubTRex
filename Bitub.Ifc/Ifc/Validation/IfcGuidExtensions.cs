using Bitub.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitub.Ifc.Validation
{
    public static class IfcGuidExtensions
    {
        // See https://technical.buildingsmart.org/resources/ifcimplementationguidance/ifc-guid/
        public const string IfcGuidAlphabet =
           //          1         2         3         4         5         6   
           //0123456789012345678901234567890123456789012345678901234567890123
           "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_$";


        public static bool IsValidIfcGuid(this string ifcGuid)
        {
            if (ifcGuid.Length != 22)
                return false;
            if (ifcGuid.Any(c => !IfcGuidAlphabet.Contains(c)))
                return false;

            return true;
        }

        public static Qualifier ToIfcGuidQualifier(this string ifcGuid)
        {
            if (IsValidIfcGuid(ifcGuid))
                return new Qualifier { Anonymous = new GlobalUniqueId { Base64 = ifcGuid } };
            else
                throw new ArgumentException("Invalid ifcGuid");
        }
    }
}
