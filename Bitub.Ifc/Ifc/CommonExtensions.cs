using System;

using Xbim.Common;
using Xbim.Ifc4.UtilityResource;

using Bitub.Transfer;

namespace Bitub.Ifc
{
    /// <summary>
    /// A IFC product classificator.
    /// </summary>
    /// <param name="p">The product</param>
    /// <returns>A qualification</returns>
    public delegate Qualifier XbimEntityQualifierDelegate(IPersistEntity p);

    public static class CommonExtensions
    {
        /// <summary>
        /// Default entity classifier based on IFC schema version and <see cref="IPersistEntity.ExpressType"/>
        /// </summary>
        public static XbimEntityQualifierDelegate DefaultXbimEntityQualifier = (p) =>
        {
            var named = new Name();
            named.Frags.Add(p.Model.SchemaVersion.ToString());
            named.Frags.Add(p.ExpressType.ExpressName);
            return named.ToQualifier();
        };

        /// <summary>
        /// Casts the current GUID to a <see cref="GlobalUniqueId"/>
        /// </summary>
        /// <param name="id">The IFC GUID</param>
        /// <param name="asGuid">Whether to cast to GUID</param>
        /// <returns></returns>
        public static GlobalUniqueId ToGlobalUniqueId(this IfcGloballyUniqueId id, bool asGuid = false)
        {
            if (asGuid)
            {
                var guid = IfcGloballyUniqueId.ConvertFromBase64(id);
                return new GlobalUniqueId
                {
                    Guid = new Bitub.Transfer.Guid
                    {
                        Raw = Google.Protobuf.ByteString.CopyFrom(guid.ToByteArray())
                    }
                };
            }
            else
            {
                return new GlobalUniqueId
                {
                    Base64 = id
                };
            }
        }

    }
}
