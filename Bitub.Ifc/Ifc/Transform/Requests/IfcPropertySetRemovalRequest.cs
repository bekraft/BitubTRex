using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

using Bitub.Transfer;

using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace Bitub.Ifc.Transform.Requests
{
    /// <summary>
    /// Property set removal package
    /// </summary>
    public class IfcPropertySetRemovalPackage : TransformPackage
    {
        private ISet<string> BlackList = new HashSet<string>();
        internal readonly bool IgnoreCase;

        internal List<IIfcRelDefinesByProperties> RelDefinesByProperties = new List<IIfcRelDefinesByProperties>();

        internal IfcPropertySetRemovalPackage(IModel source, IModel target, bool ignoreCase) : base(source, target)
        {
            IgnoreCase = ignoreCase;
        }

        /// <summary>
        /// The property set names to be removed from model.
        /// </summary>
        /// <param name="blackList">The property set names which shall be removed</param>
        internal void FillBlackListWith(IEnumerable<string> blackList)
        {
            foreach (var name in blackList)
            {
                if (IgnoreCase)
                    BlackList.Add(name.ToLower().Trim());
                else
                    BlackList.Add(name.Trim());
            }
        }

        internal bool PassesNameFilter(IIfcPropertySetDefinition p)
        {
            return !HitsNameFilter(p);
        }

        internal bool HitsNameFilter(IIfcPropertySetDefinition p)
        {
            string name = p.Name;
            if (IgnoreCase)
                return BlackList.Contains(name.ToLower().Trim());
            else
                return BlackList.Contains(name.Trim());
        }

    }

    /// <summary>
    /// Remove entire IFC property sets by given black list names.
    /// </summary>
    public class IfcPropertySetRemovalRequest : IfcTransformRequestTemplate<IfcPropertySetRemovalPackage>
    {
        /// <summary>
        /// The logger.
        /// </summary>
        public override ILogger Log { get; protected set; }

        /// <summary>
        /// Name is "Property Set Removal"
        /// </summary>
        public override string Name => "Property Set Removal";

        /// <summary>
        /// Whether to use case sensitive matches. Default is <c>false</c>
        /// </summary>
        public bool IsNameMatchingCaseSensitive { get; set; } = false;

        /// <summary>
        /// Black list of property set names. If <c>IsNameMatchingCaseSensitive</c> set to <c>true</c>, case sensitive
        /// matches are applied.
        /// </summary>
        public string[] BlackListNames { get; set; } = new string[] { };

        protected override bool IsNoopTransform { get => BlackListNames.Length == 0; }

        public override bool IsInplaceTransform { get => IsNoopTransform; }

        /// <summary>
        /// New property cut task request.
        /// </summary>
        public IfcPropertySetRemovalRequest(ILoggerFactory factory = null)
        {
            Log = factory?.CreateLogger<IfcPropertySetRemovalRequest>();
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, IfcPropertySetRemovalPackage package)
        {
            if (instance is IIfcPropertySet set)
            {
                if (package.HitsNameFilter(set))
                    return TransformActionType.Drop;                
            } 
            else if (instance is IIfcRelDefinesByProperties)
            {
                return TransformActionType.Delegate;                
            } 
            else if (instance is IIfcProperty)
            {
                return TransformActionType.Delegate;
            }

            return TransformActionType.CopyWithInverse;
        }

        protected override IPersistEntity DelegateCopy(IPersistEntity instance, IfcPropertySetRemovalPackage package)
        {
            if(instance is IIfcRelDefinesByProperties rDefProps)
            {
                if(rDefProps.RelatingPropertyDefinition is IfcPropertySetDefinitionSet setOfSet)
                {   // Only exists starting from IFC4
                    if (setOfSet.PropertySetDefinitions.Any(package.HitsNameFilter))
                    {
                        package.RelDefinesByProperties.Add(rDefProps);
                        package.Log?.Add(new TransformLogEntry(new XbimInstanceHandle(rDefProps), TransformAction.Modified));
                        return null;
                    }
                }
                else if (rDefProps.RelatingPropertyDefinition.PropertySetDefinitions.All(package.HitsNameFilter))
                {   //Reject relation if no property set remains after removal
                    package.Log?.Add(new TransformLogEntry(new XbimInstanceHandle(rDefProps), TransformAction.NotTransferred));
                    return null;
                }
            } 
            else if (instance is IIfcProperty prop)
            {
                if (prop.PartOfPset.All(package.HitsNameFilter))
                {
                    // Drop completely if only reference by dropped property sets
                    package.Log?.Add(new TransformLogEntry(new XbimInstanceHandle(prop), TransformAction.NotTransferred));
                    return null;
                }
            }

            return Copy(instance, package, true);
        }

        protected override object PropertyTransform(ExpressMetaProperty property, object hostObject, IfcPropertySetRemovalPackage package)
        {
            if(hostObject is IIfcProduct prod && property.PropertyInfo.Name == nameof(IIfcProduct.IsDefinedBy))
            {
                // Filter relations which only reference non-black listed property sets
                return EmptyToNull(prod.IsDefinedBy
                    .Where(r => r.PropertySet<IIfcPropertySetDefinition>().All(package.PassesNameFilter)));
            }
            else if (hostObject is IIfcRelDefinesByProperties rDefProps)
            {
                // Filter relations which only reference non-black listed property sets
                if (property.PropertyInfo.Name == nameof(IIfcRelDefinesByProperties.RelatingPropertyDefinition))
                {
                    var propDefinition = EmptyToNull(rDefProps.RelatingPropertyDefinition.PropertySetDefinitions.Where(package.PassesNameFilter));
                    if (null == propDefinition)
                        Log?.LogWarning($"Entity IfcRelDefinesByProperties (from #{rDefProps.EntityLabel}) became invalid on transfer.");
                    else
                        Log?.LogInformation($"Entity IfcRelDefinesByProperties (from #{rDefProps.EntityLabel}) changed since some property sets were dropped.");

                    return propDefinition;
                }
            }
            else if (hostObject is IIfcProperty prop && property.PropertyInfo.Name == nameof(IIfcProperty.PartOfPset))
            {   // Filter inverse relation of property
                return EmptyToNull(prop.PartOfPset.Where(package.PassesNameFilter));
            } 
            else if (hostObject is IIfcTypeObject tObject && property.PropertyInfo.Name == nameof(IIfcTypeObject.HasPropertySets))
            {   // Filter inverse relation of type object
                return EmptyToNull(tObject.HasPropertySets.Where(package.PassesNameFilter));
            } 
            else if (hostObject is IIfcRelDefinesByTemplate rDefTemplate && property.PropertyInfo.Name == nameof(IIfcRelDefinesByTemplate.ReferenceEquals))
            {   // Filter inverse relation of template relationship
                return EmptyToNull(rDefTemplate.RelatedPropertySets.Where(package.PassesNameFilter));
            }

            return base.PropertyTransform(property, hostObject, package);
        }

        protected override TransformResult.Code DoPostTransform(IfcPropertySetRemovalPackage package, CancelableProgress progress)
        {
            foreach(var r in package.RelDefinesByProperties)
            {
                if (progress.State.IsCanceled)
                    return TransformResult.Code.Canceled;

                if (r.RelatingPropertyDefinition is IfcPropertySetDefinitionSet setOfSet)
                {
                    // Filter for valid property sets
                    var whiteList = setOfSet.PropertySetDefinitions
                        .Where(package.PassesNameFilter)
                        .OfType<IfcPropertySetDefinition>()
                        .ToList();
                    var newRel = package.Target.NewIfcRelDefinesByProperties(new IfcPropertySetDefinitionSet(whiteList));

                    // Set object relations
                    newRel.RelatedObjects.AddRange(r.RelatedObjects
                        .Select(o => package.Map[new XbimInstanceHandle(o)])
                        .Select(h => package.Target.Instances[h.EntityLabel] as IIfcObjectDefinition));

                    Log?.LogInformation($"Modified #{r.EntityLabel} as new #{newRel.EntityLabel} dropping some property sets.");
                }
            }
            return TransformResult.Code.Finished;
        }

        protected override IfcPropertySetRemovalPackage CreateTransformPackage(IModel aSource, IModel aTarget)
        {
            var package = new IfcPropertySetRemovalPackage(aSource, aTarget, !IsNameMatchingCaseSensitive);
            package.FillBlackListWith(BlackListNames);
            return package;
        }        
    }
}
