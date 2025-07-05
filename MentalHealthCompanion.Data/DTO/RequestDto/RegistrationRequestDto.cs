using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalHealthCompanion.Data.DTO.RequestDto
{
    public class RegistrationRequestDto
    {
        public string CallBackUrl { get; set; }
        public string UserRole { get; set; }
        public string RegistrationCode { get; set; }

        public RegistrationRequestDto()
        {

        }

        public RegistrationRequestDto(string code, string userRole)
        {
            RegistrationCode = code;
            UserRole = userRole;
        }

        public RegistrationRequestDto(string userRole)
        {
            UserRole = userRole;
        }

        public RegistrationRequestDto(string url, string registrationCode, string userRole)
        {
            CallBackUrl = url;
            RegistrationCode = registrationCode;
            UserRole = userRole;
        }
    }
}
