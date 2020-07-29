using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Logging;

using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Geometry.Engine.Interop;

namespace Bitub.Ifc.Validation
{
    public class IfcGeometryValidationStamp
    {
        #region Internals
        private readonly XbimGeometryEngine Engine;
        private readonly ILogger<XbimGeometryEngine> Log;
        #endregion

        public DateTime Timestamp { get; private set; }

        private IfcGeometryValidationStamp(ILoggerFactory loggerFactory)
        {
            Log = loggerFactory.CreateLogger<XbimGeometryEngine>();
            Engine = new XbimGeometryEngine(Log);
        }

        private IEnumerable<(Type, MethodInfo)> FindGeometryEngineMatch(IPersistEntity entity)
        {
            var methods = typeof(XbimGeometryEngine).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var methodInfo in methods)
            {
                var pars = methodInfo.GetParameters().ToArray();
                if (pars.Length != 1) // only consider functinons with a single parameter
                    continue;

                if (methodInfo.ReturnParameter.ParameterType == typeof(bool))
                    continue; // excludes the equal function

                if (!pars.FirstOrDefault()?.ParameterType.IsInstanceOfType(entity) ?? true)
                    continue;

                yield return (pars.FirstOrDefault()?.ParameterType, methodInfo);
            }
        }
        /*
        private IEnumerable<(Type, IXbimGeometryObject[])> GetSolids(IPersistEntity entity)
        {
            foreach (var method in FindGeometryEngineMatch(entity))
            {
                try
                {
                    var value = method.Item2.Invoke(Engine, new object[] { entity });
                    if (value is IXbimSolid solid)
                    {
                        yield return (method.Item1, new IXbimGeometryObject[] { solid });
                    }
                    if (value is IXbimSolidSet solidSet)
                    {
                        yield return (method.Item1, new IXbimGeometryObject[] { solidSet }):
                    }
                    else
                        yield return (method.Item1, new IXbimGeometryObject[] { });
                }
                catch (Exception ex)
                {
                    getSolidRet.Item2.Add(null);
                    var msg = $"  Failed on {functionShort} for #{entity.EntityLabel}. {ex.Message}";
                    ReportAdd(msg, Brushes.Red);
                }
                yield return getSolidRet;
            }
        }
        */
    }
}
