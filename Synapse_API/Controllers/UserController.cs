using Microsoft.AspNetCore.Mvc;
using Synapse_API.Models.Dto.UserDTOs;
using Synapse_API.Services;
using Synapse_API.Utils;
using System.Security.Claims;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUserDtosAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var userDto = await _userService.GetUserDtoAsync(id);
            if (userDto == null) return NotFound();
            return Ok(userDto);
        }

        [HttpGet("get-profile")]
        public async Task<IActionResult> GetProfile()
        {
            
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(AppConstants.ErrorMessages.General.Unauthorized);

            int userId = int.Parse(userIdStr);

            var profileDto = await _userService.GetUserProfileAsync(userId);

            if (profileDto == null)
                return NotFound(AppConstants.ErrorMessages.User.UserNotFound);

            return Ok(profileDto);
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(AppConstants.ErrorMessages.User.UserIdMissing);

            int userId = int.Parse(userIdStr);

            var success = await _userService.UpdateUserProfileAsync(userId, dto);
            if (!success)
                return NotFound(AppConstants.ErrorMessages.User.UserNotFound);

            return Ok(AppConstants.SuccessMessages.User.ProfileUpdated);
        }


        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(AppConstants.ErrorMessages.User.UserIdMissing);

            int userId = int.Parse(userIdStr);

            var success = await _userService.ChangePasswordAsync(userId, dto);
            if(success == true)
            {
                return Ok(AppConstants.SuccessMessages.User.PasswordChanged);

            }
            return BadRequest(AppConstants.ErrorMessages.User.FailedToChangePassword);
        }


        [HttpGet("UserProfile")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(AppConstants.ErrorMessages.User.UserIdMissing);

            int userId = int.Parse(userIdStr);

            var userDto = await _userService.GetUserProfileAsync(userId);
            if (userDto == null)
                return NotFound(AppConstants.ErrorMessages.User.UserNotFound);

            return Ok(userDto);
        }



    }
}