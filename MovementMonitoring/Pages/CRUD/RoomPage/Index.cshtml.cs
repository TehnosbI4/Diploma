using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using MovementMonitoring.Data;
using MovementMonitoring.Models;
using MovementMonitoring.Services;
using MovementMonitoring.Utilities;

namespace MovementMonitoring.Pages.CRUD.RoomPage
{
    public record class RoomDTO(int Id, string Name, string Description, int AccessLevelId, List<SelectListItem> Selects);

    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly EntityDBContext _context;
        private readonly IRazorRenderService _renderService;
        private readonly ILogger<IndexModel> _logger;

        public IEnumerable<Room> Rooms { get; set; } = default!;
        public List<SelectListItem> AccessLevels { get; set; } = default!;

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
            Rooms = await _context.Rooms.ToListAsync();
            return new PartialViewResult
            {
                ViewName = "_ViewAll",
                ViewData = new ViewDataDictionary<IEnumerable<Room>>(ViewData, Rooms)
            };
        }

        public async Task<JsonResult> OnGetCreateOrEditAsync(int id = 0)
        {
            if (User.IsInRole("Admin"))
            {
                AccessLevels = _context.AccessLevels.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name }).ToList();
                if (id == 0)
                {
                    return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_CreateOrEdit", new RoomDTO(0, "", "", 0, AccessLevels)) });
                }
                else
                {
                    Room? room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);
                    if (room == null)
                    {
                        return new JsonResult(new { isValid = false, html = "" });
                    }
                    RoomDTO roomDTO = new(room.Id, room.Name!, room.Description!, room.AccessLevel!.Id, AccessLevels);
                    return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_CreateOrEdit", roomDTO) });
                }
            }
            return new JsonResult(new { isValid = false, html = "" });
        }

        public async Task<JsonResult> OnGetDetailsAsync(int id)
        {
            Room? room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return new JsonResult(new { isValid = false, html = "" });
            }
            return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_Details", room) });
        }

        public async Task<JsonResult> OnPostCreateOrEditAsync(int id, RoomDTO roomDTO)
        {
            if (User.IsInRole("Admin"))
            {
                try
                {
                    AccessLevel? accessLevel = await _context.AccessLevels.FirstOrDefaultAsync(m => m.Id == roomDTO.AccessLevelId);
                    if (accessLevel == null)
                    {
                        return new JsonResult(new { isValid = false, html = "" });
                    }
                    Room room = new() { Id = roomDTO.Id, Name = roomDTO.Name, Description = roomDTO.Description, AccessLevel = accessLevel };
                    if (id == 0)
                    {
                        _context.Add(room);
                    }
                    else
                    {
                        _context.Attach(room).State = EntityState.Modified;
                    }
                    await _context.SaveChangesAsync();
                    Rooms = await _context.Rooms.ToListAsync();
                    TablesUpdateList.SetTableUpdateRequest("Room");
                }
                catch (Exception)
                {

                }
                string html = await _renderService.ToStringAsync("_ViewAll", Rooms);
                return new JsonResult(new { isValid = true, html = html });
            }
            return new JsonResult(new { isValid = false, html = "" });
        }

        public async Task<JsonResult> OnPostDeleteAsync(int id)
        {
            if (User.IsInRole("Admin"))
            {
                Room? room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);
                if (room != null)
                {
                    try
                    {
                        _context.Rooms.Remove(room);
                        await _context.SaveChangesAsync();
                        Rooms = await _context.Rooms.ToListAsync();
                        TablesUpdateList.SetTableUpdateRequest("Room");
                    }
                    catch (Exception)
                    {

                    }
                }
                string html = await _renderService.ToStringAsync("_ViewAll", Rooms);
                return new JsonResult(new { isValid = true, html = html });
            }
            return new JsonResult(new { isValid = false, html = "" });
        }
    }
}
