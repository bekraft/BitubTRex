using Bitub.Dto;

namespace Bitub.Ifc
{
    public static class CommonExtensions
    {
        /// <summary>
        /// Casts the current GUID to a internal <see cref="GlobalUniqueId"/> select.
        /// </summary>
        /// <param name="id">The IFC GUID</param>
        /// <param name="asGuid">Whether to cast to a GUID or to re-use the bas64 representation</param>
        /// <returns></returns>
        public static GlobalUniqueId ToGlobalUniqueId(this Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId id, bool asGuid = false)
        {
            if (asGuid)
            {
                var guid = Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId.ConvertFromBase64(id);
                return new GlobalUniqueId
                {
                    Guid = new Guid
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

        /// <summary>
        /// Casts the current GUID to a internal <see cref="GlobalUniqueId"/> select.
        /// </summary>
        /// <param name="id">The IFC GUID</param>
        /// <param name="asGuid">Whether to cast to a GUID or to re-use the bas64 representation</param>
        /// <returns></returns>
        public static GlobalUniqueId ToGlobalUniqueId(this Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId id, bool asGuid = false)
        {
            if (asGuid)
            {
                var guid = Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId.ConvertFromBase64(id);
                return new GlobalUniqueId
                {
                    Guid = new Guid
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
