using Asp.Versioning;
using MentalHealthCompanion.Data.DTO.RequestDto;
using MentalHealthCompanion.Data.Enums;
using MentalHealthCompanion.Data.Interface;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealthCompanion.API.Controllers.v1
{
    [ApiVersion("1")]
    public class AuthenticationController : BaseController
    {
        private readonly IAuthenService _authenticationService;
        private readonly ILogger<AuthenticationController> _logger;
        public AuthenticationController(IAuthenService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid request body");
                return BadRequest(GenerateValidationErrorResponse(ModelState));
            }
            var result = await _authenticationService.LoginAsync(loginRequestDto, token);
            return result.IsSuccessful ? Ok(result) : BadRequest(result);

        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Registration([FromQuery] UserRole role, [FromBody] UserRegistrationRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(GenerateValidationErrorResponse(ModelState));

            RegistrationRequestDto registrationRequest = new(role.ToString());

            var response = await _authenticationService.RegisterAsync(model, registrationRequest);
            return response.IsSuccessful ? Ok(response) : BadRequest(response);
        }
    }
}
