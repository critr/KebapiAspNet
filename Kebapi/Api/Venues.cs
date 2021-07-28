using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using static Kebapi.LoggingHelper;
using Kebapi.Dto;
using Kebapi.DataAccess;
using Kebapi.Services;

namespace Kebapi.Api
{
    /// <summary>
    /// Defines API functionality relating to venues. 
    /// </summary>
    public class Venues : IVenues
    {
        private readonly IDal _dal;
        private readonly HttpContext _context;
        private readonly ILogger<IVenues> _logger;
        private readonly string _componentName;
        private readonly IRequestResponseHandler _rrHandler;
        private readonly InputParser _inputParser;
        private readonly DataMapper _dataMapper;

        public Venues(IDal dal, IHttpContextAccessor contextAccessor,
            ILogger<IVenues> logger, IRequestResponseHandler rrh, InputParser ip, DataMapper dm)
        {
            _dal = dal;
            _context = contextAccessor.HttpContext;
            _logger = logger;
            _componentName = GetType().Name;
            _rrHandler = rrh;
            _inputParser = ip;
            _dataMapper = dm;
        }

        /// <summary>
        /// Tries to get an <see cref="Dto.ApiVenue"/> for the venue with an Id 
        /// matching the expected <see cref="HttpContext"/> route value of 'id'.
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiVenueResponse"/>.</returns>
        public async Task<Dto.ApiVenueResponse> Get()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            Dto.ApiVenue apiVenue = null;
            List<string> errors = new();

            // Validate expected inputs.
            var unparsedId = (string)_context.GetRouteValue("id");
            bool argsValid = _inputParser.TryParsePositiveInt(unparsedId, out int id);

            _logger.LogDebug($"unparsedId={unparsedId}");
            _logger.LogDebug($"id={id}");

            // Do it.
            if (!argsValid)
            {
                msg = "Cannot invoke Get venue.";
                errors.Add($"Missing an expected integer (greater than 0) argument: id. The value supplied was '{unparsedId}'.");
                code = HttpStatusCode.BadRequest;
            }
            else
            {
                var dr = await _dal.GetVenue(id, _context.RequestAborted);
                if (dr == null)
                {
                    msg = $"Get venue did not return a result.";
                    code = HttpStatusCode.NotFound;
                }
                else
                {
                    msg = $"Get venue returned a result.";
                    apiVenue = _dataMapper.MapToApiVenue(dr);
                    code = HttpStatusCode.OK;
                }
            }

            // Result.
            Dto.ApiVenueResponse ar = _dataMapper.MapToApiVenueResponse(_dataMapper.MapToApiStatus(code, msg, errors), apiVenue);
            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug($"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;
        }

