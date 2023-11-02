using kithub.api.Areas.Identity.Data;
using kithub.api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace kithub.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public record UserRegistrationDto(
            string FirstName,
            string LastName,
            string EmailAddress,
            string Password);

        [HttpPost]
        [Route("/Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegistrationDto userDto)
        {
            if(ModelState.IsValid)
            {
                var existingUser = await _userRepository.FindExistingUser(userDto.EmailAddress);
                if (existingUser is null)
                {
                    KithubUser newUser = new()
                    {
                        Email = userDto.EmailAddress,
                        EmailConfirmed = true,
                        UserName = userDto.EmailAddress,
                        FirstName = userDto.FirstName,
                        LastName = userDto.LastName,
                        EmailAddress = userDto.EmailAddress,
                        CreateDate = DateTime.UtcNow
                    };

                    IdentityResult result = await _userRepository.CreateNewUser(newUser, userDto.Password);
                    if(result.Succeeded)
                    {
                        return Ok();
                    }
                }
            }
            return BadRequest();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Admin/GetAllUsers")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userRepository.GetAllUsers();
                var roles = new List<string>();


                if (users == null)
                {
                    return NotFound();
                }
                else
                {
                    
                    var userDtos = users.ConvertToDto(roles);

                    return Ok(userDtos);
                }

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                "Error retrieving data from the database");

            }
        }
    }
}
