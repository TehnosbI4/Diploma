using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MovementMonitoring.Pages.Navbar
{
    [Authorize(Roles = "Admin")]
    public class AdminOptionsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
