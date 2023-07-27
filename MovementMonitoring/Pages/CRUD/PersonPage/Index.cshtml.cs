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

namespace MovementMonitoring.Pages.CRUD.PersonPage
{
    public record class PersonDTO(int Id, string Guid, string Name, string Description, int AccessLevelId, List<SelectListItem> Selects, List<IFormFile> Files);
    public record class PersonDetails(Person Person, string? LastPhotoPath, List<string> GuidPaths);
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly EntityDBContext _context;
        private readonly IRazorRenderService _renderService;
        private readonly ILogger<IndexModel> _logger;

        public IEnumerable<Person> Persons { get; set; } = default!;
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
            Persons = await _context.Persons.ToListAsync();
            return new PartialViewResult
            {
                ViewName = "_ViewAll",
                ViewData = new ViewDataDictionary<IEnumerable<Person>>(ViewData, Persons)
            };
        }

        public async Task<JsonResult> OnGetCreateOrEditAsync(int id = 0)
        {
            if (User.IsInRole("Admin"))
            {
                AccessLevels = _context.AccessLevels.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name }).ToList();
                List<IFormFile> files = new List<IFormFile>();
                if (id == 0)
                {
                    return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_CreateOrEdit", new PersonDTO(0, "", "", "", 0, AccessLevels, files)) });
                }
                else
                {
                    Person? person = await _context.Persons.FirstOrDefaultAsync(m => m.Id == id);
                    if (person == null)
                    {
                        return new JsonResult(new { isValid = false, html = "" });
                    }
                    PersonDTO personDTO = new(person.Id, person.Guid!, person.Name!, person.Description!, person.AccessLevel!.Id, AccessLevels, files);
                    return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_CreateOrEdit", personDTO) });
                }
            }
            return new JsonResult(new { isValid = false, html = "" });
        }

        public async Task<JsonResult> OnGetDetailsAsync(int id)
        {
            Person? person = await _context.Persons.FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return new JsonResult(new { isValid = false, html = "" });
            }


            Movement? movement = _context.Movements.Where(x => x.CurrentPerson!.Id == id).OrderByDescending(x => x.Id).FirstOrDefault();
            string? lastPhotoPath;
            List<string> guidPaths = new List<string>();
            if (movement != null)
            {
                lastPhotoPath = movement.LastPhotoPath;
                Console.WriteLine(lastPhotoPath);
            }
            else
            {
                lastPhotoPath = null;
            }
            string guidPath = Path.Combine(@"wwwroot\pythonProject", Configurator.YamlConfig.data_path.Replace("..\\", ""), person.Guid);
            Console.WriteLine(">>>>>>>>>>>>>>>>>> " + guidPath + " " + Directory.Exists(guidPath));
            if (Directory.Exists(guidPath))
            {
                List<string> files = new List<string>();
                DirectoryInfo di = new DirectoryInfo(guidPath);
                foreach (FileInfo fi in di.GetFiles())
                {
                    string fileName = Path.Combine(guidPath, fi.Name);
                    string type = "." + fileName.Split(".").Last();
                    if (Configurator.YamlConfig.supported_extensions.Contains(type))
                    {
                        files.Add(fileName.Replace("wwwroot", "~"));
                        Console.WriteLine(fileName);
                    }
                }
                guidPaths = files;
                Console.WriteLine(guidPaths);
            }
            PersonDetails personDetails = new(person, lastPhotoPath, guidPaths);

            return new JsonResult(new { isValid = true, html = await _renderService.ToStringAsync("_Details", personDetails) });
        }

        public async Task<JsonResult> OnPostCreateOrEditAsync(int id, PersonDTO personDTO)
        {
            if (User.IsInRole("Admin"))
            {
                try
                {
                    AccessLevel? accessLevel = await _context.AccessLevels.FirstOrDefaultAsync(m => m.Id == personDTO.AccessLevelId);

                    if (accessLevel == null)
                    {
                        return new JsonResult(new { isValid = false, html = "" });
                    }

                    if (id == 0)
                    {
                        Person person = new() { Id = personDTO.Id, Guid = Guid.NewGuid().ToString(), Name = personDTO.Name, Description = personDTO.Description, AccessLevel = accessLevel };
                        _context.Add(person);
                        await WriteFilesAsync(person.Guid, personDTO.Files);
                    }
                    else
                    {
                        Person person = new() { Id = personDTO.Id, Guid = personDTO.Guid, Name = personDTO.Name, Description = personDTO.Description, AccessLevel = accessLevel };
                        _context.Attach(person).State = EntityState.Modified;
                        await WriteFilesAsync(personDTO.Guid, personDTO.Files);
                    }
                    await _context.SaveChangesAsync();
                    Persons = await _context.Persons.ToListAsync();
                    TablesUpdateList.SetTableUpdateRequest("Person");
                }
                catch (Exception)
                {

                }
                string html = await _renderService.ToStringAsync("_ViewAll", Persons);
                return new JsonResult(new { isValid = true, html = html });
            }
            return new JsonResult(new { isValid = false, html = "" });
        }

        public async Task<JsonResult> OnPostDeleteAsync(int id)
        {
            if (User.IsInRole("Admin"))
            {
                Person? person = await _context.Persons.FirstOrDefaultAsync(m => m.Id == id);
                if (person != null)
                {
                    try
                    {
                        _context.Persons.Remove(person);
                        await _context.SaveChangesAsync();
                        Persons = await _context.Persons.ToListAsync();
                        TablesUpdateList.SetTableUpdateRequest("Person");
                    }
                    catch (Exception)
                    {

                    }
                }
                string html = await _renderService.ToStringAsync("_ViewAll", Persons);
                return new JsonResult(new { isValid = true, html = html });
            }
            return new JsonResult(new { isValid = false, html = "" });
        }

        private static async Task WriteFilesAsync(string guid, List<IFormFile> files)
        {
            int i = 0;
            if (files != null)
            {
                foreach (IFormFile file in files)
                {
                    string uploadsPath = (string)Configurator.YamlConfig.uploads_path;
                    string path = Path.Combine(@"wwwroot\pythonProject", uploadsPath.Replace("..\\", ""), guid);
                    Console.WriteLine(">>>> " + path);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string fileName = Path.Combine(path, i++.ToString() + "." + file.FileName.Split(".")[^1]);
                    using FileStream fileStream = new(fileName, FileMode.Create);
                    await file.CopyToAsync(fileStream);
                }
            }
        }
    }
}
