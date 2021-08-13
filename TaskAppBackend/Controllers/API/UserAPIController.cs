using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TaskAppBackend.Models;
using TaskAppBackend.Data;
using TaskAppBackend.Helpers;
using BC = BCrypt.Net.BCrypt;

namespace TaskAppBackend.Controllers.API
{
    [AllowAnonymous]
    [RoutePrefix("api/UserApi")]
    public class UserAPIController : ApiController
    {

        private TaskAppBackendContext db = new TaskAppBackendContext();

        [ResponseType(typeof(User))]
        [HttpPost]
        [Route("Create")]
        public IHttpActionResult CreateUser(User user)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            if(DataBaseHelper.IsExistUser(db, user.Email))
                return BadRequest("Hay otro usuario con el mismo correo.");

            user.Password = BC.HashPassword(user.Password);

            try
            {
                db.Users.Add(user);
                db.SaveChanges();

                return Ok(user);
            }
            catch (Exception e)
            {
                return BadRequest($"Hubo un error, {e.Message}");
            }

        }

        [HttpPost]
        [Route("Login")]
        [ResponseType(typeof(Login))]
        public IHttpActionResult LoginUser(Login login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = db.Users.SingleOrDefault(u => u.Email == login.Email);

            if(user == null || !BC.Verify(login.Password, user.Password))
               return Unauthorized();

            var token = TokenGenerator.GenerateTokenJwt(user.Email);
            return Ok(token);
        }

    }
}
