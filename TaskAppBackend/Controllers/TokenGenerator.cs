using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace TaskAppBackend.Controllers
{
    internal static class TokenGenerator
    {
        public static string GenerateTokenJwt(string username, int id)
        {
            // Obteniendo las appsettings de token JWT
            var secretKey = ConfigurationManager.AppSettings["JWT_SECRET_KEY"];
            var audienceToken = ConfigurationManager.AppSettings["JWT_AUDIENCE_TOKEN"];
            var issuerToken = ConfigurationManager.AppSettings["JWT_ISSUER_TOKEN"];
            var expireTime = ConfigurationManager.AppSettings["JWT_EXPIRE_MINUTE"];

            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            // Creando el claimsIdentity
            ClaimsIdentity claimsIdentify = new ClaimsIdentity(new[] { new Claim("Email", username), new Claim("Id", id.ToString()) });

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.CreateJwtSecurityToken(
                audience : audienceToken,
                issuer : issuerToken,
                subject : claimsIdentify,
                notBefore : DateTime.UtcNow,
                expires : DateTime.UtcNow.AddMinutes(30),
                signingCredentials : signingCredentials
            );

            var jwtTokenString = tokenHandler.WriteToken(jwtSecurityToken);

            return jwtTokenString;

        }
    }
}