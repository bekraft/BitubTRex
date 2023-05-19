using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bitub.Dto.Rest
{
    public static class Extensions
    {
        #region Description wrapper

        /// <summary>
        /// Wraps a property name and a value given an array of values and an index. If index is out of
        /// range, it will always return a tuple of property name and <c>null</c>.
        /// </summary>
        /// <param name="values">The values</param>
        /// <param name="propertyName">The property name</param>
        /// <param name="k">The index</param>
        /// <returns>A non-null tuple</returns>
        public static (string, object) WrapPropertySafely(this object[] values, string propertyName, int k)
        {
            if (0 > k || null == values || k >= values.Length)
                return (propertyName, null);
            else
                return (propertyName, values[k]);
        }

        public static object GetValueSafely(this object[] values, int k)
        {
            if (0 > k || null == values || k >= values.Length)
                return null;
            else
                return values[k];
        }

        /// <summary>
        /// Will fold a collection of results into an array of DTOs failing on the first folded failiure.
        /// </summary>
        /// <typeparam name="E">Type of collection</typeparam>
        /// <param name="results">The unfolded results</param>
        /// <returns>The folded results or a failure.</returns>
        public static DtoResult<E[]> FailOnFirstFailure<E>(this IEnumerable<DtoResult<E>> results)
        {
            var dtos = new List<E>();
            foreach (var r in results)
            {
                if (r.IsSuccess)
                    dtos.Add(r.dto);
                else
                    return new DtoResult<E[]>(r.responseCode, r.responsePhrase);
            }
            return new DtoResult<E[]>(dtos.ToArray());
        }

        /// <summary>
        /// Will filter successes of results only.
        /// </summary>
        /// <typeparam name="E">Type of collection</typeparam>
        /// <param name="results">The unfolded results</param>
        /// <returns>The folded results or a failure.</returns>
        public static DtoResult<E[]> FilterSuccesses<E>(this IEnumerable<DtoResult<E>> results)
        {
            return new DtoResult<E[]>(results.Where(r => r.IsSuccess).Select(r => r.dto).ToArray());
        }

        /// <summary>
        /// Unwraps a wrapped DTO result propagating internal failures.
        /// </summary>
        /// <typeparam name="E">The type of DTO</typeparam>
        /// <param name="wrapped">The wrapped result</param>
        /// <returns>An unwrapped result</returns>
        public static DtoResult<E> Unwrap<E>(this DtoResult<DtoResult<E>> wrapped)
        {
            if (wrapped.IsSuccess)
                return wrapped.dto;
            else
                return new DtoResult<E>(wrapped.responseCode, wrapped.responsePhrase);
        }

        #endregion

        #region Service extensions

        /// <summary>
        /// Runs many task in a synchronized sequence.
        /// </summary>
        /// <typeparam name="T">The task generators data type</typeparam>
        /// <typeparam name="R">The task result type</typeparam>
        /// <param name="data">The generator data</param>
        /// <param name="taskGenerator">The task generator</param>
        /// <returns>The task results</returns>
        public static async Task<R[]> RunMany<T, R>(this IEnumerable<T> data, Func<T, Task<R>> taskGenerator)
        {
            var results = new List<R>();
            foreach (var dto in data)
                results.Add(await taskGenerator(dto));

            return await Task.FromResult(results.ToArray());
        }

        /// <summary>
        /// Runs many tasks as asynchronous groups of synchronized sequences.
        /// </summary>
        /// <typeparam name="T">The task generators data type</typeparam>
        /// <typeparam name="R">The task result type</typeparam>
        /// <param name="data">The generator data</param>
        /// <param name="taskGenerator">The task generator</param>
        /// <param name="generatorBins">The count of asnychronous bins.</param>
        /// <returns>The task results</returns>
        public static async Task<R[]> RunManyAsync<T, R>(this IEnumerable<T> data, Func<T, Task<R>> taskGenerator, uint generatorBins = 1)
        {
            if (1 > generatorBins)
                throw new ArgumentException("Expecting bins > 0");

            var manyInParallel = data
                .Select((dto, idx) => (dto, bin: idx % generatorBins)) // binning the whole package
                .GroupBy(t => t.bin, t => t.dto)
                .Select(g => RunMany(g, taskGenerator)); // Run a sequential put on each bin

            // Flatten once finished
            return await Task.WhenAll(manyInParallel).ContinueWith(t => t.Result.SelectMany(package => package).ToArray());
        }

        #endregion
    }
}
