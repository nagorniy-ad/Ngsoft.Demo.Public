namespace Ngsoft.Demo.Public.Api.Models.Contracts
{
    public class Contract
    {
        public int PublicId { get; set; }
        public PublicReportType ReportId { get; set; }
        public int? FilterId { get; set; }
        public string FilterName { get; set; }
        public string RegNum { get; set; }
        public EisContractType? Type { get; set; }
        public string Href { get; set; }
        public string Status { get; set; }
    }
}
