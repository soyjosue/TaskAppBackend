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
            var shareds = db.Shareds.Where(s => s.UserId == userToken.Id).ToList();

            var proyects = db.Proyects.Where(p => p.UserId == userToken.Id).ToList();

            foreach (var s in shareds)
            {
                proyects.Add(
                    db.Proyects.Where(p => p.Id == s.ProyectId).FirstOrDefault()
                    );
            }

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

            proyect.CreatedAt = DateTime.Now;
            proyect.UserId = userToken.Id;

            try
            {
                db.Proyects.Add(proyect);
                db.SaveChanges();

                var sharedCode = new SharedProyect()
                {
                    Code = Guid.NewGuid().ToString(),
                    CodePassword = Guid.NewGuid().ToString(),
                    ProyectId = proyect.Id
                };
                db.SharedProyects.Add(sharedCode);
                db.SaveChanges();

                return Ok(new
                {
                    Message = "Proyecto creado correctamente",
                    Proyect = proyect
                });
            }
            catch (Exception err)
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

            var taks = from task in db.Tasks
                       where task.ProyectId == proyect.Id
                       select task;

            var sharedProyect = db.SharedProyects.FirstOrDefault(s => s.ProyectId == proyect.Id);
            var shareds = db.Shareds.Where(s => s.ProyectId == proyect.Id);

            try
            {
                db.Tasks.RemoveRange(taks);
                db.Shareds.RemoveRange(shareds);
                db.SharedProyects.Remove(sharedProyect);
                db.Proyects.Remove(proyect);
                db.SaveChanges();

                return Ok(new
                {
                    Message = "Proyecto eliminado correctamente",
                    Proyect = proyect
                });
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }

        }

        [HttpGet]
        [Route("Members/{proyectId}")]
        public IHttpActionResult GetMembers(int proyectId, HttpRequestMessage request)
        {
            var userToken = Utils.GetUserOfToken(request);

            if (!DataBaseHelper.IsExistUser(db, userToken.Email))
                return BadRequest("El usuario no existe");

            var proyect = db.Proyects.Where(p => p.Id == proyectId).FirstOrDefault();
            if (proyect == null)
                return NotFound();

            var shareds = db.Shareds.Where(s => s.ProyectId == proyectId).ToList();

            if (shareds.Count == 0)
                return Ok(new List<User>());

            var users = new List<User>();
            foreach (var sha in shareds)
            {
                var user = db.Users.Where(u => u.Id == sha.UserId).FirstOrDefault();

                users.Add(
                    new User{ 
                        Id = user.Id,
                        Name = user.Name,
                        Lastname = user.Lastname,
                        Email = user.Email,
                        Password = ""
                    }
                    );
            }

            return Ok(users);
        }

        [HttpPost]
        [Route("AddMember")]
        public IHttpActionResult AddMemberProyect(SharedProyect sharedProyect, HttpRequestMessage request)
        {
            var userToken = Utils.GetUserOfToken(request);

            if (!DataBaseHelper.IsExistUser(db, userToken.Email))
                return BadRequest("El usuario no existe");

            var shared = db.SharedProyects.FirstOrDefault(s => s.Code == sharedProyect.Code && s.CodePassword == sharedProyect.CodePassword);

            if (shared == null)
                return BadRequest("No tienes permiso");

            var proyect = db.Proyects.FirstOrDefault(p => p.Id == shared.ProyectId);
            if (proyect == null || proyect.UserId == userToken.Id)
                return BadRequest("No puedes unirte como miembro a este proyecto.");

            var sharedUser = db.Shareds.FirstOrDefault(s => s.UserId == s.UserId && s.ProyectId == shared.ProyectId);

            if (sharedUser != null)
                return BadRequest("Ya estas en el proyecto como miembro.");

            try
            {
                db.Shareds.Add(new Shared { ProyectId = shared.ProyectId, UserId = userToken.Id });
                db.SaveChanges();

                return Ok(new
                {
                    Message = "El usuario fue agregado al proyecto exitosamente",
                    Shared = shared
                });
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpDelete]
        [Route("RemoveMember/{proyectId}")]
        public IHttpActionResult RemoveMemberProyect(int proyectId, HttpRequestMessage request)
        {
            var userToken = Utils.GetUserOfToken(request);

            if (!DataBaseHelper.IsExistUser(db, userToken.Email))
                return BadRequest("El usuario no existe");

            var shared = db.Shareds.FirstOrDefault(s => s.ProyectId == proyectId && s.UserId == userToken.Id);

            if (shared == null)
                return BadRequest("El usuario no se encuetra como miembro del proyecto.");

            try
            {
                db.Shareds.Remove(shared);
                db.SaveChanges();

                return Ok(new
                {
                    Message = "El usuario fue eliminado al proyecto exitosamente",
                    Shared = shared
                });
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpPut]
        [Route("Shared/{id}")]
        [ResponseType(typeof(SharedProyect))]
        public IHttpActionResult ChangePasswordSharedCode(int id, SharedProyect sharedProyect, HttpRequestMessage request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var shared = db.SharedProyects.FirstOrDefault(s => s.Id == id);

            if (shared == null)
                return NotFound();

            var proyect = db.Proyects.FirstOrDefault(p => p.Id == shared.ProyectId);
            var user = Utils.GetUserOfToken(request);

            if (proyect.UserId != user.Id)
                return BadRequest("Usted no tiene permiso para cambiar la contraseña");

            sharedProyect.Code = shared.Code;
            sharedProyect.Id = shared.Id;
            sharedProyect.ProyectId = shared.ProyectId;
            sharedProyect.CodePassword = Guid.NewGuid().ToString();

            try
            {
                db.Entry(shared).CurrentValues.SetValues(sharedProyect);
                db.SaveChanges();

                return Ok(new
                {
                    Message = "Codigo de compartido cambiado.",
                    SharedProyect = sharedProyect
                });
            }
            catch (Exception err)
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
