using EMSYS.Data;
using EMSYS.Models;
using EMSYS.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace EMSYS.Controllers
{
    public class ResultController : Controller
    {
        private EMSYSdbContext db;
        private Util util;
        private readonly UserManager<AspNetUsers> _userManager;
        private IWebHostEnvironment Environment;
        private ErrorLoggingService _logger;

        public ResultController(EMSYSdbContext db, Util util, UserManager<AspNetUsers> userManager, IWebHostEnvironment _Environment, ErrorLoggingService logger)
        {
            this.db = db;
            this.util = util;
            _userManager = userManager;
            Environment = _Environment;
            _logger = logger;
        }

        [Authorize(Roles = "System Admin, Instructor")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "System Admin, Instructor, Student")]
        public IActionResult StudentQuestionAnswer(string eId, string sId, int? num)
        {
            StudentExamResultList model = new StudentExamResultList();
            try
            {
                if (num == null)
                {
                    num = 1;
                }
                if (string.IsNullOrEmpty(sId))
                {
                    sId = _userManager.GetUserId(User);
                }
                int totalQuestion = db.ExamQuestions.Where(a => a.ExamId == eId).Count();
                model.Listing = new List<StudentExamViewModel>();
                StudentExam studentExam = db.StudentExams.Where(a => a.StudentId == sId && a.ExamId == eId).FirstOrDefault();
                model.ResultView = GetResultViewModel(eId, sId, studentExam, totalQuestion);
                for (int i = 1; i <= totalQuestion; i++)
                {
                    StudentExamViewModel studentExamView = new StudentExamViewModel();
                    studentExamView = GetStudentExamViewModel(eId, sId, i);
                    model.Listing.Add(studentExamView);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }

            return View(model);
        }

        [Authorize(Roles = "System Admin, Instructor")]
        public void CheckStudentQuestionOrder(string eId, string sId, int? totalQuestions)
        {
            //if exam is set to randomize question order, but student question order is not saved yet, then save now
            bool saved = db.StudentQuestionOrders.Where(a => a.ExamId == eId && a.StudentId == sId).Any();
            if (!saved)
            {
                StudentQuestionOrder studentQuestionOrder = new StudentQuestionOrder();
                studentQuestionOrder.Id = Guid.NewGuid().ToString();
                studentQuestionOrder.StudentId = sId;
                studentQuestionOrder.ExamId = eId;
                int[] randomNumbers = DataConverter.GetNumberArrayInRandomOrder(totalQuestions.Value);
                studentQuestionOrder.QuestionOrder = DataConverter.ConvertIntArrayToString(randomNumbers);
                db.StudentQuestionOrders.Add(studentQuestionOrder);
                db.SaveChanges();
            }
        }

        [Authorize(Roles = "System Admin, Instructor, Student")]
        public StudentExamViewModel GetStudentExamViewModel(string examId, string studentId, int? num)
        {
            StudentExamViewModel model = new StudentExamViewModel();
            model.Id = examId;
            bool? randomized = db.Exams.Where(a => a.Id == examId).Select(a => a.RandomizeQuestions).FirstOrDefault() ?? false;
            num = num ?? 1;
            model.QuestionNumber = num;
            string questionId = Guid.Empty.ToString();
            if (randomized == true)
            {
                string order = db.StudentQuestionOrders.Where(a => a.ExamId == examId && a.StudentId == studentId).Select(a => a.QuestionOrder).FirstOrDefault();
                int currentOrder = Convert.ToInt32(order.Split(',')[num.Value - 1]);
                var studentQuestion = db.ExamQuestions.Where(a => a.ExamId == examId && a.QuestionOrder == currentOrder).Select(a => new { a.QuestionId, a.Mark }).FirstOrDefault();
                questionId = studentQuestion?.QuestionId ?? "";
                model.Mark = studentQuestion?.Mark ?? 0;
            }
            else
            {
                var studentQuestion = db.ExamQuestions.Where(a => a.ExamId == examId && a.QuestionOrder == num).Select(a => new { a.QuestionId, a.Mark }).FirstOrDefault();
                questionId = studentQuestion?.QuestionId ?? "";
                model.Mark = studentQuestion?.Mark ?? 0;
            }
            model.QuestionId = questionId;
            model.StudentId = studentId;
            model.ExamId = examId;
            var questionModel = db.Questions.Where(a => a.Id == model.QuestionId).Select(a => new { Title = a.QuestionTitle, TypeId = a.QuestionTypeId }).FirstOrDefault();
            model.QuestionText = questionModel?.Title ?? "";
            if (questionModel != null)
            {
                model.QuestionType = db.QuestionTypes.Where(a => a.Id == questionModel.TypeId).Select(a => a.Code).ToList().FirstOrDefault();
            }
            else
            {
                model.QuestionType = "";
            }
            model.ImageFileName = db.QuestionAttachments.Where(a => a.QuestionId == model.QuestionId).Select(a => a.UniqueFileName).FirstOrDefault();

            var studentAnswerModel = db.StudentAnswerCloneds.Where(a => a.QuestionId == questionId && a.ExamId == examId && a.StudentId == studentId)
                .Select(a => new
                {
                    Id = a.Id,
                    AnswerId = a.AnswerId,
                    AnswerText = a.AnswerText,
                    MarksObtained = a.MarksObtained
                }).FirstOrDefault();
            model.StudentAnswerId = studentAnswerModel?.Id ?? "";
            model.MarksObtained = studentAnswerModel?.MarksObtained ?? 0;
            if (model.QuestionType == "MCQ")
            {
                string answerid = studentAnswerModel?.AnswerId ?? "";
                model.SelectedAnswerId = answerid;
                model.AnswerList = db.Answers.Where(a => a.QuestionId == model.QuestionId)
                    .Select(a => new AnswerOption
                    {
                        Id = a.Id,
                        Text = a.AnswerText,
                        Order = a.AnswerOrder,
                        Selected = (a.Id == answerid) ? true : false,
                        IsCorrect = a.IsCorrect
                    }).ToList();
            }
            else
            {
                model.AnswerText = studentAnswerModel?.AnswerText ?? "";
                model.CorrectAnswerText = db.Answers.Where(a => a.QuestionId == model.QuestionId).Select(a => a.AnswerText).ToList();
            }

            return model;
        }

        [Authorize(Roles = "System Admin, Instructor, Student")]
        public ResultViewModel GetResultViewModel(string eId, string sId, StudentExam studentExam, int? totalQuestion = 0)
        {
            ResultViewModel model = new ResultViewModel();
            model.ExamId = eId;
            model.StudentId = sId;
            var exam = db.Exams.Where(a => a.Id == eId).Select(a => new { MarkToPass = a.MarksToPass, ExamName = a.Name, ReleaseAnswer = a.ReleaseAnswer, StartDate = a.StartDate, Duration = a.Duration }).FirstOrDefault();
            decimal? scoreToPass = exam?.MarkToPass;
            model.ExamName = exam?.ExamName;
            model.ReleaseAnswer = exam?.ReleaseAnswer ?? false;
            model.TotalQuestions = (totalQuestion == null || totalQuestion == 0) ? db.ExamQuestions.Where(a => a.ExamId == eId).Count() : totalQuestion;
            model.StudentName = db.UserProfiles.Where(a => a.AspNetUserId == sId).Select(a => a.FullName).FirstOrDefault();
            model.TotalScore = (from t1 in db.ExamQuestions
                                where t1.ExamId == eId
                                select t1.Mark).Sum();
            model.AnsweredCorrect = db.StudentAnswerCloneds.Where(a => a.StudentId == sId && a.ExamId == eId && a.MarksObtained > 0).Count();
            model.ScoreToPass = scoreToPass;
            if (studentExam == null)
            {
                studentExam = new StudentExam();
            }
            model.YourScore = studentExam.Result ?? db.StudentAnswerCloneds.Where(a => a.StudentId == sId && a.ExamId == eId && a.MarksObtained > 0).Select(a => a.MarksObtained).Sum();
            model.Passed = studentExam.Passed ?? (model.YourScore >= model.ScoreToPass) ? true : false;
            model.StartDateTime = studentExam.IsoUtcStartedOn ?? "";
            model.EndDateTime = studentExam.IsoUtcEndedOn ?? "";
            studentExam.StartedOn = (studentExam.StartedOn == null) ? exam.StartDate : studentExam.StartedOn;
            if (studentExam.EndedOn != null && studentExam.StartedOn != null)
            {
                model.TimeTaken = Math.Round((studentExam.EndedOn - studentExam.StartedOn).Value.TotalMinutes, 2).ToString();
            }
            else
            {
                model.TimeTaken = exam.Duration.ToString();
            }
            return model;
        }

        [Authorize(Roles = "System Admin, Instructor")]
        public async Task<IActionResult> GetPartialViewListing(string sort, string search, int? pg, int? size)
        {
            try
            {
                List<ColumnHeader> headers = new List<ColumnHeader>();
                if (string.IsNullOrEmpty(sort))
                {
                    sort = ExamResultListConfig.DefaultSortOrder;
                }
                headers = ListUtil.GetColumnHeaders(ExamResultListConfig.DefaultColumnHeaders, sort);
                var list = ReadExamResults();
                string searchMessage = ExamResultListConfig.SearchMessage;
                list = ExamResultListConfig.PerformSearch(list, search);
                list = ExamResultListConfig.PerformSort(list, sort);
                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = pg ?? 1;
                ViewData["CurrentSearch"] = search;
                int? total = list.Count();
                int? defaultSize = ExamResultListConfig.DefaultPageSize;
                size = size == 0 || size == null ? (defaultSize != -1 ? defaultSize : total) : size == -1 ? total : size;
                ViewData["CurrentSize"] = size;
                PaginatedList<ExamResultViewModel> result = await PaginatedList<ExamResultViewModel>.CreateAsync(list, pg ?? 1, size.Value, total.Value, headers, searchMessage);
                return PartialView("~/Views/Result/_MainList.cshtml", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return PartialView("~/Views/Shared/Error.cshtml", null);
        }


        [Authorize(Roles = "System Admin, Instructor")]
        public IQueryable<ExamResultViewModel> ReadExamResults()
        {
            IQueryable<string> exams = Enumerable.Empty<string>().AsQueryable();
            DateTime? now = util.GetSystemTimeZoneDateTimeNow();
            if (User.IsInRole("System Admin"))
            {
                exams = db.StudentExams.OrderBy(a => a.StartedOn).Select(a => a.ExamId).Distinct();
            }
            else if (User.IsInRole("Instructor"))
            {
                string teacherId = _userManager.GetUserId(User);
                exams = db.Exams.Where(x => x.CreatedBy == teacherId).Join(db.StudentExams, x => x.Id, y => y.ExamId, (x, y) => x.Id).Distinct();
            }
            var result = exams.Join(db.Exams, eId => eId, exam => exam.Id, (eId, exam) => new { Exam = exam })
               .Select(a => new ExamResultViewModel
               {
                   Id = a.Exam.Id,
                   ExamName = a.Exam.Name,
                   MarksToPass = a.Exam.MarksToPass,
                   StudentPassed = db.StudentExams.Count(se => se.ExamId == a.Exam.Id && se.Passed == true),
                   StudentFailed = db.StudentExams.Count(se => se.ExamId == a.Exam.Id && se.Passed == false),
                   StudentNotStarted = db.ExamClassHubs.Where(ec => ec.ExamId == a.Exam.Id)
                                       .Join(db.StudentClasses, ec => ec.ClassHubId, sc => sc.ClassId, (ec, sc) => sc.StudentId).Distinct()
                                       .Except(db.StudentExams.Where(se => se.ExamId == a.Exam.Id).Select(se => se.StudentId)).Count(),
                   StartDate = a.Exam.StartDate,
                   StartDateIsoUtc = a.Exam.IsoUtcStartDate
               });
            return result;
        }


        [Authorize(Roles = "System Admin, Instructor")]
        public IActionResult StudentResult(string Id)
        {
            ExamViewModel model = new ExamViewModel();
            model.Id = Id;
            var detail = db.Exams.Where(a => a.Id == Id).Select(a => new { Name = a.Name, MarksToPass = a.MarksToPass, Duration = a.Duration, IsoUtcStartDate = a.IsoUtcStartDate, IsoUtcEndDate = a.IsoUtcEndDate, CreatedBy = a.CreatedBy }).FirstOrDefault();
            model.Name = detail?.Name ?? "";
            model.MarksToPass = detail?.MarksToPass ?? 0;
            model.Duration = detail?.Duration ?? 0;
            model.StartDateIsoUtc = detail?.IsoUtcStartDate ?? "";
            model.EndDateIsoUtc = detail?.IsoUtcEndDate ?? "";
            model.CreatedAndModified = util.GetCreatedAndModified(detail?.CreatedBy ?? "", "", "", "");
            model.ClassIdList = new List<string>();
            model.ClassIdList = (from t1 in db.ExamClassHubs
                                 join t2 in db.ClassHubs on t1.ClassHubId equals t2.Id
                                 where t1.ExamId == Id
                                 select t2.Name).ToList();
            model.ClassName = model.ClassIdList.Count > 0 ? String.Join(",", model.ClassIdList) : "";
            return View(model);
        }

        [Authorize(Roles = "System Admin, Instructor")]
        public async Task<IActionResult> GetPartialViewStudentResultListing(string Id, string sort, string search, int? pg, int? size)
        {
            try
            {
                List<ColumnHeader> headers = new List<ColumnHeader>();
                if (string.IsNullOrEmpty(sort))
                {
                    sort = ResultByExamListConfig.DefaultSortOrder;
                }
                headers = ListUtil.GetColumnHeaders(ResultByExamListConfig.DefaultColumnHeaders, sort);
                var list = ReadResultsByExamId(Id);
                string searchMessage = ResultByExamListConfig.SearchMessage;
                list = ResultByExamListConfig.PerformSearch(list, search);
                list = ResultByExamListConfig.PerformSort(list, sort);
                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = pg ?? 1;
                ViewData["CurrentSearch"] = search;
                int? total = list.Count();
                int? defaultSize = ResultByExamListConfig.DefaultPageSize;
                size = size == 0 || size == null ? (defaultSize != -1 ? defaultSize : total) : size == -1 ? total : size;
                ViewData["CurrentSize"] = size;
                PaginatedList<ResultViewModel> result = await PaginatedList<ResultViewModel>.CreateAsync(list, pg ?? 1, size.Value, total.Value, headers, searchMessage);
                return PartialView("~/Views/Result/_ResultsByExam.cshtml", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return PartialView("~/Views/Shared/Error.cshtml", null);
        }

        [Authorize(Roles = "System Admin, Instructor")]
        public IQueryable<ResultViewModel> ReadResultsByExamId(string Id)
        {
            var result = from t1 in db.StudentExams
                         join t2 in db.UserProfiles on t1.StudentId equals t2.AspNetUserId
                         where t1.ExamId == Id
                         select new ResultViewModel
                         {
                             ExamId = t1.ExamId,
                             StudentId = t1.StudentId,
                             StudentName = t2.FullName,
                             Passed = t1.Passed,
                             TotalScore = t1.Result,
                             StudentClassNameList = (from t3 in db.StudentClasses
                                                     join t4 in db.ClassHubs on t3.ClassId equals t4.Id
                                                     join t5 in db.ExamClassHubs on t4.Id equals t5.ClassHubId
                                                     where t3.StudentId == t1.StudentId && t5.ExamId == Id
                                                     select t4.Name).Distinct().ToList()
                         };
            result = result.Select(a => new ResultViewModel
            {
                ExamId = a.ExamId,
                StudentId = a.StudentId,
                StudentName = a.StudentName,
                Passed = a.Passed,
                TotalScore = a.TotalScore,
                StudentClassNameList = a.StudentClassNameList
            });

            return result;
        }

    }
}
