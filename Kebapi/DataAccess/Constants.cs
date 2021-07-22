namespace Kebapi.DataAccess
{
    // Dal Constants.

    /// <summary>
    /// Database table names.
    /// </summary>
    internal static class DbTable
    {
        public const string Venues = "venues";
        public const string Users = "users";
        public const string UserFavouriteVenues = "user_favourite_venues";
        public const string LookupRoles = "lookup_roles";
        public const string LookupUserAccountStatus 
            = "lookup_user_account_status";
        public const string Media = "media";
    }

    /// <summary>
    /// Defined Dal results. 
    /// </summary>
    internal static class Result 
    {
        /// <summary>
        /// AddUser result.
        /// </summary>
        internal enum AddUserResult : int
        {
            // 0 "User added."
            OK = 0,
            // -1 "A user already exists with that email."
            ErrorDuplicateEmail = -1,
            // -2 "A user already exists with that username."
            ErrorDuplicateUsername = -2,
        }

        /// <summary>
        /// AddUserFavourite result. 
        /// </summary>
        internal enum AddUserFavouriteResult : int
        {
            // 0 "User favourite added."
            OK = 0,
            // -1 "Cannot add favourite. User does not exist."
            ErrorInexistentUser = -1,
            // -2 "Cannot add favourite. Venue does not exist."
            ErrorInexistentVenue = -2,
            //  -3 "Favourite already exists."
            ErrorDuplicate = -3,
        }

        /// <summary>
        /// RemoveUserFavourite result. 
        /// </summary>
        internal enum RemoveUserFavouriteResult : int
        {
            // 0 "User favourite removed."
            OK = 0,
            // -1 "No favourite to remove matching that user and venue."
            ErrorInexistent = -1,
        }

        /// <summary>
        /// GetUserCount result. 
        /// </summary>
        internal enum GetUserCountResult : int
        {
            // 0 "Got count."
            OK = 0,
        }

        /// <summary>
        /// GetVenueCount result. 
        /// </summary>
        internal enum GetVenueCountResult : int
        {
            // 0 "Got count."
            OK = 0,
        }

    }
}

