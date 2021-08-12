namespace TaskAppBackend.Migration
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using TaskAppBackend.Data;

    internal sealed class Configuration : DbMigrationsConfiguration<TaskAppBackend.Data.TaskAppBackendContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "TaskAppBackend.Data.TaskAppBackendContext";
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(TaskAppBackendContext context)
        {
            //base.Seed(context);
        }
    }
}