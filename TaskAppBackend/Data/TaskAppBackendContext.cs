using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

using TaskAppBackend.Migration;
using TaskAppBackend.Models;

namespace TaskAppBackend.Data
{
    public class TaskAppBackendContext : DbContext
    {

        public TaskAppBackendContext() : base("name=TaskAppBackendContext")
        {
            Database.SetInitializer<TaskAppBackendContext>(
                new MigrateDatabaseToLatestVersion<TaskAppBackendContext, Configuration>()
            );
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Proyect> Proyects { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<SharedProyect> SharedProyects { get; set; }
        public DbSet<Shared> Shareds { get; set; }

    }
}