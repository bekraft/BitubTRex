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
        #region Internals

        private readonly ISet<string> RemovePropertySet = new HashSet<string>();
        private readonly ISet<string> KeepPropertySet = new HashSet<string>();

        #endregion

        internal readonly bool IgnoreCase;

        internal readonly bool RemovePSetOnConflict;

        internal List<IIfcRelDefinesByProperties> RelDefinesByProperties = new List<IIfcRelDefinesByProperties>();

        internal IfcPropertySetRemovalPackage(IModel source, IModel target, bool ignoreCase, bool removePSetOnConflict) : base(source, target)
        {
            IgnoreCase = ignoreCase;
            RemovePSetOnConflict = removePSetOnConflict;
        }

        /// <summary>
        /// The property set names to be removed from model.
        /// </summary>
        /// <param name="pSetNames">The property set names which shall be removed</param>
        internal void AddToRemovePropertySet(IEnumerable<string> pSetNames)
        {
            foreach (var name in pSetNames)
            {
                if (IgnoreCase)
                    RemovePropertySet.Add(name.ToLower().Trim());
                else
                    RemovePropertySet.Add(name.Trim());
            }
        }

        /// <summary>
        /// The property set names to be removed from model.
        /// </summary>
        /// <param name="pSetNames">The property set names which shall be removed</param>
        internal void AddToKeepPropertySet(IEnumerable<string> pSetNames)
        {
            foreach (var name in pSetNames)
            {
                if (IgnoreCase)
                    KeepPropertySet.Add(name.ToLower().Trim());
                else
                    KeepPropertySet.Add(name.Trim());
            }
        }

        internal bool HasConflicts(ILogger logger = null)
        {
            bool hasConflicts = false;
            foreach (string name in RemovePropertySet)
            {
                if (KeepPropertySet.Contains(name))
                {
                    hasConflicts = true;
                    if (null != logger)
                        logger.LogWarning("Property set '{0}' is marked both for removal and to be kept. RemovePSetOnConflict = {1}", name, RemovePSetOnConflict);
                    else
                        return hasConflicts;
                }
            }
            return hasConflicts;
        }

        // Keep property set
        internal bool PassesNameFilter(IIfcPropertySetDefinition p)
        {
            return !HitsNameFilter(p);
        }

        // Dropped by name filter
        internal bool HitsNameFilter(IIfcPropertySetDefinition p)
        {
            string name = IgnoreCase ? p.Name.ToString().ToLower().Trim() : p.Name.ToString();

            if (KeepPropertySet.Count == 0)
            {
                // No white list, maybe only black list or nothing (noop tranform)
                return RemovePropertySet.Contains(name);
            } 
            else if (RemovePropertySet.Count == 0)
            {
                // Only to be kept names defined
                return !KeepPropertySet.Contains(name);
            }
            else
            {   // Both lists are populated
                if (RemovePropertySet.Contains(name) && KeepPropertySet.Contains(name))
                    // Use priority flag
                    return RemovePSetOnConflict;
                else
                    // Otherwise hit if removal list is hit or keep list doesn't hit
                    return RemovePropertySet.Contains(name) || !KeepPropertySet.Contains(name);
            }
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
        /// Whether to remove property sets marked for keeping if they mentioned also for removal.
        /// Will keep property set by default.
        /// </summary>
        public bool IsRemovingPSetOnConflict { get; set; } = false;

        /// <summary>
        /// Black list of property set names. 
        /// If <see cref="IsNameMatchingCaseSensitive"/> set to <c>true</c>, case sensitive matches are applied.
        /// </summary>
        public string[] RemovePropertySet { get; set; } = new string[] { };

        /// <summary>
        /// White list of property set names.
        /// If <see cref="IsNameMatchingCaseSensitive"/> set to <c>true</c>, case sensitive matches are applied.
        /// </summary>
        public string[] KeepPropertySet { get; set; } = new string[] { };

        protected override bool IsNoopTransform { get => RemovePropertySet.Length == 0; }

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

            return TransformActionType.Copy;
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

            return Copy(instance, package, false);
        }

        protected override object PropertyTransform(ExpressMetaProperty property, object hostObject, IfcPropertySetRemovalPackage package)
        {
            if(hostObject is IIfcProduct prod && property.PropertyInfo.Name == nameof(IIfcProduct.IsDefinedBy)) // Inverse
            {
                // Filter relations which only reference non-black listed property sets
                return null; // FIXED: TREXDYNAMO-1 EmptyToNull(prod.IsDefinedBy.Where(r => r.PropertySet<IIfcPropertySetDefinition>().All(package.PassesNameFilter)));
            }
            else if (hostObject is IIfcRelDefinesByProperties rDefProps)
            {
                // Filter relations which only reference non-black listed property sets
                if (property.PropertyInfo.Name == nameof(IIfcRelDefinesByProperties.RelatingPropertyDefinition))
                {
                    var propDefinition = EmptyToNull(rDefProps.RelatingPropertyDefinition.PropertySetDefinitions.Where(package.PassesNameFilter));
                    if (null == propDefinition)
                    {   // If no one left, dump a warning since relation is now an orphan
                        var targetObject = package.Map[new XbimInstanceHandle(rDefProps)];
                        Log?.LogWarning($"Entity IfcRelDefinesByProperties (#{rDefProps.EntityLabel} => #{targetObject.EntityLabel}) became invalid on transfer.");
                        return propDefinition;
                    }
                    else
                    {
                        if (propDefinition.Count() > 1)
                            // Only IFC4+, if more than one use a set
                            return new IfcPropertySetDefinitionSet(propDefinition.Cast<IfcPropertySetDefinition>().ToList());
                        else
                            // Otherwise, return first
                            return propDefinition.First();
                    }
                }
            }
            else if (hostObject is IIfcProperty prop && property.PropertyInfo.Name == nameof(IIfcProperty.PartOfPset)) // Inverse
            {   // Filter inverse relation of property
                return null; // FIXED: TREXDYNAMO-1 EmptyToNull(prop.PartOfPset.Where(package.PassesNameFilter));
            } 
            else if (hostObject is IIfcTypeObject tObject && property.PropertyInfo.Name == nameof(IIfcTypeObject.HasPropertySets))
            {   // Filter inverse relation of type object
                return EmptyToNull(tObject.HasPropertySets.Where(package.PassesNameFilter));
            } 
            else if (hostObject is IIfcRelDefinesByTemplate rDefTemplate && property.PropertyInfo.Name == nameof(IIfcRelDefinesByTemplate.RelatedPropertySets))
            {   // Filter inverse relation of template relationship
                return EmptyToNull(rDefTemplate.RelatedPropertySets.Where(package.PassesNameFilter));
            }

            return base.PropertyTransform(property, hostObject, package);
        }

        protected override TransformResult.Code DoPostTransform(IfcPropertySetRemovalPackage package, CancelableProgressing progress)
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
            var package = new IfcPropertySetRemovalPackage(aSource, aTarget, !IsNameMatchingCaseSensitive, IsRemovingPSetOnConflict);
            package.AddToRemovePropertySet(RemovePropertySet);
            package.AddToKeepPropertySet(KeepPropertySet);
            return package;
        }        
    }
}
