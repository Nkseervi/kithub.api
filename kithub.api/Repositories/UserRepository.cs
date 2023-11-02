using kithub.api.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace kithub.api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<KithubUser> _userManager;

        public UserRepository(UserManager<KithubUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IEnumerable<KithubUser>> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            return users;

        }
        public async Task<KithubUser> FindExistingUser(string emailAddress)
        {
            return await _userManager.FindByEmailAsync(emailAddress); 
        }
        public async Task<IdentityResult> CreateNewUser(KithubUser newUser, string password)
        {
            return await _userManager.CreateAsync(newUser, password);
        }
    }
}
