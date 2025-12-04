using System.Security.Claims;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.Auth;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /Auth/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập thì redirect về Home
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        // POST: /Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Xác thực user
            var user = await _authService.AuthenticateAsync(model.Username, model.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                return View(model);
            }

            // Tạo claims identity
            var claimsIdentity = _authService.CreateClaimsIdentity(user);

            // Tạo authentication properties
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe 
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            // Sign in
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect về returnUrl hoặc Home
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // Redirect theo role
            if (user.Role == "Customer")
            {
                return RedirectToAction("Index", "Home");
            }
            else if (user.Role == "Housekeeping")
            {
                return RedirectToAction("MyTasks", "Housekeeping", new { area = "Admin" });
            }
            else // Admin, Manager
            {
                return RedirectToAction("Index", "Hotel", new { area = "Admin" });
            }
        }

        // GET: /Auth/Register (Public - cho phép đăng ký công khai cho User/Guest)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            var model = new RegisterViewModel();
            return View(model);
        }

        // POST: /Auth/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Tự động set role là Customer cho user đăng ký công khai
            model.Role = "Customer";

            try
            {
                var user = await _authService.RegisterAsync(model);
                TempData["SuccessMessage"] = $"Tài khoản {user.Username} đã được tạo thành công! Vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // GET: /Auth/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
