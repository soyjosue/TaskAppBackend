using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskAppBackend.Models
{
    public class Task
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El titulo de la tarea es obligatorio.")]
        public string Title { get; set; }
        public bool IsChecked { get; set; }


        public int ProyectId { get; set; }
        public int UserId { get; set; }

    }
}