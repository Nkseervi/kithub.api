using kithub.api.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace kithub.api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<KithubUser> _userManager;
		private readonly KithubDbContext _context;

		public UserRepository(UserManager<KithubUser> userManager, KithubDbContext context)
        {
            _userManager = userManager;
			_context = context;
		}
		public async Task<KithubUser> GetUserById(string id)
		{
			var user = await _userManager.FindByIdAsync(id);

			return user;

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
            var result = await _userManager.CreateAsync(newUser, password);

            if(result.Succeeded)
            {
                var cart = createNewCart(new Cart { UserId = newUser.Id });
            }

            return result;
        }
		private async Task<Cart> createNewCart(Cart newCart)
		{
			var result = await _context.Carts.AddAsync(newCart);
            await _context.SaveChangesAsync();
            return result.Entity;
		}

	}
}
