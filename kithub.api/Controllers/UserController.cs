using kithub.api.Areas.Identity.Data;
using kithub.api.Extensions;
using kithub.api.models.Dtos;
using kithub.api.Repositories;
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

		[HttpGet]
		public async Task<LoggedInUserDto> GetById()
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
			KithubUser user = await _userRepository.GetUserById(userId);
            Cart cart = await _userRepository.getCart(new Cart() { UserId = userId });
			return user.ConvertToDto(cart);
		}

        #region Register New User
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
            if (ModelState.IsValid)
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
                    if (result.Succeeded)
                    {
                        return Ok();
                    }
                }
            }
            return BadRequest();
        } 
        #endregion

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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Admin/GetAllRoles")]
        public async Task<IEnumerable<string?>> GetAllRoles()
        {            
            var roles = await _userRepository.GetAllRoles();
            return roles.Select(r => r.Name).ToList();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Admin/CreateNewRole")]
        public async Task<IActionResult> CreateNewRole(string roleName)
        {
            if (ModelState.IsValid)
            {
                if (await _userRepository.IfRoleExists(roleName) == false)
                {
                    IdentityResult result = await _userRepository.CreateNewRole(roleName);
                    if (result.Succeeded)
                    {
                        return Ok();
                    }
                }
            }
            return BadRequest();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Admin/AddRole")]
        public async Task<IActionResult> AddARole(UserRolePairDto pairing)
        {
            string? loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (loggedInUserId is not null)
            {
                var user = await _userRepository.FindExistingUser(pairing.UserName);

                //_logger.LogInformation("Admin {Admin} added user {User} to role {Role}",
                //    loggedInUserId, user.Id, pairing.RoleName);

                var result = await _userRepository.AddRole(pairing.RoleName, user);
                if (result.Succeeded)
                {
                    return Ok(result);
                }
        }
            return BadRequest();
        }
    }
}
