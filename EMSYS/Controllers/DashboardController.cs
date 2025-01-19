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
                model.UpcomingExamCharts = GetTopUpcomingExam(5);
                if (User.IsInRole("Student"))
                {
                    model.StudentBestPerformanceExams = GetStudentBestPerformanceExamList(currentUserId, 5);
                    model.StudentWeakPerformanceExams = GetStudentWeakPerformanceExamList(currentUserId, 5);

                    model.StudentWeakPerformanceExams = model.StudentWeakPerformanceExams
                        .Where(a => !model.StudentBestPerformanceExams.Select(b => b.ExamId).Contains(a.ExamId))
                        .ToList();
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
        public IActionResult GetAveragePerformanceByMonth()
        {
            List<DecimalChart> list = new List<DecimalChart>();
            try
            {
                if (User.IsInRole("System Admin"))
                {
                    list = (from t1 in db.StudentExams
                            group t1.Result by t1.StartedOn.Value.Month into g
                            select new DecimalChart
                            {
                                DataValue = g.Average(),
                                DataLabel = g.Key.ToString()
                            }).ToList();
                }
                if (User.IsInRole("Instructor"))
                {
                    string userid = _userManager.GetUserId(User);
                    list = (from t1 in db.StudentExams
                            join t2 in db.Exams on t1.ExamId equals t2.Id
                            where t2.CreatedBy == userid
                            group t1.Result by t1.StartedOn.Value.Month into g
                            select new DecimalChart
                            {
                                DataValue = g.Average(),
                                DataLabel = g.Key.ToString()
                            }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return Ok(list.OrderBy(o => o.DataValue));
        }

        public List<IntChart> GetTopQuestionsAnsweredWrongly()
        {
            List<IntChart> result = new List<IntChart>();
            try
            {
                if (User.IsInRole("System Admin"))
                {
                    result = (from t1 in db.StudentAnswerCloneds
                              join t2 in db.Questions on t1.QuestionId equals t2.Id
                              where t1.MarksObtained == 0 // Filter incorrect answers
                              group t1 by new { t1.QuestionId, t2.QuestionTitle } into g
                              select new IntChart
                              {
                                  DataValue = g.Count(), // Count wrong answers for each question
                                  DataLabel = g.Key.QuestionTitle // Use the question title as the label
                              }).OrderByDescending(a => a.DataValue) // Order by the number of wrong answers
                                .Take(5) // Take the top 5
                                .ToList();

                }
                if (User.IsInRole("Instructor"))
                {
                    string userid = _userManager.GetUserId(User);
                    result = (from t1 in db.StudentAnswerCloneds
                              join t2 in db.Questions on t1.QuestionId equals t2.Id
                              join t3 in db.Exams on t1.ExamId equals t3.Id
                              where t1.MarksObtained == 0 && t3.CreatedBy == userid
                              group t1 by new { t1.QuestionId, t2.QuestionTitle } into g
                              select new IntChart
                              {
                                  DataValue = g.Count(), // Count wrong answers for each question
                                  DataLabel = g.Key.QuestionTitle // Use the question title as the label
                              }).OrderByDescending(a => a.DataValue) // Order by the number of wrong answers
                                .Take(5) // Take the top 5
                                .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return result;
        }

        [HttpPost]
        public IActionResult OkResultForTopQuestionsAnsweredWrongly()
        {
            List<IntChart> list = new List<IntChart>();
            try
            {
                list = GetTopQuestionsAnsweredWrongly();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return Ok(list);
        }

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
                list = GetTopStudentsByExam(id, 10);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }

            return Ok(list);
        }

        //id = Exam Id, num = Number of records to get and display
        public List<DecimalChart> GetTopStudentsByExam(string id, int num)
        {
            List<DecimalChart> result = new List<DecimalChart>();
            try
            {
                if (User.IsInRole("System Admin"))
                {
                    result = (from t1 in db.StudentExams
                              join t2 in db.Exams on t1.ExamId equals t2.Id
                              join t3 in db.UserProfiles on t1.StudentId equals t3.AspNetUserId
                              where t1.ExamId == id && t2.IsActive == true
                              select new DecimalChart
                              {
                                  DataValue = t1.Result,
                                  DataLabel = t3.FullName
                              }).OrderByDescending(a => a.DataValue).Take(num).ToList();
                }
                if (User.IsInRole("Instructor"))
                {
                    string userid = _userManager.GetUserId(User);
                    result = (from t1 in db.StudentExams
                              join t2 in db.Exams on t1.ExamId equals t2.Id
                              join t3 in db.UserProfiles on t1.StudentId equals t3.AspNetUserId
                              where t1.ExamId == id && t2.IsActive == true && t2.CreatedBy == userid
                              select new DecimalChart
                              {
                                  DataValue = t1.Result,
                                  DataLabel = t3.FullName
                              }).OrderByDescending(a => a.DataValue).Take(num).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return result;
        }

        //num = Number of records to get and display
        public List<UpcomingExamChart> GetTopUpcomingExam(int num)
        {
            List<string> takenExams = new List<string>();
            List<UpcomingExamChart> result = new List<UpcomingExamChart>();
            List<UpcomingExamChart> filteredResult = new List<UpcomingExamChart>();
            try
            {
                string published = util.GetGlobalOptionSetId(ProjectEnum.ExamStatus.Published.ToString(), "ExamStatus");
                string userid = _userManager.GetUserId(User);
                DateTime? now = util.GetSystemTimeZoneDateTimeNow();
                result = (from t1 in db.Exams
                          where t1.IsActive == true && t1.IsPublished == true && t1.StartDate > now
                          orderby t1.StartDate
                          select new UpcomingExamChart
                          {
                              Id = t1.Id,
                              Name = t1.Name,
                              StartDate = t1.StartDate,
                              IsoUtcStartDate = t1.IsoUtcStartDate,
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
                              select t1).ToList().Except(takenExams.Select(id => new UpcomingExamChart { Id = id })).Distinct().ToList();
                }

                filteredResult = result.Take(num).ToList();
                if (filteredResult != null)
                {
                    foreach (var item in filteredResult)
                    {
                        item.Countdown = item.StartDate.Value - now.Value;
                        item.CountdownHour = (item.Countdown.TotalMinutes / 60).ToString("0");
                        item.CountdownMinute = (item.Countdown.TotalMinutes % 60).ToString("0");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return result.Take(num).ToList();
        }

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
        public List<ResultViewModel> GetStudentBestPerformanceExamList(string studentId, int num)
        {
            List<ResultViewModel> result = new List<ResultViewModel>();
            try
            {
                result = (from t1 in db.StudentExams
                          join t2 in db.Exams on t1.ExamId equals t2.Id
                          where t1.StudentId == studentId && t1.EndedOn != null
                          orderby t1.Result descending
                          select new ResultViewModel
                          {
                              ExamId = t2.Id,
                              ExamName = t2.Name,
                              YourScore = t1.Result
                          }).Take(num).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return result;
        }

        //studentId = student Id, num = Number of records to get and display
        public List<ResultViewModel> GetStudentWeakPerformanceExamList(string studentId, int num)
        {
            List<ResultViewModel> result = new List<ResultViewModel>();
            try
            {
                result = (from t1 in db.StudentExams
                          join t2 in db.Exams on t1.ExamId equals t2.Id
                          where t1.StudentId == studentId && t1.EndedOn != null
                          orderby t1.Result ascending
                          select new ResultViewModel
                          {
                              ExamId = t2.Id,
                              ExamName = t2.Name,
                              YourScore = t1.Result
                          }).Take(num).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return result;
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