using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Net;
using static Kebapi.LoggingHelper;
using Kebapi.DataAccess;
using Kebapi.Services;
using System;

namespace Kebapi.Api
{
    /// <summary>
    /// Defines API functionality relating to system admins.
    /// </summary>
    // Note: Unlike with Users and Venues, opting not to pipe results of these 
    // actions through our DataMapper since they aren't 'true' (meaning everyday
    // or typical) API calls.
    public class Admins : IAdmins
    {
        private readonly IDal _dal;
        private readonly HttpContext _context;
        private readonly ILogger<IAdmins> _logger;
        private readonly string _componentName;
        private readonly IRequestResponseHandler _rrHandler;

        public Admins(IDal dal, IHttpContextAccessor contextAccessor, 
            ILogger<IAdmins> logger, IRequestResponseHandler rrh) 
        {
            _dal = dal;
            _context = contextAccessor.HttpContext;
            _logger = logger;
            _componentName = GetType().Name;
            _rrHandler = rrh;
        }

        /// <summary>
        /// Creates an empty KebApi database if it doesn't already exist.
        /// </summary>
        /// <returns></returns>
        public async Task CreateDb()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            var code = HttpStatusCode.InternalServerError;
            string ar = null;

            try 
            {
                await _dal.CreateKebApiDatabase(_context.RequestAborted);
                code = HttpStatusCode.OK;
                ar = "Success.";
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing {MethodName()}.");
                ar = "Failed. Check the error log for details.";
            }

            await _rrHandler.SetResponse(_context, code, ar);
            _logger.LogDebug($"Response was: {ar}");

            _logger.LogDebug(ExitMsg(_componentName, MethodName()));
        }

        /// <summary>
        /// Drops the KebApi database if it exists.
        /// </summary>
        /// <returns></returns>
        public async Task DropDb()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            var code = HttpStatusCode.InternalServerError;
            string ar = null;

            try
            {
                await _dal.DropKebApiDatabase(_context.RequestAborted);
                code = HttpStatusCode.OK;
                ar = "Success.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing {MethodName()}.");
                ar = "Failed. Check the error log for details.";
            }

            await _rrHandler.SetResponse(_context, code, ar);
            _logger.LogDebug($"Response was: {ar}");

            _logger.LogDebug(ExitMsg(_componentName, MethodName()));
        }

        /// <summary>
        /// Resets a KebApi database, meaning in most cases* it will be dropped, 
        /// created, have its schema added. *If the database does not exist, it 
        /// may not be dropped, for example.
        /// </summary>
        /// <returns></returns>
        public async Task ResetDb()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            var code = HttpStatusCode.InternalServerError;
            string ar = null;

            try
            {
                await _dal.ResetKebApiDatabase(_context.RequestAborted);
                code = HttpStatusCode.OK;
                ar = "Success.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing {MethodName()}.");
                ar = "Failed. Check the error log for details.";
            }

            await _rrHandler.SetResponse(_context, code, ar);
            _logger.LogDebug($"Response was: {ar}");

            _logger.LogDebug(ExitMsg(_componentName, MethodName()));
        }

        /// <summary>
        /// Resets a KebApi database (equivalent to calling 
        /// <see cref="ResetDb"/>) followed by an insert of test data.
        /// </summary>
        /// <returns></returns>
        public async Task ResetTestDb()
        {
            _logger.LogDebug(EnterMsg(_componentName, MethodName()));

            var code = HttpStatusCode.InternalServerError;
            string ar = null;

            try
            {
                await _dal.ResetKebApiTestDatabase(_context.RequestAborted);
                code = HttpStatusCode.OK;
                ar = "Success.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing {MethodName()}.");
                ar = "Failed. Check the error log for details.";
            }

            await _rrHandler.SetResponse(_context, code, ar);
            _logger.LogDebug($"Response was: {ar}");

            _logger.LogDebug(ExitMsg(_componentName, MethodName()));            
        }


    }
}
