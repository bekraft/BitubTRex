using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Xbim.Common;
using Xbim.Common.Enumerations;
using Xbim.Common.ExpressValidation;
using Xbim.Common.Step21;
using Xbim.Common.XbimExtensions;

namespace Bitub.Ifc.Validation
{
    /// <summary>
    /// IFC model validation "stamp".
    /// </summary>
    public class IfcSchemaValidationStamp : IEquatable<IfcSchemaValidationStamp>
    {
        public DateTime Timestamp { get; private set; }

        private class ValidationResultEqualityComparer : IEqualityComparer<ValidationResult>
        {
            public bool Equals(ValidationResult x, ValidationResult y)
            {
                return IsSameResult(x, y);
            }

            public int GetHashCode(ValidationResult obj)
            {
                return obj.GetHashCode();
            }
        }

        /// <summary>
        /// According schema.
        /// </summary>
        public IEnumerable<XbimSchemaVersion> SchemaVersion { get => SchemaResultLookup.Select(g => g.Key); }

        /// <summary>
        /// Validation results by schemata.
        /// </summary>
        public ILookup<XbimSchemaVersion, ValidationResult> SchemaResultLookup { get; private set; }

        /// <summary>
        /// Current results of this stamp.
        /// </summary>
        public IEnumerable<ValidationResult> Results { get => SchemaResultLookup.SelectMany(g => g);  }

        /// <summary>
        /// Results per persitent entity.
        /// </summary>
        public ILookup<XbimInstanceHandle, ValidationResult> InstanceResults 
        { 
            get => Results.Where(r => r.Item is IPersistEntity).ToLookup(r => new XbimInstanceHandle(r.Item as IPersistEntity)); 
        }

        public static IfcSchemaValidationStamp OfModel(IModel model, ValidationFlags validationFlags = ValidationFlags.All)
        {
            return OfInstances(model.Instances, validationFlags);
        }

        public static IfcSchemaValidationStamp OfInstances(IEnumerable<IPersistEntity> instances, ValidationFlags validationFlags = ValidationFlags.All)
        {
            var validator = new Validator()
            {
                CreateEntityHierarchy = true,
                ValidateLevel = validationFlags
            };

            return new IfcSchemaValidationStamp
            {
                Timestamp = DateTime.Now,
                SchemaResultLookup = instances
                    .SelectMany(instance => validator.Validate(instance).Select(result => (instance.Model.SchemaVersion, result)))
                    .ToLookup(g => g.SchemaVersion, g => g.result)
            };
        }

        public static bool IsSameResult(ValidationResult a, ValidationResult b)
        {
            return (a.Item == b.Item) 
                && (a.IssueType == b.IssueType)
                && String.Equals(a.IssueSource, b.IssueSource, StringComparison.Ordinal)
                && String.Equals(a.Message, b.Message, StringComparison.Ordinal);
        }

        public static bool IsSameResultInContext(ValidationResult a, ValidationResult b)
        {
            ValidationResult r1 = a;
            ValidationResult r2 = b;
            bool isSameInContext;
            do
            {
                isSameInContext = IsSameResult(r1, r2);
                r1 = r1.Context;
                r2 = r2.Context;
            } while (isSameInContext && (null != r1) && (null != r2));

            return isSameInContext && (null == r1) && (null == r2);
        }

        /// <summary>
        /// Does a set difference operation by comparing results A and B. 
        /// </summary>
        /// <param name="aResults">Result A</param>
        /// <param name="bResults">Result B</param>
        /// <returns>Returns A without B</returns>
        public static IEnumerable<ValidationResult> Diff(IEnumerable<ValidationResult> aResults, IEnumerable<ValidationResult> bResults)
        {
            var left = new HashSet<ValidationResult>(aResults, new ValidationResultEqualityComparer());
            foreach (var bResult in bResults)
                left.Remove(bResult);

            return left.ToArray();
        }

        public IEnumerable<ValidationResult> Diff(IfcSchemaValidationStamp other)
        {
            return Diff(Results, other.Results);
        }

        /// <summary>
        /// Whether both results are equivalent. The compare is not sensitive to the order of results since
        /// it matches per item. The time stamp isn't considered.
        /// </summary>
        /// <param name="other">The other stamp.</param>
        /// <returns>True, if both have the same issues, same issue types and messages</returns>
        public bool Equals(IfcSchemaValidationStamp other)
        {
            var diffA = Diff(Results, other.Results);
            var diffB = Diff(other.Results, Results);
            return !diffA.Any() && !diffB.Any();
        }

        // A schema mandatory proposition failure
        private bool IsComplianceFailure(ValidationResult r)
        {
            return ValidationFlags.None != (r.IssueType & (ValidationFlags.Properties | ValidationFlags.Inverses));
        }

        // A WHERE clause failure
        private bool IsConstraintFailure(ValidationResult r)
        {
            return ValidationFlags.None != (r.IssueType & (ValidationFlags.EntityWhereClauses | ValidationFlags.TypeWhereClauses));
        }

        /// <summary>
        /// Whether the results indicate no conflicts with constraint rules (WHERE clauses) of the referenced schema.
        /// </summary>
        public bool IsConstraintToSchema { get => !Unfold().Any(IsConstraintFailure); }

        /// <summary>
        /// Whether the results indicate no schema conflict in missing references and properties.
        /// </summary>
        public bool IsCompliantToSchema { get => !Unfold().Any(IsComplianceFailure); }

        /// <summary>
        /// Flattens all validations results.
        /// </summary>
        /// <returns>An unfold flat hierarchy of results in topological order (children fellow parents)</returns>
        public IEnumerable<ValidationResult> Unfold(ValidationFlags filter = ValidationFlags.All)
        {
            var stack = new Stack<ValidationResult>(Results ?? Enumerable.Empty<ValidationResult>());
            while (stack.Count > 0)
            {
                var result = stack.Pop();
                foreach (var child in result.Details.Where(d => filter.HasFlag(d.IssueType)))
                    stack.Push(child);

                yield return result;
            }
        }

        /// <summary>
        /// Returns all compliance failures by schema version.
        /// </summary>
        public ILookup<XbimSchemaVersion, ValidationResult> SchemaComplianceFailures
        {
            get => SchemaResultLookup
                .SelectMany(g => g.Where(IsComplianceFailure).Select(result => (g.Key, result)))
                .ToLookup(g => g.Key, g => g.result);
        }

        /// <summary>
        /// Returns all constraint failures by schema version.
        /// </summary>
        public ILookup<XbimSchemaVersion, ValidationResult> SchemaConstraintFailures
        {
            get => SchemaResultLookup
                .SelectMany(g => g.Where(IsConstraintFailure).Select(result => (g.Key, result)))
                .ToLookup(g => g.Key, g => g.result);
        }

        public override bool Equals(object obj)
        {
            if (obj is IfcSchemaValidationStamp s)
                return Equals(s);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return 1627825885 + EqualityComparer<IEnumerable<ValidationResult>>.Default.GetHashCode(Results);
        }
    }
}
