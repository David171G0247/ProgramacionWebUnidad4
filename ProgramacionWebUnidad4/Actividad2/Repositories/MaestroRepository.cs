using Actividad2.Models;
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
        public override bool Validar(Maestro maestro)
        {
            if (string.IsNullOrEmpty(maestro.Nombre))
                throw new Exception("El nombre no puede estar vacio");
            if (string.IsNullOrWhiteSpace(maestro.Contrasena))
                throw new Exception("La contraseña no puede estar vacía");
            if (maestro.Contrasena.Length <= 7)
                throw new Exception("La contraseña debe contener más de 7 caracteres");
            if (maestro.Clave <= 0)
                throw new Exception("Escriba la clave del maestro");
            return true;
        }
    }
}
