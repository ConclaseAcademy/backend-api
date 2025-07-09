using System.ComponentModel.DataAnnotations;

namespace MentalHealthCompanion.Data.DTO
{
    public class ResetAdminPasswordDto
    {
        public required string SentPassword { get; set; }
        [MinLength(4)]
        public required string NewPassword { get; set; }

        [MinLength(4)]
        [Compare("NewPassword", ErrorMessage ="passwords do not match")]
        public required string ConfirmPassword { get; set; }
    }
}
