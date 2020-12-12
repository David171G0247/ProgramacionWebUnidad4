using Actividad1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Actividad1.Repositories
{
    public class UsuarioRepository<T> where T:class
    {
        public controlusuariosContext Context { get; set; }
        public UsuarioRepository(controlusuariosContext context)
        {
            Context = context;
        }

        public Usuarios ObtenerUsuarioPorId(int id)
        {
            return Context.Usuarios.FirstOrDefault(x => x.Id == id);
        }
        public Usuarios ObtenerUsuarioPorCorreo(string correo)
        {
            return Context.Usuarios.FirstOrDefault(x => x.Correo == correo);
        }
        public Usuarios ObtenerUsuario(Usuarios id)
        {
            return Context.Find<Usuarios>(id);
        }

        public bool Validaciones(Usuarios usuario)
        {
            if (string.IsNullOrEmpty(usuario.Nombre))
                throw new Exception("Ingrese el nombre de usuario.");
            if (string.IsNullOrEmpty(usuario.Correo))
                throw new Exception("Ingrese el correo electrónico del usuario.");
            if (string.IsNullOrEmpty(usuario.Contrasena))
                throw new Exception("Ingrese la contraseña del usuario.");
            return true;
        }

        public virtual void Insertar(Usuarios usuario)
        {
            if (Validaciones(usuario))
            {
                Context.Add(usuario);
                Context.SaveChanges();
            }
        }
        public virtual void Editar(Usuarios usuario)
        {
            if (Validaciones(usuario))
            {
                Context.Update<Usuarios>(usuario);
                Context.SaveChanges();
            }
        }
        public virtual void Eliminar(Usuarios usuario)
        {
            Context.Remove<Usuarios>(usuario);
            Context.SaveChanges();
        }
    }
}
