using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using static Kebapi.LoggingHelper;
using Kebapi.Domain;
using Kebapi.Dto;
using Kebapi.DataAccess;
using Kebapi.Services;
using Kebapi.Services.Hashing;
using Kebapi.Services.Authentication;

namespace Kebapi.Api
{
    /// <summary>
    /// Defines API functionality relating to regular users.
    /// </summary>
    public class Users : IUsers, IParsableSettings<UserRegistrationSettings>
    {
        private readonly IDal _dal;
        private readonly HttpContext _context;
        private readonly ILogger<IUsers> _logger;
        private readonly string _componentName;
        private readonly IHashingService _hashingService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRequestResponseHandler _rrHandler;
        private readonly InputParser _inputParser;
        private readonly DataMapper _dataMapper;
        private readonly UserRegistrationSettings _settings;

        public Users(
            IDal dal, 
            IHashingService hashingService,
            IAuthenticationService authenticationService, 
            IHttpContextAccessor contextAccessor, 
            ILogger<IUsers> logger, 
            UserRegistrationSettings settings, 
            IRequestResponseHandler rrh, 
            InputParser ip, DataMapper dm) 
        {
            _dal = dal;
            _context = contextAccessor.HttpContext;
            _hashingService = hashingService;
            _authenticationService = authenticationService;
            _logger = logger;
            _componentName = GetType().Name;
            _rrHandler = rrh;
            _inputParser = ip;
            _dataMapper = dm;
            // Config settings used by this class.
            _settings = ParseSettings(settings);
        }

        /// <summary>
        /// Tries to get an <see cref="Dto.ApiUser"/> for the user with an Id 
        /// matching the expected <see cref="HttpContext"/> route value of 'id'.
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiUserResponse"/>.</returns>
        public async Task<Dto.ApiUserResponse> Get()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            Dto.ApiUser apiUser = null;
            List<string> errors = new();

            // Validate expected inputs.
            var unparsedId = (string)_context.GetRouteValue("id");
            bool argsValid = _inputParser.TryParsePositiveInt(unparsedId, 
                out int id);
            _logger.LogDebug($"unparsedId={unparsedId}");
            _logger.LogDebug($"id={id}");

            // Do it.
            if (!argsValid)
            {
                msg = "Cannot invoke Get user.";
                errors.Add(
                    $"Missing an expected integer (greater than 0) argument: id. The value supplied was '{unparsedId}'.");
                code = HttpStatusCode.BadRequest;
            }
            else
            {
                var dr = await _dal.GetUser(id, _context.RequestAborted);
                if (dr == null)
                {
                    msg = $"Get user did not return a result.";
                    code = HttpStatusCode.NotFound;
                }
                else
                {
                    msg = $"Get user returned a result.";
                    apiUser = _dataMapper.MapToApiUser(dr);
                    code = HttpStatusCode.OK;
                }
            }

            // Result.
            Dto.ApiUserResponse ar = _dataMapper.MapToApiUserResponse(
                _dataMapper.MapToApiStatus(code, msg, errors), apiUser);

            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug($"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;
        }

        /// <summary>
        /// Tries to get an <see cref="Dto.ApiUser"/> for the user with a 
        /// Username matching the expected <see cref="HttpContext"/> query variable
        /// of 'username'.
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiUserResponse"/>.</returns>
        public async Task<Dto.ApiUserResponse> GetByUsername()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            Dto.ApiUser apiUser = null;
            List<string> errors = new();

            // Validate expected inputs.
            var unparsedUsername = _inputParser.GetQueryVar(
                _context, "username");
            bool sanitised = _inputParser.TryParseString(unparsedUsername, 
                out string username);
            _logger.LogDebug($"unparsedUsername={unparsedUsername}");
            _logger.LogDebug($"username={username}");

