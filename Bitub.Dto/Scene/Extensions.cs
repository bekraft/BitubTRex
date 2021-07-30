using Bitub.Dto.Spatial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Bitub.Dto.Scene
{
    /// <summary>
    /// Scene model extensions.
    /// </summary>
    public static class Extensions
    {
        #region Transform context

        public static string ToLinedString(this Rotation r)
        {
            return string.Format("{0} {1} {2}", r.Rx.ToLinedString(), r.Ry.ToLinedString(), r.Rz.ToLinedString());
        }

        #endregion

        #region ComponentScene context

        /// <summary>
        /// Produces a cluster lookup by given classifier extraction
        /// </summary>
        /// <param name="model">The scene</param>
        /// <param name="extractor">The extraction delegate</param>
        /// <returns>A lookup by classifier</returns>
        public static ILookup<Qualifier, Component> ClusterBy(this ComponentScene model, Func<Component, Qualifier> extractor)
        {
            return model.Components.ToLookup(c => extractor(c));
        }

        /// <summary>
        /// Produces an enumerable of classifiers by given extraction.
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="extractor">The extractor</param>
        /// <returns>Dictionary referencing from ID to classifier</returns>
        public static Dictionary<Component, Qualifier> ClassifyBy(this ComponentScene model, Func<Component, Qualifier> extractor)
        {
            return model.Components.ToDictionary(c => c, c => extractor(c));
        }

        /// <summary>
        /// Returns all components in depth-first order. It ensures that components
        /// are visited in the correct hierarchical order.
        /// </summary>
        /// <param name="model">The model</param>
        /// <returns>An enumerable component</returns>
        public static IEnumerable<Component> DepthFirstOrder(this ComponentScene model)
        {
            // Build parentId lookup and generate queue by roots
            var hierarchy = model.Components.ToLookup(c => c.Parent);
            var stack = new Stack<Component>(hierarchy[null]);

            while(stack.Count > 0)
            {
                var component = stack.Pop();
                if(hierarchy.Contains(component.Id))
                {
                    foreach (var c in hierarchy[component.Id])
                        stack.Push(c);
                }
                yield return component;
            }
        }

        /// <summary>
        /// Returns all components in leveling order. Components of the same
        /// hierarchical depth are yield subsequently.
        /// </summary>
        /// <param name="model">The model</param>
        /// <returns>An enumerable component</returns>
        public static IEnumerable<Component> LevelOrder(this ComponentScene model)
        {
            // Build parentId lookup and generate queue by roots
            var hierarchy = model.Components.ToLookup(c => c.Parent);
            var queue = new Queue<Component>(hierarchy[null]);

            while (queue.Count > 0)
            {
                var component = queue.Dequeue();
                if (hierarchy.Contains(component.Id))
                {
                    foreach (var c in hierarchy[component.Id])
                        queue.Enqueue(c);
                }
                yield return component;
            }
        }

        #endregion
    }
}