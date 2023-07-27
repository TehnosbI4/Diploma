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

namespace MovementMonitoring.Pages.CRUD.MovementPage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly EntityDBContext _context;
        private readonly IRazorRenderService _renderService;
        private readonly ILogger<IndexModel> _logger;

        public IEnumerable<Movement> Movements { get; set; } = default!;

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
            Movements = await _context.Movements.ToListAsync();
            return new PartialViewResult
            {
                ViewName = "_ViewAll",
                ViewData = new ViewDataDictionary<IEnumerable<Movement>>(ViewData, Movements)
            };
        }
        
        public async Task<JsonResult> OnGetDetailsAsync(int id)
        {
            Movement? movement = await _context.Movements.FirstOrDefaultAsync(m => m.Id == id);
            if (movement == null)
            {
                return new JsonResult(new { isValid = false, html = "" });
            }
            return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_Details", movement) });
        }
    }
}
