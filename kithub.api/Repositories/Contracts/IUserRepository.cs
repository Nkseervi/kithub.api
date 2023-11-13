using kithub.api.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace kithub.api.Repositories.Contracts
{
    public interface IUserRepository
    {
        Task<IdentityResult> AddRole(string roleName, KithubUser user);
        Task<IdentityResult> CreateNewRole(string roleName);
        Task<IdentityResult> CreateNewUser(KithubUser newUser, string password);
        Task<KithubUser> FindExistingUser(string emailAddress);
        Task<IEnumerable<IdentityRole>> GetAllRoles();
        Task<IEnumerable<KithubUser>> GetAllUsers();
        Task<Cart> getCart(Cart newCart);
        Task<KithubUser> GetUserById(string id);
        Task<bool> IfRoleExists(string roleName);
    }
}