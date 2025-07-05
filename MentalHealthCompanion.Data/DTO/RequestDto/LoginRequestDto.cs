namespace MentalHealthCompanion.Data.DTO.RequestDto
{
    public class LoginRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
