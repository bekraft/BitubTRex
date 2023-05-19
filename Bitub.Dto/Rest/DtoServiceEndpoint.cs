using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace Bitub.Dto.Rest
{
    abstract public class DtoServiceEndpoint<T,E> : IServiceEndpoint where E : ApiClient where T : IDtoEntity
    {
        #region Internals
        protected readonly E apiClient;
        #endregion

        protected DtoServiceEndpoint(E client)
        {
            apiClient = client;
        }

        public string ResourceURI { get; protected set; }

        public abstract bool IsRooted { get; }

        public abstract MediaTypeWithQualityHeaderValue ContentHeader { get; }

        public async Task<DtoResult<T>> Get(object id)
        {
            return await apiClient.SendRequest<T>(this, HttpMethod.Get, id.ToString());
        }

        public async Task<DtoResult<T[]>> Get()
        {
            return await apiClient.SendRequest<T[]>(this, HttpMethod.Get);
        }

        public async Task<DtoResult<T[]>> GetMany(uint parallelRequests = 1)
        {                        
            var shallowAttributes = await Get();
            var detailedAttributes = await shallowAttributes.ThenAsync(async r => 
                Extensions.FailOnFirstFailure(await r.RunManyAsync(instance => Get(instance.Id), parallelRequests)));
            var dtoDscp = new DtoDescriptor<T>();
            return detailedAttributes
                .Unwrap()
                .Then(details => 
                    dtoDscp.Aggregate(shallowAttributes.dto, details, DtoAggregateMethodFlag.NonIdAndDefinedOnly).ToArray());

        }

        public async Task<DtoResult<T>> Put(T dto)
        {
            return await apiClient.SendRequest<T>(this, HttpMethod.Put, dto, dto.Id.ToString());
        }

        public async Task<DtoResult<T>[]> PutMany(IEnumerable<T> dtos, uint requestsInParallel = 1)
        {
            return await dtos.RunManyAsync(dto => Put(dto), requestsInParallel);
        }

        public async Task<DtoResult<T>> Create(T dto)
        {
            return await apiClient.SendRequest<T>(this, HttpMethod.Post, dto);
        }

    }
}
