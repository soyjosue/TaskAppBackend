using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Web;
using TaskAppBackend.Controllers.API;

namespace TaskAppBackend.Helpers
{
    public class Utils
    {
        public static string GetHeaderElement(string nameItem, HttpRequestMessage request)
        {
            string itemValue = "";
            foreach (var item in request.Headers)
            {
                if (item.Key.Equals(nameItem))
                {
                    itemValue = item.Value.First();
                    break;
                }
            }

            return itemValue;
        }

        public static UserToken GetUserOfToken(HttpRequestMessage request)
        {
            var token = Utils.GetHeaderElement(Literals.token_header, request);

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            var tokenEl = jsonToken as JwtSecurityToken;

            var userID = tokenEl.Claims.First(claim => claim.Type == "Id").Value;
            var userEmail = tokenEl.Claims.First(claim => claim.Type == "Email").Value;

            return new UserToken
            { 
                Id = Int32.Parse(userID),
                Email = userEmail
            };
        }
    }
}