using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bitub.Dto.Rest
{
    public abstract class DefaultServiceEndpointImpl<T> : IServiceEndpoint where T : IDtoEntity
    {
        public readonly ApiClient apiClient;

        public uint DefaultConnectionBins { get; set; } = 10;

        protected DefaultServiceEndpointImpl(ApiClient api)
        {
            this.apiClient = api;
        }

        public async Task<DtoResult<T[]>> GetAll()
        {
            return await apiClient.SendRequest<T[]>(this, HttpMethod.Get);
        }

        public abstract string ResourceUri { get; }

        public abstract bool IsRooted { get; }

        public async Task<DtoResult<T>> GetInstance(object id)
        {
            return await apiClient.SendRequest<T>(this, HttpMethod.Get, id.ToString());
        }

        public async Task<DtoResult<T[]>> GetAllByInstance() => await GetAllByInstance(DefaultConnectionBins);

        public async Task<DtoResult<T[]>> GetAllByInstance(uint connectionBins)
        {
            var shallowAttributes = await GetAll();
            var detailedAttributes = await shallowAttributes.ThenAsync(
                async r => Extensions.FailOnFirstFailure(await r.RunManyAsync(instance => GetInstance(instance.Id), connectionBins)));
            var dtoDscp = new DtoDescriptor<T>();
            return detailedAttributes.Unwrap().Then(
                details => dtoDscp.Aggregate(shallowAttributes.dto, details, DtoAggregateMethodFlag.NonIdAndDefinedOnly).ToArray());
        }

        public async Task<DtoResult<T[]>> GetAllByInstance(IEnumerable<Guid> ids) => await GetAllByInstance(ids, DefaultConnectionBins);

        public async Task<DtoResult<T[]>> GetAllByInstance(IEnumerable<Guid> ids, uint connectionBins)
        {
            return Extensions.FailOnFirstFailure(await ids.RunManyAsync(id => GetInstance(id), connectionBins));
        }

        public async Task<DtoResult<T>> Put(T dto)
        {
            return await apiClient.SendRequest<T>(this, HttpMethod.Put, dto, dto.Id.ToString());
        }

        public async Task<DtoResult<T>[]> PutMany(IEnumerable<T> dtos)
        {
            return await dtos.RunMany(dto => Put(dto));
        }

        public async Task<DtoResult<T>[]> PutManyAsync(IEnumerable<T> dtos) => await PutManyAsync(dtos, DefaultConnectionBins);

        public async Task<DtoResult<T>[]> PutManyAsync(IEnumerable<T> dtos, uint connectionBins)
        {
            return await dtos.RunManyAsync(dto => Put(dto), connectionBins);
        }

        public async Task<DtoResult<T>> Create(T dto)
        {
            return await apiClient.SendRequest<T>(this, HttpMethod.Post, dto);
        }

    }
}