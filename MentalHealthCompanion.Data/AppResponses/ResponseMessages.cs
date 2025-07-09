namespace MentalHealthCompanion.Data.AppResponses
{
    public class ResponseMessages
    {
        public const string Success = "Successful";
        public const string UserExist = "Email address already used.";
        public const string UserRegistrationFailed = "User registration failed";
        public const string UserRegistrationSuccessful = "User registration successful";
        public const string UserNotFound = "User does not exist.";
        public const string ProfileUpdateFailed = "Cannot complete registration at the momemt. Please try again later.";
        public const string InvalidRegistrationCode = "Invalid registration code. Please contactbthe admin.";
        public const string TokenError = "An error generating token.";
        public const string Exception = "Something went wrong, please try again later.";
        public const string InvalidPassword = "Invalid credentials.";
        public const string UpdateException = "user with email already exists";
    }
}
