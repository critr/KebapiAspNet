namespace Kebapi.Domain
{
    /// <summary>
    /// Domain attributes representing a user.
    /// </summary>
    public static class User
    {
        /// <summary>
        /// Used to implement soft delete of Users.
        /// </summary>
        public enum AccountStatus : byte
        {
            Active = 2,
            Inactive = 1,
        }

        /// <summary>
        /// Used for Role-based authorisation of Users.
        /// </summary>
        public enum Role : byte
        {
            Admin = 1,
            User = 2,
            Everyone = 99,
        }

    }
}
