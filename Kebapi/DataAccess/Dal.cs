using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Kebapi.Domain;
using Kebapi.DataAccess.Sql;
using static Kebapi.LoggingHelper;

namespace Kebapi.DataAccess
{

    /// <summary>
    /// DAL (Data Access Layer).
    /// </summary>
    // Note: SQL statements are encapsulated in separate classes.
    public class Dal : IDal, IParsableSettings<DalSettings>
    {
        // Default limit for rows to return from the database.
        // If we don't get a _maxSelectRows from settings, this is the value it
        // will bet set to. 100 may or may not be realistic, but going with it.
        private const int DefaultMaxSelectRows = 100; 

        // These will be set from configuration
        private readonly string _connectionString;
        private readonly string _dbName;

        // Maximum number of rows to return from database Selects. Equivalent
        // of FETCH NEXT @_maxSelectRows.
        private readonly int _maxSelectRows;

        // We may or may not want a hard-coded limits to guard against what
        // comes in from config. Going with it.
        private const int MaxSelectRowsUpperLimit = 5000;
        private const int MaxSelectRowsLowerLimit = 1;

        // Settings from configuration.
        private readonly DalSettings _settings;

        public Dal(DalSettings settings)
        {
            // Config settings used by this class.
            _settings = ParseSettings(settings);

            _connectionString = _settings.ConnectionString;
            // Prior call to ParseSettings ensures this is a valid int not requiring a cast.
            _maxSelectRows = (int)_settings.MaxSelectRows;

            // Grab/parse the useful bits from that connection string
            var sb = new SqlConnectionStringBuilder(_connectionString);
            _dbName = sb.InitialCatalog.Trim();
            if (string.IsNullOrEmpty(_dbName))
                throw new ArgumentException(
                    $"{nameof(Dal)}: Database name is missing from the connection string.");
            if (IsMasterDatabase(_dbName))
                throw new ArgumentException(
                    $"{nameof(Dal)}: Database name cannot be that of the master database.");

        }

        /// <summary>
        /// The current database name.
        /// </summary>
        // Read-only prop. Only allowing 1 setup path through constructor config.
        public string DbName { get { return _dbName; } }

        /// <summary>
        /// The current connection string.
        /// </summary>
        // Read-only prop. Only allowing 1 setup path through constructor config.
        public string ConnectionString { get { return _connectionString; } }


        /// <summary>
        /// Drop db if it exists.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DropKebApiDatabase(CancellationToken cancellationToken)
        {
            await DropDatabase(_dbName, cancellationToken);
        }

        /// <summary>
        /// Create empty db if it doesn't exist.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CreateKebApiDatabase(CancellationToken cancellationToken)
        {
            await CreateDatabase(_dbName, cancellationToken);
        }

        /// <summary>
        /// On db perform: drop, create, add schema.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ResetKebApiDatabase(CancellationToken cancellationToken)
        {
            await ResetDatabase(_dbName, cancellationToken);
        }

        /// <summary>
        /// On db perform: drop, create, add schema, add test data.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ResetKebApiTestDatabase(CancellationToken cancellationToken)
        {
            await ResetTestDatabase(_dbName, cancellationToken);
        }



        // Note: Drop, Create, Exists database operations operate against
        // master db and therefore take care of their own connections.

        // Creates and returns an open connection to a database.
        // This will typically be called with either _dbName or "master" as
        // databaseName.
        private async Task<SqlConnection> CreateOpenConnection(string databaseName,
            CancellationToken cancellationToken)
        {
            // Substitute databaseName into connection string as needed.
            var connectionString = _connectionString;
            if (databaseName != _dbName)
            {
                var cb = new SqlConnectionStringBuilder(connectionString) {
                    InitialCatalog = databaseName };
                connectionString = cb.ToString();
            }

            var cn = new SqlConnection(connectionString);
            await cn.OpenAsync(cancellationToken);
            return cn;
        }

