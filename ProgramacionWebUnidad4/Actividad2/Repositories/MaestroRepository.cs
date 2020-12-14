using Actividad2.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Actividad2.Repositories
{
    public class MaestroRepository : Repository<Maestro>
    {
        public MaestroRepository(rolesusuariosContext contex) : base(contex)
        {
        }
        public Maestro GetMaestroByClave(int clave)
        {
            return Context.Maestro.FirstOrDefault(x => x.Clave == clave);
        }
        public Maestro ObtenerAlumnosPorMaestro(int id)
        {
            return Context.Maestro.Include(x => x.Alumno).FirstOrDefault(x => x.Id == id);
        }
        public override bool Validar(Maestro maestro)
        {
            if (string.IsNullOrEmpty(maestro.Nombre))
                throw new Exception("Ingrese el nombre del maestro");
            if (string.IsNullOrWhiteSpace(maestro.Contrasena))
                throw new Exception("Ingrese una contraseña");
            if (maestro.Contrasena.ToString().Length <= 7)
                throw new Exception("La contraseña debe tener más de 7 caracteres");
            if (maestro.Clave <= 0)
                throw new Exception("Ingrese la clave del maestro.");       
            return true;
        }
    }
}
