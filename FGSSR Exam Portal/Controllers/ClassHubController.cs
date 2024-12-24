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
    public class ClassHubController : Controller
    {
        private EMSYSdbContext db;
        private Util util;
        private readonly UserManager<AspNetUsers> _userManager;
        private ErrorLoggingService _logger;

        public ClassHubController(EMSYSdbContext db, Util util, UserManager<AspNetUsers> userManager, ErrorLoggingService logger)
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
            ClassHubViewModel model = new ClassHubViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "View");
            }
            return View(model);
        }

        public IActionResult Edit(string Id)
        {
            ClassHubViewModel model = new ClassHubViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "Edit");
            }
            //when loading the edit page, if the record don't have Active/Inactive value, by default, set to Active
            model.ActiveInactiveSelectlist = util.GetActiveInactiveDropDown(model.IsActive ?? "Active");
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(ClassHubViewModel model)
        {
            try
            {
                ValidateModel(model);

                if (!ModelState.IsValid)
                {
                    //when model.IsActive is null, means that user select "Please Select" and click Save button,
                    //pass "" to GetActiveInactiveDropDown method, so that the "Please Select" is selected
                    model.ActiveInactiveSelectlist = util.GetActiveInactiveDropDown(model.IsActive ?? "");
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


        public ClassHubViewModel GetViewModel(string Id, string type)
        {
            ClassHubViewModel model = new ClassHubViewModel();
            ClassHub cl = db.ClassHubs.Where(a => a.Id == Id).FirstOrDefault();
            model.Id = cl.Id;
            model.Name = cl.Name;
            model.IsActive = cl.IsActive == true ? "Active" : "Inactive";
            model.ActiveInactiveSelectlist = util.GetActiveInactiveDropDown("Active");
            if (type == "View")
            {
                model.CreatedAndModified = util.GetCreatedAndModified(cl.CreatedBy, cl.IsoUtcCreatedOn, cl.ModifiedBy, cl.IsoUtcModifiedOn);
            }
            return model;
        }

        public void ValidateModel(ClassHubViewModel model)
        {
            if (model != null)
            {
                bool duplicated = false;
                if (model.Id != null)
                {
                    duplicated = db.ClassHubs.Where(a => a.Name == model.Name && a.Id != model.Id).Any();
                }
                else
                {
                    duplicated = db.ClassHubs.Where(a => a.Name == model.Name).Select(a => a.Id).Any();
                }
                if (duplicated == true)
                {
                    ModelState.AddModelError("Name", Resource.ThisIsADuplicatedValue);
                }
            }
        }

        public void SaveRecord(ClassHubViewModel model)
        {
            if (model != null)
            {
                if (model.Id == null)
                {
                    ClassHub cl = new ClassHub();
                    cl.Id = Guid.NewGuid().ToString();
                    cl.Name = model.Name;
                    cl.IsActive = model.IsActive == "Active" ? true : false;
                    cl.CreatedBy = _userManager.GetUserId(User);
                    cl.CreatedOn = util.GetSystemTimeZoneDateTimeNow();
                    cl.IsoUtcCreatedOn = util.GetIsoUtcNow();
                    db.ClassHubs.Add(cl);
                    db.SaveChanges();
                }
                else
                {
                    ClassHub cl = db.ClassHubs.Where(a => a.Id == model.Id).FirstOrDefault();
                    cl.Name = model.Name;
                    cl.IsActive = model.IsActive == "Active" ? true : false;
                    cl.ModifiedBy = _userManager.GetUserId(User);
                    cl.ModifiedOn = util.GetSystemTimeZoneDateTimeNow();
                    cl.IsoUtcModifiedOn = util.GetIsoUtcNow();
                    db.Entry(cl).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public IActionResult Delete(string Id)
        {
            try
            {
                if (Id != null)
                {
                    ClassHub cl = db.ClassHubs.Where(a => a.Id == Id).FirstOrDefault();
                    if (cl != null)
                    {
                        db.ClassHubs.Remove(cl);
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
                    sort = ClassListConfig.DefaultSortOrder;
                }
                headers = ListUtil.GetColumnHeaders(ClassListConfig.DefaultColumnHeaders, sort);
                var list = ReadClasses();
                string searchMessage = ClassListConfig.SearchMessage;
                list = ClassListConfig.PerformSearch(list, search);
                list = ClassListConfig.PerformSort(list, sort);
                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = pg ?? 1;
                ViewData["CurrentSearch"] = search;
                int? total = list.Count();
                int? defaultSize = StudentInClassListConfig.DefaultPageSize;
                size = size == 0 || size == null ? (defaultSize != -1 ? defaultSize : total) : size == -1 ? total : size;
                ViewData["CurrentSize"] = size;
                PaginatedList<ClassHubViewModel> result = await PaginatedList<ClassHubViewModel>.CreateAsync(list, pg ?? 1, size.Value, total.Value, headers, searchMessage);
                return PartialView("~/Views/ClassHub/_MainList.cshtml", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return PartialView("~/Views/Shared/Error.cshtml", null);
        }

        public IQueryable<ClassHubViewModel> ReadClasses()
        {
            var list = (from t1 in db.ClassHubs
                        select new ClassHubViewModel
                        {
                            Id = t1.Id,
                            Name = t1.Name,
                            IsActive = t1.IsActive == true ? "Active" : "Inactive"
                        });
            return list;
        }

        public async Task<IActionResult> GetPartialViewStudentInClassListing(string classId, string sort, string search, int? pg, int? size)
        {
            try
            {
                List<ColumnHeader> headers = new List<ColumnHeader>();
                if (string.IsNullOrEmpty(sort))
                {
                    sort = StudentInClassListConfig.DefaultSortOrder;
                }
                headers = ListUtil.GetColumnHeaders(StudentInClassListConfig.DefaultColumnHeaders, sort);
                var list = ReadStudentInClasses(classId);
                string searchMessage = StudentInClassListConfig.SearchMessage;
                list = StudentInClassListConfig.PerformSearch(list, search);
                list = StudentInClassListConfig.PerformSort(list, sort);
                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = pg ?? 1;
                ViewData["CurrentSearch"] = search;
                int? total = list.Count();
                int? defaultSize = StudentInClassListConfig.DefaultPageSize;
                size = size == 0 || size == null ? (defaultSize != -1 ? defaultSize : total) : size == -1 ? total : size;
                ViewData["CurrentSize"] = size;
                PaginatedList<StudentViewModel> result = await PaginatedList<StudentViewModel>.CreateAsync(list, pg ?? 1, size.Value, total.Value, headers, searchMessage);
                return PartialView("~/Views/ClassHub/_StudentInClassList.cshtml", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return PartialView("~/Views/Shared/Error.cshtml", null);
        }

        public IQueryable<StudentViewModel> ReadStudentInClasses(string classId)
        {
            var list = (from t1 in db.ClassHubs
                        join t2 in db.StudentClasses on t1.Id equals t2.ClassId
                        join t3 in db.UserProfiles on t2.StudentId equals t3.AspNetUserId
                        where t1.Id == classId
                        select new StudentViewModel
                        {
                            Id = t3.Id,
                            FullName = t3.FullName
                        });
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
