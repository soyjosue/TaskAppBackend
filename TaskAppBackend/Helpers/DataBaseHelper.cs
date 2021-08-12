using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskAppBackend.Data;

namespace TaskAppBackend.Helpers
{
    public static class DataBaseHelper
    {
        public static bool IsExistUser(TaskAppBackendContext db, string email)
        {
            var user = (from userDb in db.Users
                        where userDb.Email == email
                        select userDb).ToList();

            if(user.Count == 0)
            {
                return false;
            }

            return true;
        }
    }
}