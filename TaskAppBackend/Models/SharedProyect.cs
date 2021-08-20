using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskAppBackend.Models
{
    public class SharedProyect
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string CodePassword { get; set; }

        public int ProyectId { get; set; }
    }
}