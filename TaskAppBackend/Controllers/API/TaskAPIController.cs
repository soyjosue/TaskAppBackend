using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TaskAppBackend.Data;
using TaskAppBackend.Helpers;
using TaskAppBackend.Models;

namespace TaskAppBackend.Controllers.API
{
    [Authorize]
    [RoutePrefix("api/TaskApi")]
    public class TaskAPIController : ApiController
    {
        private TaskAppBackendContext db = new TaskAppBackendContext();

        [HttpGet]
        [Route("{proyectId}")]
        public IHttpActionResult GetTaskByProyectId(int proyectId, HttpRequestMessage request)
        {
            var userToken = Utils.GetUserOfToken(request);

            var tasks = from task in db.Tasks
                        where task.ProyectId == proyectId
                        && task.UserId == userToken.Id
                        select task;

            return Ok(tasks);
        }

        [HttpPost]
        [Route("{idProyect}/Create")]
        [ResponseType(typeof(Task))]
        public IHttpActionResult CreateTask(int idProyect, Task task, HttpRequestMessage request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userToken = Utils.GetUserOfToken(request);

            if (!DataBaseHelper.IsExistUser(db, userToken.Email))
                return BadRequest("Usuario no existe");

            if (!DataBaseHelper.IsTheCorrectAuthTask(db, idProyect, userToken))
                return BadRequest("No tienes permiso en este proyecto.");

            task.IsChecked = false;
            task.ProyectId = idProyect;
            task.UserId = userToken.Id;

            try
            {
                db.Tasks.Add(task);
                db.SaveChanges();

                return Ok(new {
                    Message = "Tarea agregada correctamente",
                    Task = task
                });
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpPut]
        [Route("Edit/{taskId}")]
        [ResponseType(typeof(Task))]
        public IHttpActionResult EditTask(int taskId, Task task, HttpRequestMessage request)
        {
            if (!DataBaseHelper.IsExistTask(db, taskId))
                return NotFound();

            var taskOld = db.Tasks.FirstOrDefault(ta => ta.Id == taskId);
            var userToken = Utils.GetUserOfToken(request);

            if (taskOld.UserId != userToken.Id)
                return BadRequest("No tiene permiso para modificar esta tarea");

            if (task.Title == null)
                task.Title = taskOld.Title;

            task.Id = taskOld.Id;
            task.ProyectId = taskOld.ProyectId;
            task.UserId = taskOld.UserId;

            try
            {
                db.Entry(taskOld).CurrentValues.SetValues(task);
                db.SaveChanges();

                return Ok(new { 
                    Message = "Tarea editada correctamente",
                    Task = task
                });
            } catch(Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpDelete]
        [Route("Delete/{taskId}")]
        public IHttpActionResult DeleteTask(int taskId, HttpRequestMessage request)
        {
            if (!DataBaseHelper.IsExistTask(db, taskId))
                return NotFound();

            var userToken = Utils.GetUserOfToken(request);

            var task = db.Tasks.FirstOrDefault(tas => tas.Id == taskId);
            
            if(!DataBaseHelper.IsTheCorrectAuthTask(db, task.ProyectId, userToken))
                return BadRequest("No tienes permiso en este proyecto.");

            try
            {
                db.Tasks.Remove(task);
                db.SaveChanges();

                return Ok(new { 
                    Message = "Tarea eliminada correctamente",
                    Task = task
                });
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }
    }
}
