namespace MentalHealthCompanion.Data.DTO.RequestDto
{
    public class UserRegistrationRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string EmailAddress { get; set; }
        public string Password { get; set; }
    }
}
