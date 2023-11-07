using kithub.api.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace kithub.api.Repositories.Contracts
{
    public interface IUserRepository
    {
        Task<IdentityResult> CreateNewUser(KithubUser newUser, string password);
        Task<KithubUser> FindExistingUser(string emailAddress);
        Task<IEnumerable<KithubUser>> GetAllUsers();
        Task<Cart> getCart(Cart newCart);
        Task<KithubUser> GetUserById(string id);
	}
}