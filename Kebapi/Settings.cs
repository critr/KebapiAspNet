namespace Kebapi
{
    /// <summary>
    /// POCO for strongly-typed application settings. 
    /// </summary>
    public class Settings
    {
        public ApiSettings Api { get; set; }
        public DalSettings Dal { get; set; }
    }

    public class ApiSettings
    {
        public PagingSettings Paging { get; set; }
        public UserRegistrationSettings UserRegistration { get; set; }
        public AuthSettings Auth { get; set; }
    }

    // Authentication.
    public class AuthSettings
    {
        public TokenValidationSettings TokenValidation { get; set; }
    }

    // Data access layer.
    public class DalSettings
    {
        // Database connection string.
        public string ConnectionString { get; set; }
        // Maximum number of rows to return from database Selects. If null, a default
        // will be used.
        public int? MaxSelectRows { get; set; }
    }

    // Limits when paging results, e.g. get me rows 5 through 10 and ensure those
    // two numbers (5 and 10) are sensible.
    public class PagingSettings
    {
        // Lower bound for start row; intended to allow zero- or one-based.
        public int MinStartRow { get; set; }
        // When a row count is specified, bound it by this minimum.
        public int MinRowCount { get; set; }
        // When a row count is specified, bound it by this maximum.
        public int MaxRowCount { get; set; }
    }

    // Settings when adding new users.
    public class UserRegistrationSettings
    {
        public int MinUsernameLength { get; set; }
        public int MinPasswordLength { get; set; }
    }

    // Security token settings.
    public class TokenValidationSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpireMinutes { get; set; }
        // Should be long and cryptic. This key is used to hash the final token
        // and that hash is what prevents tokens being tampered with.
        public string SigningKey { get; set; }
    }
}
