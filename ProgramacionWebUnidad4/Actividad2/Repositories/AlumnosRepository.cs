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
        public Alumno GetAlumnoPorNoControl(string noControl)
        {
            return Context.Alumno.FirstOrDefault(x => x.NumeroControl.ToLower() == noControl.ToLower());
        }

        public override bool Validar(Alumno alumno)
        {
            if (string.IsNullOrEmpty(alumno.NumeroControl))
                throw new Exception("Ingrese el número de control del alumno.");
            if (string.IsNullOrEmpty(alumno.Nombre))
                throw new Exception("Ingrese el nombre del alumno.");
            if (alumno.IdMaestro == null || alumno.IdMaestro <= 0)
                throw new Exception("Debe asignarle un maestro al alumno.");
            if (alumno.NumeroControl.Length < 8 || alumno.NumeroControl.Length > 8)
                throw new Exception("El número de control debe ser de 8 dígitos.");
            return true;
        }
    }
}
