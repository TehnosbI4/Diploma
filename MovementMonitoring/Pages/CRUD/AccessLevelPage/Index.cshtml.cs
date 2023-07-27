using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using MovementMonitoring.Data;
using MovementMonitoring.Models;
using MovementMonitoring.Services;
using MovementMonitoring.Utilities;

namespace MovementMonitoring.Pages.CRUD.AccessLevelPage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly EntityDBContext _context;
        private readonly IRazorRenderService _renderService;
        private readonly ILogger<IndexModel> _logger;

        public IEnumerable<AccessLevel> AccessLevels { get; set; } = default!;

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
            AccessLevels = await _context.AccessLevels.ToListAsync();
            return new PartialViewResult
            {
                ViewName = "_ViewAll",
                ViewData = new ViewDataDictionary<IEnumerable<AccessLevel>>(ViewData, AccessLevels)
            };
        }

        public async Task<JsonResult> OnGetCreateOrEditAsync(int id = 0)
        {
            if (User.IsInRole("Admin"))
            {
                if (id == 0)
                {
                    return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_CreateOrEdit", new AccessLevel()) });
                }
                else
                {
                    AccessLevel? accesslevel = await _context.AccessLevels.FirstOrDefaultAsync(m => m.Id == id);
                    if (accesslevel == null)
                    {
                        return new JsonResult(new { isValid = false, html = "" });
                    }
                    return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_CreateOrEdit", accesslevel) });
                }
            }
            return new JsonResult(new { isValid = false, html = "" });
        }

        public async Task<JsonResult> OnGetDetailsAsync(int id)
        {
            AccessLevel? accesslevel = await _context.AccessLevels.FirstOrDefaultAsync(m => m.Id == id);
            if (accesslevel == null)
            {
                return new JsonResult(new { isValid = false, html = "" });
            }
            return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_Details", accesslevel) });
        }

        public async Task<JsonResult> OnPostCreateOrEditAsync(int id, AccessLevel accessLevel)
        {
            if (User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        if (id == 0)
                        {
                            _context.Add(accessLevel);
                        }
                        else
                        {
                            _context.Attach(accessLevel).State = EntityState.Modified;
                        }
                        await _context.SaveChangesAsync();
                        AccessLevels = await _context.AccessLevels.ToListAsync();
                        TablesUpdateList.SetTableUpdateRequest("AccessLevel");
                    }
                    catch (Exception)
                    {

                    }
                    string html = await _renderService.ToStringAsync("_ViewAll", AccessLevels);
                    return new JsonResult(new { isValid = true, html = html });
                }
                else
                {
                    string html = await _renderService.ToStringAsync("_CreateOrEdit", accessLevel);
                    return new JsonResult(new { isValid = false, html = html });
                }
            }
            return new JsonResult(new { isValid = false, html = "" });
        }

        public async Task<JsonResult> OnPostDeleteAsync(int id)
        {
            if (User.IsInRole("Admin"))
            {
                AccessLevel? accessLevel = await _context.AccessLevels.FirstOrDefaultAsync(m => m.Id == id);
                if (accessLevel != null)
                {
                    try
                    {
                        _context.AccessLevels.Remove(accessLevel);
                        await _context.SaveChangesAsync();
                        AccessLevels = await _context.AccessLevels.ToListAsync();
                        TablesUpdateList.SetTableUpdateRequest("AccessLevel");
                    }
                    catch (Exception)
                    {

                    }
                }
                string html = await _renderService.ToStringAsync("_ViewAll", AccessLevels);
                return new JsonResult(new { isValid = true, html = html });
            }
            return new JsonResult(new { isValid = false, html = "" });
        }
    }
}
