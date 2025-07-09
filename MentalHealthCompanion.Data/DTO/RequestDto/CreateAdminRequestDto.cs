using System.ComponentModel.DataAnnotations;

namespace MentalHealthCompanion.Data.DTO
{
    public class CreateAdminRequestDto
    {
        [EmailAddress(ErrorMessage ="Invalid Email Address")]
        public required string Email { get; set; }
    }
}
