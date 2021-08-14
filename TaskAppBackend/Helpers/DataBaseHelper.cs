using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskAppBackend.Controllers.API;
using TaskAppBackend.Data;

namespace TaskAppBackend.Helpers
{
    public static class DataBaseHelper
    {
        public static bool IsExistUser(TaskAppBackendContext db, string email)
        {
            var user = db.Users.FirstOrDefault(us => us.Email == email);

            if (user == null)
                return false;

            return true;
        }

        public static bool IsTheCorrectAuthTask(TaskAppBackendContext db, int id, UserToken userToken)
        {
            var proyect = db.Proyects.FirstOrDefault(pro => pro.Id == id);

            if (proyect == null || proyect.UserId != userToken.Id)
                return false;

            return true;
        }

        public static bool IsExistTask(TaskAppBackendContext db, int id)
        {
            var task = db.Tasks.FirstOrDefault(tas => tas.Id == id);

            if (task == null)
                return false;

            return true;
        }
    }
}