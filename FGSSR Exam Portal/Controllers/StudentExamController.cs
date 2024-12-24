using EMSYS.Models;
using EMSYS.Resources;
using EMSYS.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static EMSYS.Models.ProjectEnum;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using EMSYS.Data;

namespace EMSYS.Controllers
{
    [Authorize]
    public class StudentExamController : Controller
    {
        private EMSYSdbContext db;
        private Util util;
        private readonly UserManager<AspNetUsers> _userManager;
        private ErrorLoggingService _logger;

        public StudentExamController(EMSYSdbContext db, Util util, UserManager<AspNetUsers> userManager, ErrorLoggingService logger)
        {
            this.db = db;
            this.util = util;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult CurrentExam()
        {
            return View();
        }
        public IActionResult UpcomingExam()
        {
            return View();
        }
        public IActionResult PastExam()
        {
            return View();
        }

        public async Task<IActionResult> GetPartialViewListing(string status, string sort, string search, int? pg, int? size)
        {
            try
            {
                List<ColumnHeader> headers = new List<ColumnHeader>();
                if (string.IsNullOrEmpty(sort))
                {
                    sort = UpcomingCurrentPastExamListConfig.DefaultSortOrder;
                }
                headers = ListUtil.GetColumnHeaders(UpcomingCurrentPastExamListConfig.DefaultColumnHeaders, sort);
                var list = ReadStudentExams(status);
                string searchMessage = UpcomingCurrentPastExamListConfig.SearchMessage;
                list = UpcomingCurrentPastExamListConfig.PerformSearch(list, search);
                list = UpcomingCurrentPastExamListConfig.PerformSort(list, sort);
                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = pg ?? 1;
                ViewData["CurrentSearch"] = search;
                int? total = list.Count();
                int? defaultSize = UpcomingCurrentPastExamListConfig.DefaultPageSize;
                size = size == 0 || size == null ? (defaultSize != -1 ? defaultSize : total) : size == -1 ? total : size;
                ViewData["CurrentSize"] = size;
                ViewData["ExamStatus"] = status;
                PaginatedList<ExamViewModel> result = await PaginatedList<ExamViewModel>.CreateAsync(list, pg ?? 1, size.Value, total.Value, headers, searchMessage);
                return PartialView("~/Views/StudentExam/_MainList.cshtml", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return PartialView("~/Views/Shared/Error.cshtml", null);
        }

        public IQueryable<ExamViewModel> ReadStudentExams(string status)
        {
            string currentUserId = _userManager.GetUserId(User);
            List<string> studentClass = db.StudentClasses.Where(a => a.StudentId == currentUserId).Select(a => a.ClassId).ToList();
            string publishedId = db.GlobalOptionSets.Where(a => a.Code == ExamStatus.Published.ToString()).Select(a => a.Id).FirstOrDefault();

            IQueryable<ExamViewModel> list = Enumerable.Empty<ExamViewModel>().AsQueryable();
            DateTime? now = util.GetSystemTimeZoneDateTimeNow();
            List<string> studentTakenExams = new List<string>();

            if (status == "current")
            {
                studentTakenExams = db.StudentExams.Where(a => a.EndedOn != null && a.StudentId == currentUserId).Select(a => a.ExamId).ToList();
                list = (from t1 in db.Exams
                        join t2 in db.ExamClassHubs on t1.Id equals t2.ExamId
                        let totalQuestion = db.ExamQuestions.Where(a => a.ExamId == t1.Id).Count()
                        where t1.IsPublished == true && t1.StartDate <= now && t1.EndDate >= now &&
                        studentTakenExams.Contains(t1.Id) == false && studentClass.Contains(t2.ClassHubId)
                        select new ExamViewModel
                        {
                            Id = t1.Id,
                            Name = t1.Name,
                            Duration = t1.Duration,
                            StartDate = t1.StartDate,
                            StartDateIsoUtc = t1.IsoUtcStartDate,
                            EndDate = t1.EndDate,
                            EndDateIsoUtc = t1.IsoUtcEndDate,
                            StudentExamStatus = "Current",
                            TotalQuestions = totalQuestion
                        }).Distinct();
            }
            else if (status == "past")
            {
                studentTakenExams = db.StudentExams.Where(a => a.EndedOn != null && a.StudentId == currentUserId).Select(a => a.ExamId).ToList();
                list = (from t1 in db.Exams.AsNoTracking()
                        join t2 in db.ExamClassHubs.AsNoTracking() on t1.Id equals t2.ExamId
                        join t3 in db.StudentExams on t1.Id equals t3.ExamId into g1
                        from t4 in g1.DefaultIfEmpty()
                        let totalQuestion = db.ExamQuestions.Where(a => a.ExamId == t1.Id).Count()
                        where (t1.StartDate <= now && t1.EndDate <= now) || t4.StudentId == currentUserId
                        select new ExamViewModel
                        {
                            Id = t1.Id,
                            Name = t1.Name,
                            Duration = t1.Duration,
                            StartDate = t4 != null ? t4.StartedOn : t1.StartDate,
                            StartDateIsoUtc = t4 != null ? t4.IsoUtcStartedOn : t1.IsoUtcStartDate,
                            EndDate = t4 != null ? t4.EndedOn : t1.EndDate,
                            EndDateIsoUtc = t4 != null ? t4.IsoUtcEndedOn : t1.IsoUtcEndDate,
                            StudentExamStatus = "Past",
                            TotalQuestions = totalQuestion,
                            Result = t4 != null ? t4.Result : 0
                        }).Distinct();
            }
            else
            {
                //upcoming
                list = (from t1 in db.Exams.AsNoTracking()
                        join t2 in db.ExamClassHubs.AsNoTracking() on t1.Id equals t2.ExamId
                        let totalQuestion = db.ExamQuestions.Where(a => a.ExamId == t1.Id).Count()
                        where t1.StartDate >= now && t1.IsPublished == true && studentClass.Contains(t2.ClassHubId)
                        select new ExamViewModel
                        {
                            Id = t1.Id,
                            Name = t1.Name,
                            Duration = t1.Duration,
                            StartDate = t1.StartDate,
                            StartDateIsoUtc = t1.IsoUtcStartDate,
                            EndDate = t1.EndDate,
                            EndDateIsoUtc = t1.IsoUtcEndDate,
                            StudentExamStatus = "Upcoming",
                            TotalQuestions = totalQuestion
                        }).Distinct();
            }

            return list;
        }

        public IActionResult TimesUp(string eId, string sId)
        {
            ExamViewModel model = new ExamViewModel();
            model.Id = eId;
            model.StudentId = sId;
            model.Name = db.Exams.Where(a => a.Id == eId).Select(a => a.Name).FirstOrDefault();
            return View(model);
        }

        public IActionResult Exit(string eId, string sId)
        {
            StudentExam studentExam = SaveStudentEndTime(eId, sId);
            return RedirectToAction("studentquestionanswer", "result", new { eId = eId, sId = sId });
        }

        public StudentExam SaveStudentEndTime(string eId, string sId)
        {
            decimal? totalMarksObtained = db.StudentAnswers.Where(a => a.StudentId == sId && a.ExamId == eId).Select(a => a.MarksObtained).Sum();
            StudentExam studentExam = db.StudentExams.Where(a => a.StudentId == sId && a.ExamId == eId).FirstOrDefault();
            if (studentExam.EndedOn == null)
            {
                studentExam.Result = totalMarksObtained;
                decimal? passingMark = db.Exams.Where(a => a.Id == eId).Select(a => a.MarksToPass).FirstOrDefault() ?? 0;
                studentExam.Passed = (totalMarksObtained >= passingMark) ? true : false;
                studentExam.EndedOn = util.GetSystemTimeZoneDateTimeNow();
                studentExam.IsoUtcEndedOn = util.GetIsoUtcNow();
                db.Entry(studentExam).State = EntityState.Modified;
                db.SaveChanges();
            }
            return studentExam;
        }

        public ResultViewModel GetResultViewModel(string eId, string sId, StudentExam studentExam)
        {
            ResultViewModel model = new ResultViewModel();
            model.ExamId = eId;
            model.StudentId = sId;
            var exam = db.Exams.Where(a => a.Id == eId).Select(a => new { MarkToPass = a.MarksToPass, ExamName = a.Name, ReleaseAnswer = a.ReleaseAnswer }).FirstOrDefault();
            decimal? scoreToPass = exam?.MarkToPass;
            model.ExamName = exam?.ExamName;
            model.ReleaseAnswer = exam?.ReleaseAnswer;
            model.TotalQuestions = db.ExamQuestions.Where(a => a.ExamId == eId).Count();
            model.Passed = studentExam.Passed;
            model.StudentName = db.UserProfiles.Where(a => a.AspNetUserId == sId).Select(a => a.FullName).FirstOrDefault();
            model.YourScore = studentExam.Result;
            model.TotalScore = (from t1 in db.ExamQuestions
                                where t1.ExamId == eId
                                select t1.Mark).Sum();
            model.AnsweredCorrect = db.StudentAnswerCloneds.Where(a => a.StudentId == sId && a.ExamId == eId && a.MarksObtained > 0).Count();
            model.ScoreToPass = scoreToPass;
            model.StartDateTime = studentExam.IsoUtcStartedOn;
            model.EndDateTime = studentExam.IsoUtcEndedOn;
            double mins = Math.Round((studentExam.EndedOn - studentExam.StartedOn).Value.TotalMinutes, 2);
            model.TimeTaken = mins.ToString();
            return model;
        }

        public IActionResult ConfirmTakeExam(string eId)
        {
            if (ExamUnpublished(eId) == true)
            {
                return RedirectToAction("examended");
            }
            ExamViewModel model = new ExamViewModel();
            var exam = db.Exams.Where(a => a.Id == eId).Select(a => new { Name = a.Name, Duration = a.Duration, RandomizeQuestions = a.RandomizeQuestions, Description = a.Description }).FirstOrDefault();
            model.Name = exam?.Name ?? "";
            model.Description = exam?.Description ?? "";
            model.Duration = exam?.Duration;
            model.TotalQuestions = db.ExamQuestions.Where(a => a.ExamId == eId).Count();
            model.Id = eId;
            model.StudentId = _userManager.GetUserId(User);
            model.AlreadyStarted = db.StudentExams.Where(a => a.StudentId == model.StudentId && a.ExamId == model.Id).Any();
            return View(model);
        }

        public IActionResult ExamEnded()
        {
            return View();
        }

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

        public bool ExamUnpublished(string eId)
        {
            bool? isPublished = db.Exams.Where(a => a.Id == eId).Select(a => a.IsPublished).FirstOrDefault();
            if (isPublished == false)
            {
                return true;
            }
            return false;
        }

        public IActionResult TakeExam(string eId, string sId, int? num)
        {
            if (ExamUnpublished(eId) == true)
            {
                return RedirectToAction("examended");
            }
            if (num == null)
            {
                num = 1;
            }
            StudentExamViewModel model = new StudentExamViewModel();
            if (!string.IsNullOrEmpty(eId))
            {
                if (!StartTimeSaved(eId))
                {
                    SaveStudentStartTime(eId);
                }
                if (string.IsNullOrEmpty(sId))
                {
                    sId = _userManager.GetUserId(User);
                }

                //even if student change url to any question number that they want, we will still return them the current question that he/she need to answer
                int answered = db.StudentAnswers.Where(a => a.ExamId == eId && a.StudentId == sId).Count();
                //if student change url num parameter to 5, but he/she have answered until question 2 only, bring the student back to the question 2
                if (num != (answered + 1) && num != answered)
                {
                    if (num == (answered + 2))
                    {
                        num = answered + 1;
                    }
                    else
                    {
                        num = answered;
                    }
                }

                //get the details to be displayed on the screen
                model = GetStudentExamViewModel(eId, sId, num);
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult TakeExam(StudentExamViewModel model)
        {
            try
            {
                if (ExamUnpublished(model.Id) == true)
                {
                    return RedirectToAction("examended");
                }
                if (string.IsNullOrEmpty(model.AnswerId) && string.IsNullOrEmpty(model.AnswerText))
                {
                    TempData["NotifyFailed"] = Resource.PleaseSubmitAnAnswer;
                }
                else
                {
                    SaveStudentAnswer(model);
                    TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
                }
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("takeexam", new { eId = model.Id, sId = model.StudentId, num = model.QuestionNumber });
        }

        public bool StartTimeSaved(string Id)
        {
            bool saved = false;
            string currentStudentId = _userManager.GetUserId(User);
            saved = db.StudentExams.Where(a => a.StudentId == currentStudentId && a.ExamId == Id).Any();
            return saved;
        }

        //student clicked start exam
        public void SaveStudentStartTime(string Id)
        {
            StudentExam studentExam = new StudentExam();
            studentExam.Id = Guid.NewGuid().ToString();
            studentExam.ExamId = Id;
            studentExam.StudentId = _userManager.GetUserId(User);
            studentExam.StartedOn = util.GetSystemTimeZoneDateTimeNow();
            studentExam.IsoUtcStartedOn = util.GetIsoUtcNow();
            db.StudentExams.Add(studentExam);
            db.SaveChanges();
        }

        public string GetQuestionId(string eId, string sId, int? promptedQuestionNumber)
        {
            int? totalQuestion = db.ExamQuestions.Where(a => a.ExamId == eId).Count();
            bool randomized = db.Exams.Where(a => a.Id == eId).Select(a => a.RandomizeQuestions).FirstOrDefault() ?? false;
            string questionId = Guid.Empty.ToString();
            if (randomized == true)
            {
                CheckStudentQuestionOrder(eId, sId, totalQuestion);
                string order = db.StudentQuestionOrders.Where(a => a.ExamId == eId && a.StudentId == sId).Select(a => a.QuestionOrder).FirstOrDefault();
                int currentOrder = Convert.ToInt32(order.Split(',')[promptedQuestionNumber.Value - 1]);
                questionId = db.ExamQuestions.Where(a => a.ExamId == eId && a.QuestionOrder == currentOrder).Select(a => a.QuestionId).FirstOrDefault();
            }
            else
            {
                questionId = db.ExamQuestions.Where(a => a.ExamId == eId && a.QuestionOrder == promptedQuestionNumber).Select(a => a.QuestionId).FirstOrDefault();
            }
            return questionId;
        }

        public StudentExamViewModel GetStudentExamViewModel(string examId, string studentId, int? num)
        {
            StudentExamViewModel model = new StudentExamViewModel();
            model.Id = examId;
            var examModel = db.Exams.Where(a => a.Id == examId).Select(a => new { RandomizeQuestions = a.RandomizeQuestions, Name = a.Name, Duration = a.Duration, ReleaseAnswer = a.ReleaseAnswer }).FirstOrDefault();
            model.ExamName = examModel?.Name ?? "";
            model.TotalQuestion = db.ExamQuestions.Where(a => a.ExamId == examId).Count();
            model.Duration = examModel.Duration;
            model.ReleaseAnswer = examModel.ReleaseAnswer;
            model.AnsweredUntil = db.StudentAnswers.Where(a => a.StudentId == studentId && a.ExamId == examId).Count();
            bool? randomized = examModel.RandomizeQuestions ?? false;
            num = num ?? 1;
            model.QuestionNumber = num;
            string questionId = Guid.Empty.ToString();
            if (randomized == true)
            {
                CheckStudentQuestionOrder(examId, studentId, model.TotalQuestion);
                string order = db.StudentQuestionOrders.Where(a => a.ExamId == examId && a.StudentId == studentId).Select(a => a.QuestionOrder).FirstOrDefault();
                int currentOrder = Convert.ToInt32(order.Split(',')[num.Value - 1]);
                questionId = db.ExamQuestions.Where(a => a.ExamId == examId && a.QuestionOrder == currentOrder).Select(a => a.QuestionId).FirstOrDefault();
                model.Mark = db.ExamQuestions.Where(a => a.ExamId == examId && a.QuestionOrder == currentOrder).Select(a => a.Mark).FirstOrDefault();
            }
            else
            {
                questionId = db.ExamQuestions.Where(a => a.ExamId == examId && a.QuestionOrder == num).Select(a => a.QuestionId).FirstOrDefault();
                model.Mark = db.ExamQuestions.Where(a => a.ExamId == examId && a.QuestionOrder == num).Select(a => a.Mark).FirstOrDefault();
            }
            model.QuestionId = questionId;
            model.StudentId = studentId;
            model.ExamId = examId;
            var questionModel = db.Questions.Where(a => a.Id == model.QuestionId).Select(a => new { Title = a.QuestionTitle, TypeId = a.QuestionTypeId }).FirstOrDefault();
            model.QuestionText = questionModel?.Title ?? "";
            model.ImageFileName = db.QuestionAttachments.Where(a => a.QuestionId == model.QuestionId).Select(a => a.UniqueFileName).FirstOrDefault();
            model.QuestionType = db.QuestionTypes.Where(a => a.Id == questionModel.TypeId).Select(a => a.Code).ToList().FirstOrDefault();

            var studentAnswerModel = db.StudentAnswerCloneds.Where(a => a.QuestionId == questionId && a.ExamId == examId && a.StudentId == studentId).Select(a => new { Id = a.Id, AnswerId = a.AnswerId, AnswerText = a.AnswerText }).FirstOrDefault();
            model.StudentAnswerId = studentAnswerModel?.Id ?? "";
            if (model.QuestionType == "MCQ")
            {
                string answerid = studentAnswerModel?.AnswerId ?? "";
                model.SelectedAnswerId = answerid;
                model.AnswerList = db.Answers.Where(a => a.QuestionId == model.QuestionId)
                    .Select(a => new AnswerOption { Id = a.Id, Text = a.AnswerText, Order = a.AnswerOrder, Selected = (a.Id == answerid) ? true : false })
                    .ToList();
            }
            else
            {
                model.AnswerText = studentAnswerModel?.AnswerText ?? "";
            }

            return model;
        }

        public void SaveStudentAnswer(StudentExamViewModel model)
        {
            bool isNew = false;
            StudentAnswer studentAnswer = db.StudentAnswers.Where(a => a.ExamId == model.Id && a.StudentId == model.StudentId && a.QuestionId == model.QuestionId).FirstOrDefault();
            if (studentAnswer == null)
            {
                isNew = true;
                studentAnswer = new StudentAnswer();
                studentAnswer.Id = Guid.NewGuid().ToString();
                studentAnswer.StudentId = model.StudentId;
                studentAnswer.ExamId = model.ExamId;
                studentAnswer.QuestionId = model.QuestionId;
            }
            if (model.QuestionType == "MCQ")
            {
                studentAnswer.AnswerId = model.AnswerId;
                bool correct = db.Answers.Where(a => a.Id == model.AnswerId).Select(a => a.IsCorrect).ToList().FirstOrDefault();
                studentAnswer.MarksObtained = (correct == true) ? model.Mark : 0;
            }
            else if (model.QuestionType == "SA")
            {
                studentAnswer.AnswerText = model.AnswerText;
                List<string> correctList = db.Answers.Where(a => a.QuestionId == model.QuestionId).Select(a => a.AnswerText.ToLower()).ToList();
                if (correctList.Contains(model.AnswerText.ToLower()))
                {
                    studentAnswer.MarksObtained = model.Mark;
                }
                else
                {
                    studentAnswer.MarksObtained = 0;
                }
            }
            else
            {
                studentAnswer.AnswerText = model.AnswerText;
            }
            if (isNew)
            {
                db.StudentAnswers.Add(studentAnswer);
                db.SaveChanges();
                CloneStudentAnswer(studentAnswer);
            }
            else
            {
                db.Entry(studentAnswer).State = EntityState.Modified;
                StudentAnswerCloned studentAnswerCloned = db.StudentAnswerCloneds.Find(studentAnswer.Id);   
                studentAnswerCloned.AnswerId = studentAnswer.AnswerId;
                studentAnswerCloned.AnswerText = studentAnswer.AnswerText;
                studentAnswerCloned.MarksObtained = studentAnswer.MarksObtained;
                db.Entry(studentAnswerCloned).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void CloneStudentAnswer(StudentAnswer studentAnswer)
        {
            if (studentAnswer != null)
            {
                StudentAnswerCloned studentAnswerCloned = new StudentAnswerCloned();
                studentAnswerCloned.Id = studentAnswer.Id;
                studentAnswerCloned.ExamId = studentAnswer.ExamId;
                studentAnswerCloned.StudentId = studentAnswer.StudentId;
                studentAnswerCloned.QuestionId = studentAnswer.QuestionId;
                studentAnswerCloned.AnswerId = studentAnswer.AnswerId;
                studentAnswerCloned.AnswerText = studentAnswer.AnswerText;
                studentAnswerCloned.MarksObtained = studentAnswer.MarksObtained;
                db.StudentAnswerCloneds.Add(studentAnswerCloned);
                db.SaveChanges();
            }
        }

        //student clicked exit and end the exam
        public void StudentExitExam(StudentExamViewModel model)
        {
            StudentExam studentExam = db.StudentExams.Find(model.Id);
            studentExam.EndedOn = util.GetSystemTimeZoneDateTimeNow();
            studentExam.IsoUtcEndedOn = util.GetIsoUtcNow();
            db.Entry(studentExam).State = EntityState.Modified;
            db.SaveChanges();
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
