using System;

namespace Ngsoft.Demo.Public.Api.Models
{
    public class Purchase
    {
        public int PublicId { get; set; }
        public PublicReportType ReportId { get; set; }
        public int? FilterId { get; set; }
        public string FilterName { get; set; }
        public string NotificationNumber { get; set; }
        public string SourceContractType { get; set; }
        public string PurchaseLink { get; set; }
        public DateTime? PublishDate { get; set; }
    }
}
