using System.Threading;
using System.Threading.Tasks;
using Kebapi.Dto;
using Kebapi.DataAccess;
using Kebapi.Services.Token;
using Kebapi.Services.Hashing;

namespace Kebapi.Services.Authentication
{
    /// <summary>
    /// Authentication service. Or... do I know you.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IDal _dal;
        private readonly TokenService _tokenService;
        private readonly IHashingService _hashingService;

        public AuthenticationService(IDal dal, TokenService ts, IHashingService hs)
        {
            _dal = dal;
            _tokenService = ts;
            _hashingService = hs;
        }

        /// <summary>
        /// Tries to authenticate a user given their username or email, and their
        /// password.
        /// </summary>
        /// <param name="usernameOrEmail"></param>
        /// <param name="password"></param>
        /// <param name="ct"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiSecurityToken"/> containing a token if the user can 
        /// be authenticated, null otherwise.
        /// </returns>
        public async Task<Dto.ApiSecurityToken> AuthenticateUser(string usernameOrEmail, string password, CancellationToken ct)
        {

            Dto.DalUser user = null;

            // Attempt to find the user in the db with the supplied loginUsernameOrEmail. 
            // Use a simple hint to optimise order of search. (We will always search
            // the second field if not found in the first.)
            if (usernameOrEmail.Contains("@"))
            {
                user = await _dal.GetUserByEmail(usernameOrEmail, ct) ??
                    await _dal.GetUserByUsername(usernameOrEmail, ct);
            }
            else
            {
                user = await _dal.GetUserByUsername(usernameOrEmail, ct) ??
                    await _dal.GetUserByEmail(usernameOrEmail, ct);
            }

            Dto.ApiSecurityToken st = null;

            if (user != null)
                // User exists.
                if (_hashingService.IsValueInHashBundle(password, user.PasswordHash))
                    // User's password is correct.
                    st = _tokenService.CreateToken(user);

            return st;

        }

        /// <summary>
        /// Tries to authenticate a user given their username or email, and their
        /// password.
        /// </summary>
        /// <param name="usernameOrEmail"></param>
        /// <param name="password"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiSecurityToken"/> containing a token if the user can 
        /// be authenticated, null otherwise.
        /// </returns>
        public async Task<Dto.ApiSecurityToken> AuthenticateUser(string usernameOrEmail, string password)
        {
            return await AuthenticateUser(usernameOrEmail, password, CancellationToken.None);
        }
    }
}
