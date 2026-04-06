using System.Threading.Tasks;

namespace InventorySales.Application.Interfaces
{
    public interface IMailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}