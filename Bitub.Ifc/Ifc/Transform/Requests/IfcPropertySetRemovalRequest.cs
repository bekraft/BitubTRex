using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Bitub.Dto;

using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace Bitub.Ifc.Transform.Requests
{
    public enum FilterRuleStrategyType
    {
        IncludeBeforeExclude, ExcludeBeforeInclude
    }

    /// <summary>
    /// Property set removal package
    /// </summary>
    public class IfcPropertySetRemovalPackage : TransformPackage
    {
        #region Internals
        private FilterRuleStrategyType removalStrategy;
        internal List<IIfcRelDefinesByProperties> relDefinesByProperties = new List<IIfcRelDefinesByProperties>();
        #endregion

        /// <summary>
        /// Names to be excluded.
        /// </summary>
        public ISet<string> ExcludeName { get; private set; }

        /// <summary>
        /// Names to be included exclusively.
        /// </summary>
        public ISet<string> IncludeExclusivelyName { get; private set; }

        /// <summary>
        /// Property definition relations which ought to be modified.
        /// </summary>
        public IEnumerable<IIfcRelDefinesByProperties> ModifiedRelDefinesProperties
        { 
            get => ImmutableList.ToImmutableList(relDefinesByProperties); 
        }

        internal IfcPropertySetRemovalPackage(IModel source, IModel target, 
            bool ignoreCase, FilterRuleStrategyType strategy) : base(source, target)
        {
            var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            ExcludeName = new HashSet<string>(comparer);
            IncludeExclusivelyName = new HashSet<string>(comparer);
            removalStrategy = strategy;
        }

        public bool HasNameConflicts(ILogger logger = null)
        {
            var conflicts = ExcludeName
                .Where(n => IncludeExclusivelyName.Contains(n));
            if (null != logger)
                conflicts.ForEach(n => logger.LogWarning("Property set '{0}' is marked for removal and inclusion.", n));

            return conflicts.Any();        
        }

        /// <summary>
        /// Passes by default if empty filter or not contained.
        /// </summary>
        /// <param name="pset">The property set</param>
        /// <returns>True, if passes by name</returns>
        public bool PassesExclusionFilter(IIfcPropertySetDefinition pset)
        {
            return ExcludeName.Count == 0 || !ExcludeName.Contains(pset.Name.ToString());
        }

        /// <summary>
        /// Passes by default if empty filter or not contained.
        /// </summary>
        /// <param name="pset">The property set</param>
        /// <returns>True, if passes by name</returns>
        public bool PassesInclusionFilter(IIfcPropertySetDefinition pset)
        {
            return IncludeExclusivelyName.Count == 0 || IncludeExclusivelyName.Contains(pset.Name.ToString());
        }

        /// <summary>
        /// Passes both filters in sequence given by <see cref="FilterRuleStrategyType"/>.
        /// </summary>
        /// <param name="pset">The property set</param>
        /// <returns>True, if set passes by its name</returns>
        public bool PassesNameFilter(IIfcPropertySetDefinition pset)
        {
            switch (removalStrategy)
            {
                case FilterRuleStrategyType.IncludeBeforeExclude:
                    if (IncludeExclusivelyName.Count > 0)
                        return IncludeExclusivelyName.Contains(pset.Name.ToString());
                    else
                        return PassesExclusionFilter(pset);
                case FilterRuleStrategyType.ExcludeBeforeInclude:
                    if (ExcludeName.Count > 0)
                        return !ExcludeName.Contains(pset.Name.ToString());
                    else
                        return PassesInclusionFilter(pset);                    
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Inverse to <see cref="PassesNameFilter(IIfcPropertySetDefinition)"/>.
        /// </summary>
        /// <param name="pset">The property set</param>
        /// <returns>True, if set hits by its name</returns>
        public bool HitsNameFilter(IIfcPropertySetDefinition pset) => !PassesNameFilter(pset);
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
        public FilterRuleStrategyType FilterRuleStrategy { get; set; } = FilterRuleStrategyType.IncludeBeforeExclude;

        /// <summary>
        /// Black list of property set names. 
        /// If <see cref="IsNameMatchingCaseSensitive"/> set to <c>true</c>, case sensitive matches are applied.
        /// </summary>
        public string[] ExludePropertySetByName { get; set; } = new string[] { };

        /// <summary>
        /// White list of property set names.
        /// If <see cref="IsNameMatchingCaseSensitive"/> set to <c>true</c>, case sensitive matches are applied.
        /// </summary>
        public string[] IncludePropertySetByName { get; set; } = new string[] { };

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
                        package.relDefinesByProperties.Add(rDefProps);
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
            foreach(var r in package.relDefinesByProperties)
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
            var package = new IfcPropertySetRemovalPackage(aSource, aTarget, !IsNameMatchingCaseSensitive, FilterRuleStrategy);
            ExludePropertySetByName.ForEach(n => package.ExcludeName.Add(n));
            IncludePropertySetByName.ForEach(n => package.IncludeExclusivelyName.Add(n));            
            return package;
        }        
    }
}