        /// <summary>
        /// Gets zero, one, or more, <see cref="Dto.ApiVenue"/>(s) dependent on 
        /// a) the venues present in the data set, and b) the expected 
        /// <see cref="HttpContext"/> query variables of 'startRow' (which row of
        /// the data set to start at) and 'rowCount' (how many rows of the 
        /// data set to return.)
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO type
        /// <see cref="Dto.ApiVenuesResponse"/>.</returns>
        public async Task<Dto.ApiVenuesResponse> GetSome()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            List<Dto.ApiVenue> apiVenues = null;
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
            var dr = await _dal.GetVenues(
                startRow, rowCount, _context.RequestAborted);
            if (dr == null)
            {
                msg = $"Get venues did not return a result.";
                code = HttpStatusCode.NotFound;
            }
            else
            {
                msg = $"Get venues returned a result.";
                apiVenues = dr.ConvertAll(new Converter<DalVenue, ApiVenue>(
                    (du) => { return _dataMapper.MapToApiVenue(du); }));
                code = HttpStatusCode.OK;
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
        /// Tries to get the total number of existing venues.
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
            var dr = await _dal.GetVenueCount(_context.RequestAborted);
            if (dr.DalResult.Code == (int)Result.GetVenueCountResult.OK)
            {
                msg = dr.DalResult.Message;
                code = HttpStatusCode.OK;
            }
            else
                throw new InvalidOperationException(
                    $"Dal.GetVenueCount returned an unexpected code: {dr.DalResult.Code}");

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
        /// Tries to get the distance to a venue from a given geographic point 
        /// expressed in latitude and longitude. The response contains an 
        /// <see cref="Dto.ApiVenueDistance"/> for the venue with an Id matching 
        /// the expected <see cref="HttpContext"/> route value of 'id', with 
        /// distances calculated from the latitude and longitude expressed in the
        /// query variables 'originLat' and 'originLng'.
        /// </summary>
        /// <returns>A <see cref="Task"/> of DTO types
        /// <see cref="Dto.ApiVenueDistanceResponse"/>.</returns>
        public async Task<Dto.ApiVenueDistanceResponse> GetDistance() 
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            Dto.ApiVenueDistance apiVenueDistance = null; 
            List<string> errors = new();

            // Validate expected inputs.
            var unparsedId = (string)_context.GetRouteValue("id");
            bool idValid = _inputParser.TryParsePositiveInt(unparsedId, 
                out int id);
            var unparsedOriginLat = _inputParser.GetQueryVar(_context, "originLat");
            bool latValid = _inputParser.TryParseLatitude(unparsedOriginLat, 
                out double originLat);
            var unparsedOriginLng = _inputParser.GetQueryVar(_context, "originLng");
            bool lngValid = _inputParser.TryParseLongitude(unparsedOriginLng, 
                out double originLng);

            _logger.LogDebug($"unparsedId={unparsedId}");
            _logger.LogDebug($"id={id}");
            _logger.LogDebug($"unparsedOriginLat={unparsedOriginLat}");
            _logger.LogDebug($"originLat={originLat}");
            _logger.LogDebug($"unparsedOriginLng={unparsedOriginLng}");
            _logger.LogDebug($"originLng={originLng}");

            if (!idValid) errors.Add(
                $"Missing an expected integer (greater than 0) argument: id. The value supplied was '{unparsedId}'.");
            if (!latValid) errors.Add(
                $"Missing an expected double (between -90 and 90) argument: originLat. The value supplied was '{unparsedOriginLat}'.");
            if (!lngValid) errors.Add(
                $"Missing an expected double (between -180 and 180) argument: originLng. The value supplied was '{unparsedOriginLng}'.");
            if (errors.Count > 0)
            {
                msg = "Cannot invoke Get distance to venue.";
                code = HttpStatusCode.BadRequest;
            }
            else
            {
                var dr = await _dal.GetVenueDistance(id, originLat, originLng, _context.RequestAborted);
                if (dr == null)
                {
                    msg = $"Get distance to venue did not return a result.";
                    code = HttpStatusCode.NotFound;
                }
                else
                {
                    msg = $"Get distance to venue returned a result.";
                    apiVenueDistance = _dataMapper.MapToApiVenueDistance(dr);
                    code = HttpStatusCode.OK;
                }
            }

            // Result.
            Dto.ApiVenueDistanceResponse ar = _dataMapper.MapToApiVenueDistanceResponse(_dataMapper.MapToApiStatus(code, msg, errors), apiVenueDistance);
            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug($"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;
        }

        public async Task<Dto.ApiVenuesNearbyResponse> GetNearby()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            // Taking pessimistic approach that assumes failure.
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string msg = null;
            List<Dto.ApiVenueDistance> apiVenueDistances = null;
            List<string> errors = new();

            // Validate expected inputs.
            var unparsedOriginLat = _inputParser.GetQueryVar(_context, "originLat");
            bool latValid = _inputParser.TryParseLatitude(unparsedOriginLat,
                out double originLat);
            var unparsedOriginLng = _inputParser.GetQueryVar(_context, "originLng");
            bool lngValid = _inputParser.TryParseLongitude(unparsedOriginLng,
                out double originLng);
            // The remaining are optional, always getting a default if invalid/not supplied.
            var unparsedWithinMetres = _inputParser.GetQueryVar(_context, "withinMetres");
            bool withinValid = _inputParser.TryParsePositiveDouble(unparsedWithinMetres, 
                out double withinMetres);
            if (!withinValid) withinMetres = 0; // 0 is ignore this constraint.
            int startRow = _inputParser.ParseStartRow(
                _inputParser.GetQueryVarAsCardinal(_context, "startRow"));
            int rowCount = _inputParser.ParseRowCount(
                _inputParser.GetQueryVarAsCardinal(_context, "rowCount"));

            _logger.LogDebug($"unparsedOriginLat={unparsedOriginLat}");
            _logger.LogDebug($"originLat={originLat}");
            _logger.LogDebug($"unparsedOriginLng={unparsedOriginLng}");
            _logger.LogDebug($"originLng={originLng}");
            _logger.LogDebug($"unparsedWithinMetres={unparsedWithinMetres}");
            _logger.LogDebug($"withinMetres={withinMetres}");
            _logger.LogDebug($"startRow={startRow}");
            _logger.LogDebug($"rowCount={rowCount}");

            if (!latValid) errors.Add(
                $"Missing an expected double (between -90 and 90) argument: originLat. The value supplied was '{unparsedOriginLat}'.");
            if (!lngValid) errors.Add(
                $"Missing an expected double (between -180 and 180) argument: originLng. The value supplied was '{unparsedOriginLng}'.");
            if (errors.Count > 0)
            {
                msg = "Cannot invoke Get venues nearby.";
                code = HttpStatusCode.BadRequest;
            }
            else
            {
                var dr = await _dal.GetVenuesNearby(originLat, originLng, 
                    withinMetres, startRow, rowCount, _context.RequestAborted);
                if (dr == null)
                {
                    msg = $"Get venues nearby did not return a result.";
                    code = HttpStatusCode.NotFound;
                }
                else
                {
                    msg = $"Get venues nearby returned a result.";
                    apiVenueDistances = dr.ConvertAll(new Converter<DalVenueDistance, ApiVenueDistance>(
                    (du) => { return _dataMapper.MapToApiVenueDistance(du); }));
                    code = HttpStatusCode.OK;
                }
            }

            // Result.
            Dto.ApiVenuesNearbyResponse ar = _dataMapper.MapToApiVenuesNearbyResponse(_dataMapper.MapToApiStatus(code, msg, errors), apiVenueDistances);
            await _rrHandler.SetResponse(_context, code, ar);

            _logger.LogDebug($"Response was: {JsonSerializer.Serialize(ar)}");
            _logger.LogDebug(ExitMsg(_componentName, MethodName()));

            return ar;
        }

    }
}
