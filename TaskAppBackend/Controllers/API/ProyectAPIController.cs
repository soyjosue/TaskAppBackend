using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Description;
using TaskAppBackend.Data;
using TaskAppBackend.Helpers;
using TaskAppBackend.Models;

namespace TaskAppBackend.Controllers.API
{
    [Authorize]
    [RoutePrefix("api/ProyectApi")]
    public class ProyectAPIController : ApiController
    {
        private TaskAppBackendContext db = new TaskAppBackendContext();

        [HttpGet]
        public IHttpActionResult GetProyects(HttpRequestMessage request)
        {
            var userToken = Utils.GetUserOfToken(request);

            var proyects = (from proyect in db.Proyects
                            where proyect.UserId == userToken.Id
                            select proyect).ToList();

            return Ok(proyects);
        }

        [Route("Create")]
        [HttpPost]
        [ResponseType(typeof(Proyect))]
        public IHttpActionResult CreateProyect(Proyect proyect, HttpRequestMessage request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userToken = Utils.GetUserOfToken(request);

            if (!DataBaseHelper.IsExistUser(db, userToken.Email))
                return BadRequest("Usuario no existe");

            var user =  db.Users.FirstOrDefault(us => us.Id == userToken.Id);

            proyect.CreatedAt = DateTime.Now;
            proyect.UserId = user.Id;

            try
            {
                db.Proyects.Add(proyect);
                db.SaveChanges();

                return Ok(proyect);
            } catch(Exception err)
            {
                return BadRequest(err.Message);       
            }
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public IHttpActionResult DeleteProyect(int id, HttpRequestMessage request)
        {
            var userToken = Utils.GetUserOfToken(request);

            if (!DataBaseHelper.IsExistUser(db, userToken.Email))
                return BadRequest("El usuario no existe.");

            var proyect = db.Proyects.FirstOrDefault(pro => pro.Id == id);

            if (proyect == null)
                return NotFound();

            if (proyect.UserId != userToken.Id)
                return BadRequest("No tienes permiso para eliminar este proyecto");

            try
            {
                db.Proyects.Remove(proyect);
                db.SaveChanges();

                return Ok(proyect);
            }catch(Exception err)
            {
                return BadRequest(err.Message);
            }

        }
    }

    public class UserToken
    {
        public int Id { get; set; }
        public string Email { get; set; }
    }
}
