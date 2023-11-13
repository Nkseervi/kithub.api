using kithub.api.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace kithub.api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<KithubUser> _userManager;
		private readonly KithubDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(UserManager<KithubUser> userManager,
                        KithubDbContext context,
                        RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
			_context = context;
            _roleManager = roleManager;
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
        public async Task<IEnumerable<IdentityRole>> GetAllRoles()
        {
            return await _roleManager.Roles.ToListAsync();
        }
        public async Task<KithubUser> FindExistingUser(string emailAddress)
        {
            return await _userManager.FindByEmailAsync(emailAddress); 
        }
        public async Task<bool> IfRoleExists(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }
        public async Task<IdentityResult> CreateNewUser(KithubUser newUser, string password)
        {
            var result = await _userManager.CreateAsync(newUser, password);

            if(result.Succeeded)
            {
                await getCart(new Cart { UserId = newUser.Id });
            }

            return result;
        }
        public async Task<IdentityResult> CreateNewRole(string roleName)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole { Name = roleName});

            return result;
        }
        public async Task<IdentityResult> AddRole(string roleName, KithubUser user)
        {
            return await _userManager.AddToRoleAsync(user, roleName);
        }
        public async Task<Cart> getCart(Cart newCart)
		{
            if (await _context.Carts.AnyAsync(c => c.UserId == newCart.UserId))
            {
                return await _context.Carts
                                        .Where(c => c.UserId == newCart.UserId)
                                        .FirstAsync();
            }
            else
            {
                var result = await _context.Carts.AddAsync(newCart);
                await _context.SaveChangesAsync();
                return result.Entity;
            }
		}

	}
}
