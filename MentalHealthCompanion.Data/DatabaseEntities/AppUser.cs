namespace MentalHealthCompanion.Data.DatabaseEntities
{
    public class AppUser : BaseEntity
    {
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public bool IsAccountActivated { get; set; } = false;
        public string EmailAddress { get; set; } = string.Empty;

        public string? Password {  get; set; }
    }
}
