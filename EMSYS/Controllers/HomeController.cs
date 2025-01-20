using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using EMSYS.Resources;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using EMSYS.Utils;
using EMSYS.Data;

namespace EMSYS.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private IWebHostEnvironment Environment;
        private EMSYSdbContext db;
        private IConfiguration Configuration;
        private Util util;

        public HomeController(IWebHostEnvironment environment, EMSYSdbContext _db, IConfiguration _Configuration, Util util)
        {
            Environment = environment;
            db = _db;
            Configuration = _Configuration;
            this.util = util;
        }

        public async Task<IActionResult> Index()
        {
            string connection = Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connection))
            {
                return View("Error");
            }
            try
            {
                //When the web app is launched for the first time and the aspnetusers table does not exist in the database, the SQL command will be executed to create the tables
                if (!IsTableExists("aspnetusers", connection))
                {
                    string sqlFilePath = Path.Combine(Environment.WebRootPath, "SQL\\1.0.0.sql");
                    await ExecuteSqlFileAsync(sqlFilePath, connection);
                    //insert demo data
                    if (util.GetAppSettingsValue("demoData") == "true")
                    {
                        string demoDataSqlFilePath = Path.Combine(Environment.WebRootPath, "SQL\\demodata.sql");
                        await ExecuteSqlFileAsync(demoDataSqlFilePath, connection);
                    }
                }
            }
            catch (Exception)
            {
                return View("Error");
            }

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("index", "dashboard");
            }
            return View();
        }

        public IActionResult UnauthorizedAccess()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("UnauthorizedTwo");
            }
            return RedirectToAction("UnauthorizedOne");
        }

        public IActionResult UnauthorizedOne()
        {
            ViewBag.Message = Resource.YouDontHavePermissionToAccess;
            return View();
        }
        public IActionResult UnauthorizedTwo()
        {
            ViewBag.Message = Resource.YouDontHavePermissionToAccess;
            return View();
        }

        [AllowAnonymous]
        public IActionResult ChangeLanguage(string lang)
        {
            if (lang != null)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
                lang = "en";
            }

            Response.Cookies.Append("Language", lang);

            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }


        private static IEnumerable<string> SplitSqlStatements(string sqlScript)
        {
            // Make line endings standard to match RegexOptions.Multiline
            sqlScript = Regex.Replace(sqlScript, @"(\r\n|\n\r|\n|\r)", "\n");

            // Split by "GO" statements
            var statements = Regex.Split(
                    sqlScript,
                    @"^[\t ]*GO[\t ]*\d*[\t ]*(?:--.*)?$",
                    RegexOptions.Multiline |
                    RegexOptions.IgnorePatternWhitespace |
                    RegexOptions.IgnoreCase);

            // Remove empties, trim, and return
            return statements
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim(' ', '\n'));
        }
        private bool IsTableExists(string tableName, string connectionString)
        {
            const string query = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName);
                con.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
        private async Task ExecuteSqlFileAsync(string filePath, string connectionString)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException($"SQL file not found: {filePath}");
            }
            var fileContents = await System.IO.File.ReadAllTextAsync(filePath);
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                foreach (var sql in SplitSqlStatements(fileContents))
                {
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}