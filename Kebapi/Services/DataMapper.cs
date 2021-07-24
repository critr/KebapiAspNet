using Kebapi.Dto;
using System.Collections.Generic;
using System.Net;

namespace Kebapi.Services
{
    /// <summary>
    /// Data mapping functions. 
    /// </summary>
    // Slicing is a bit open at this scale, so keeping all in 1 file and in 1 place,
    // and for least bloat going with straight MapToXXX methods.
    // 
    // Some alternative mapping options:
    // . Make DTO factory classes that take one object and return another.
    // . Go with C# implicit or explicit conversion operators.
    // . Use a third-party mapper. (AutoMapper not really designed for immutable
    // classes.)
    //
    // To aid any future direction, tagging methods as follows:
    // #DataComposer    - Aggregates various inputs into another structure.
    // #Dal-to-Api      - A mapping between Dal and Api layers.
    // #Root            - A primitive low-level mapping, such as an affected id
    //                    or row count.
    // #Filter          - Place to filter data between layers, e.g. a user's
    //                    password hash probably shouldn't make its way to a
    //                    front-end. Mostly will apply to anything #Dal-to-Api.
    //
    // (Also opting for verbose and clear so purposely avoiding overloads that
    // would lead to this sort of thing:
    //      var apiId     = MapToSomething(new DalAffectedId());
    //      var apiStatus = MapToSomething(
    //                          HttpStatusCode.OK, "", new List<string>());  .)
    public class DataMapper
    {

        // API-wide mappings

        // Scope: global. #DataComposer
        public ApiStatus MapToApiStatus(HttpStatusCode code, string msg,
            List<string> errors)
        {
            return new ApiStatus { 
                StatusCode = code, Message = msg, Errors = errors };
        }

        // Scope: global. Currently only used in Users. #Root, #Dal-to-Api
        // For any op affecting an Id. nothing really happens in it, but could
        // at a future point.
        public ApiAffectedId MapToApiAffectedId(DalAffectedId o)
        {
            return new ApiAffectedId() { Value = o.Value };
        }
        // Scope: global. #Root, #Dal-to-Api
        // For any op affecting rows. Nothing really happens in it, but could
        // at a future point.
        public ApiAffectedRows MapToApiAffectedRows(DalAffectedRows o)
        {
            return new ApiAffectedRows() { Count = o.Count };
        }

        // Scope: global. #DataComposer
        public ApiAffectedIdResponse MapToApiAffectedIdResponse(
            ApiStatus apiStatus, ApiAffectedId apiAffectedId)
        {
            return new ApiAffectedIdResponse() { 
                ApiStatus = apiStatus, ApiAffectedId = apiAffectedId };
        }
        // Scope: global. #DataComposer
        public ApiAffectedRowsResponse MapToApiAffectedRowsResponse(
            ApiStatus apiStatus, ApiAffectedRows apiAffectedRows)
        {
            return new ApiAffectedRowsResponse() { 
                ApiStatus = apiStatus, ApiAffectedRows = apiAffectedRows };
        }

        // User-related mappings

        // Scope: Users. #Dal-to-Api, #Filter
        public ApiUser MapToApiUser(DalUser o)
        {
            return new ApiUser() {
                Id = o.Id, Email = o.Email, Name = o.Name, Surname = o.Surname,
                Username = o.Username };
        }

        // Scope: Users. #Dal-to-Api, #Filter
        public ApiUserAccountStatus MapToApiUserAccountStatus(
            DalUserAccountStatus o)
        {
            return new ApiUserAccountStatus() { Id = o.Id, Status = o.Status };
        }

        // Scope: Users. #DataComposer
        public ApiUserLoginResponse MapToApiUserLoginResponse(
            ApiStatus apiStatus, ApiSecurityToken token) 
        {
            return new ApiUserLoginResponse() {
                ApiStatus = apiStatus, ApiSecurityToken = token };
        }

        // Scope: Users. #DataComposer
        public ApiUserResponse MapToApiUserResponse(
            ApiStatus apiStatus, ApiUser apiUser)
        {
            return new ApiUserResponse() { 
                ApiStatus = apiStatus, ApiUser = apiUser };
        }

        // Scope: Users. #DataComposer
        public ApiUsersResponse MapToApiUsersResponse(
            ApiStatus apiStatus, List<ApiUser> apiUs)
        {
            return new ApiUsersResponse() { 
                ApiStatus = apiStatus, ApiUser = apiUs };
        }
        // Scope: Users. #DataComposer
        public ApiUserAccountStatusResponse MapToApiUserAccountStatusResponse(
            ApiStatus apiStatus, ApiUserAccountStatus apiUas)
        {
            return new ApiUserAccountStatusResponse() {
                ApiStatus = apiStatus, ApiUserAccountStatus = apiUas };
        }

        // Venue-related mappings

        // Scope: Venues, Users. #Dal-to-Api, #Filter
        public ApiVenue MapToApiVenue(DalVenue o)
        {
            return new ApiVenue() { 
                Id = o.Id, Name = o.Name, GeoLat = o.GeoLat, GeoLng = o.GeoLng,
                Address = o.Address, Rating = o.Rating,
                MainMediaPath = o.MainMediaPath };
        }
        // Scope: Venues. #DataComposer
        public ApiVenueResponse MapToApiVenueResponse(
            ApiStatus apiStatus, ApiVenue apiV)
        {
            return new ApiVenueResponse() {
                ApiStatus = apiStatus, ApiVenue = apiV };
        }

        // Scope: Venues, Users. #DataComposer
        public ApiVenuesResponse MapToApiVenuesResponse(
            ApiStatus apiStatus, List<ApiVenue> apiVs)
        {
            return new ApiVenuesResponse() { 
                ApiStatus = apiStatus, ApiVenue = apiVs };
        }

        // Scope: Venues. #Dal-to-Api, #Filter
        public ApiVenueDistance MapToApiVenueDistance(DalVenueDistance o)
        {
            return new ApiVenueDistance()
            {
                Id = o.Id,
                Name = o.Name,
                Rating = o.Rating,
                MainMediaPath = o.MainMediaPath,
                DistanceInMetres = o.DistanceInMetres,
                DistanceInKilometres = o.DistanceInKilometres,
                DistanceInMiles = o.DistanceInMiles
            };
        }
        // Scope: Venues. #DataComposer
        public ApiVenueDistanceResponse MapToApiVenueDistanceResponse(
            ApiStatus apiStatus, ApiVenueDistance apiVenueDistance) 
        {
            return new ApiVenueDistanceResponse() { ApiStatus = apiStatus, 
                ApiVenueDistance = apiVenueDistance };
        }

    }
}
