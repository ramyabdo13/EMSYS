using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMSYS.Models;
using EMSYS.Utils;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Reflection;
using EMSYS.Data;

namespace EMSYS.Controllers
{
    [Authorize]
    public class LoginHistoryController : Microsoft.AspNetCore.Mvc.Controller
    {
        private EMSYSdbContext db;
        private Util util;
        private readonly UserManager<AspNetUsers> _userManager;
        private ErrorLoggingService _logger;

        public LoginHistoryController(EMSYSdbContext db, Util util, UserManager<AspNetUsers> userManager, ErrorLoggingService logger)
        {
            this.db = db;
            this.util = util;
            _userManager = userManager;
            _logger = logger;
        }


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetPartialViewLoginHistories(string sort, string search, int? pg, int? size)
        {
            try
            {
                List<ColumnHeader> headers = new List<ColumnHeader>();
                if (string.IsNullOrEmpty(sort))
                {
                    sort = LoginHistoryListConfig.DefaultSortOrder;
                }
                headers = ListUtil.GetColumnHeaders(LoginHistoryListConfig.DefaultColumnHeaders, sort);
                var list = ReadLoginHistories();
                string searchMessage = LoginHistoryListConfig.SearchMessage;
                list = LoginHistoryListConfig.PerformSearch(list, search);
                list = LoginHistoryListConfig.PerformSort(list, sort);
                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = pg ?? 1;
                ViewData["CurrentSearch"] = search;
                int? total = list.Count();
                int? defaultSize = LoginHistoryListConfig.DefaultPageSize;
                size = size == 0 || size == null ? (defaultSize != -1 ? defaultSize : total) : size == -1 ? total : size;
                ViewData["CurrentSize"] = size;
                PaginatedList<LoginHistoryViewModel> result = await PaginatedList<LoginHistoryViewModel>.CreateAsync(list, pg ?? 1, size.Value, total.Value, headers, searchMessage);
                return PartialView("~/Views/LoginHistory/_MainList.cshtml", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return PartialView("~/Views/Shared/Error.cshtml", null);
        }

        public IQueryable<LoginHistoryViewModel> ReadLoginHistories()
        {
            var currentUserID = _userManager.GetUserId(User);
            var currentUserRole = util.GetCurrentUserRoleNameList(currentUserID);
            var list = from t1 in db.LoginHistories
                       let t2 = db.AspNetUsers.Where(a => a.Id == t1.AspNetUserId).SingleOrDefault()
                       let t3 = db.UserProfiles.Where(a => t2.Id == a.AspNetUserId).SingleOrDefault()
                       where currentUserRole.Contains("System Admin") ? true : t1.AspNetUserId == currentUserID
                       orderby t1.LoginDateTime
                       select new LoginHistoryViewModel
                       {
                           Id = t1.Id,
                           AspNetUserId = t2.Id,
                           UserProfileId = t3.Id,
                           Username = t2.UserName,
                           FullName = t3.FullName,
                           LoginDateTime = t1.LoginDateTime.Value,
                           IsoUtcLoginDateTime = t1.IsoUtcLoginDateTime
                       };
            return list;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
                if (util != null)
                {
                    util.Dispose();
                    util = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}