﻿using System;
using System.Linq;
using System.Collections.Generic;

using Bitub.Dto;

using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Common.Metadata;

namespace Bitub.Ifc.Concept
{
    public static class XbimIfcConceptExtensions
    {
        #region Internals

        private static Comparison<ExpressType> ExpressTypeComparision = (a, b) => Math.Sign(a.TypeId - b.TypeId);

        private static IEnumerable<ExpressType> ToExpressBottomUpPath(this ExpressType expressType, short[] ascTypeIds)
        {
            int propertyCount = Int32.MaxValue;            
            do
            {
                if (null != ascTypeIds && -1 < Array.BinarySearch(ascTypeIds, expressType.TypeId))
                {   // If exists in the exclusively given type IDs
                    yield return expressType;
                } 
                else if (!expressType.Type.IsAbstract || expressType.Properties.Count < propertyCount || null == expressType.SuperType)
                {   // Otherwise only if non-indicating only
                    propertyCount = expressType.Properties.Count;
                    yield return expressType;
                }

                expressType = expressType.SuperType;
            } while (null != expressType);
        }

        private static IEnumerable<ExpressType> ToExpressBottomUpPath<T>(short[] ascTypeIds) where T : IPersistEntity
        {
            return new ExpressType(typeof(T)).ToExpressBottomUpPath(ascTypeIds);
        }

        #endregion

        public static Classifier ToClassifier(this IPersistEntity instance, params ExpressType[] markerTypes)
        {
            var typeIds = markerTypes?.Select(t => t.TypeId).ToArray();
            Array.Sort(typeIds);

            var classifier = new Classifier();
            instance.ExpressType.ToExpressBottomUpPath(typeIds)
                .Select(type => instance.Model.SchemaVersion.ToQualifiedName(type))
                .Reverse()
                .ForEach(qualifier => classifier.Path.Add(qualifier));
            return classifier;
        }

        public static Classifier ToClassifier<T>(this XbimSchemaVersion schemaVersion, params ExpressType[] markerTypes) where T : IPersistEntity
        {
            var typeIds = markerTypes?.Select(t => t.TypeId).ToArray();
            Array.Sort(typeIds);

            var classifier = new Classifier();
            ToExpressBottomUpPath<T>(typeIds)
                .Select(type => ToQualifiedName(schemaVersion, type))
                .Reverse()
                .ForEach(qualifier => classifier.Path.Add(qualifier));
            return classifier;
        }

        internal static Classifier ToInternalClassifier(this XbimSchemaVersion schemaVersion, ExpressType expressType, short[] ascTypeIds)
        {
            var classifier = new Classifier();
            ToExpressBottomUpPath(expressType, ascTypeIds)
                .Select(type => ToQualifiedName(schemaVersion, type))
                .Reverse()
                .ForEach(qualifier => classifier.Path.Add(qualifier));
            return classifier;
        }

        public static Classifier ToClassifier(this XbimSchemaVersion schemaVersion, ExpressType expressType, params ExpressType[] markerTypes)
        {
            var typeIds = markerTypes?.Select(t => t.TypeId).ToArray();
            Array.Sort(typeIds);
            return ToInternalClassifier(schemaVersion, expressType, typeIds);
        }

        public static Classifier ToClassifier(this XbimSchemaVersion schemaVersion, Type entityType, params ExpressType[] markerTypes)
        {
            var typeIds = markerTypes?.Select(t => t.TypeId).ToArray();
            Array.Sort(typeIds);

            if (!typeof(IPersistEntity).IsAssignableFrom(entityType))
                throw new NotSupportedException($"Given type '{entityType.FullName}' isn't an mapped EXPRESS type");

            var classifier = new Classifier();
            ToExpressBottomUpPath(new ExpressType(entityType), typeIds)
                .Select(type => ToQualifiedName(schemaVersion, type))
                .Reverse()
                .ForEach(qualifier => classifier.Path.Add(qualifier));
            return classifier;
        }

        public static Qualifier ToQualifiedName(this XbimSchemaVersion schemaVersion, ExpressType expressType)
        {
            var q = new Qualifier();
            q.Named = new Name();
            q.Named.Frags.Add(schemaVersion.ToString());
            q.Named.Frags.Add(expressType.Name);
            return q;
        }

        public static IEnumerable<Classifier> ToImplementingClassifiers<T>(this XbimSchemaVersion schemaVersion, params ExpressType[] markerTypes) where T : IPersistEntity
        {
            var typeIds = markerTypes?.Select(t => t.TypeId).ToArray();
            Array.Sort(typeIds);

            return IfcAssemblyScope.SchemaAssemblyScope[schemaVersion].metadata
                .Types()
                .Where(t => typeof(T).IsAssignableFrom(t.Type))
                .Select(t => ToInternalClassifier(schemaVersion, t, typeIds));
        }

        public static IDictionary<Type, Classifier> ToImplementingClassification<T>(this XbimSchemaVersion schemaVersion) where T : IPersistEntity
        {
            return IfcAssemblyScope.SchemaAssemblyScope[schemaVersion].metadata
                .Types()
                .Where(t => typeof(T).IsAssignableFrom(t.Type))
                .ToDictionary(t => t.Type, t => ToInternalClassifier(schemaVersion, t, null));                
        }
    }
}