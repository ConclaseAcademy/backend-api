namespace MentalHealthCompanion.Data.DTO.RequestDto
{
    public class EmailRequestDto
    {
        public string RecipientEmail { get; set; }
        public string SenderEmail { get; set; }
        public List<string>? Cc { get; set; }
        public List<string>? Bc { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}
