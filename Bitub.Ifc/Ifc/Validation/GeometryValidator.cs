using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Logging;

using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Geometry.Engine.Interop;

using Xbim.Ifc4.Interfaces;

using Bitub.Ifc;

namespace Bitub.Ifc.Validation
{
    public class GeometryValidator
    {
        #region Internals
        private readonly XbimGeometryEngine engine;
        private readonly ILogger<XbimGeometryEngine> log;
        #endregion

        public GeometryValidator(ILoggerFactory loggerFactory)
        {
            log = loggerFactory.CreateLogger<XbimGeometryEngine>();
            engine = new XbimGeometryEngine(log);
        }

        public bool IsReturningSolidsOnly { get; set; } = true;

        public bool IsUsingEngineReflection { get; set; } = false;

        internal IEnumerable<(Type, MethodInfo)> FindGeometryEngineMatch(IPersistEntity entity)
        {
            var methods = typeof(XbimGeometryEngine).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var methodInfo in methods)
            {
                var pars = methodInfo.GetParameters().ToArray();

                if (!typeof(IXbimGeometryObject).IsAssignableFrom(methodInfo.ReturnParameter.ParameterType))
                    continue;

                if (pars.Length > 2)
                    continue;

                if (!pars.FirstOrDefault()?.ParameterType.IsInstanceOfType(entity) ?? true)
                    continue;

                var param2nd = pars.Skip(1).FirstOrDefault()?.ParameterType;
                if (null == param2nd || !typeof(ILogger).IsAssignableFrom(param2nd))
                    continue;

                yield return (pars.FirstOrDefault()?.ParameterType, methodInfo);
            }
        }
        
        internal IEnumerable<(Type, IXbimGeometryObject)> GetGeometryObjects(IPersistEntity entity, bool useDedicatedCreateMethod, bool acceptOnlySolids = true)
        {
            foreach (var (type, methodInfo) in FindGeometryEngineMatch(entity))
            {
                object geometryObject;
                try
                {
                    if (useDedicatedCreateMethod && entity is IIfcGeometricRepresentationItem gItem)
                        geometryObject = engine.Create(gItem, log);
                    else
                        geometryObject = methodInfo.Invoke(engine, new object[] { entity, log });
                }
                catch (Exception e)
                {
                    log.LogError("Got exception '{0}' with call to '{1}({2})'", e.Message, methodInfo.Name, type.Name);
                    continue;
                }

                if (geometryObject is IXbimSolid solid)
                {
                    yield return (type, solid);
                }
                if (geometryObject is IXbimSolidSet solidSet)
                {
                    yield return (type, solidSet);
                }
                else if (!acceptOnlySolids)
                    yield return (type, geometryObject as IXbimGeometryObject);
            }
        }

        public ILookup<IIfcProduct, GeometryIssue> GetIssuesFromProducts(IEnumerable<IIfcProduct> products)
        {
            return products.Where(product => null != product?.Representation).SelectMany(product => product.Representation.Representations
                    .SelectMany(r => r.Items
                        .SelectMany(item => GeometryIssue.FromInstanceHandle(this, new XbimInstanceHandle(item)).Select(issue => (product, issue)))))
                .ToLookup(g => g.product, g => g.issue);
        }

        public ILookup<IIfcProduct,GeometryIssue> GetIssuesFromModel(IModel model)
        {
            return GetIssuesFromProducts(model.Instances.OfType<IIfcProduct>());
        }

        public ILookup<IModel, GeometryIssue> GetIssuesFromRepresentationItems(IEnumerable<IIfcRepresentationItem> representationItems)
        {
            return representationItems
                .SelectMany(item => GeometryIssue.FromInstanceHandle(this, new XbimInstanceHandle(item)))
                .ToLookup(issue => issue.Model);
        }
    }
}
