using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MiniMarketApp.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuariosController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private bool IsAdminUser()
        {
            return User.IsInRole("Admin") || User.Identity?.Name == "admin@marketrubik.com";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdminUser()) return Forbid();

            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UsuarioViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var isLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow;

                userList.Add(new UsuarioViewModel
                {
                    Id = u.Id,
                    Email = u.Email ?? u.UserName ?? "Sin Email",
                    Rol = roles.FirstOrDefault() ?? "Cliente",
                    Activo = !isLockedOut,
                    LockoutEnd = u.LockoutEnd
                });
            }

            return View(userList);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            if (!IsAdminUser()) return Forbid();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearUsuarioViewModel model)
        {
            if (!IsAdminUser()) return Forbid();

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Ya existe un usuario registrado con este correo electrónico.");
                    return View(model);
                }

                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(model.Rol))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Rol));
                    }
                    await _userManager.AddToRoleAsync(user, model.Rol);

                    TempData["SuccessMessage"] = $"Usuario {model.Email} creado correctamente con el rol {model.Rol}.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(string userId, string nuevoRol)
        {
            if (!IsAdminUser()) return Forbid();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!await _roleManager.RoleExistsAsync(nuevoRol))
            {
                await _roleManager.CreateAsync(new IdentityRole(nuevoRol));
            }
            await _userManager.AddToRoleAsync(user, nuevoRol);

            TempData["SuccessMessage"] = $"Rol de {user.Email} cambiado a '{nuevoRol}' exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEstado(string userId)
        {
            if (!IsAdminUser()) return Forbid();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            if (user.Email == "admin@marketrubik.com")
            {
                TempData["ErrorMessage"] = "No se puede desactivar la cuenta principal de administrador.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.SetLockoutEnabledAsync(user, true);

            var isCurrentlyLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
            if (isCurrentlyLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["SuccessMessage"] = $"Usuario {user.Email} ha sido ACTIVADO.";
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                TempData["SuccessMessage"] = $"Usuario {user.Email} ha sido DESACTIVADO (eliminación lógica).";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class UsuarioViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
    }

    public class CrearUsuarioViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Formato de correo no válido.")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "La contraseña es obligatoria.")]
        [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Debe seleccionar un rol.")]
        public string Rol { get; set; } = "Cliente";
    }
}
