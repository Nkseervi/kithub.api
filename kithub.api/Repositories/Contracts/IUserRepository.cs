using kithub.api.Areas.Identity.Data;

namespace kithub.api.Repositories.Contracts
{
    public interface IUserRepository
    {
        Task<IEnumerable<KithubUser>> GetAllUsers();
    }
}