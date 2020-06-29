using System.Linq;
using System.Collections.Generic;

using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.DateTimeResource;

namespace Bitub.Ifc
{
    /// <summary>
    /// Aggregated IFC model meta data history access.
    /// </summary>
    public class IfcMetadataHistory
    {
        /// <summary>
        /// The referenced model.
        /// </summary>
        public IModel Model { get; private set; }

        /// <summary>
        /// The project name.
        /// </summary>
        public string ProjectName 
        { 
            get => Model.Instances.FirstOrDefault<IIfcProject>()?.Name; 
        }

        /// <summary>
        /// A new history access.
        /// </summary>
        /// <param name="ifcModel"></param>
        public IfcMetadataHistory(IModel ifcModel)
        {
            Model = ifcModel;            
        }

        public IfcAuthoringMetadata Latest
        {
            get {                
                var latest = OwnerHistory.OrderBy(i => (long)i.LastModifiedDate).LastOrDefault();
                return ToMetadata(latest);
            }
        }

        private static System.DateTime GetModifiedOrCreation(IIfcOwnerHistory ownerHistory)
        {
            return (ownerHistory.LastModifiedDate ?? ownerHistory.CreationDate).ToDateTime();
        }

        public IEnumerable<IfcAuthoringMetadata> Chronically
        {
            get {
                foreach (var ownerHistory in OwnerHistory.OrderBy(i => GetModifiedOrCreation(i).Ticks))
                    yield return ToMetadata(ownerHistory);
            }
        }

        protected IfcAuthoringMetadata ToMetadata(IIfcOwnerHistory ifcOwnerHistory)
        {
            var metaData = new IfcAuthoringMetadata
            {
                Owner = ifcOwnerHistory?.OwningUser?.ThePerson?.ToAuthorData(),
                Editor = ifcOwnerHistory?.LastModifyingUser?.ThePerson?.ToAuthorData(),
                AuthoringApplication = ifcOwnerHistory?.OwningApplication?.ToApplicationData(),
                LastKnownAccess = GetModifiedOrCreation(ifcOwnerHistory)
            };
            return metaData;
        }

        public IEnumerable<IIfcOwnerHistory> OwnerHistory 
        { 
            get => Model?.Instances.OfType<IIfcOwnerHistory>() ?? Enumerable.Empty<IIfcOwnerHistory>(); 
        }
    }
}
