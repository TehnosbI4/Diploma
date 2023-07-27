using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MovementMonitoring.Data;
using MovementMonitoring.Hubs;
using MovementMonitoring.Models;
using MovementMonitoring.Services;

namespace MovementMonitoring.Pages.CRUD.ViolationPage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly EntityDBContext _context;
        private readonly IRazorRenderService _renderService;
        private readonly ILogger<IndexModel> _logger;

        public IEnumerable<Violation> Violations { get; set; } = default!;

        public IndexModel(EntityDBContext context, ILogger<IndexModel> logger, IRazorRenderService renderService)
        {
            _context = context;
            _logger = logger;
            _renderService = renderService;
        }

        public void OnGet()
        {

        }

        public async Task<PartialViewResult> OnGetViewAllPartial()
        {
            Violations = await _context.Violations.ToListAsync();
            return new PartialViewResult
            {
                ViewName = "_ViewAll",
                ViewData = new ViewDataDictionary<IEnumerable<Violation>>(ViewData, Violations)
            };
        }
        
        public async Task<JsonResult> OnGetDetailsAsync(int id)
        {
            Violation? violation = await _context.Violations.FirstOrDefaultAsync(m => m.Id == id);
            if (violation == null)
            {
                return new JsonResult(new { isValid = false, html = "" });
            }
            return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_Details", violation) });
        }
    }
}
