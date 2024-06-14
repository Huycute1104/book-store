using Repository.Models;
using System.Diagnostics.Metrics;

namespace BookStore.Service
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}
