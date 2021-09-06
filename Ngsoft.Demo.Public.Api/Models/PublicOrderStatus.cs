using System;

namespace Ngsoft.Demo.Public.Api.Models
{
    public class PublicOrderStatus
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Quantity { get; set; }
    }
}
