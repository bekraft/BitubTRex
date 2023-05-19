using System.Linq;
using System.Collections.Generic;

namespace Bitub.Dto.Scene
{
    /// <summary>
    /// Simple half edge structure (here named Arc).
    /// </summary>
    /// <typeparam name="T">The embedding type</typeparam>
    public class Arc<T>
    {
        #region Internals
        private Arc<T> twin;

        #endregion

        public Arc(T target)
        {
            this.Target = target;
        }

        public T Target { get; private set; }

        public T Origin => Twin.Target;

        public Arc<T> Twin 
        { 
            get => twin;
            set
            {
                if (null == value)
                {
                    twin = null;
                }
                else
                {
                    if (null != value.Twin)
                        // Decouple twin's twin
                        value.Twin.Twin = null;

                    // Couple twins
                    twin = value;
                    value.Twin = this;
                }
            }
        }

        public Arc<T> NextF { get; set; }

        public Arc<T> NextV => Twin.NextF;

        public override bool Equals(object obj)
        {
            return obj is Arc<T> arc &&
                   EqualityComparer<T>.Default.Equals(Target, arc.Target) &&
                   EqualityComparer<T>.Default.Equals(Origin, arc.Origin);
        }

        public override int GetHashCode()
        {
            int hashCode = 635979287;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Target);
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Origin);
            return hashCode;
        }
    }

    public static class ArcExtensions
    {
        public static IEnumerable<Arc<T>> ToNextF<T>(this Arc<T> start)
        {
            Arc<T> arc = start;
            do
            {
                yield return arc;
            } 
            while (start != (arc = arc.NextF) && (null != arc));
        }

        public static IEnumerable<Arc<T>> FromLoop<T>(this IEnumerable<T> loopT)
        {
            List<Arc<T>> primaryLoop = new List<Arc<T>>();
            var loop = loopT.ToArray();
            for (int k = 0; k < loop.Length; ++k)
            {
                var arc = new Arc<T>(loop[(k + 1) % loop.Length]);
                arc.Twin = new Arc<T>(loop[k]) { Twin = arc };
                primaryLoop.Add(arc);
            }

            for (int k = 0; k < loop.Length; ++k)
            {
                primaryLoop[k].NextF = primaryLoop[(k + 1) % loop.Length];
            }

            return primaryLoop.ToArray();
        }
    }
}
