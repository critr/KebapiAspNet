using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Kebapi.Dto;

namespace Kebapi.Services.Token
{    
    /// <summary>
    /// Provides security tokens based on JWT (Jason Web Tokens).
    /// </summary>
    public class TokenService : IParsableSettings<TokenValidationSettings>
    {
        private const int DefaultExpireMinutes = 60;
        private readonly TokenValidationSettings _settings;

        public TokenService(TokenValidationSettings settings)
        {
            // Config settings used by this class.
            _settings = ParseSettings(settings);
        }

        /// <summary>
        /// Creates a JWT-based ApiSecurityToken for the specified user with a
        /// minimum set of Claims.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>A DTO <see cref="Dto.ApiSecurityToken"/></returns>
        // Note: Perhaps obvious, but Dto.DalUser here isn't a coupling to Dal,
        // it's a coupling to a DTO that we named such to reflect the "low-level"
        // nature of its data content. We can easily overload this method for
        // any other DTO containing the reqired data.
        public Dto.ApiSecurityToken CreateToken(Dto.DalUser user)
        {
            // A username and roles are required for ASP.NET Core's
            // authorization to work.
            var claims = new List<Claim>
                {
                    new Claim("username", user.Username),
                    new Claim("id", user.Id.ToString()),
                    new Claim("displayname", user.Name),
                };

            // Our API only supports 1 role per user at the mo, but
            // correct way is to add a claim for each role.
            //foreach (var role in user.Roles)
            //{
                claims.Add(new Claim(ClaimTypes.Role, user.Role));
            //}

            // Build the token with our helper class.
            var token = JwtHelper.GetJwtToken(
                username: user.Username,
                signingKey: _settings.SigningKey,
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                expiration: TimeSpan.FromMinutes(_settings.ExpireMinutes),
                additionalClaims: claims.ToArray()
                );

            // Compose our object containing the built token.
            return new Dto.ApiSecurityToken()
            {
                // This turns the token into a string so it can be used as a
                // bearer token.
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expires = token.ValidTo,
            };

        }

        /// <summary>
        /// Parses settings for this component. 
        /// </summary>
        /// <remarks>
        /// Throws if any settings are invalid.
        /// </remarks>
        /// <param name="unparsed"></param>
        /// <returns>Parsed <see cref="TokenValidationSettings"/>.</returns>
        public TokenValidationSettings ParseSettings(TokenValidationSettings unparsed)
        {
            var audience = unparsed.Audience;
            if (string.IsNullOrEmpty(audience))
                throw new ArgumentException(
                    $"{nameof(TokenService)}: A value for Audience is required. Check configuration.");

            var expireMinutes = unparsed.ExpireMinutes;
            // Allowing 0, which means immediate expiration. (Almost certainly
            // not useful in production, but could be very useful for tests.)
            if (expireMinutes < 0)
                expireMinutes = DefaultExpireMinutes;

            var issuer = unparsed.Issuer;
            if (string.IsNullOrEmpty(issuer))
                throw new ArgumentException(
                    $"{nameof(TokenService)}: A value for Issuer is required. Check configuration.");

            var signingKey = unparsed.SigningKey;
            if (string.IsNullOrEmpty(signingKey))
                throw new ArgumentException(
                    $"{nameof(TokenService)}: A value for SigningKey is required. Check Environment.");

            return new TokenValidationSettings()
            {
                Audience = audience,
                ExpireMinutes = expireMinutes,
                Issuer = issuer,
                SigningKey = signingKey
            };
        }

    }

}
