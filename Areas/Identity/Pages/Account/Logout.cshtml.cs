using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using food_delivery.Models;

namespace food_delivery.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<CustomUser> _signInManager;

        public LogoutModel(SignInManager<CustomUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Index");
        }
    }
}