            // Do it.
            if (!sanitised)
            {
                msg = "Cannot invoke Get user by username.";
                errors.Add(
                    $"That username isn't allowed. The value supplied was '{unparsedUsername}'.");
                code = HttpStatusCode.BadRequest;
            }
            else
            {
                errors = FindErrors(username);
                if (errors.Count > 0)
                {
                    msg = "Wonky info received.";
                    code = HttpStatusCode.BadRequest;
                }
                else
                {
                    var dr = await _dal.GetUserByUsername(
                        username, _context.RequestAborted);
                    if (dr == null)
                    {
                        msg = $"Get user by username did not return a result.";
                        code = HttpStatusCode.NotFound;
                    }
                    else
                    {
                        msg = $"Get user by username returned a result.";
                        apiUser = _dataMapper.MapToApiUser(dr);
                        code = HttpStatusCode.OK;
                    }
                }
            }

            // Result.
            Dto.ApiUserResponse ar = _dataMapper.MapToApiUserResponse(
                _dataMapper.MapToApiStatus(code, msg, errors), apiUser);

            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug($"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;

            // Returns a List of errors in the given argument. If there are no
            // errors, that List will have a Count of zero.
            static List<string> FindErrors(string username)
            {
                // List of all errors found.
                List<string> errors = new();

                // Illustrative errors of the kind we could pick up on.

                if (string.IsNullOrWhiteSpace(username))
                {
                    errors.Add(
                        "Missing needed info. Username is blank or empty (null).");
                    return errors;
                }

                if (username.Length < 3 || username.Length > 40)
                    errors.Add("Username must be between 3 and 40 in length.");

                return errors;
            }

        }

        /// <summary>
        /// Tries to get the total number of registered users.
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiAffectedRowsResponse"/>.</returns>
        public async Task<Dto.ApiAffectedRowsResponse> GetCount()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            Dto.ApiAffectedRows apiAffectedRows = null;
            List<string> errors = new();

            // No inputs to validate.

            // Do it.
            var dr = await _dal.GetUserCount(_context.RequestAborted);
            if (dr.DalResult.Code == (int)Result.GetUserCountResult.OK)
            {
                msg = dr.DalResult.Message;
                code = HttpStatusCode.OK;
            }
            else
                throw new InvalidOperationException(
                    $"Dal.GetUserCount returned an unexpected code: {dr.DalResult.Code}");

            // Result.
            apiAffectedRows = _dataMapper.MapToApiAffectedRows(dr.DalAffectedRows);

            Dto.ApiAffectedRowsResponse ar = 
                _dataMapper.MapToApiAffectedRowsResponse(
                    _dataMapper.MapToApiStatus(code, msg, errors), 
                    apiAffectedRows);
            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug($"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));
            return ar;
        }

        /// <summary>
        /// Tries to get the <see cref="Dto.ApiUserAccountStatus"/> of the 
        /// user with an Id matching the 
        /// expected <see cref="HttpContext"/> route value of 'id'.
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiUserAccountStatusResponse"/>.</returns>
        public async Task<Dto.ApiUserAccountStatusResponse> GetAccountStatus()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            Dto.ApiUserAccountStatus apiUas = null;
            List<string> errors = new();

            // Validate expected inputs.
            var unparsedId = (string)_context.GetRouteValue("id");
            bool argsValid = _inputParser.TryParsePositiveInt(unparsedId, 
                out int id);
            _logger.LogDebug($"unparsedId={unparsedId}");
            _logger.LogDebug($"id={id}");

            // Do it.
            if (!argsValid)
            {
                msg = "Cannot invoke Get user account status.";
                errors.Add(
                    $"Missing an expected integer (greater than 0) argument: id. The value supplied was '{unparsedId}'.");
                code = HttpStatusCode.BadRequest;
            }
            else
            {
                var dr = await _dal.GetUserAccountStatus(
                    id, _context.RequestAborted);
                if (dr == null)
                {
                    msg = $"Get user account status did not return a result.";
                    code = HttpStatusCode.NotFound;
                }
                else
                {
                    msg = $"Get user account status returned a result.";
                    apiUas = _dataMapper.MapToApiUserAccountStatus(dr);
                    code = HttpStatusCode.OK;
                }
            }

            // Result.
            Dto.ApiUserAccountStatusResponse ar = 
                _dataMapper.MapToApiUserAccountStatusResponse(
                    _dataMapper.MapToApiStatus(code, msg, errors), apiUas);

            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug($"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;

        }

