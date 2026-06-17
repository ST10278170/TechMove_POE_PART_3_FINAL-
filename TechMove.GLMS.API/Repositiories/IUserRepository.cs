using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
