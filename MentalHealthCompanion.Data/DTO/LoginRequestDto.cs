﻿namespace MentalHealthCompanion.Data.DTO
{
    public class LoginRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
