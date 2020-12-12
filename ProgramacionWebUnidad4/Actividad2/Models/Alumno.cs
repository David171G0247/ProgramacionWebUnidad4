using System;
using System.Collections.Generic;

namespace Actividad2.Models
{
    public partial class Alumno
    {
        public int Id { get; set; }
        public string NumeroControl { get; set; }
        public string Nombre { get; set; }
        public int? IdMaestro { get; set; }

        public virtual Maestro IdMaestroNavigation { get; set; }
    }
}
