using Kebapi.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kebapi.Domain;

namespace Kebapi.DataAccess
{
    /// <summary>
    /// Dal (Data Access Layer) interface.
    /// </summary>
    public interface IDal
    {
        string ConnectionString { get; }
        string DbName { get; }

        Task<DalAffectedRows> ActivateUser(int id, CancellationToken cancellationToken);
        Task<DalResultWithAffectedId> AddUser(string username, string name, string surname, string email, string passwordHash, User.Role userRole, User.AccountStatus userAccountStatus, CancellationToken cancellationToken);
        Task<DalResultWithAffectedId> AddUserFavourite(int id, int venueId, CancellationToken cancellationToken);
        Task CreateKebApiDatabase(CancellationToken cancellationToken);
        Task<DalAffectedRows> DeactivateUser(int id, CancellationToken cancellationToken);
        Task DropKebApiDatabase(CancellationToken cancellationToken);
        Task<DalUser> GetUser(int id, CancellationToken cancellationToken);
        Task<DalUserAccountStatus> GetUserAccountStatus(int id, CancellationToken cancellationToken);
        Task<DalUser> GetUserByEmail(string email, CancellationToken cancellationToken);
        Task<DalUser> GetUserByUsername(string username, CancellationToken cancellationToken);
        Task<DalResultWithAffectedRows> GetUserCount(CancellationToken cancellationToken);
        Task<List<DalVenue>> GetUserFavourites(int id, int startRow, int rowCount, CancellationToken cancellationToken);
        Task<List<DalUser>> GetUsers(int startRow, int rowCount, CancellationToken cancellationToken);
        Task<DalVenue> GetVenue(int id, CancellationToken cancellationToken);
        Task<DalResultWithAffectedRows> GetVenueCount(CancellationToken cancellationToken);
        Task<List<DalVenue>> GetVenues(int startRow, int rowCount, CancellationToken cancellationToken);
        DalAffectedId MapToDalAffectedId(int? id);
        DalAffectedRows MapToDalAffectedRows(int count);
        DalResult MapToDalResult(string message, int code);
        Task<DalResultWithAffectedId> RemoveUserFavourite(int id, int venueId, CancellationToken cancellationToken);
        Task ResetKebApiDatabase(CancellationToken cancellationToken);
        Task ResetKebApiTestDatabase(CancellationToken cancellationToken);
    }
}