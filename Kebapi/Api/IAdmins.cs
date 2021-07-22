using System.Threading.Tasks;

namespace Kebapi.Api
{
    /// <summary>
    /// Interface for API functionality relating to system admins.
    /// </summary>
    public interface IAdmins
    {
        Task CreateDb();
        Task DropDb();
        Task ResetDb();
        Task ResetTestDb();
    }
}