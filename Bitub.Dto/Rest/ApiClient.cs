using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bitub.Dto.Rest
{
    public abstract class ApiClient : IDisposable
    {
        #region Internals
        protected readonly HttpClient httpClient;
        protected MediaTypeWithQualityHeaderValue defaultRequestContentType = new MediaTypeWithQualityHeaderValue("application/json");
        #endregion

        public readonly ApiContext apiContext;

        protected ApiClient(ApiContext apiContext, string baseUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri($"{baseUrl}");
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.apiContext = apiContext;
        }

        public Uri BaseUrl
        {
            get => httpClient.BaseAddress;
        }

        public bool Cancel()
        {
            throw new NotImplementedException();
        }

        public TimeSpan TimeOut
        {
            get => httpClient.Timeout;
            set => httpClient.Timeout = value;
        }

        public abstract string GetResourceUri(IServiceEndpoint endpoint, string scope, string query);

        public async Task<DtoResult<T>> SendRequest<T>(IServiceEndpoint endpoint, HttpMethod httpMethod,
             string subScope = null, string query = null)
        {
            return await SendRequest<T>(endpoint, httpMethod, null, subScope, query);
        }

        public async Task<DtoResult<T>> SendRequest<T>(IServiceEndpoint endpoint, HttpMethod httpMethod,
            object request, string subScope = null, string query = null)
        {
            using (var httpRequest = new HttpRequestMessage(httpMethod, GetResourceUri(endpoint, subScope, query)))
            {
                if (null != request)
                {
                    httpRequest.Content = new StringContent(apiContext.ToJson(request), System.Text.Encoding.UTF8);
                    httpRequest.Content.Headers.ContentType = defaultRequestContentType;
                }

                httpRequest.Headers.Authorization = apiContext.AuthSchemeAndToken;

                var response = await httpClient.SendAsync(httpRequest, CancellationToken.None);
                var result = await DtoResult<T>.FromResponse<T>(response, apiContext, CancellationToken.None);
                response.Dispose();
                return result;
            }
        }

        public bool? IsConnected
        {
            get => apiContext.AuthSchemeAndToken != null;
        }

        public void Dispose()
        {
            lock (httpClient)
            {
                //cancellationTokenSource.Cancel();
                httpClient.Dispose();
            }
        }

    }
}
