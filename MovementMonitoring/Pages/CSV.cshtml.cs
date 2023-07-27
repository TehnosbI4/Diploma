using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using System.IO.Compression;

namespace MovementMonitoring.Pages
{
    [Authorize(Roles = "Admin")]
    public class CSVModel : PageModel
    {
        private record class TableInfo(string? Name, string? TableType, string? TableCatalog);
        public IActionResult OnGet()
        {
            return Page();
        }
        public async Task<IActionResult> OnPostDownloadAsync()
        {
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=maindb;Trusted_Connection=True;MultipleActiveResultSets=true";
            string uploadPath = "wwwroot/downloads";
            Directory.Delete(uploadPath, true);
            Directory.CreateDirectory(uploadPath);
            using SqlConnection connection = new(connectionString);
            connection.Open();
            foreach (DataRow tableInfo in connection.GetSchema("Tables").Rows)
            {
                string tableName = tableInfo["TABLE_NAME"].ToString()!;
                Directory.CreateDirectory($"{uploadPath}/CSV");
                string fileName = $"{uploadPath}/CSV/{tableName}.csv";
                using SqlCommand sqlCmd = new($"SELECT * FROM [{tableName}]", connection);
                using SqlDataReader reader = sqlCmd.ExecuteReader();
                using StreamWriter sw = new(fileName);
                object[] output = new object[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    output[i] = reader.GetName(i);
                }
                sw.WriteLine(string.Join("|", output));
                while (reader.Read())
                {
                    reader.GetValues(output);
                    sw.WriteLine(string.Join("|", output));
                }
            }
            Console.WriteLine(">>>>>>>>>>>> " + Directory.GetFiles($"{uploadPath}/CSV").Length);

            //if (Directory.GetFiles($"{uploadPath}/CSV").Length > 0)
            //{
            string zipPath = $"{uploadPath}/statistics.zip";
            ZipFile.CreateFromDirectory($"{uploadPath}/CSV", zipPath);
            Console.WriteLine(">>>>>>>>>>>>>>>>>>> " + Path.GetFullPath(Directory.GetFiles($"{uploadPath}")[0]));
            return File(new FileStream(zipPath, FileMode.Open), "text/plain", "statistics.zip");
            //}
            //return null;
        }
    }
}
