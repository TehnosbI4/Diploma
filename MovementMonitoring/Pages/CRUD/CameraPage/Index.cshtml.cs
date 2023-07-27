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

namespace MovementMonitoring.Pages.CRUD.CameraPage
{
    public record class CameraDTO(int Id, string Name, string Description, int RoomId, List<SelectListItem> Selects);

    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly EntityDBContext _context;
        private readonly IRazorRenderService _renderService;
        private readonly ILogger<IndexModel> _logger;

        public IEnumerable<Camera> Cameras { get; set; } = default!;
        public List<SelectListItem> Rooms { get; set; } = default!;

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
            Cameras = await _context.Cameras.ToListAsync();
            return new PartialViewResult
            {
                ViewName = "_ViewAll",
                ViewData = new ViewDataDictionary<IEnumerable<Camera>>(ViewData, Cameras)
            };
        }

        public async Task<JsonResult> OnGetCreateOrEditAsync(int id = 0)
        {
            if (User.IsInRole("Admin"))
            {
                Rooms = _context.Rooms.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name }).ToList();
                if (id == 0)
                {
                    return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_CreateOrEdit", new CameraDTO(0, "", "", 0, Rooms)) });
                }
                else
                {
                    Camera? camera = await _context.Cameras.FirstOrDefaultAsync(m => m.Id == id);
                    if (camera == null)
                    {
                        return new JsonResult(new { isValid = false, html = "" });
                    }
                    CameraDTO cameraDTO = new(camera.Id, camera.Name!, camera.Description!, camera.Room!.Id, Rooms);
                    return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_CreateOrEdit", cameraDTO) });
                }
            }
            return new JsonResult(new { isValid = false, html = "" });
        }

        public async Task<JsonResult> OnGetDetailsAsync(int id)
        {
            Camera? camera = await _context.Cameras.FirstOrDefaultAsync(m => m.Id == id);
            if (camera == null)
            {
                return new JsonResult(new { isValid = false, html = "" });
            }
            return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_Details", camera) });
        }

        public async Task<JsonResult> OnPostCreateOrEditAsync(int id, CameraDTO cameraDTO)
        {
            if (User.IsInRole("Admin"))
            {
                try
                {
                    Room? room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == cameraDTO.RoomId);
                    if (room == null)
                    {
                        return new JsonResult(new { isValid = false, html = "" });
                    }
                    Camera camera = new() { Id = cameraDTO.Id, Name = cameraDTO.Name, Description = cameraDTO.Description, Room = room };
                    if (id == 0)
                    {
                        _context.Add(camera);
                    }
                    else
                    {
                        _context.Attach(camera).State = EntityState.Modified;
                    }
                    await _context.SaveChangesAsync();
                    Cameras = await _context.Cameras.ToListAsync();
                    TablesUpdateList.SetTableUpdateRequest("Camera");
                }
                catch (Exception)
                {

                }
                string html = await _renderService.ToStringAsync("_ViewAll", Cameras);
                return new JsonResult(new { isValid = true, html = html });
            }
            return new JsonResult(new { isValid = false, html = "" });
        }

        public async Task<JsonResult> OnPostDeleteAsync(int id)
        {
            if (User.IsInRole("Admin"))
            {
                Camera? camera = await _context.Cameras.FirstOrDefaultAsync(m => m.Id == id);
                if (camera != null)
                {
                    try
                    {
                        _context.Cameras.Remove(camera);
                        await _context.SaveChangesAsync();
                        Cameras = await _context.Cameras.ToListAsync();
                        TablesUpdateList.SetTableUpdateRequest("Camera");
                    }
                    catch (Exception)
                    {

                    }
                }
                string html = await _renderService.ToStringAsync("_ViewAll", Cameras);
                return new JsonResult(new { isValid = true, html = html });
            }
            return new JsonResult(new { isValid = false, html = "" });
        }
    }
}
