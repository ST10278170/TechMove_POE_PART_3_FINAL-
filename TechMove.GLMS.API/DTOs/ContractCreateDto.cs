namespace TechMove.GLMS.API.DTOs
{
    public class ContractCreateDto
    {
        public string ClientName { get; set; } = string.Empty;
        public string ServiceLevel { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ClientId { get; set; }
    }
}
