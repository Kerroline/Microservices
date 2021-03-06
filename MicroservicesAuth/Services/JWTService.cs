using Microsoft.IdentityModel.Tokens;
using MSAuth.DAL;
using MSAuth.Helpers;
using MSAuth.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MSAuth.Services
{
    public interface IJWTService
    {
        Task<AuthResponse> GeneratePairTokensAsync(UserWithRoles userRoles);

        string GenerateJWSToken(UserWithRoles userRoles);
        Task<RefreshTokenModel> RefreshTokenAsync(string token);
        Task<bool> RevokeToken(string token);
    }
    public class JWTService : IJWTService
    {
        private readonly MSAuthContext _authContext;
        private readonly IJwtSigningEncodingKey _signingSymmetricProvider;

        public JWTService(
            IJwtSigningEncodingKey _signingEncodingKey,
            MSAuthContext authContext)
        {
            this._signingSymmetricProvider = _signingEncodingKey;
            this._authContext = authContext;
        }


        public async Task<AuthResponse> GeneratePairTokensAsync(UserWithRoles userRoles)
        {
            var tokenClaims = GenerateTokenClaims(userRoles);
            string jwsToken = GenerateJWS(tokenClaims);
            var refreshToken = CreateRefreshTokenModel(userRoles.User.Id);

            await SaveRefreshTokenAsync(refreshToken);

            return new AuthResponse(jwsToken, refreshToken.Token);
        }

        public string GenerateJWSToken(UserWithRoles userRoles)
        {
            var tokenClaims = GenerateTokenClaims(userRoles);
            string jwsToken = GenerateJWS(tokenClaims);
            return jwsToken;
        }

        public async Task<RefreshTokenModel> RefreshTokenAsync(string token)
        {
            var currentTokenModel =  GetRefreshToken(token);
            if(currentTokenModel is not null)
            {
                if (RefreshTokenIsValid(currentTokenModel))
                {
                    var newRefreshToken = CreateRefreshTokenModel(currentTokenModel.ByUserId);
                    await DeleteAllTokensForUserAsync(newRefreshToken.ByUserId);
                    await SaveRefreshTokenAsync(newRefreshToken);
                    return newRefreshToken;
                }
            }
            return null;
        }

        public async Task<bool> RevokeToken(string token)
        {
            string userId = string.Empty;
            var refreshToken = GetRefreshToken(token);
            if (refreshToken is not null)
            {
                // this refresh token
                if (RefreshTokenIsValid(refreshToken))
                {
                    userId = refreshToken.ByUserId;
                }
            }
            else
            {
                //this access token
                var handler = new JwtSecurityTokenHandler();
                var jwsToken = handler.ReadJwtToken(token);
                
                var expUnixDate = jwsToken.Claims.Where(t => t.Type == "exp").SingleOrDefault().Value;

                var expDate = UnixTimeToDateTime(long.Parse(expUnixDate));

                if(expDate > DateTime.UtcNow)
                {
                    userId = jwsToken.Claims.Where(t => t.Type == "Id").SingleOrDefault().Value;
                } 
            }

            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }
            else
            {
                await DeleteAllTokensForUserAsync(userId);
                return true;
            }   
        }

        private IEnumerable<Claim> GenerateTokenClaims(UserWithRoles userWithRoles)
        {
            TokenClaims tokenClaims = new(userWithRoles.User.Id, userWithRoles.User.UserName, userWithRoles.Roles);
            List<Claim> claims = new();
            claims.Add(new Claim("Id", tokenClaims.Id));
            claims.Add(new Claim(ClaimsIdentity.DefaultNameClaimType, tokenClaims.Username));   
            foreach (var role in tokenClaims.Roles)
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));
            }
            return claims;
        }

        private string GenerateJWS(IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
                   issuer: "MSAuth",
                   audience: "MS_",
                   notBefore: DateTime.UtcNow,
                   claims: claims,
                   expires: DateTime.UtcNow.AddMinutes(15),
                   signingCredentials: new SigningCredentials(
                           _signingSymmetricProvider.GetKey(),
                           _signingSymmetricProvider.SigningAlgorithm)
               );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            return jwtToken;
        }

        private RefreshTokenModel CreateRefreshTokenModel(string userId)
        {
            var randomBytes = new byte[64];
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                
                rngCryptoServiceProvider.GetBytes(randomBytes);
            }
            var tokenModel = new RefreshTokenModel
            {
                Token = GenerateRefreshToken(),
                Expires = DateTime.UtcNow.AddDays(30),
                Created = DateTime.UtcNow,
                ByUserId = userId,
            };
            return tokenModel;
        }
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {

                rngCryptoServiceProvider.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
        private bool RefreshTokenIsValid (RefreshTokenModel model)
        {
            return DateTime.UtcNow < model.Expires;
        }

        private async Task<bool>  SaveRefreshTokenAsync(RefreshTokenModel model)
        {
            _authContext.RefreshTokens.Add(model);
            int value = await _authContext.SaveChangesAsync();
            return value > 0;
        }

        private RefreshTokenModel GetRefreshToken(string token)
        {
            return _authContext.RefreshTokens.SingleOrDefault(t => t.Token == token);
        }

        private async Task<bool> DeleteRefreshTokenAsync(RefreshTokenModel token)
        {
            _authContext.RefreshTokens.Remove(token);
            int value = await _authContext.SaveChangesAsync();
            return value > 0;
        }

        private async Task<bool> DeleteAllTokensForUserAsync(string userId) 
        {
            var tokenList = _authContext.RefreshTokens.Where(t => t.ByUserId == userId);

            _authContext.RemoveRange(tokenList);
            int value = await _authContext.SaveChangesAsync();
            return value > 0;
        }

        private DateTime UnixTimeToDateTime(long unixTime)
        {
            DateTime dtDateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime);
            return dtDateTime;
        }

    }
}
