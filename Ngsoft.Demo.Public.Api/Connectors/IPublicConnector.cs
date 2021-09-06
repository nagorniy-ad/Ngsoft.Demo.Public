using Ngsoft.Demo.Public.Api.Models;
using Ngsoft.Demo.Public.Api.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ngsoft.Demo.Public.Api.Connectors
{
    public interface IPublicConnector
    {
        Task<IEnumerable<PublicFilter>> GetFiltersAsync();
        Task<Guid> CreatePurchasesOrderAsync(int filterId, DateTime dateFrom, DateTime dateTo, PublicOrderSubtype? subtype = null);
        Task<Guid> CreateContractsOrderAsync(int filterId, DateTime dateFrom, DateTime dateTo, PublicOrderSubtype? subtype = null);
        Task<Guid> UpdatePurchasesOrderAsync(int filterId, DateTime dateFrom, DateTime dateTo, PublicOrderSubtype? subtype = null);
        Task<Guid> UpdateContractsOrderAsync(int filterId, DateTime dateFrom, DateTime dateTo, PublicOrderSubtype? subtype = null);
        Task<PublicOrderStatus> GetPurchasesOrderStatusAsync(Guid taskId);
        Task<PublicOrderStatus> GetContractsOrderStatusAsync(Guid taskId);
        Task<IEnumerable<Purchase>> GetPurchasesResultAsync(Guid taskId, int pageIndex);
        Task<IEnumerable<Contract>> GetContractsResultAsync(Guid taskId, int pageIndex);
    }
}