        /// <summary>
        /// Gets zero, one, or more, <see cref="Dto.ApiUser"/>(s) dependent on 
        /// a) the users present in the data set, and b) the expected 
        /// <see cref="HttpContext"/> query variables of 'startRow' (which row of
        /// the data set to start at) and 'rowCount' (how many rows of the 
        /// data set to return.)
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiUsersResponse"/>.</returns>
        public async Task<Dto.ApiUsersResponse> GetSome()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            List<Dto.ApiUser> apiUsers = null;
            List<string> errors = new();

            // Grab any expected query vars if present.
            // ints will default to zero if not present.
            // Parsing constrains to limits and ensures we'll always get a result.
            int startRow = _inputParser.ParseStartRow(
                _inputParser.GetQueryVarAsCardinal(_context, "startRow"));
            int rowCount = _inputParser.ParseRowCount(
                _inputParser.GetQueryVarAsCardinal(_context, "rowCount"));
            _logger.LogDebug($"startRow={startRow}");
            _logger.LogDebug($"rowCount={rowCount}");

            // Do it.
            var dr = await _dal.GetUsers(
                startRow, rowCount, _context.RequestAborted);
            if (dr == null)
            {
                msg = $"Get users did not return a result.";
                code = HttpStatusCode.NotFound;
            }
            else
            {
                msg = $"Get users returned a result.";
                apiUsers = dr.ConvertAll(new Converter<DalUser, ApiUser>(
                    (du) => { return _dataMapper.MapToApiUser(du); }));
                code = HttpStatusCode.OK;
            }

            // Result.
            Dto.ApiUsersResponse ar = _dataMapper.MapToApiUsersResponse(
                _dataMapper.MapToApiStatus(code, msg, errors), apiUsers);
            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug(
                $"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;
        }

        /// <summary>
        /// Gets zero, one, or more, <see cref="Dto.ApiVenue"/>(s) that are the 
        /// favourite venues of the user with an Id matching 
        /// the expected <see cref="HttpContext"/> route value of 'id'. Results are 
        /// further constrained by the query variables of 'startRow' (which row of
        /// the data set to start at) and 'rowCount' (how many rows of the 
        /// data set to return.)
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiVenuesResponse"/>.</returns>
        public async Task<Dto.ApiVenuesResponse> GetSomeFavourites()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            List<Dto.ApiVenue> apiVenues = null;
            List<string> errors = new();

            // Validate expected inputs.
            var unparsedId = (string)_context.GetRouteValue("id");
            bool argsValid = _inputParser.TryParsePositiveInt(unparsedId, 
                out int id);
            _logger.LogDebug($"unparsedId={unparsedId}");
            _logger.LogDebug($"id={id}");

            // Grab any expected query vars if present.
            // ints will default to zero if not present.
            // Parsing constrains to limits and ensures we'll always get a result.
            int startRow = _inputParser.ParseStartRow(
                _inputParser.GetQueryVarAsCardinal(_context, "startRow"));
            int rowCount = _inputParser.ParseRowCount(
                _inputParser.GetQueryVarAsCardinal(_context, "rowCount"));
            _logger.LogDebug($"startRow={startRow}");
            _logger.LogDebug($"rowCount={rowCount}");

            // Do it.
            if (!argsValid)
            {
                msg = "Cannot invoke Get user favourites.";
                errors.Add(
                    $"Missing an expected integer (greater than 0) argument: id. The value supplied was '{unparsedId}'.");
                code = HttpStatusCode.BadRequest;
            }
            else
            {

                var dr = await _dal.GetUserFavourites(
                    id, startRow, rowCount, _context.RequestAborted);
                if (dr == null)
                {
                    msg = $"Get user favourites did not return a result.";
                    code = HttpStatusCode.NotFound;
                }
                else
                {
                    msg = $"Get user favourites returned a result.";
                    apiVenues = dr.ConvertAll(
                        new Converter<DalVenue, ApiVenue>(
                            (dv) => { return _dataMapper.MapToApiVenue(dv); }));
                    code = HttpStatusCode.OK;
                }
            }

