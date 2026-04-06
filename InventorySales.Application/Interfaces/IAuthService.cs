using InventorySales.Application.DTOs.Auth;
using InventorySales.Application.DTOs.Common;
using System.Threading.Tasks;

namespace InventorySales.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result> RegisterAsync(string email, string password);
        Task<Result<string>> LoginAsync(LoginDto request);
        Task<Result> ConfirmEmailAsync(string userId, string token); 
    }
}