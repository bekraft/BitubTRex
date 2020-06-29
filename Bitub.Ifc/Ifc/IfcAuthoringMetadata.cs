using System.Linq;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc
{
    /// <summary>
    /// Address data.
    /// </summary>
    public struct AddressData
    {
        public IfcAddressTypeEnum? Type { get; set; }
        public string Address { get; set; }
    }

    /// <summary>
    /// Authors data referring to a person.
    /// </summary>
    public struct AuthorData
    {
        public string Name { get; set; }
        public string GivenName { get; set; }
        public OrganisationData[] Organisations { get; set; }
        public AddressData[] Addresses { get; set; }
    }

    /// <summary>
    /// Organisation identification & addresses.
    /// </summary>
    public struct OrganisationData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AddressData[] Addresses { get; set; }
    }

    /// <summary>
    /// Application details
    /// </summary>
    public struct ApplicationData
    {
        public string ApplicationName { get; set; }
        public string ApplicationID { get; set; }
        public string Version { get; set; }
    }

    /// <summary>
    /// Full meta data.
    /// </summary>
    public class IfcAuthoringMetadata
    {
        public AuthorData? Editor { get; set; }
        public AuthorData? Owner { get; set; }
        public ApplicationData? AuthoringApplication { get; set; }
        public System.DateTime LastKnownAccess { get; set; } = System.DateTime.Now;

        public XbimEditorCredentials ToEditorCredentials()
        {
            return new XbimEditorCredentials()
            {
                ApplicationFullName = AuthoringApplication?.ApplicationName,
                ApplicationIdentifier = AuthoringApplication?.ApplicationID,
                ApplicationVersion = AuthoringApplication?.Version,
                EditorsFamilyName = Editor?.Name,
                EditorsGivenName = Editor?.GivenName,
                EditorsOrganisationName = string.Join("/", Editor?.Organisations?.Select(o => o.Name) ?? new string[0])
            };
        }
    }
}
