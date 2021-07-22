using System;
using System.Threading;
using Kebapi.DataAccess;
using Kebapi.Services.Hashing;
using Kebapi.Services.Authentication;
using Kebapi.Dto;

namespace Kebapi.Test
{
    /// <summary>
    /// A mock authentication wrapper exposing several mocked users, along with 
    /// convenience methods to add mock users to and authenticate them from a 
    /// data store.
    /// </summary>
    public class MockAuthentication
    {
        private readonly IDal _dal;
        private readonly IHashingService _hs;
        private readonly IAuthenticationService _a12ns;

        public MockAuthentication(IDal dal, IHashingService hs, IAuthenticationService a12ns)
        {
            _dal = dal;
            _hs = hs;
            _a12ns = a12ns;
        }

        /// <summary>
        /// Mock user model. 
        /// </summary>
        // Exposed in case you want to roll your own.
        public class User
        {
            public string Username;
            public string Name;
            public string Surname;
            public string Email;
            public string Password;
            public Domain.User.Role Role;
            public Domain.User.AccountStatus AccountStatus;
        }

        // Convenience users. In case you don't want to roll your own.
        /// <summary>
        /// A standard mock user with admin role.
        /// </summary>
        public static User StandardTestUserWithRoleAdmin 
        { 
            get 
            { 
                return new User() 
                {
                    Username = "TestUser1WithRoleAdmin",
                    Name = "Name1",
                    Surname = "Surname1",
                    Email = "TestUser1@example.com",
                    Password = "TestUser1Password",
                    Role = Domain.User.Role.Admin,
                    AccountStatus = Domain.User.AccountStatus.Active,
                }; 
            } 
        }
        /// <summary>
        /// A standard mock user with user role.
        /// </summary>
        public static User StandardTestUserWithRoleUser
        {
            get
            {
                return new User()
                {
                    Username = "TestUser2WithRoleUser",
                    Name = "Name2",
                    Surname = "Surname2",
                    Email = "TestUser2@example.com",
                    Password = "TestUser2Password",
                    Role = Domain.User.Role.User,
                    AccountStatus = Domain.User.AccountStatus.Active,
                };
            }
        }
        /// <summary>
        /// A standard mock user with everyone role.
        /// </summary>
        public static User StandardTestUserWithRoleEveryone
        {
            get
            {
                return new User()
                {
                    Username = "TestUser3WithRoleEveryone",
                    Name = "Name3",
                    Surname = "Surname3",
                    Email = "TestUser3@example.com",
                    Password = "TestUser3Password",
                    Role = Domain.User.Role.Everyone,
                    AccountStatus = Domain.User.AccountStatus.Active,
                };
            }
        }

        /// <summary>
        /// Convenience method for adding a mocked user to our data store.
        /// </summary>
        /// <param name="user"></param>
        public void AddUser(User user)
        {
            var hashedPwd = _hs.GenerateHashBundleFromValue(user.Password);
            var dalResult = _dal.AddUser(user.Username,
                                         user.Name,
                                         user.Surname,
                                         user.Email,
                                         hashedPwd,
                                         user.Role,
                                         user.AccountStatus,
                                         CancellationToken.None).Result;
            _ = dalResult.DalAffectedId.Value ??
                throw new ArgumentException(@"AddUser: Dal returned a 
null user id when attempting to add a test user.");
        }

        /// <summary>
        /// Convenience method for authenticating a mocked user against our 
        /// authentication service.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public ApiSecurityToken AuthenticateUser(User user) 
        {
            return _a12ns.AuthenticateUser(user.Username, user.Password, CancellationToken.None).Result;
        }


    }
}
