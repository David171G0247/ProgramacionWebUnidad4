using System;
using System.Collections.Generic;

namespace Actividad2.Models
{
    public partial class Maestro
    {
        public Maestro()
        {
            Alumno = new HashSet<Alumno>();
        }

        public int Id { get; set; }
        public int Clave { get; set; }
        public string Nombre { get; set; }
        public string Contrasena { get; set; }
        public ulong Activo { get; set; }

        public virtual ICollection<Alumno> Alumno { get; set; }
    }
}
