using System;
using System.Collections.Generic;

namespace Actividad1.Models
{
    public partial class Usuarios
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public int Codigo { get; set; }
        public ulong Activo { get; set; }
    }
}
