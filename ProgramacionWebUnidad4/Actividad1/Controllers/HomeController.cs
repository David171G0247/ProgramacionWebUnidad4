using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Actividad1.Helpers;
using Actividad1.Models;
using Actividad1.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Actividad1.Controllers
{
    
    public class HomeController : Controller
    {
        public IWebHostEnvironment Environment { get; set; }
        public HomeController(IWebHostEnvironment env)
        {
            Environment = env;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult IniciarSesion()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> IniciarSesion(Usuarios usuario, bool persistente)
        {
            controlusuariosContext context = new controlusuariosContext();
            UsuarioRepository<Usuarios> repository = new UsuarioRepository<Usuarios>(context);
            var datos = repository.ObtenerUsuarioPorCorreo(usuario.Correo);
            if (datos != null && HashHelper.GetHash(usuario.Contrasena) == datos.Contrasena)
            {
                if (datos.Activo == 1)
                {
                    List<Claim> info = new List<Claim>();
                    info.Add(new Claim(ClaimTypes.Name, "Usuario" + datos.Nombre));
                    //info.Add(new Claim(ClaimTypes.Role, "Cliente"));
                    info.Add(new Claim("Correo", datos.Correo));
                    info.Add(new Claim("Nombre", datos.Nombre));

                    var claimsidentity = new ClaimsIdentity(info, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsprincipal = new ClaimsPrincipal(claimsidentity);

                    if (persistente == true)
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsprincipal,
                            new AuthenticationProperties { IsPersistent = true });
                    }
                    else
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsprincipal,
                            new AuthenticationProperties { IsPersistent = false });
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Esta cuenta aún no ha sido activada. Necesita ir a su correo y activarla");
                    return View(usuario);
                }
            }
            else
            {
                ModelState.AddModelError("", "Sus credenciales son incorrectas.");
                return View(usuario);
            }
        }

        [AllowAnonymous]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Registrar(Usuarios usuario, string contraseña1, string contraseña2)
        {
            controlusuariosContext context = new controlusuariosContext();
            UsuarioRepository<Usuarios> repository = new UsuarioRepository<Usuarios>(context);
            try
            {
                if (context.Usuarios.Any(x => x.Correo == usuario.Correo))
                {
                    ModelState.AddModelError("", "Este correo electrónico ya se encuentra registrado.");
                    return View(usuario);
                }
                else
                {
                    if (contraseña1 == contraseña2)
                    {
                        usuario.Contrasena = HashHelper.GetHash(contraseña1);
                        usuario.Codigo = CodeHelper.GetCode();
                        usuario.Activo = 0;
                        repository.Insertar(usuario);

                        MailMessage mensaje = new MailMessage();
                        mensaje.From = new MailAddress("sistemascomputacionales7g@gmail.com", "OursCafé");
                        mensaje.To.Add(usuario.Correo);
                        mensaje.Subject = "Confirma tu correo electrónico en OursCafé";
                        string text = System.IO.File.ReadAllText(Environment.WebRootPath + "/ConfirmarCorreo.html");
                        mensaje.Body = text.Replace("{##codigo##}", usuario.Codigo.ToString());
                        mensaje.IsBodyHtml = true;

                        SmtpClient cliente = new SmtpClient("smtp.gmail.com", 587);
                        cliente.EnableSsl = true;
                        cliente.UseDefaultCredentials = false;
                        cliente.Credentials = new NetworkCredential("sistemascomputacionales7g@gmail.com", "sistemas7g");
                        cliente.Send(mensaje);
                        return RedirectToAction("Activar");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Las contraseñas no son iguales.");
                        return View(usuario);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(usuario);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public IActionResult Activar()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Activar(int codigo)
        {
            controlusuariosContext context = new controlusuariosContext();
            UsuarioRepository<Usuarios> repository = new UsuarioRepository<Usuarios>(context);
            var usuario = context.Usuarios.FirstOrDefault(x => x.Codigo == codigo);

            if (usuario != null && usuario.Activo == 0)
            {
                var code = usuario.Codigo;
                if (codigo == code)
                {
                    usuario.Activo = 1;
                    repository.Editar(usuario);
                    return RedirectToAction("IniciarSesion");
                }
                else
                {
                    ModelState.AddModelError("", "El código ingresado no coincide.");
                    return View((object)codigo);
                }
            }
            else
            {
                ModelState.AddModelError("", "El usuario no existe.");
                return View((object)codigo);
            }
        }
        [Authorize]
        public IActionResult CambiarContra()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult CambiarContra(string correo, string contra, string nuevaContraseña1, string nuevaContraseña2)
        {
            controlusuariosContext context = new controlusuariosContext();
            UsuarioRepository<Usuarios> repository = new UsuarioRepository<Usuarios>(context);
            try
            {
                var usuario = repository.ObtenerUsuarioPorCorreo(correo);

                if (usuario.Contrasena != HashHelper.GetHash(contra))
                {
                    ModelState.AddModelError("", "La contraseña que ingresó es incorrecta.");
                    return View();
                }
                else
                {
                    if (nuevaContraseña1 != nuevaContraseña2)
                    {
                        ModelState.AddModelError("", "Las nuevas contraseñas no son iguales.");
                        return View();
                    }
                    else if (usuario.Contrasena == HashHelper.GetHash(nuevaContraseña1))
                    {
                        ModelState.AddModelError("", "La nueva contraseña no puede ser igual a la actual.");
                        return View();
                    }
                    else
                    {
                        usuario.Contrasena = HashHelper.GetHash(nuevaContraseña1);
                        repository.Editar(usuario);
                        return RedirectToAction("IniciarSesion");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [AllowAnonymous]
        public IActionResult RecuperarContra()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecuperarContra(string correo)
        {
            try
            {
                controlusuariosContext context = new controlusuariosContext();
                UsuarioRepository<Usuarios> repository = new UsuarioRepository<Usuarios>(context);
                var usuario = repository.ObtenerUsuarioPorCorreo(correo);

                if (usuario != null)
                {
                    var contraTemp = CodeHelper.GetCode();
                    MailMessage mensaje = new MailMessage();
                    mensaje.From = new MailAddress("sistemascomputacionales7g@gmail.com", "Mercado Libre");
                    mensaje.To.Add(correo);
                    mensaje.Subject = "Recupera tu contraseña de Mercado Libre";
                    string text = System.IO.File.ReadAllText(Environment.WebRootPath + "/RecuperarContra.html");
                    mensaje.Body = text.Replace("{##contraTemp##}", contraTemp.ToString());
                    mensaje.IsBodyHtml = true;

                    SmtpClient cliente = new SmtpClient("smtp.gmail.com", 587);
                    cliente.EnableSsl = true;
                    cliente.UseDefaultCredentials = false;
                    cliente.Credentials = new NetworkCredential("sistemascomputacionales7g@gmail.com", "sistemas7g");
                    cliente.Send(mensaje);
                    usuario.Contrasena = HashHelper.GetHash(contraTemp.ToString());
                    repository.Editar(usuario);
                    return RedirectToAction("IniciarSesion");
                }
                else
                {
                    ModelState.AddModelError("", "El correo electrónico que ingresó no se encuentra registrado :(");
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View((object)correo);
            }
        }

        [Authorize]
        public IActionResult EliminarCuenta()
        {
            return RedirectToAction("IniciarSesion");
        }

        // Aún no funciona
        [Authorize]
        [HttpPost]
        public IActionResult EliminarCuenta(string correo, string contra)
        {
            try
            {
                controlusuariosContext context = new controlusuariosContext();
                UsuarioRepository<Usuarios> repository = new UsuarioRepository<Usuarios>(context);
                var usuario = repository.ObtenerUsuarioPorCorreo(correo);
                if (usuario != null)
                {
                    if (HashHelper.GetHash(contra) == usuario.Contrasena)
                    {
                        repository.Eliminar(usuario);
                    }
                    else
                    {
                        ModelState.AddModelError("", "La contraseña está incorrecta.");
                        return View();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Ocurrió un error. Inténtelo en unos minutos.");
                return View();
            }
        }
    }
}
