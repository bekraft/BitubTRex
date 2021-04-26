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
    public class DtoApiClient : IDisposable
    {
        #region Internals
        protected readonly HttpClient httpClient;
        protected CancellationTokenSource cancellationTokenSource;

        protected MediaTypeWithQualityHeaderValue defaultRequestContentType = new MediaTypeWithQualityHeaderValue("application/json");
        #endregion

        public DtoApiClient()
        {
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

        /*
        public async Task<DtoResult<T>> SendRequest<T>(DtoServiceEndpoint<T> endpoint, HttpMethod httpMethod, string subScope = null)
        {
            return await SendRequest<T>(endpoint, httpMethod, null, subScope);
        }

        public async Task<DtoResult<T>> SendRequest<T>(DtoServiceEndpoint<T> endpoint, HttpMethod httpMethod, object request, string subScope = null)
        {
            using (var httpRequest = new HttpRequestMessage(httpMethod, GetResourceUri(endpoint, subScope)))
            {
                if (null != request)
                    httpRequest.Content = new StringContent(ApplicationContext.ToJson(request), System.Text.Encoding.UTF8);

                httpRequest.Content.Headers.ContentType = _defaultRequestContentType;
                lock (ApplicationContext)
                    httpRequest.Headers.Authorization = ApplicationContext.AuthSchemeAndToken;

                var response = await httpClient.SendAsync(httpRequest, CancellationToken).ConfigureAwait(false);
                var result = await DtoResult<T>.FromResponse<T>(response, CancellationToken);
                response.Dispose();
                return result;
            }
        }

        public async Task<DtoResult<AuthenticationHeaderValue>> LoginSingle(string userId, string password)
        {
            return await Authorize.RequestSingleGrant(userId, password).ContinueWith(r => r.Result.Then(g =>
            {
                lock (ApplicationContext)
                    ApplicationContext.AuthSchemeAndToken = g;
                return g;
            }));
        }

        */
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
