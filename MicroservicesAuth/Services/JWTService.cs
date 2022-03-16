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
        bool RevokeToken(string token, string ipAddress);
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
            var currentTokenModel =  GetRefreshTokenAsync(token);
            if(currentTokenModel is not null)
            {
                if (RefreshTokenIsValid(currentTokenModel))
                {
                    var newRefreshToken = CreateRefreshTokenModel(currentTokenModel.ByUserId);
                    await SaveRefreshTokenAsync(newRefreshToken);
                    await DeleteRefreshTokenAsync(currentTokenModel);
                    return newRefreshToken;
                }
            }
            return null;
        }

        public bool RevokeToken(string token, string ipAddress)
        {

            throw new NotImplementedException();
        }

        private IEnumerable<Claim> GenerateTokenClaims(UserWithRoles userWithRoles)
        {
            TokenClaims tokenClaims = new(userWithRoles.User.Id, userWithRoles.User.UserName, userWithRoles.Roles);
            List<Claim> claims = new();
            claims.Add(new Claim("Id", tokenClaims.Id));
            claims.Add(new Claim("Username", tokenClaims.Username));
            foreach (var role in tokenClaims.Roles)
            {
                claims.Add(new Claim("Role", role));
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

        private RefreshTokenModel GetRefreshTokenAsync(string token)
        {
            return _authContext.RefreshTokens.SingleOrDefault(t => t.Token == token);
        }

        private async Task<bool> DeleteRefreshTokenAsync(RefreshTokenModel token)
        {
            _authContext.RefreshTokens.Remove(token);
            int value = await _authContext.SaveChangesAsync();
            return value > 0;
        }

    }
}