        // Creates a new empty database if it doesn't exist.
        private async Task CreateDatabase(string databaseName,
            CancellationToken cancellationToken)
        {
            // Safeguard. We will connect to the master database for this operation,
            // but we cannot attempt to create another master database.
            if (IsMasterDatabase(databaseName)) 
                throw new ArgumentException(
                    "Operation cannot be invoked on the master database.");
            if (!IsKebApiDatabase(databaseName)) 
                throw new ArgumentException(
                    "Operation cannot be invoked on databases other than a KebApi database.");

            using (var cn = await CreateOpenConnection("master", cancellationToken))
            {
                var cmd = new SqlCommand(
                    @$"
                        IF db_id('{databaseName}') IS Null 
                        BEGIN
                            CREATE DATABASE [{databaseName}]; 
                        END;
                    ", cn);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            };
        }

        // Creates a KebApiDb schema in a database. The operation is expected 
        // to run on a database with no KebApiDb schema present.
        private static async Task CreateTables(SqlConnection cn,
            CancellationToken cancellationToken)
        {

            foreach (var cmd in Commands())
            {
                cmd.Connection = cn;
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            static IEnumerable<SqlCommand> Commands()
            {
                yield return new SqlCommand(
                    SchemaSql.Create[$"Table.{DbTable.LookupRoles}"]);
                yield return new SqlCommand(
                    SchemaSql.Create[$"Table.{DbTable.LookupUserAccountStatus}"]);
                yield return new SqlCommand(
                    SchemaSql.Create[$"Table.{DbTable.Users}"]);
                yield return new SqlCommand(
                    SchemaSql.Create[$"Table.{DbTable.Media}"]);
                yield return new SqlCommand(
                    SchemaSql.Create[$"Table.{DbTable.Venues}"]);
                yield return new SqlCommand(
                    SchemaSql.Create[$"Table.{DbTable.UserFavouriteVenues}"]);
            }
        }

        // Inserts sample test data into a database having a KebApiDb schema.
        private static async Task InsertSampleTestData(SqlConnection cn, 
            CancellationToken cancellationToken)
        {

            foreach (var cmd in Commands())
            {
                cmd.Connection = cn;
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            static IEnumerable<SqlCommand> Commands()
            {
                // lookup_roles
                yield return new SqlCommand(
                    SampleDataSql.Populate[$"Table.{DbTable.LookupRoles}"]);
                // lookup_user_account_status
                yield return new SqlCommand(
                    SampleDataSql.Populate[$"Table.{DbTable.LookupUserAccountStatus}"]);
                // users
                yield return new SqlCommand
                    (SampleDataSql.Populate[$"Table.{DbTable.Users}"]);
                // media
                yield return new SqlCommand(
                    SampleDataSql.Populate[$"Table.{DbTable.Media}"]);
                // venues
                yield return new SqlCommand(
                    SampleDataSql.Populate[$"Table.{DbTable.Venues}"]);
                // user_favourite_venues
                yield return new SqlCommand(
                    SampleDataSql.Populate[$"Table.{DbTable.UserFavouriteVenues}"]);
            }
        }

        // Drops a database if it exists.
        private async Task DropDatabase(string databaseName, 
            CancellationToken cancellationToken)
        {
            // Safeguard. We will connect to the master database for this operation,
            // but we cannot attempt to drop the master database.
            if (IsMasterDatabase(databaseName)) 
                throw new ArgumentException("Operation cannot be invoked on the master database.");
            if (!IsKebApiDatabase(databaseName)) 
                throw new ArgumentException("Operation cannot be invoked on databases other than a KebApi database.");

            using (var cn = await CreateOpenConnection("master", cancellationToken))
            {
                var cmd = new SqlCommand(
                    @$"
                        IF db_id('{databaseName}') IS NOT Null 
                        BEGIN 
                            ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            DROP DATABASE [{databaseName}]; 
                        END;
                    ", cn);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            };
        }



        // Drops and recreates a database conforming to a KebApiDb schema.
        private async Task ResetDatabase(string databaseName,
            CancellationToken cancellationToken)
        {
            // Safeguard. We will connect to the master database for some of these
            // operations, but we cannot attempt to drop/create another master database.
            // Note: DropDatabase and CreateDatabase already have safeguards; this
            // is an extra layer of protection.
            if (IsMasterDatabase(databaseName)) 
                throw new ArgumentException("Operation cannot be invoked on the master database.");
            if (!IsKebApiDatabase(databaseName)) 
                throw new ArgumentException("Operation cannot be invoked on databases other than a KebApi database.");

            await DropDatabase(databaseName, cancellationToken);
            await CreateDatabase(databaseName, cancellationToken);
            using (var cn = await CreateOpenConnection(databaseName, cancellationToken))
            {
                await CreateTables(cn, cancellationToken);
            };

        }

        // Drops and recreates a database conforming to a KebApiDb schema, and
        // seeds it with sample test data.
        private async Task ResetTestDatabase(string databaseName, 
            CancellationToken cancellationToken)
        {
            await ResetDatabase(databaseName, cancellationToken);
            using (var cn = await CreateOpenConnection(databaseName, cancellationToken))
            {
                await InsertSampleTestData(cn, cancellationToken);
            };
        }



        // Integrity checking helpers

        // Check if a database is the master database.
        private static bool IsMasterDatabase(string databaseName)
        {
            return string.Compare(databaseName, "master", ignoreCase: true) == 0;
        }

        // Check if a database is a KebApi database.
        private bool IsKebApiDatabase(string databaseName)
        {
            // Simplistic placeholder. Should check schema.
            return string.Compare(databaseName, _dbName, ignoreCase: true) == 0;
        }

        // Ensures we never have an unreasonable value for maximum number of rows
        // that can be returned. Applies to SQL queries that use LIMIT clause for
        // rows returned. We may or may not want a hard-coded safety net to override
        // what can already be set in config. Going with it.
        private static bool IsWithinLimitForRowCount(int value)
        {
            return value >= MaxSelectRowsLowerLimit && value <= MaxSelectRowsUpperLimit;
        }





        // Mapping Helpers
        /// <summary>
        /// Create a DTO from the given message and code.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        /// <returns>A DTO <see cref="Dto.DalUser"/></returns>
        public Dto.DalResult MapToDalResult(string message, int code)
        {
            return new Dto.DalResult() { Message = message, Code = code };
        }
        /// <summary>
        /// Create a DTO from the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A DTO <see cref="Dto.DalAffectedId"/></returns>
        public Dto.DalAffectedId MapToDalAffectedId(int? id)
        {
            return new Dto.DalAffectedId() { Value = id };
        }
        /// <summary>
        /// Create a DTO from the given count.
        /// </summary>
        /// <param name="count"></param>
        /// <returns>A DTO <see cref="Dto.DalAffectedRows"/></returns>
        public Dto.DalAffectedRows MapToDalAffectedRows(int count)
        {
            return new Dto.DalAffectedRows() { Count = count };
        }


        // Users

        // Get* Helper.
        private static async Task<Dto.DalUser> GetUserCmd(SqlCommand getCommand, 
            CancellationToken cancellationToken)
        {
            Dto.DalUser r = null;
            using (var reader = await getCommand.ExecuteReaderAsync(cancellationToken))
            {
                if (reader.HasRows)
                {
                    // Should only ever be 1 row. Only take the first row regardless.
                    await reader.ReadAsync(cancellationToken);
                    var idField = reader.GetOrdinal("id");
                    var userNameField = reader.GetOrdinal("username");
                    var nameField = reader.GetOrdinal("name");
                    var surnameField = reader.GetOrdinal("surname");
                    var emailField = reader.GetOrdinal("email");
                    var passwordHashField = reader.GetOrdinal("password_hash");
                    var roleField = reader.GetOrdinal("role");
                    var accountStatusIdField = reader.GetOrdinal("account_status_id");
                    r = new Dto.DalUser()
                    {
                        Id = await reader.GetFieldValueAsync<int>(
                            idField, cancellationToken),
                        Username = await reader.GetFieldValueAsync<string>(
                            userNameField, cancellationToken),
                        Name = await reader.GetFieldValueAsync<string>(
                            nameField, cancellationToken),
                        Surname = await reader.GetFieldValueAsync<string>(
                            surnameField, cancellationToken),
                        Email = await reader.GetFieldValueAsync<string>(
                            emailField, cancellationToken),
                        PasswordHash = await reader.GetFieldValueAsync<string>(
                            passwordHashField, cancellationToken),
                        Role = await reader.GetFieldValueAsync<string>(
                            roleField, cancellationToken),
                        AccountStatusId = await reader.GetFieldValueAsync<byte>(
                            accountStatusIdField, cancellationToken),
                    };
                }
            }
            return r;
        }

        /// <summary>
        /// Tries to get the user matching the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.DalUser"/> 
        /// if there is a match, or of null otherwise.
        /// </returns>
        public async Task<Dto.DalUser> GetUser(int id, 
            CancellationToken cancellationToken)
        {
            Dto.DalUser r = null;
            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Users["GetUser"], cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    r = await GetUserCmd(cmd, cancellationToken);
                }
            }

            return r;
        }

        /// <summary>
        /// Tries to get the user matching the given email address.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.DalUser"/> 
        /// if there is a match, or of null otherwise.
        /// </returns>
        public async Task<Dto.DalUser> GetUserByEmail(string email, 
            CancellationToken cancellationToken)
        {
            Dto.DalUser r = null;
            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Users["GetUserByEmail"], cn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    r = await GetUserCmd(cmd, cancellationToken);
                }
            }

            return r;
        }

        /// <summary>
        /// Tries to get the user matching the given username.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.DalUser"/> 
        /// if there is a match, or of null otherwise.
        /// </returns>
        public async Task<Dto.DalUser> GetUserByUsername(string username, 
            CancellationToken cancellationToken)
        {
            Dto.DalUser r = null;
            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Users["GetUserByUsername"], cn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    r = await GetUserCmd(cmd, cancellationToken);
                }
            }

            return r;
        }

