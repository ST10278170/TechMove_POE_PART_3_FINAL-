using System.ComponentModel.DataAnnotations;

namespace TechMove.GLMS.API.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ContactPerson { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Region { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public string CompanyRegistrationNumber { get; set; } = string.Empty;

        public string VATNumber { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}
