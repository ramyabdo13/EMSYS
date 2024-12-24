using EMSYS.Models;
using EMSYS.Resources;
using EMSYS.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using EMSYS.Data;

namespace EMSYS.Controllers
{
    [Authorize(Roles = "System Admin")]
    public class SubjectController : Controller
    {
        private EMSYSdbContext db;
        private Util util;
        private readonly UserManager<AspNetUsers> _userManager;
        private ErrorLoggingService _logger;

        public SubjectController(EMSYSdbContext db, Util util, UserManager<AspNetUsers> userManager, ErrorLoggingService logger)
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

        public IActionResult ViewRecord(string Id)
        {
            SubjectViewModel model = new SubjectViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "View");
            }
            return View(model);
        }

        public IActionResult Edit(string Id)
        {
            SubjectViewModel model = new SubjectViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "Edit");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(SubjectViewModel model)
        {
            try
            {
                ValidateModel(model);

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                SaveRecord(model);
                TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("index");
        }

        public IActionResult Delete(string Id)
        {
            try
            {
                if (Id != null)
                {
                    Subject cl = db.Subjects.Where(a => a.Id == Id).FirstOrDefault();
                    if (cl != null)
                    {
                        db.Subjects.Remove(cl);
                        db.SaveChanges();
                    }
                }
                TempData["NotifySuccess"] = Resource.RecordDeletedSuccessfully;
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("index");
        }

        public async Task<IActionResult> GetPartialViewListing(string sort, string search, int? pg, int? size)
        {
            try
            {
                List<ColumnHeader> headers = new List<ColumnHeader>();
                if (string.IsNullOrEmpty(sort))
                {
                    sort = SubjectListConfig.DefaultSortOrder;
                }
                headers = ListUtil.GetColumnHeaders(SubjectListConfig.DefaultColumnHeaders, sort);
                var list = ReadSubjects();
                string searchMessage = SubjectListConfig.SearchMessage;
                list = SubjectListConfig.PerformSearch(list, search);
                list = SubjectListConfig.PerformSort(list, sort);
                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = pg ?? 1;
                ViewData["CurrentSearch"] = search;
                int? total = list.Count();
                int? defaultSize = SubjectListConfig.DefaultPageSize;
                size = size == 0 || size == null ? (defaultSize != -1 ? defaultSize : total) : size == -1 ? total : size;
                ViewData["CurrentSize"] = size;
                PaginatedList<SubjectViewModel> result = await PaginatedList<SubjectViewModel>.CreateAsync(list, pg ?? 1, size.Value, total.Value, headers, searchMessage);
                return PartialView("~/Views/Subject/_MainList.cshtml", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return PartialView("~/Views/Shared/Error.cshtml", null);
        }

        public IQueryable<SubjectViewModel> ReadSubjects()
        {
            var list = (from t1 in db.Subjects
                        select new SubjectViewModel
                        {
                            Id = t1.Id,
                            Name = t1.Name
                        });
            return list;
        }

        public SubjectViewModel GetViewModel(string Id, string type)
        {
            SubjectViewModel model = new SubjectViewModel();
            Subject cl = db.Subjects.Where(a => a.Id == Id).FirstOrDefault();
            model.Id = cl.Id;
            model.Name = cl.Name;
            if (type == "View")
            {
                model.CreatedAndModified = util.GetCreatedAndModified(cl.CreatedBy, cl.IsoUtcCreatedOn, cl.ModifiedBy, cl.IsoUtcModifiedOn);
            }
            return model;
        }

        public void ValidateModel(SubjectViewModel model)
        {
            if (model != null)
            {
                bool duplicated = false;
                if (model.Id != null)
                {
                    duplicated = db.Subjects.Where(a => a.Name == model.Name && a.Id != model.Id).Any();
                }
                else
                {
                    duplicated = db.Subjects.Where(a => a.Name == model.Name).Select(a => a.Id).Any();
                }
                if (duplicated == true)
                {
                    ModelState.AddModelError("Name", Resource.ThisIsADuplicatedValue);
                }
            }
        }

        public void SaveRecord(SubjectViewModel model)
        {
            if (model != null)
            {
                if (model.Id == null)
                {
                    Subject cl = new Subject();
                    cl.Id = Guid.NewGuid().ToString();
                    cl.Name = model.Name;
                    cl.CreatedBy = _userManager.GetUserId(User);
                    cl.CreatedOn = util.GetSystemTimeZoneDateTimeNow();
                    cl.IsoUtcCreatedOn = util.GetIsoUtcNow();
                    db.Subjects.Add(cl);
                    db.SaveChanges();
                }
                else
                {
                    Subject cl = db.Subjects.Where(a => a.Id == model.Id).FirstOrDefault();
                    cl.Name = model.Name;
                    cl.ModifiedBy = _userManager.GetUserId(User);
                    cl.ModifiedOn = util.GetSystemTimeZoneDateTimeNow();
                    cl.IsoUtcModifiedOn = util.GetIsoUtcNow();
                    db.Entry(cl).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
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
