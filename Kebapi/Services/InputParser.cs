using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;

namespace Kebapi.Services
{
    /// <summary>
    /// Parses various inputs to domain-wide functions.
    /// </summary>
    public class InputParser : IParsableSettings<PagingSettings>
    {
        private readonly PagingSettings _pagingSettings;

        public InputParser(PagingSettings ps) 
        {
            // Config settings used by this class.
            _pagingSettings = ParseSettings(ps);
        }

        // A start row and row count are used for paging results.
        // Here we make sure those values are bounded.
        /// <summary>
        /// Ensures start row is within configured limits.
        /// </summary>
        /// <remarks>
        /// A start row and row count are used for paging results.
        /// </remarks>
        /// <param name="startRow"></param>
        /// <returns></returns>
        public int ParseStartRow(int startRow)
        {
            // If less than the configured minimum, return the configured minimum.
            return startRow < _pagingSettings.MinStartRow 
                ? _pagingSettings.MinStartRow 
                : startRow;
        }
        /// <summary>
        /// Ensures row count is within configured limits.
        /// </summary>
        /// <remarks>
        /// A start row and row count are used for paging results.
        /// </remarks>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        public int ParseRowCount(int rowCount)
        {
            // If outside the configured minimum and maximum, return the configured
            // maximum.
            return (rowCount < _pagingSettings.MinRowCount || rowCount > _pagingSettings.MaxRowCount) 
                ? _pagingSettings.MaxRowCount 
                : rowCount;
        }

        /// <summary>
        /// If s can be converted to a positive integer, returns true
        /// together with the result in the out parameter, false otherwise.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="result"></param>
        /// <returns></returns>
#nullable enable
        public bool TryParsePositiveInt(string? s, out int result)
#nullable restore
        {
            if (int.TryParse(s, out result) && result > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// If s can be parsed, returns true together with the result in the out 
        /// parameter, false otherwise.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        // Intended as a place to apply any general sanitisation, stop lists, 
        // parsing, etc. for string inputs to the API.
#nullable enable
        public bool TryParseString(string? s, out string result)
#nullable restore
        {
            // Doing a bare minimum for now.
            result = (s ?? "").Trim();
            return true;
        }


        // Some query var helpers when handling http requests.
        // e.g. www.example.com/search?somevartoget=3&anothervartoget=hi
        /// <summary>
        /// Looks for a query variable named varName in the request of the HttpContext 
        /// and tries to get it as a StringValues. If that is successful, returns 
        /// the StringValues. In all other cases, returns StringValues.Empty. (The 
        /// latter incudes varName not found and cannot be obtained as StringValues.)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="varName"></param>
        /// <returns></returns>
        public StringValues GetQueryVar(HttpContext context, string varName)
        {
            StringValues stringVal = StringValues.Empty;
            if (context.Request.Query.ContainsKey(varName))
            {
                context.Request.Query.TryGetValue(varName, out stringVal);
            }
            return stringVal;
        }
        // Note: The *As* flavours of these helpers deliberately ignore the edge
        // case that appears to be the reason for StringValues having been used 
        // as the out value in
        //      IQueryCollection.TryGetValue(string key,
        //           out Microsoft.Extensions.Primitives.StringValues value).
        // That reason being that the following is perfectly valid although seldom
        // seen out in the wild: /page?color=blue&color=red&color=yellow.
        // Those helpers instead assume and treat any StringValue as string.
        /// <summary>
        /// Looks for a query variable named varName in the request of the HttpContext 
        /// and tries to convert it to an integer. If that is successful, returns 
        /// the integer value. In all other cases, returns 0. (The latter incudes 
        /// varName not found and cannot be converted.)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="varName"></param>
        /// <returns></returns>
        public int GetQueryVarAsInt(HttpContext context, string varName)
        {
            int intVal = 0;
            StringValues stringVal = GetQueryVar(context, varName);
            if (int.TryParse(stringVal.ToString(), out int res))
            {
                intVal = res;
            }
            return intVal;
        }
        // A cardinal number is a whole number >= 0.
        /// <summary>
        /// Looks for a query variable named varName in the request of the HttpContext 
        /// and tries to convert it to a cardinal. If that is successful, returns 
        /// the cardinal value. In all other cases, returns 0. (The latter incudes 
        /// varName not found and cannot be converted.)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="varName"></param>
        /// <returns></returns>
        public int GetQueryVarAsCardinal(HttpContext context, string varName)
        {
            return Math.Abs(GetQueryVarAsInt(context, varName));
        }

        /// <summary>
        /// Parses settings for this component. 
        /// </summary>
        /// <remarks>
        /// Throws if any settings are invalid.
        /// </remarks>
        /// <param name="unparsed"></param>
        /// <returns>Parsed <see cref="PagingSettings"/>.</returns>
        public PagingSettings ParseSettings(PagingSettings unparsed)
        {
            if (unparsed.MinStartRow < 0)
                throw new ArgumentException($"Invalid {nameof(unparsed.MinStartRow)} in settings.");

            if (unparsed.MinRowCount < 0)
                throw new ArgumentException($"Invalid {nameof(unparsed.MinRowCount)} in settings.");

            if (unparsed.MaxRowCount < 0)
                throw new ArgumentException($"Invalid {nameof(unparsed.MaxRowCount)} in settings.");

            // Passing the preceding checks means we're parsed.
            return new PagingSettings
            {
                MinStartRow = unparsed.MinStartRow,
                MinRowCount = unparsed.MinRowCount,
                MaxRowCount = unparsed.MaxRowCount,
            };            
        }
    }
}
