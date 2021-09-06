using System.Collections.Generic;

namespace Ngsoft.Demo.Public.Api.Responses
{
    internal class GetResultResponse<T>
    {
        public List<T> RequestedData { get; set; }
    }
}
