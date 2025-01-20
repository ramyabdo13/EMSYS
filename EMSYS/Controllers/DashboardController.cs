using EMSYS.Data;
using EMSYS.Models;
using EMSYS.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using static EMSYS.Models.ProjectEnum;

namespace EMSYS.Controllers
{
    [Authorize]
    public class DashboardController : Microsoft.AspNetCore.Mvc.Controller
    {
        private EMSYSdbContext db;
        private Util util;
        private readonly UserManager<AspNetUsers> _userManager;
        private ErrorLoggingService _logger;

        public DashboardController(EMSYSdbContext db, Util util, UserManager<AspNetUsers> userManager, ErrorLoggingService logger)
        {
            this.db = db;
            this.util = util;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            DashboardViewModel model = new DashboardViewModel();
            try
            {
                string currentUserId = _userManager.GetUserId(User);
                string publishedId = db.GlobalOptionSets.Where(a => a.Code == ExamStatus.Published.ToString()).Select(a => a.Id).FirstOrDefault();
                DateTime? now = util.GetSystemTimeZoneDateTimeNow();
                if (User.IsInRole("System Admin"))
                {
                    string teacherRoleId = db.AspNetRoles.Where(a => a.Name == "Instructor").Select(a => a.Id).FirstOrDefault();
                    string studentRoleId = db.AspNetRoles.Where(a => a.Name == "Student").Select(a => a.Id).FirstOrDefault();
                    model.TotalInstructor = db.AspNetUserRoles.Where(a => a.RoleId == teacherRoleId).Count();
                    model.TotalStudent = db.AspNetUserRoles.Where(a => a.RoleId == studentRoleId).Count();
                    model.ExamPassed = db.StudentExams.Where(a => a.Passed == true).Select(a => a.ExamId).Distinct().Count();
                    model.ExamFailed = db.StudentExams.Where(a => a.Passed == false).Select(a => a.ExamId).Distinct().Count();
                }
                model.ExamInProgress = GetExamInProgress(-1)?.Count();
                model.ExamCompleted = GetExamEnded(-1)?.Count();
                if (User.IsInRole("Instructor"))
                {
                    model.ExamPassed = (from t1 in db.StudentExams
                                        join t2 in db.Exams on t1.ExamId equals t2.Id
                                        where t2.CreatedBy == currentUserId && t1.Passed == true
                                        select t1.ExamId).Distinct().Count();
                    model.ExamFailed = (from t1 in db.StudentExams
                                        join t2 in db.Exams on t1.ExamId equals t2.Id
                                        where t2.CreatedBy == currentUserId && t1.Passed == false
                                        select t1.ExamId).Distinct().Count();
                }
                if (User.IsInRole("Student"))
                {
                    model.ExamPassed = (from t1 in db.StudentExams
                                        join t2 in db.Exams on t1.ExamId equals t2.Id
                                        where t1.StudentId == currentUserId && t1.Passed == true
                                        select t1.ExamId).Distinct().Count();
                    model.ExamFailed = (from t1 in db.StudentExams
                                        join t2 in db.Exams on t1.ExamId equals t2.Id
                                        where t1.StudentId == currentUserId && t1.Passed == false
                                        select t1.ExamId).Distinct().Count();
                }
               
                model.DefaultExamToDisplayTopTenStudent = (from t1 in db.StudentExams
                                                           join t2 in db.Exams on t1.ExamId equals t2.Id
                                                           where t2.IsActive == true && t1.Result != null && (User.IsInRole("Instructor") ? t2.CreatedBy == currentUserId : t1.Id != null)
                                                           orderby t1.EndedOn descending
                                                           select t2.Id).FirstOrDefault();
                model.ExamSelectList = (from t1 in db.StudentExams
                                        join t2 in db.Exams on t1.ExamId equals t2.Id
                                        where t2.IsActive == true && t1.Result != null && (User.IsInRole("Instructor") ? t2.CreatedBy == currentUserId : t1.Id != null)
                                        select new SelectListItem
                                        {
                                            Text = t2.Name,
                                            Value = t2.Id
                                        }).Distinct().ToList();
                if (model.ExamSelectList != null)
                {
                    foreach (var item in model.ExamSelectList)
                    {
                        if (item.Value == model.DefaultExamToDisplayTopTenStudent)
                        {
                            item.Selected = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return View(model);
        }

        [HttpPost]
       

    

        [HttpPost]
       

        [HttpPost]
        public IActionResult OkResultForTopStudentsByExam(string id)
        {
            List<DecimalChart> list = new List<DecimalChart>();
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    DateTime? now = util.GetSystemTimeZoneDateTimeNow();
                    id = db.Exams.Where(a => a.EndDate <= now).OrderByDescending(a => a.EndDate).Select(a => a.Id).FirstOrDefault();
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }

            return Ok(list);
        }

        //id = Exam Id, num = Number of records to get and display
     

        //num = Number of records to get and display
        

        //num = Number of records to get and display
        public List<UpcomingExamChart> GetExamInProgress(int num)
        {
            List<string> takenExams = new List<string>();
            List<UpcomingExamChart> result = new List<UpcomingExamChart>();
            try
            {
                string published = util.GetGlobalOptionSetId(ProjectEnum.ExamStatus.Published.ToString(), "ExamStatus");
                string userid = _userManager.GetUserId(User);
                DateTime? now = util.GetSystemTimeZoneDateTimeNow();
                result = (from t1 in db.Exams
                          where t1.IsActive == true && t1.IsPublished == true && t1.StartDate <= now && t1.EndDate >= now
                          orderby t1.StartDate
                          select new UpcomingExamChart
                          {
                              Id = t1.Id,
                              CreatedById = t1.CreatedBy
                          }).ToList();
                if (User.IsInRole("Instructor"))
                {
                    result = result.Where(a => a.CreatedById == userid).ToList();
                }
                if (User.IsInRole("Student"))
                {
                    takenExams = db.StudentExams.Where(a => a.StudentId == userid).Select(a => a.ExamId).ToList();
                    result = (from t1 in result
                              join t2 in db.ExamClassHubs on t1.Id equals t2.ExamId
                              join t3 in db.StudentClasses on t2.ClassHubId equals t3.ClassId
                              where t3.StudentId == userid
                              select t1).Where(t1 => !takenExams.Contains(t1.Id)).Distinct().ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return (num == -1) ? result : result.Take(num).ToList();
        }

        //num = Number of records to get and display
        public List<UpcomingExamChart> GetExamEnded(int num)
        {
            List<UpcomingExamChart> result = new List<UpcomingExamChart>();
            try
            {
                string published = util.GetGlobalOptionSetId(ProjectEnum.ExamStatus.Published.ToString(), "ExamStatus");
                string userid = _userManager.GetUserId(User);
                DateTime? now = util.GetSystemTimeZoneDateTimeNow();
                result = (from t1 in db.Exams
                          where t1.IsActive == true && t1.IsPublished == true && t1.StartDate <= now && t1.EndDate <= now
                          orderby t1.StartDate
                          select new UpcomingExamChart
                          {
                              CreatedById = t1.CreatedBy
                          }).ToList();
                if (User.IsInRole("Instructor"))
                {
                    result = result.Where(a => a.CreatedById == userid).ToList();
                }
                if (User.IsInRole("Student"))
                {
                    result = db.StudentExams.Where(a => a.StudentId == userid).Select(a => new UpcomingExamChart { Id = a.ExamId }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return (num == -1) ? result : result.Take(num).ToList();
        }


        //studentId = student Id, num = Number of records to get and display
  

        //studentId = student Id, num = Number of records to get and display
    

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