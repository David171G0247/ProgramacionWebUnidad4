using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Actividad2.Helpers;
using Actividad2.Models;
using Actividad2.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Actividad2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult IniciarSesionMaestro()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> IniciarSesionMaestro(Maestro m)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository repos = new MaestroRepository(context);
            var maestro = repos.GetMaestroByClave(m.Clave);
            try
            {
                if (maestro != null && maestro.Contrasena == HashHelper.GetHash(m.Contrasena))
                {
                    if (maestro.Activo == 1)
                    {
                        List<Claim> info = new List<Claim>();
                        info.Add(new Claim(ClaimTypes.Name, "Usuario" + maestro.Nombre));
                        info.Add(new Claim(ClaimTypes.Role, "Maestro"));
                        info.Add(new Claim("Clave", maestro.Clave.ToString()));
                        info.Add(new Claim("Nombre", maestro.Nombre));
                        info.Add(new Claim("Id", maestro.Id.ToString()));

                        var claimsidentity = new ClaimsIdentity(info, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsprincipal = new ClaimsPrincipal(claimsidentity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsprincipal,
                            new AuthenticationProperties { IsPersistent = true });
                        return RedirectToAction("Index", maestro.Clave);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Su cuenta se encuentra inactiva.");
                        return View(m);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "La clave o la contraseña es incorrecta.");
                    return View(m);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(m);
            }
        }

        [AllowAnonymous]
        public IActionResult IniciarSesionDirector()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> IniciarSesionDirector(Director d)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            Repository<Director> repos = new Repository<Director>(context);

            var director = context.Director.FirstOrDefault(x => x.Clave == d.Clave);

            try
            {
                if (director != null && director.Contrasena == HashHelper.GetHash(d.Contrasena))
                {
                    List<Claim> info = new List<Claim>();

                    info.Add(new Claim(ClaimTypes.Name, "Usuario" + director.Nombre));
                    info.Add(new Claim("Clave", director.Clave.ToString()));
                    info.Add(new Claim(ClaimTypes.Role, "Director"));
                    info.Add(new Claim("Nombre", director.Nombre));

                    var claimsIdentity = new ClaimsIdentity(info, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal,
                    new AuthenticationProperties { IsPersistent = true });

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "La clave o la contraseña es incorrecta");
                    return View(d);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(d);
            }
        }
        [AllowAnonymous]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Director")]
        public IActionResult ListaMaestros()
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestros = repository.GetAll();
            return View(maestros);
        }

        [Authorize(Roles = "Director")]
        public IActionResult DarAltaMaestros()
        {
            return View();
        }

        [Authorize(Roles = "Director")]
        [HttpPost]
        public IActionResult DarAltaMaestros(Maestro m)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository repository = new MaestroRepository(context);

            try
            {
                var maestro = repository.GetMaestroByClave(m.Clave);
                if (maestro == null)
                {
                    m.Activo = 1;
                    m.Contrasena = HashHelper.GetHash(m.Contrasena);
                    repository.Insert(m);
                    return RedirectToAction("ListaMaestros");
                }
                else
                {
                    ModelState.AddModelError("", "La clave que ingresó no está disponible.");
                    return View(m);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(m);
            }
        }

        [Authorize(Roles = "Director")]
        [HttpPost]
        public IActionResult EstadoMaestro(Maestro m)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.GetById(m.Id);
            if (maestro != null && maestro.Activo == 0)
            {
                maestro.Activo = 1;
                repository.Update(maestro);
            }
            else
            {
                maestro.Activo = 0;
                repository.Update(maestro);
            }
            return RedirectToAction("ListaMaestros");
        }

        [Authorize(Roles = "Director")]
        public IActionResult ModificarInfoMaestros(int id)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.GetById(id);
            if (maestro != null)
            {
                return View(maestro);
            }
            return RedirectToAction("ListaMaestros");
        }

        [Authorize(Roles = "Director")]
        [HttpPost]
        public IActionResult ModificarInfoMaestros(Maestro m)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.GetById(m.Id);
            try
            {
                if (maestro != null)
                {
                    maestro.Nombre = m.Nombre;
                    repository.Update(maestro);
                }
                return RedirectToAction("ListaMaestros");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(maestro);
            }
        }

        [Authorize(Roles = "Director")]
        public IActionResult CambiarContraMaestro(int id)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.GetById(id);
            if (maestro == null)
            {
                return RedirectToAction("ListaMaestros");
            }
            return View(maestro);
        }

        [Authorize(Roles = "Director")]
        [HttpPost]
        public IActionResult CambiarContraMaestro(Maestro m, string nuevaContra, string nuevaContraConfirm)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.GetById(m.Id);

            try
            {
                if (maestro != null)
                {
                    if (nuevaContra == maestro.Contrasena)
                    {
                        ModelState.AddModelError("", "La nueva contraseña no puedo ser igual a la actual.");
                        return View(maestro);
                    }
                    else
                    {
                        if (nuevaContra == nuevaContraConfirm)
                        {
                            maestro.Contrasena = nuevaContra;
                            maestro.Contrasena = HashHelper.GetHash(nuevaContra);
                            repository.Update(maestro);
                        }
                        else
                        {
                            ModelState.AddModelError("", "Las contraseñas no son iguales.");
                            return View(maestro);
                        }
                    }
                }
                return RedirectToAction("ListaMaestros");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(maestro);
            }
        }

        [Authorize(Roles = "Maestro, Director")]
        public IActionResult ListaAlumnos(int id)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.ObtenerAlumnosPorMaestro(id);
            if (maestro != null)
            {
                if (User.IsInRole("Maestro"))
                {
                    if (User.Claims.FirstOrDefault(x => x.Type == "Id").Value == maestro.Id.ToString())
                    {
                        return View(maestro);
                    }
                    else
                    {
                        return RedirectToAction("Denegado");
                    }
                }
                else
                {
                    return View(maestro);
                }
            }
            else
            {
                return RedirectToAction("ListaAlumnos");
            }
        }

        [Authorize(Roles = "Maestro, Director")]
        public IActionResult AlumnoAgregar(int id)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository repository = new MaestroRepository(context);
            AlumnoViewModel viewModel = new AlumnoViewModel();
            viewModel.Maestro = repository.GetById(id);
            if (viewModel.Maestro != null)
            {
                if (User.IsInRole("Maestro"))
                {
                    if (User.Claims.FirstOrDefault(x => x.Type == "Id").Value == viewModel.Maestro.Id.ToString())
                    {
                        return View(viewModel);
                    }
                    else
                    {
                        return RedirectToAction("Denegado");
                    }
                }
                else
                {
                    return View(viewModel);
                }
            }
            return View(viewModel);
        }


        [Authorize(Roles = "Maestro, Director")]
        [HttpPost]
        public IActionResult AlumnoAgregar(AlumnoViewModel viewModel)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository maestroRepository = new MaestroRepository(context);
            AlumnosRepository alumnosRepository = new AlumnosRepository(context);
            try
            {
                if (viewModel.Maestro.Activo == 1)
                {
                    var id = maestroRepository.GetMaestroByClave(viewModel.Maestro.Clave).Id;
                    viewModel.Alumno.IdMaestro = id;
                    alumnosRepository.Insert(viewModel.Alumno);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "No se pueden asignar alumnos a maestros inactivos.");
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                viewModel.Maestro = maestroRepository.GetById(viewModel.Maestro.Id);
                viewModel.Maestros = maestroRepository.GetAll();
                ModelState.AddModelError("", ex.Message);
                return View(viewModel);
            }
        }

        [Authorize(Roles = "Maestro, Director")]
        public IActionResult AlumnoEditar(int id)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository maestroRepository = new MaestroRepository(context);
            AlumnosRepository alumnosRepository = new AlumnosRepository(context);
            AlumnoViewModel viewModel = new AlumnoViewModel();
            viewModel.Alumno = alumnosRepository.GetById(id);
            viewModel.Maestros = maestroRepository.GetAll();
            if (viewModel.Alumno != null)
            {
                if (User.IsInRole("Maestro"))
                {
                    viewModel.Maestro = maestroRepository.GetById(viewModel.Alumno.IdMaestro);
                    if (User.Claims.FirstOrDefault(x => x.Type == "NoControl").Value == viewModel.Maestro.NoControl.ToString())
                    {
                        return View(viewModel);
                    }
                    else
                    {
                        return RedirectToAction("Denegado");
                    }
                }
                else
                    return View(viewModel);
            }
            else return RedirectToAction("Index");
        }

        [Authorize(Roles = "Maestro, Director")]
        [HttpPost]
        public IActionResult AlumnoEditar(AlumnoViewModel viewModel)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            MaestroRepository maestroRepository = new MaestroRepository(context);
            AlumnosRepository alumnosRepository = new AlumnosRepository(context);
            try
            {
                var alumno = alumnosRepository.GetById(viewModel.Alumno.Id);
                if (alumno != null)
                {
                    alumno.Nombre = viewModel.Alumno.Nombre;
                    if (User.IsInRole("Director"))
                    {
                        alumno.IdMaestro = viewModel.Alumno.IdMaestro;
                    }
                    alumnosRepository.Update(alumno);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "El alumno que intentó buscar no existe.");
                    viewModel.Maestro = maestroRepository.GetById(viewModel.Alumno.IdMaestro);
                    viewModel.Maestros = maestroRepository.GetAll();
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                viewModel.Maestro = maestroRepository.GetById(viewModel.Alumno.IdMaestro);
                viewModel.Maestros = maestroRepository.GetAll();
                return View(viewModel);
            }
        }

        [Authorize(Roles = "Maestro, Director")]
        [HttpPost]
        public IActionResult AlumnoEliminar(Alumno a)
        {
            rolesusuariosContext context = new rolesusuariosContext();
            AlumnosRepository repository = new AlumnosRepository(context);
            var alumno = repository.GetById(a.Id);
            if (alumno != null)
            {
                repository.Delete(alumno);
            }
            else
            {
                ModelState.AddModelError("", "El alumnó que intentó eliminar no existe.");
            }
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public IActionResult Denegado()
        {
            return View();
        }
    }
}