            // Result.
            Dto.ApiVenuesResponse ar = _dataMapper.MapToApiVenuesResponse(
                _dataMapper.MapToApiStatus(code, msg, errors), apiVenues);
            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug(
                $"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;
        }


        /// <summary>
        /// Tries to invoke a 'soft undelete' of the user 
        /// with an Id matching the expected <see cref="HttpContext"/> route 
        /// value of 'id'.
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiAffectedRowsResponse"/>.</returns>
        public async Task<Dto.ApiAffectedRowsResponse> Activate()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Set user to Active.
            var r = await SetActiveState(true);

            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return r;
        }

        /// <summary>
        /// Tries to invoke a 'soft delete' of the user 
        /// with an Id matching the expected <see cref="HttpContext"/> route 
        /// value of 'id'.
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type 
        /// <see cref="Dto.ApiAffectedRowsResponse"/>.</returns>
        public async Task<Dto.ApiAffectedRowsResponse> Deactivate()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Set user to Inactive.
            var r = await SetActiveState(false);

            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return r;
        }

        // Helper invoked by Activate and Deactivate. Tries to set the active
        // state of the user with an Id matching the expected route value of
        // 'id', to IsActive.
        private async Task<Dto.ApiAffectedRowsResponse> SetActiveState(
            bool IsActive)
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            Dto.ApiAffectedRows apiAffectedRows = null;
            List<string> errors = new();
            // publicActionName is intended as a public-facing descriptor.
            // (MethodName() wouldn't be right in this case.)
            var publicActionName = (IsActive ? "Activate" : "Deactivate");

            // Validate expected inputs.
            var unparsedId = (string)_context.GetRouteValue("id");
            bool argsValid = _inputParser.TryParsePositiveInt(unparsedId, 
                out int id);

            _logger.LogDebug($"unparsedId={unparsedId}");
            _logger.LogDebug($"id={id}");

            // Do it.
            if (!argsValid)
            {
                msg = $"Cannot invoke {publicActionName} user.";
                errors.Add(
                    $"Missing an expected integer (greater than 0) argument: id. The value supplied was '{unparsedId}'.");
                code = HttpStatusCode.BadRequest;
            }
            else
            {
                // Returns rows affected. Should be 0 or 1, unless unique user
                // constraint has been broken.
                DalAffectedRows dr;
                if (IsActive)
                    dr = await _dal.ActivateUser(id, _context.RequestAborted);
                else
                    dr = await _dal.DeactivateUser(id, _context.RequestAborted);

                if (dr.Count == 0)
                {
                    msg = $"{publicActionName} user did nothing. Could not find user.";
                    code = HttpStatusCode.NotFound;
                }
                else
                {
                    msg = $"{publicActionName} user succeeded.";
                    code = HttpStatusCode.OK;
                }
                apiAffectedRows = _dataMapper.MapToApiAffectedRows(dr);
            }

            // Result.
            Dto.ApiAffectedRowsResponse ar = _dataMapper
                .MapToApiAffectedRowsResponse(
                _dataMapper.MapToApiStatus(code, msg, errors), apiAffectedRows);
            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug(
                $"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;
        }

        /// <summary>
        /// Tries to add (register) a user from an 
        /// <see cref="Dto.ApiRegisterUser"/> request.
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiAffectedIdResponse"/>.</returns>
        public async Task<Dto.ApiAffectedIdResponse> Add()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            Dto.ApiAffectedId apiAffectedId = null;
            List<string> errors = null;

            Dto.ApiRegisterUser request = null;
            try
            {
                request = await _rrHandler
                    .GetRequest<Dto.ApiRegisterUser>(_context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    $"GetRequest failed when expecting a <{nameof(Dto.ApiRegisterUser)}> in the body. The request and/or its JSON content may be incorrectly formed.");
            }
            _logger.LogDebug($"request={request}");

            if (request == null)
            {
                msg = "Wonky register user request received. Check the request method and body.";
                code = HttpStatusCode.BadRequest;
            }
            else
            {
                // Check our incoming user registration data.
                errors = FindErrors(request);
                if (errors.Count > 0)
                {
                    msg = "Wonky info received.";
                    code = HttpStatusCode.BadRequest;
                }
                else
                {
                    // User registration data is ok, attempt the registration.
                    
                    // Get a hash of the user's password first.
                    var hashedPassword = _hashingService
                        .GenerateHashBundleFromValue(request.Password);
                    // Add the user with a default role and status, and the
                    // password we hashed.
                    var dr = await _dal.AddUser(request.Username,
                                                request.Name,
                                                request.Surname,
                                                request.Email,
                                                passwordHash: hashedPassword,
                                                User.Role.User,
                                                User.AccountStatus.Active,
                                                _context.RequestAborted);
                    if (dr.DalResult.Code == (int)Result.AddUserResult.OK)
                    {
                        msg = "User registered.";
                        code = HttpStatusCode.Created;
                        apiAffectedId = _dataMapper.MapToApiAffectedId(
                            dr.DalAffectedId);
                    }
                    else if (dr.DalResult.Code 
                        == (int)Result.AddUserResult.ErrorDuplicateEmail 
                        || dr.DalResult.Code 
                        == (int)Result.AddUserResult.ErrorDuplicateUsername)
                    {
                        // Note: This is BadRequest in Node.js version of this
                        // project.
                        msg = "User is already registered.";
                        code = HttpStatusCode.UnprocessableEntity;
                    }

                }
            }
            // Result.
            var ar = new ApiAffectedIdResponse() { ApiStatus = _dataMapper
                .MapToApiStatus(
                code, msg, errors), ApiAffectedId = apiAffectedId };
            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug(
                $"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;

            // Returns a List of errors in the given argument. If there are no
            // errors, that List will have a Count of zero.
            List<string> FindErrors(Dto.ApiRegisterUser request)
            {
                // List of all errors found.
                List<string> errors = new();

                // Illustrative errors of the kind we could pick up on.

                if (request == null)
                {
                    errors.Add(
                        "Missing user registration info. The request is empty (null).");
                    return errors;
                }

                // Mandatory fields.
                List<string> missing = new();
                if (string.IsNullOrWhiteSpace(request.Username))
                    missing.Add("Username");
                if (string.IsNullOrWhiteSpace(request.Email))
                    missing.Add("Email");
                if (string.IsNullOrWhiteSpace(request.Password))
                    missing.Add("Password");
                if (missing != null && missing.Count > 0)
                    errors.Add(
                        $"Missing needed info: {string.Join(", ", missing)}.");

                // We won't validate further if mandatory info is missing.
                if (errors.Count > 0)
                    return errors;


                // Email validation.

                // NOTE: Complex regexes tend to eliminate valid (per spec)
                // emails, so something like below, coupled with a process to
                // actually send an email (user clicks link) is probably not far
                // from a good solution.                    
                if (!(request.Email.Contains("@") && request.Email.Contains(".")))
                    errors.Add("Email doesn't look right.");

                // Username validation.
                int configMinUserNameLength = _settings.MinUsernameLength;
                if (request.Username.Length < configMinUserNameLength)
                    errors.Add("Username must be longer. At least 3 in length.");

                // Password validation.
                int configMinPasswordLength = _settings.MinPasswordLength;
                if (request.Password.Length < configMinPasswordLength)
                    errors.Add("Password must be longer. At least 8 in length.");

                return errors;
            }

        }


        /// <summary>
        /// Tries to add a venue to a user's favourites where the Ids of those
        /// entities respectively match the expected <see cref="HttpContext"/> 
        /// route values of 'id' and 'venueId'.
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiAffectedIdResponse"/>.</returns>
        public async Task<Dto.ApiAffectedIdResponse> AddFavourite()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            Dto.ApiAffectedId apiAffectedId = null;
            List<string> errors = new();

            // Validate expected inputs.
            var unparsedId = (string)_context.GetRouteValue("id");
            var unparsedVenueId = (string)_context.GetRouteValue("venueId");
            bool arg1Valid = _inputParser
                .TryParsePositiveInt(unparsedId, out int id);
            bool arg2Valid = _inputParser
                .TryParsePositiveInt(unparsedVenueId, out int venueId);
            _logger.LogDebug($"unparsedId={unparsedId}");
            _logger.LogDebug($"unparsedVenueId={unparsedVenueId}");
            _logger.LogDebug($"id={id}");
            _logger.LogDebug($"venueId={venueId}");

            // Do it.
            if (!(arg1Valid && arg2Valid))
            {
                msg = $"Cannot invoke Add favourite.";
                if (!arg1Valid)
                    errors.Add(
                        $"Missing an expected integer (greater than 0) argument: id. The value supplied was '{unparsedId}'.");
                if (!arg2Valid)
                    errors.Add(
                        $"Missing an expected integer (greater than 0) argument: venueId. The value supplied was '{unparsedVenueId}'.");
                code = HttpStatusCode.BadRequest;
            }
            else
            {
                var dr = await _dal.AddUserFavourite(
                    id, venueId, _context.RequestAborted);
                if (dr.DalResult.Code == (int)Result.AddUserFavouriteResult.OK)
                {
                    msg = $"Favourite added.";
                    code = HttpStatusCode.OK;
                }
                else if (dr.DalResult.Code 
                    == (int)Result.AddUserFavouriteResult.ErrorDuplicate)
                {
                    msg = $"Favourite already exists.";
                    code = HttpStatusCode.OK;
                }
                else if (dr.DalResult.Code 
                    == (int)Result.AddUserFavouriteResult.ErrorInexistentUser 
                    || dr.DalResult.Code 
                    == (int)Result.AddUserFavouriteResult.ErrorInexistentVenue)
                {
                    msg = $"Cannot add favourite. User and/or venue does not exist.";
                    code = HttpStatusCode.UnprocessableEntity;
                }
                else
                    throw new InvalidOperationException(
                        $"Dal.AddUserFavourite returned an unexpected code: {dr.DalResult.Code}");

                apiAffectedId = _dataMapper.MapToApiAffectedId(dr.DalAffectedId);
            }

            // Result.
            Dto.ApiAffectedIdResponse ar = _dataMapper
                .MapToApiAffectedIdResponse(_dataMapper
                .MapToApiStatus(code, msg, errors), apiAffectedId);
            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug($"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;
        }

        /// <summary>
        /// Tries to remove a venue from a user's favourites where the Ids of those
        /// entities respectively match the expected <see cref="HttpContext"/> 
        /// route values of 'id' and 'venueId'.
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiAffectedIdResponse"/>.</returns>
        public async Task<Dto.ApiAffectedIdResponse> RemoveFavourite()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            Dto.ApiAffectedId apiAffectedId = null;
            List<string> errors = new();

            // Validate expected inputs.
            var unparsedId = (string)_context.GetRouteValue("id");
            var unparsedVenueId = (string)_context.GetRouteValue("venueId");
            bool arg1Valid = _inputParser.TryParsePositiveInt(unparsedId, 
                out int id);
            bool arg2Valid = _inputParser.TryParsePositiveInt(unparsedVenueId, 
                out int venueId);
            _logger.LogDebug($"unparsedId={unparsedId}");
            _logger.LogDebug($"unparsedVenueId={unparsedVenueId}");
            _logger.LogDebug($"id={id}");
            _logger.LogDebug($"venueId={venueId}");

            // Do it.
            if (!(arg1Valid && arg2Valid))
            {
                msg = $"Cannot invoke Remove favourite.";
                if (!arg1Valid)
                    errors.Add(
                        $"Missing an expected integer (greater than 0) argument: id. The value supplied was '{unparsedId}'.");
                if (!arg2Valid)
                    errors.Add(
                        $"Missing an expected integer (greater than 0) argument: venueId. The value supplied was '{unparsedVenueId}'.");
                code = HttpStatusCode.BadRequest;
            }
            else
            {
                var dr = await _dal.RemoveUserFavourite(
                    id, venueId, _context.RequestAborted);
                if (dr.DalResult.Code 
                    == (int)Result.RemoveUserFavouriteResult.OK)
                    msg = $"Favourite removed.";
                else if (dr.DalResult.Code 
                    == (int)Result.RemoveUserFavouriteResult.ErrorInexistent)
                    msg = $"Favourite does not exist.";
                else
                    throw new InvalidOperationException(
                        $"Dal.RemoveUserFavourite returned an unexpected code: {dr.DalResult.Code}");

                code = HttpStatusCode.OK;
                apiAffectedId = _dataMapper.MapToApiAffectedId(dr.DalAffectedId);
            }

            // Result.
            Dto.ApiAffectedIdResponse ar = _dataMapper
                .MapToApiAffectedIdResponse(_dataMapper
                .MapToApiStatus(code, msg, errors), apiAffectedId);
            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug($"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;
        }

        /// <summary>
        /// Tries to authenticate a user from an 
        /// <see cref="Dto.ApiUserLogin"/> request.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiUserLoginResponse"/>.
        /// </returns>
        public async Task<Dto.ApiUserLoginResponse> Authenticate()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            ApiSecurityToken st = null;
            List<string> errors = null;

            Dto.ApiUserLogin request = null;
            try
            {
                request = await _rrHandler.GetRequest<Dto.ApiUserLogin>(_context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    $"GetRequest failed when expecting a <{nameof(Dto.ApiUserLogin)}> in the body. The request and/or its JSON content may be incorrectly formed.");
            }
            _logger.LogDebug($"request={request}");

            if (request == null)
            {
                msg = "Wonky login request received. Check the request method and body.";
                code = HttpStatusCode.BadRequest;
            }
            else
            {
                // Check our incoming user registration data.
                errors = FindErrors(request);
                if (errors.Count > 0)
                {
                    msg = "Wonky info received.";
                    code = HttpStatusCode.BadRequest;
                }
                else
                {
                    st = await _authenticationService
                        .AuthenticateUser(
                        request.UsernameOrEmail, 
                        request.Password,
                        _context.RequestAborted);
                    if (st == null) 
                    {
                        msg = "Authentication failed.";
                        code = HttpStatusCode.Unauthorized;
                    }
                    else
                    {
                        msg = "Authentication succeeded.";
                        code = HttpStatusCode.OK;
                    }
                }
            }

            // Result.
            Dto.ApiUserLoginResponse ar = _dataMapper
                .MapToApiUserLoginResponse(_dataMapper
                .MapToApiStatus(code, msg, errors), st);
            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug($"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;

            // Returns a List of errors in the given argument. If there are no
            // errors, that List will have a Count of zero.
            static List<string> FindErrors(Dto.ApiUserLogin request)
            {
                // List of all errors found.
                List<string> errors = new();

                // Illustrative errors of the kind we could pick up on.
                if (request == null)
                {
                    errors.Add(
                        "Missing user login info. The request is empty (null).");
                    return errors;
                }

                // Mandatory fields.
                List<string> missing = new();
                if (string.IsNullOrWhiteSpace(request.UsernameOrEmail))
                    missing.Add("UsernameOrEmail");
                if (string.IsNullOrWhiteSpace(request.Password))
                    missing.Add("Password");
                if (missing != null && missing.Count > 0)
                    errors.Add($"Missing needed info: {string.Join(", ", missing)}.");

                return errors;
            }
        }

        /// <summary>
        /// Parses settings for this component. 
        /// </summary>
        /// <remarks>
        /// Throws if any settings are invalid.
        /// </remarks>
        /// <param name="unparsed"></param>
        /// <returns>Parsed <see cref="UserRegistrationSettings"/>.</returns>
        public UserRegistrationSettings ParseSettings(UserRegistrationSettings unparsed)
        {
            if (unparsed.MinUsernameLength <= 0)
                throw new ArgumentException(
                    $"Invalid {nameof(unparsed.MinUsernameLength)} in settings.");

            if (unparsed.MinPasswordLength <= 0)
                throw new ArgumentException(
                    $"Invalid {nameof(unparsed.MinPasswordLength)} in settings.");

            // Passing the preceding checks means we're parsed.
            return new UserRegistrationSettings
            {
                MinUsernameLength = unparsed.MinUsernameLength,
                MinPasswordLength = unparsed.MinPasswordLength,
            };
        }
    }
}
