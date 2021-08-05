using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xbim.Common;
using Xbim.Common.Geometry;

using Xbim.Ifc4.Interfaces;

using Bitub.Dto.Spatial;
using Bitub.Ifc.Export;
using Xbim.Ifc2x3.SharedBldgElements;

namespace Bitub.Ifc.Validation
{
    [Flags]
    public enum GeometryIssueType
    {
        IsTopologicallyOpenShell = 0x01,
        IsGeometricallyOpenShell = 0x02,
        IsOpenShell = 0x03,
        HasUnconnectedShells = 0x04,
        IsInvalid = 0x08,
        IsUncheckedType = 0x10,
    }

    public sealed class GeometryIssue
    {
        public Type ObjectType { get; private set; }
        public GeometryIssueType IssueFlag { get; private set; } = 0;
        public double? Volume { get; private set; }
        public double EnclosedVolume { get; private set; }
        public ABox BoundingBox { get; private set; }
        public string Message { get; private set; }
        public XbimInstanceHandle InstanceHandle { get; private set; }

        public bool HasGeometricalIssues { get => IssueFlag != 0; }

        public GeometryIssue Parent { get; private set; }

        #region Internals

        private GeometryIssue(GeometryValidator validator, XbimInstanceHandle instanceHandle)
        {
            Validator = validator;
            InstanceHandle = instanceHandle;
        }

        /* TODO FromSolid
        private static GeometryIssue FromSolid(GeometryValidator validator, XbimInstanceHandle instanceHandle, Type objectType, IXbimSolid solid)
        {

            var template = new GeometryIssue(validator, instanceHandle);
            template.ObjectType = objectType;
            template.BoundingBox = solid.BoundingBox.ToABox();
            template.Volume = solid.Volume;
            template.EnclosedVolume = solid.Volume ?? 0;
            template.Message = $"Singleton '{solid.GeometryType}' solid.";
            if (!template.Volume.HasValue)
                template.IssueFlag |= GeometryIssueType.IsOpenShell;
            if (!solid.IsValid)
                template.IssueFlag |= GeometryIssueType.IsInvalid;            
            return template;            
        }
        */

        /* TODO FromSolidSet
        private static IEnumerable<GeometryIssue> FromSolidSet(GeometryValidator validator, XbimInstanceHandle instanceHandle, Type objectType, IXbimSolidSet solidSet)
        {
            var template = new GeometryIssue(validator, instanceHandle);
            template.ObjectType = objectType;
            template.BoundingBox = solidSet.BoundingBox.ToABox();
            template.Volume = solidSet.Volume;
            template.EnclosedVolume = solidSet.VolumeValid;
            template.Message = $"Set of {solidSet.Count} solids.";

            if (!solidSet.Volume.HasValue || solidSet.VolumeValid != solidSet.Volume.Value && solidSet.Any(g => !g.Volume.HasValue || !g.IsValid))
                template.IssueFlag |= GeometryIssueType.IsOpenShell;
            if (!solidSet.IsValid || solidSet.Any(g => !g.IsValid))
                template.IssueFlag |= GeometryIssueType.IsInvalid;

            yield return template;
            foreach (var part in solidSet)
            {
                var child = FromSolid(validator, instanceHandle, objectType, part);
                child.Parent = template;
                yield return child;
            }
        }
        */

        /* TODO FromInstanceHandle
        public static IEnumerable<GeometryIssue> FromInstanceHandle(GeometryValidator validator, XbimInstanceHandle instanceHandle)
        {
            var persistent = instanceHandle.Model.Instances[instanceHandle.EntityLabel];
            if (persistent is IIfcRepresentationItem item)
            {
                foreach (var (gType, gObject) in validator.GetGeometryObjects(item, validator.IsUsingEngineReflection, validator.IsReturningSolidsOnly))
                {
                    if (gObject is IXbimSolid solid)
                    {
                        yield return FromSolid(validator, instanceHandle, gType, solid);
                    }
                    else if (gObject is IXbimSolidSet set)
                    {
                        foreach (var partialIssue in FromSolidSet(validator, instanceHandle, gType, set))
                            yield return partialIssue;
                    }
                    else
                    {
                        var issue = new GeometryIssue(validator, instanceHandle);
                        issue.IssueFlag = GeometryIssueType.IsUncheckedType;
                        issue.Message = $"Unchecked type '{gType}' found implementing '{instanceHandle.EntityExpressType.Name}'.";
                        yield return issue;
                    }                    
                }
            }
        }
        */

        #endregion

        public IModel Model { get => InstanceHandle.Model; }

        public GeometryValidator Validator { get; private set; }

    }
}
