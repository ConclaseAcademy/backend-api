namespace MentalHealthCompanion.Data.Options
{
    public class JwtOptions
    {
        public const string Section = "JwtOptions";
        public string SigningKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
    }
}
