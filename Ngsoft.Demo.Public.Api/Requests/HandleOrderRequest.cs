using Newtonsoft.Json;
using System;

namespace Ngsoft.Demo.Public.Api.Requests
{
    internal class HandleOrderRequest
    {
        public int FilterId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PublicOrderSubtype? Subtype { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}
