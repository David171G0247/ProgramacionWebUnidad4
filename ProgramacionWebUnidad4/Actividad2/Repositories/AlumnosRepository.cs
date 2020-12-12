using Actividad2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Actividad2.Repositories
{
    public class AlumnosRepository : Repository<Alumno>
    {
        public AlumnosRepository(rolesusuariosContext context) : base(context)
        {

        }
        public Alumno ObtenerAlumnoPorNoControl(string noControl)
        {
            return Context.Alumno.FirstOrDefault(x => x.NumeroControl.ToLower() == noControl.ToLower());
        }

        public override bool Validar(Alumno alumno)
        {
            if (string.IsNullOrEmpty(alumno.NumeroControl))
                throw new Exception("Ingrese el número de control del alumno.");
            if (string.IsNullOrEmpty(alumno.Nombre))
                throw new Exception("Ingrese el nombre del alumno.");
            if (alumno.IdMaestro.ToString() == null || alumno.IdMaestro <= 0)
                throw new Exception("Debe asignarle un maestro al alumno.");
            return true;
        }
    }
}
