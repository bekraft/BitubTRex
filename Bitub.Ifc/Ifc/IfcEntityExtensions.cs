using System.Linq;

using Bitub.Dto;

using Xbim.Common;
using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc
{
    public static class IfcEntityExtensions
    {
        public static Qualifier ToQualifiedTypeName(this IPersistEntity e)
        {
            var q = new Qualifier();
            q.Named = new Name();
            q.Named.Frags.Add(e.Model.SchemaVersion.ToString());
            q.Named.Frags.Add(e.ExpressType.Name);
            return q;
        }

        public static ApplicationData ToApplicationData(this IIfcApplication ifcApplication)
        {
            return new ApplicationData
            {
                ApplicationID = ifcApplication?.ApplicationIdentifier,
                ApplicationName = ifcApplication?.ApplicationFullName,
                Version = ifcApplication?.Version
            };
        }

        public static OrganisationData ToOrganisationData(this IIfcOrganization ifcOrganization)
        {
            return new OrganisationData
            {
                Name = ifcOrganization?.Name,
                Id = ifcOrganization?.Identification,
                Description = ifcOrganization?.Description,
                Addresses = ifcOrganization?.Addresses.Select(i => i.ToAddressData()).ToArray()
            };
        }

        public static AddressData ToAddressData(this IIfcAddress ifcAddress)
        {
            return new AddressData
            {
                Type = ifcAddress?.Purpose,
                Address = ifcAddress?.Description
            };
        }

        public static AuthorData ToAuthorData(this IIfcPerson ifcPerson)
        {
            return new AuthorData
            {
                Name = ifcPerson?.FamilyName,
                GivenName = ifcPerson?.GivenName,
                Addresses = ifcPerson?.Addresses.Select(i => i.ToAddressData()).ToArray(),
                Organisations = ifcPerson?.EngagedIn.Select(i => i.TheOrganization.ToOrganisationData()).ToArray()
            };
        }

        public static Xbim.Ifc.XbimEditorCredentials ToEditorCredentials(this AuthorData authorData, ApplicationData? applicationData = null)
        {
            return new Xbim.Ifc.XbimEditorCredentials
            {
                EditorsFamilyName = authorData.Name,
                EditorsGivenName = authorData.GivenName,                
                EditorsOrganisationName = authorData.Organisations?.FirstOrDefault().Name,
                ApplicationFullName = applicationData?.ApplicationName ?? "github.com/bekraft/BitubTRex",
                ApplicationIdentifier = applicationData?.ApplicationID ?? "BitubTRex"
            };
        }
    }
}
