using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;

using System.Collections;

namespace Bitub.Dto.Rest
{
    /// <summary>
    /// Data transfer object wrapping a result, a response HTTP code and response phrase.
    /// </summary>
    /// <typeparam name="T">The inner type of DTO</typeparam>
    public sealed class DtoResult<T> : IEnumerable<T>, IEquatable<DtoResult<T>>
    {
        public readonly T dto;
        public readonly HttpStatusCode responseCode;
        public readonly string responsePhrase;

        internal class Enumerator : IEnumerator<T>
        {
            private bool isInitiated;
            private T dto;

            public Enumerator(DtoResult<T> r)
            {
                this.dto = r.dto;
            }

            public T Current { get => isInitiated ? dto : default(T); }

            object IEnumerator.Current { get => Current; }

            public void Dispose()
            {
                dto = default(T);
            }

            public bool MoveNext()
            {
                bool wasInitiated = isInitiated;
                isInitiated = true;
                return !wasInitiated && null != dto;
            }

            public void Reset()
            {
                isInitiated = false;
            }
        }

        public DtoResult(T dto, HttpStatusCode code = HttpStatusCode.OK, string responsePhrase = null)
        {
            this.dto = dto;
            responseCode = code;
            this.responsePhrase = responsePhrase;
        }

        public DtoResult(HttpStatusCode code, string responsePhrase = null) 
            : this(default(T), code, responsePhrase)
        {
        }

        public static async Task<DtoResult<E>> FromResponse<E>(HttpResponseMessage response,
            ApiContext applicationContext, CancellationToken cancellation)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                case HttpStatusCode.NonAuthoritativeInformation:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.ResetContent:
                case HttpStatusCode.PartialContent:
                    return await response.Content
                        .ReadAsStringAsync()
                        .ContinueWith(json => new DtoResult<E>(applicationContext.FromJson<E>(json.Result)), cancellation);
                default:
                    return await Task.FromResult(new DtoResult<E>(response.StatusCode, response.ReasonPhrase));
            }
        }

        public static Task<DtoResult<E>> FromResponse<E>(Task<HttpResponseMessage> response,
            ApiContext applicationContext, CancellationToken cancellation)
        {
            return response.ContinueWith<DtoResult<E>>( (t) =>
            {
                var serializeTask = FromResponse<E>(t.Result, applicationContext, cancellation);
                serializeTask.Wait(cancellation);
                return serializeTask.Result;
            }, cancellation);
        }

        /// <summary>
        /// Any response code of HTTP 200 group.
        /// </summary>
        public bool IsSuccess
        {
            get => (200 <= (int)responseCode) && (300 > (int)responseCode);
        }

        public DtoResult<R> Then<R>(Func<T,R> f)
        {
            if (null != dto)
                return new DtoResult<R>(f(dto));
            else
                return new DtoResult<R>(responseCode);
        }

        public async Task<DtoResult<R>> ThenAsync<R>(Func<T, Task<R>> f)
        {
            if (null != dto)
                return new DtoResult<R>(await f(dto));
            else
                return new DtoResult<R>(responseCode, responsePhrase);
        }

        /// <summary>
        /// Every result code of 20X will return a DTO otherwise an exception is thrown.
        /// </summary>
        public T DtoOrFail
        {
            get
            {
                switch (responseCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.Created:
                    case HttpStatusCode.Accepted:
                    case HttpStatusCode.NonAuthoritativeInformation:
                    case HttpStatusCode.NoContent:
                    case HttpStatusCode.ResetContent:
                    case HttpStatusCode.PartialContent:
                        return dto;
                    default:
                        throw new Exception($"Code {responseCode}: {responsePhrase}");
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public override string ToString()
        {
            return IsSuccess ? $"Dto{{{dto?.ToString()}}}" : $"Failure ({responseCode}) '{responsePhrase}'";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DtoResult<T>);
        }

        public bool Equals(DtoResult<T> other)
        {
            return other != null &&
                   EqualityComparer<T>.Default.Equals(dto, other.dto) &&
                   responseCode == other.responseCode &&
                   responsePhrase == other.responsePhrase;
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable) new object[] { dto, responseCode, responsePhrase }).GetHashCode();
        }
    }
}
