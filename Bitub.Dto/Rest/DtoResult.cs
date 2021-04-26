using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;

using System.Text.Json;
using System.Collections;

namespace Bitub.Dto.Rest
{
    public enum DtoResultType    
    {
        Success,
        Failure,
        Exception
    }

    public sealed class DtoResult<T> : IEnumerable<T>, IEquatable<DtoResult<T>>
    {
        public readonly T Dto;
        public readonly HttpStatusCode ResponseCode;
        public readonly string ResponsePhrase;

        internal class Enumerator : IEnumerator<T>
        {
            private bool isInitiated;
            private T dto;

            public Enumerator(DtoResult<T> r)
            {
                dto = r.Dto;
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
            Dto = dto;
            ResponseCode = code;
            ResponsePhrase = responsePhrase;
        }

        public DtoResult(HttpStatusCode code, string responsePhrase = null) 
            : this(default(T), code, responsePhrase)
        {
        }

        public static async Task<DtoResult<E>> FromResponse<E>(HttpResponseMessage response, CancellationToken cancellation)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await response.Content
                        .ReadAsStringAsync()
                        .ContinueWith(json => new DtoResult<E>(JsonSerializer.Deserialize<E>(json.Result)), cancellation);
                default:
                    return await Task.FromResult(new DtoResult<E>(response.StatusCode, response.ReasonPhrase));
            }
        }

        public static Task<DtoResult<E>> FromResponse<E>(Task<HttpResponseMessage> response, CancellationToken cancellation)
        {
            return response.ContinueWith<DtoResult<E>>( (t) =>
            {
                var serializeTask = FromResponse<E>(t.Result, cancellation);
                serializeTask.Wait(cancellation);
                return serializeTask.Result;
            }, cancellation);
        }

        public bool IsSuccess { get => ResponseCode == HttpStatusCode.OK; }

        public DtoResult<R> Then<R>(Func<T,R> f)
        {
            if (null != Dto)
                return new DtoResult<R>(f(Dto));
            else
                return new DtoResult<R>(ResponseCode);
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
            return IsSuccess ? $"Dto{{{Dto?.ToString()}}}" : $"Failure ({ResponseCode}) '{ResponsePhrase}'";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DtoResult<T>);
        }

        public bool Equals(DtoResult<T> other)
        {
            return other != null &&
                   EqualityComparer<T>.Default.Equals(Dto, other.Dto) &&
                   ResponseCode == other.ResponseCode &&
                   ResponsePhrase == other.ResponsePhrase;
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable) new object[] { Dto, ResponseCode, ResponsePhrase }).GetHashCode();
        }
    }
}
