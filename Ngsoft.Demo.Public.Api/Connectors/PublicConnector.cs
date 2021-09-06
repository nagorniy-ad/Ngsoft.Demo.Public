using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ngsoft.Demo.Public.Api.Auth;
using Ngsoft.Demo.Public.Api.Exceptions;
using Ngsoft.Demo.Public.Api.Models;
using Ngsoft.Demo.Public.Api.Models.Contracts;
using Ngsoft.Demo.Public.Api.Requests;
using Ngsoft.Demo.Public.Api.Resolvers;
using Ngsoft.Demo.Public.Api.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ngsoft.Demo.Public.Api.Connectors
{
    public class PublicConnector : IPublicConnector
    {
        private const string CREATE_ORDER_ACTION_NAME = "create";
        private const string UPDATE_ORDER_ACTION_NAME = "update";
        private readonly HttpClient _httpClient;
        private readonly ITokenStorage _tokenStorage;
        private readonly string _userName;
        private readonly string _password;
        private readonly ILogger _logger;

        public PublicConnector(HttpClient httpClient, ITokenStorage tokenStorage, string userName, string password, ILogger<PublicConnector> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tokenStorage = tokenStorage ?? throw new ArgumentNullException(nameof(tokenStorage));
            _userName = userName ?? throw new ArgumentNullException(nameof(userName));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<PublicFilter>> GetFiltersAsync()
        {
            var response = await ExecuteRequestAsync<GetFiltersResponse>(token =>
            {
                var url = $"/user/filters?token={token}";
                return new HttpRequestMessage(HttpMethod.Post, url);
            });
            return response.Result.Status.Code switch
            {
                200 => response.Result.Content.Filters,
                404 => new List<PublicFilter>(),
                _ => throw new PublicException(response.Result.Status.Description, response.Response)
            };
        }

        #region Create Orders

        public Task<Guid> CreatePurchasesOrderAsync(int filterId, DateTime dateFrom, DateTime dateTo, PublicOrderSubtype? subtype = null) =>
            ExecuteHandleOrderRequestAsync(CREATE_ORDER_ACTION_NAME, PublicOrderType.PURCHASES, filterId, dateFrom, dateTo, subtype);

        public Task<Guid> CreateContractsOrderAsync(int filterId, DateTime dateFrom, DateTime dateTo, PublicOrderSubtype? subtype = null) =>
            ExecuteHandleOrderRequestAsync(CREATE_ORDER_ACTION_NAME, PublicOrderType.CONTRACTS, filterId, dateFrom, dateTo, subtype);

        #endregion

        #region Update Orders

        public Task<Guid> UpdatePurchasesOrderAsync(int filterId, DateTime dateFrom, DateTime dateTo, PublicOrderSubtype? subtype = null) =>
            ExecuteHandleOrderRequestAsync(UPDATE_ORDER_ACTION_NAME, PublicOrderType.PURCHASES, filterId, dateFrom, dateTo, subtype);

        public Task<Guid> UpdateContractsOrderAsync(int filterId, DateTime dateFrom, DateTime dateTo, PublicOrderSubtype? subtype = null) =>
            ExecuteHandleOrderRequestAsync(UPDATE_ORDER_ACTION_NAME, PublicOrderType.CONTRACTS, filterId, dateFrom, dateTo, subtype);

        #endregion

        public Task<PublicOrderStatus> GetPurchasesOrderStatusAsync(Guid taskId) =>
            ExecuteGetOrderStatusRequestAsync(PublicOrderType.PURCHASES, taskId);

        public Task<PublicOrderStatus> GetContractsOrderStatusAsync(Guid taskId) =>
            ExecuteGetOrderStatusRequestAsync(PublicOrderType.CONTRACTS, taskId);

        public Task<IEnumerable<Purchase>> GetPurchasesResultAsync(Guid taskId, int pageIndex) =>
            ExecuteGetResultRequestAsync<Purchase>(PublicOrderType.PURCHASES, taskId, pageIndex);

        public Task<IEnumerable<Contract>> GetContractsResultAsync(Guid taskId, int pageIndex) =>
            ExecuteGetResultRequestAsync<Contract>(PublicOrderType.CONTRACTS, taskId, pageIndex);

        #region Private Methods

        private async Task<Guid> ExecuteHandleOrderRequestAsync(string actionName, string type, int filterId, DateTime dateFrom, DateTime dateTo, PublicOrderSubtype? subtype = null)
        {
            string content = null;
            var response = await ExecuteRequestAsync<HandleOrderResponse>(token =>
            {
                var url = $"/{type}/{actionName}?token={token}";
                var payload = new HandleOrderRequest
                {
                    FilterId = filterId,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Subtype = subtype
                };
                content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings() { DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffffffzzz" });
                return new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                };
            });
            var json = JsonConvert.SerializeObject(response);
            _logger.LogDebug($"ORDER REQUEST TYPE {type}, CONTENT {content}, RESPONSE {json}.");
            return response.Result.Status.Code switch
            {
                200 => response.Result.Content.TaskId,
                _ => throw new PublicException(response.Result.Status.Description, response.Response)
            };
        }

        private async Task<PublicOrderStatus> ExecuteGetOrderStatusRequestAsync(string type, Guid taskId)
        {
            string content = null;
            var response = await ExecuteRequestAsync<PublicOrderStatus>(token =>
            {
                var url = $"/{type}/status?token={token}";
                var payload = new
                {
                    TaskId = taskId
                };
                content = JsonConvert.SerializeObject(payload);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                };
                return requestMessage;
            });
            var json = JsonConvert.SerializeObject(response);
            _logger.LogDebug($"ORDER STATUS REQUEST TYPE {type}, TASK_ID {taskId}, CONTENT {content}, RESPONSE {json}.");
            return response.Result.Status.Code switch
            {
                200 => response.Result.Content,
                _ => throw new PublicException(response.Result.Status.Description, response.Response)
            };
        }

        private async Task<IEnumerable<T>> ExecuteGetResultRequestAsync<T>(string type, Guid taskId, int pageIndex)
        {
            var settings = new JsonSerializerSettings { ContractResolver = new GetResultDataContractResolver(type) };
            var response = await ExecuteRequestAsync<GetResultResponse<T>>(token =>
            {
                var url = $"/{type}/result?token={token}";
                var payload = new
                {
                    TaskId = taskId,
                    PageIndex = pageIndex
                };
                var json = JsonConvert.SerializeObject(payload);
                return new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }, settings);
            return response.Result.Status.Code switch
            {
                200 => response.Result.Content.RequestedData,
                _ => throw new PublicException(response.Result.Status.Description, response.Response)
            };
        }

        private async Task<PublicResponse<T>> ExecuteRequestAsync<T>(Func<string, HttpRequestMessage> buildRequest, JsonSerializerSettings settings = null)
        {
            const int MAX_TRY_COUNT = 3;
            int tryCount = 0;
            while (true)
            {
                tryCount++;
                var token = _tokenStorage.Get();
                if (token == null)
                {
                    token = await AuthorizeAsync();
                    _tokenStorage.Save(token);
                }
                var request = buildRequest.Invoke(token);
                var response = await _httpClient.SendAsync(request);
                var data = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PublicResult<T>>(data, settings);
                if (result.Status.Code == 401)
                {
                    _tokenStorage.Delete();
                    if (tryCount <= MAX_TRY_COUNT)
                    {
                        continue;
                    }
                }
                return new PublicResponse<T> { Result = result, Response = response };
            }
        }

        private async Task<string> AuthorizeAsync()
        {
            var url = $"/user/login";
            var payload = new
            {
                UserName = _userName,
                Password = _password
            };
            var json = JsonConvert.SerializeObject(payload);
            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _httpClient.SendAsync(message);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PublicResult<AuthorizeResponse>>(data);
            return result.Status.Code switch
            {
                200 => result.Content.Token,
                _ => throw new PublicException(result.Status.Description, response)
            };
        }

        #endregion

        #region Nested Classes

        private class PublicResponse<T>
        {
            public PublicResult<T> Result { get; set; }
            public HttpResponseMessage Response { get; set; }
        }

        #endregion
    }
}
