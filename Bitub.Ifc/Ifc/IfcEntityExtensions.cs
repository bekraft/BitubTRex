using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Bitub.Dto;
using Bitub.Ifc.Concept;

using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc
{
    public delegate IEnumerable<T> Unfold<T>(IPersistEntity host) where T : IPersistEntity;

    public static class IfcEntityExtensions
    {
        /// <summary>
        /// Creates an unfolding hierarchy delegate following decomposition and spatially nested
        /// relations.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <returns>A lambda which unfolds a given context</returns>
        public static Unfold<T> NewUnfoldContainer<T>() where T : IIfcObjectDefinition
        {
            return (host) =>
            {
                if (host is IIfcObjectDefinition parent)
                    return IfcProductRelationExtensions.Children<T>(parent);
                else
                    return Enumerable.Empty<T>();
            };
        }

        /// <summary>
        /// Creates an unfolding hierarchy delegate following only spatially nested
        /// relations.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <returns>A lambda which unfolds a given context</returns>
        public static Unfold<T> NewUnfoldSpatialContainer<T>() where T : IIfcProduct
        {
            return (host) =>
            {
                if (host is IIfcSpatialElement parent)
                    return IfcProductRelationExtensions.ChildProducts<T>(parent);
                else
                    return Enumerable.Empty<T>();
            };
        }

        /// <summary>
        /// Creates an unfolding hierarchy delegate following only spatially nested
        /// relations.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <returns>A lambda which unfolds a given context</returns>
        public static Unfold<T> NewUnfoldComposition<T>() where T : IIfcObjectDefinition
        {
            return (host) =>
            {
                if (host is IIfcObjectDefinition parent)
                    return IfcProductRelationExtensions.SubObjects<T>(parent);
                else
                    return Enumerable.Empty<T>();
            };
        }

        public static Qualifier ToQualifiedName(this IPersistEntity instance)
        {
            var q = new Qualifier();
            q.Named = new Name();
            q.Named.Frags.Add(instance.Model.SchemaVersion.ToString());
            q.Named.Frags.Add(instance.ExpressType.Name);
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