        /// <summary>
        /// Tries to get the account status of the user matching the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.DalUserAccountStatus"/> 
        /// if there is a match, or of null otherwise.
        /// </returns>
        public async Task<Dto.DalUserAccountStatus> GetUserAccountStatus(int id, 
            CancellationToken cancellationToken)
        {
            Dto.DalUserAccountStatus r = null;

            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Users["GetUserAccountStatus"], cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync(cancellationToken);
                            var idField = reader.GetOrdinal("id");
                            var statusField = reader.GetOrdinal("status");
                            r = new Dto.DalUserAccountStatus()
                            {
                                Id = await reader.GetFieldValueAsync<byte>(
                                    idField, cancellationToken),
                                Status = await reader.GetFieldValueAsync<string>(
                                    statusField, cancellationToken),
                            };
                        }
                    }
                }
            }

            return r;

        }

        /// <summary>
        /// Tries to get the current number of users.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.DalResultWithAffectedRows"/> comprising a
        /// <see cref="Result.GetUserCountResult"/> and a count of the affected rows.
        /// If successful, that count will be the number of users.
        /// </returns>
        // Intended for Admin-level use, e.g. for maintenance or simple stat
        // gathering. Leaving access responsibility to be handled higher up the
        // stack.
        public async Task<Dto.DalResultWithAffectedRows> GetUserCount(
            CancellationToken cancellationToken)
        {
            Dto.DalResultWithAffectedRows r = null;

            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Users["GetUserCount"], cn))
                {
                    var cnt = await cmd.ExecuteScalarAsync(cancellationToken);
                    if (!(cnt is int)) 
                        throw new Exception(
                            $"Executing SQL for {MethodName()} did not return an integer as expected.");
                    r = new Dto.DalResultWithAffectedRows()
                    {
                        DalResult = MapToDalResult(
                            "Got count.", (int)Result.GetUserCountResult.OK),
                        DalAffectedRows = MapToDalAffectedRows((int)cnt),
                    };
                }
            }
            return r;
        }

        /// <summary>
        /// Tries to get the favourite venues of the user matching the given id,
        /// beginning at the given start row of the data set, and continuing no
        /// further than the given number of rows.
        /// <para>
        /// Start row and row count are bounded, defaulting to presets if not 
        /// supplied or invalid.
        /// </para>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startRow"></param>
        /// <param name="rowCount"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of List of DTO type
        /// <see cref="Dto.DalVenue"/> if it succeeds, or of null otherwise.
        /// </returns>
        public async Task<List<Dto.DalVenue>> GetUserFavourites(
            int id, int startRow, int rowCount, CancellationToken cancellationToken)
        {
            List<Dto.DalVenue> r = null;

            int offset = ParseStartRow(startRow);
            int limit = ParseRowCount(rowCount);

            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Users["GetUserFavourites"], cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@offset", offset);
                    cmd.Parameters.AddWithValue("@limit", limit);
                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (reader.HasRows)
                        {
                            r = new List<Dto.DalVenue>();
                            var idField = reader.GetOrdinal("id");
                            var nameField = reader.GetOrdinal("name");
                            var geoLatField = reader.GetOrdinal("geo_lat");
                            var geoLngField = reader.GetOrdinal("geo_lng");
                            var addressField = reader.GetOrdinal("address");
                            var ratingField = reader.GetOrdinal("rating");
                            var mainMediaPathField = reader.GetOrdinal("media_path");
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                r.Add(new Dto.DalVenue()
                                {
                                    Id = await reader.GetFieldValueAsync<int>(
                                        idField, cancellationToken),
                                    Name = await reader.GetFieldValueAsync<string>(
                                        nameField, cancellationToken),
                                    GeoLat = await reader.GetFieldValueAsync<decimal>(
                                        geoLatField, cancellationToken),
                                    GeoLng = await reader.GetFieldValueAsync<decimal>(
                                        geoLngField, cancellationToken),
                                    Address = await reader.GetFieldValueAsync<string>(
                                        addressField, cancellationToken),
                                    Rating = await reader.GetFieldValueAsync<byte>(
                                        ratingField, cancellationToken),
                                    MainMediaPath = await reader.GetFieldValueAsync<string>(
                                        mainMediaPathField, cancellationToken),
                                });
                            };
                        }
                    }
                }
            }
            return r;
        }

        /// <summary>
        /// Tries to get the users beginning at the given start row of the data 
        /// set, and continuing no further than the given number of rows.
        /// <para>
        /// Start row and row count are bounded, defaulting to presets if not 
        /// supplied or invalid.
        /// </para>
        /// </summary>
        /// <param name="startRow"></param>
        /// <param name="rowCount"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of List of DTO type
        /// <see cref="Dto.DalUser"/> if it succeeds, or of null otherwise.
        /// </returns>
        public async Task<List<Dto.DalUser>> GetUsers(
            int startRow, int rowCount, CancellationToken cancellationToken)
        {
            List<Dto.DalUser> r = null;

            int offset = ParseStartRow(startRow);
            int limit = ParseRowCount(rowCount);

            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Users["GetUsers"], cn))
                {
                    cmd.Parameters.AddWithValue("@offset", offset);
                    cmd.Parameters.AddWithValue("@limit", limit);
                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (reader.HasRows)
                        {
                            r = new List<Dto.DalUser>();
                            var idField = reader.GetOrdinal("id");
                            var usernameField = reader.GetOrdinal("username");
                            var nameField = reader.GetOrdinal("name");
                            var surnameField = reader.GetOrdinal("surname");
                            var emailField = reader.GetOrdinal("email");
                            var passwordHashField = reader.GetOrdinal("password_hash");
                            var roleField = reader.GetOrdinal("role");
                            var accountStatusIdField = reader.GetOrdinal("account_status_id");
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                r.Add(new Dto.DalUser()
                                {
                                    Id = await reader.GetFieldValueAsync<int>(
                                        idField, cancellationToken),
                                    Username = await reader.GetFieldValueAsync<string>(
                                        usernameField, cancellationToken),
                                    Name = await reader.GetFieldValueAsync<string>(
                                        nameField, cancellationToken),
                                    Surname = await reader.GetFieldValueAsync<string>(
                                        surnameField, cancellationToken),
                                    Email = await reader.GetFieldValueAsync<string>(
                                        emailField, cancellationToken),
                                    PasswordHash = await reader.GetFieldValueAsync<string>(
                                        passwordHashField, cancellationToken),
                                    Role = await reader.GetFieldValueAsync<string>(
                                        roleField, cancellationToken),
                                    AccountStatusId = await reader.GetFieldValueAsync<byte>(
                                        accountStatusIdField, cancellationToken),
                                });
                            };
                        }
                    }

                }
            }
            return r;
        }


        /// <summary>
        /// Tries to set the account status to an active state for the user matching 
        /// the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.DalAffectedRows"/> indicating the rows affected. 
        /// Should be 0 (no match to given id) or 1, unless unique id constraint for users has been broken.
        /// </returns>
        // Will return number of rows affected. Should be 0 or 1, unless unique
        // id constraint for users has been broken.
        public async Task<Dto.DalAffectedRows> ActivateUser(int id, 
            CancellationToken cancellationToken)
        {
            Dto.DalAffectedRows r = null;
            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Users["ActivateUser"], cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    var dr = await cmd.ExecuteNonQueryAsync(cancellationToken);
                    r = new Dto.DalAffectedRows() { Count = dr };
                }
            }
            return r;
        }

        /// <summary>
        /// Tries to set the account status to an inactive state for the user matching 
        /// the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.DalAffectedRows"/> indicating the rows affected. 
        /// Should be 0 (no match to given id) or 1, unless unique id constraint for users has been broken.
        /// </returns>
        // Will return number of rows affected. Should be 0 or 1, unless unique
        // id constraint for user has been broken.
        public async Task<Dto.DalAffectedRows> DeactivateUser(int id, 
            CancellationToken cancellationToken)
        {
            Dto.DalAffectedRows r = null;
            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Users["DeactivateUser"], cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    var dr = await cmd.ExecuteNonQueryAsync(cancellationToken);
                    r = new Dto.DalAffectedRows() { Count = dr };
                }
            }
            return r;
        }

        /// <summary>
        /// Tries to add a user with the given arguments.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <param name="surname"></param>
        /// <param name="email"></param>
        /// <param name="passwordHash"></param>
        /// <param name="userRole"></param>
        /// <param name="userAccountStatus"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.DalResultWithAffectedId"/> 
        /// comprising a <see cref="Result.AddUserResult"/> and an affected id.
        /// If successful, that id will be the id of the new user, null otherwise.
        /// </returns>
        public async Task<Dto.DalResultWithAffectedId> AddUser(string username, 
            string name, string surname, string email, string passwordHash, 
            User.Role userRole, User.AccountStatus userAccountStatus, 
            CancellationToken cancellationToken)
        {
            Dto.DalResultWithAffectedId r = null;

            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Users["AddUser"], cn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@surname", surname);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@roleId", userRole);
                    cmd.Parameters.AddWithValue("@accountStatusId", userAccountStatus);
                    // Here we're going to rely on the database to tell us if it's
                    // not happy with our insert. This assumes our db has the right
                    // constraints. If we don't want to assume that, we'd need
                    // to go with something like AddUserAlternative in DomainSQL.cs
                    // or sp/transactions instead.
                    try
                    {
                        var id = await cmd.ExecuteScalarAsync(cancellationToken);
                        if (!(id is int))
                            throw new Exception(
                                $"Executing SQL for {MethodName()} did not return an integer as expected.");
                        r = new Dto.DalResultWithAffectedId()
                        {                                    
                            DalResult = MapToDalResult(
                                "User added.", (int)Result.AddUserResult.OK),
                            DalAffectedId = MapToDalAffectedId((int)id)
                        };
                    }
                    catch (SqlException e)
                    {
                        // Violation of UNIQUE KEY constraint.
                        if (e.Number == 2627)
                        {
                            if (e.Message.Contains("'uq_user_email'"))
                                r = new Dto.DalResultWithAffectedId()
                                {
                                    DalResult = MapToDalResult(
                                        "A user already exists with that email.", 
                                        (int)Result.AddUserResult.ErrorDuplicateEmail),
                                    DalAffectedId = MapToDalAffectedId(null),
                                };

                            else if (e.Message.Contains("'uq_user_username'"))
                                r = new Dto.DalResultWithAffectedId()
                                {
                                    DalResult = MapToDalResult(
                                        "A user already exists with that username.", 
                                        (int)Result.AddUserResult.ErrorDuplicateUsername),
                                    DalAffectedId = MapToDalAffectedId(null),
                                };

                            else
                                throw;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            return r;
        }


        /// <summary>
        /// Tries to add a venue to a user's favourites, for the given user id 
        /// and venue id.
        /// </summary>
        /// <param name="id">Id of the user to affect.</param>
        /// <param name="venueId">Id of the venue to add.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.DalResultWithAffectedId"/> 
        /// comprising a <see cref="Result.AddUserFavouriteResult"/> and an 
        /// affected id. If successful, that id will be the id of the new favourite, 
        /// null otherwise.
        /// </returns>
        public async Task<Dto.DalResultWithAffectedId> AddUserFavourite(
            int id, int venueId, CancellationToken cancellationToken)
        {
            Dto.DalResultWithAffectedId r = null;

            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Users["AddUserFavourite"], cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@venueId", venueId);
                    // Here we're going to rely on the database to tell us if it's
                    // not happy with our insert. This assumes our db has the right
                    // constraints. If we don't want to assume that, we'd need
                    // to go with something like AddUserAlternative in DomainSQL.cs
                    // or sp/transactions instead.
                    try
                    {
                        var favId = await cmd.ExecuteScalarAsync(cancellationToken);
                        if (!(favId is int))
                            throw new Exception(
                                $"Executing SQL for {MethodName()} did not return an integer as expected.");
                        r = new Dto.DalResultWithAffectedId()
                        {
                            DalResult = MapToDalResult(
                                "User favourite added.", 
                                (int)Result.AddUserFavouriteResult.OK),
                            DalAffectedId = MapToDalAffectedId((int?)favId),
                        };
                    }
                    catch (SqlException e)
                    {
                        // The INSERT statement conflicted with the FOREIGN KEY
                        // constraint.
                        if (e.Number == 547)
                        {
                            if (e.Message.Contains("\"fk_user_favourite_venues_users_id\""))
                                r = new Dto.DalResultWithAffectedId()
                                {
                                    DalResult = MapToDalResult(
                                        "Cannot add favourite. User does not exist.", 
                                        (int)Result.AddUserFavouriteResult.ErrorInexistentUser),
                                    DalAffectedId = MapToDalAffectedId(null),
                                };
                            else if (e.Message.Contains("\"fk_user_favourite_venues_venues_id\""))
                                r = new Dto.DalResultWithAffectedId()
                                {
                                    DalResult = MapToDalResult(
                                        "Cannot add favourite. Venue does not exist.", 
                                        (int)Result.AddUserFavouriteResult.ErrorInexistentVenue),
                                    DalAffectedId = MapToDalAffectedId(null),
                                };
                            else
                                throw;
                        }
                        // Violation of UNIQUE KEY constraint.
                        else if (e.Number == 2627)
                        {
                            if (e.Message.Contains("'uq_user_venue'"))
                                r = new Dto.DalResultWithAffectedId()
                                {
                                    DalResult = MapToDalResult(
                                        "Favourite already exists.", 
                                        (int)Result.AddUserFavouriteResult.ErrorDuplicate),
                                    DalAffectedId = MapToDalAffectedId(null),
                                };

                            else
                                throw;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            return r;
        }

        /// <summary>
        /// Tries to remove a venue from a user's favourites, for the given 
        /// user id and venue id.
        /// </summary>
        /// <param name="id">Id of the user to affect.</param>
        /// <param name="venueId">Id of the venue to remove.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.DalResultWithAffectedId"/> 
        /// comprising a <see cref="Result.RemoveUserFavouriteResult"/> and an 
        /// affected id. If successful, that id will be the id of the removed 
        /// favourite, null otherwise.
        /// </returns>
        public async Task<Dto.DalResultWithAffectedId> RemoveUserFavourite(
            int id, int venueId, CancellationToken cancellationToken)
        {
            Dto.DalResultWithAffectedId r = null;

            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Users["RemoveUserFavourite"], cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@venueId", venueId);
                    var favId = await cmd.ExecuteScalarAsync(cancellationToken);
                    // Null is expected for favId whenever we try to remove a
                    // favourite that doesn't exist.
                    r = new Dto.DalResultWithAffectedId()
                    {
                        DalResult = favId != null 
                        ? MapToDalResult(
                            "User favourite removed.", 
                            (int)Result.RemoveUserFavouriteResult.OK)
                        : MapToDalResult(
                            "No favourite to remove matching that user and venue.", 
                            (int)Result.RemoveUserFavouriteResult.ErrorInexistent),
                        DalAffectedId = MapToDalAffectedId((int?)favId),
                    };
                }
            }
            return r;
        }




        // Venues

        /// <summary>
        /// Tries to get the venue matching the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.DalVenue"/> 
        /// if there is a match, or of null otherwise.
        /// </returns>
        public async Task<Dto.DalVenue> GetVenue(int id, CancellationToken cancellationToken)
        {
            Dto.DalVenue r = null;
            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Venues["GetVenue"], cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (reader.HasRows)
                        {
                            // Should only ever be 1 row. Only take the first row regardless.
                            await reader.ReadAsync(cancellationToken);
                            var idField = reader.GetOrdinal("id");
                            var nameField = reader.GetOrdinal("name");
                            var geoLatField = reader.GetOrdinal("geo_lat");
                            var geoLngField = reader.GetOrdinal("geo_lng");
                            var addressField = reader.GetOrdinal("address");
                            var ratingField = reader.GetOrdinal("rating");
                            var mainMediaPathField = reader.GetOrdinal("main_media_path");
                            r = new Dto.DalVenue()
                            {
                                Id = await reader.GetFieldValueAsync<int>(
                                    idField, cancellationToken),
                                Name = await reader.GetFieldValueAsync<string>(
                                    nameField, cancellationToken),
                                GeoLat = await reader.GetFieldValueAsync<decimal>(
                                    geoLatField, cancellationToken),
                                GeoLng = await reader.GetFieldValueAsync<decimal>(
                                    geoLngField, cancellationToken),
                                Address = await reader.GetFieldValueAsync<string>(
                                    addressField, cancellationToken),
                                Rating = await reader.GetFieldValueAsync<byte>(
                                    ratingField, cancellationToken),
                                MainMediaPath = await reader.GetFieldValueAsync<string>(
                                    mainMediaPathField, cancellationToken),
                            };

                        }
                    }
                }
            }

            return r;
        }

        /// <summary>
        /// Tries to get the venues beginning at the given start row of the data 
        /// set, and continuing no further than the given number of rows.
        /// <para>
        /// Start row and row count are bounded, defaulting to presets if not 
        /// supplied or invalid.
        /// </para>
        /// </summary>
        /// <param name="startRow"></param>
        /// <param name="rowCount"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of List of DTO type
        /// <see cref="Dto.DalVenue"/> if it succeeds, or of null otherwise.
        /// </returns>
        public async Task<List<Dto.DalVenue>> GetVenues(
            int startRow, int rowCount, CancellationToken cancellationToken)
        {
            List<Dto.DalVenue> r = null;

            int offset = ParseStartRow(startRow);
            int limit = ParseRowCount(rowCount);

            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Venues["GetVenues"], cn))
                {
                    cmd.Parameters.AddWithValue("@offset", offset);
                    cmd.Parameters.AddWithValue("@limit", limit);
                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (reader.HasRows)
                        {
                            r = new List<Dto.DalVenue>();
                            var idField = reader.GetOrdinal("id");
                            var nameField = reader.GetOrdinal("name");
                            var geoLatField = reader.GetOrdinal("geo_lat");
                            var geoLngField = reader.GetOrdinal("geo_lng");
                            var addressField = reader.GetOrdinal("address");
                            var ratingField = reader.GetOrdinal("rating");
                            var mainMediaPathField = reader.GetOrdinal("main_media_path");
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                r.Add(new Dto.DalVenue()
                                {
                                    Id = await reader.GetFieldValueAsync<int>(
                                        idField, cancellationToken),
                                    Name = await reader.GetFieldValueAsync<string>(
                                        nameField, cancellationToken),
                                    GeoLat = await reader.GetFieldValueAsync<decimal>(
                                        geoLatField, cancellationToken),
                                    GeoLng = await reader.GetFieldValueAsync<decimal>(
                                        geoLngField, cancellationToken),
                                    Address = await reader.GetFieldValueAsync<string>(
                                        addressField, cancellationToken),
                                    Rating = await reader.GetFieldValueAsync<byte>(
                                        ratingField, cancellationToken),
                                    MainMediaPath = await reader.GetFieldValueAsync<string>(
                                        mainMediaPathField, cancellationToken),
                                });
                            };
                        }
                    }

                }
            }
            return r;
        }

        /// <summary>
        /// Tries to get the current number of venues.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.DalResultWithAffectedRows"/> comprising a
        /// <see cref="Result.GetVenueCountResult"/> and a count of the affected rows.
        /// If successful, that count will be the number of venues.
        /// </returns>
        // Intended for Admin-level use, e.g. for maintenance or simple stat gathering.
        // Leaving access responsibility to be handled higher up the stack.
        public async Task<Dto.DalResultWithAffectedRows> GetVenueCount(
            CancellationToken cancellationToken)
        {
            Dto.DalResultWithAffectedRows r = null;

            using (var cn = await CreateOpenConnection(_dbName, cancellationToken))
            {
                using (var cmd = new SqlCommand(DomainSql.Venues["GetVenueCount"], cn))
                {
                    var cnt = await cmd.ExecuteScalarAsync(cancellationToken);
                    if (!(cnt is int))
                        throw new Exception(
                            $"Executing SQL for {MethodName()} did not return an integer as expected.");
                    r = new Dto.DalResultWithAffectedRows()
                    {
                        DalResult = MapToDalResult(
                            "Got count.", 
                            (int)Result.GetVenueCountResult.OK),
                        DalAffectedRows = MapToDalAffectedRows((int)cnt),
                    };
                }
            }
            return r;
        }





        // Helpers

        private static int ParseStartRow(int startRow)
        {
            return Math.Abs(startRow);
        }

        private int ParseRowCount(int rowCount)
        {
            var rows = Math.Abs(rowCount);
            if (rows == 0 || rows > _maxSelectRows)
                return _maxSelectRows;
            else
                return rows;
        }

        /// <summary>
        /// Parses settings for this component. 
        /// </summary>
        /// <remarks>
        /// Throws if any settings are invalid.
        /// </remarks>
        /// <param name="unparsed"></param>
        /// <returns>Parsed <see cref="DalSettings"/>.</returns>
        public DalSettings ParseSettings(DalSettings unparsed)
        {
            var cs = unparsed.ConnectionString.Trim();
            if (string.IsNullOrEmpty(cs))
                throw new ArgumentException(
                    $"{nameof(Dal)}: Connection string is missing from settings.");

            var msr = unparsed.MaxSelectRows ?? DefaultMaxSelectRows;
            if (!IsWithinLimitForRowCount(msr))
                throw new ArgumentOutOfRangeException(
                    $"{nameof(Dal)}: The value for MaxSelectRows in settings is {msr} " +
                    $"which is outside the allowed range of {MaxSelectRowsLowerLimit} " +
                    $"and {MaxSelectRowsUpperLimit}.");
            
            return new DalSettings 
            {
                ConnectionString = cs,
                MaxSelectRows = msr,
            };
        }
    }


}
