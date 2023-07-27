using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MovementMonitoring.Pages.Layout
{
    [Authorize]
    public class TablesModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
