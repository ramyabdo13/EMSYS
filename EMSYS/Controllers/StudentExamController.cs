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

        [HttpGet]
        public IActionResult CurrentExam()
        {
            string currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return View(new List<ExamViewModel>());
            }

            List<string> studentClasses = db.StudentClasses
                .Where(a => a.StudentId == currentUserId)
                .Select(a => a.ClassId)
                .ToList();

            if (!studentClasses.Any())
            {
                return View(new List<ExamViewModel>());
            }

            DateTime? now = util.GetSystemTimeZoneDateTimeNow();
            List<string> takenExams = db.StudentExams
                .Where(a => a.EndedOn != null && a.StudentId == currentUserId)
                .Select(a => a.ExamId)
                .ToList();

            var allExams = db.Exams.AsNoTracking().ToList();
            var examClassConnections = db.ExamClassHubs.AsNoTracking().ToList();

            var results = new List<ExamViewModel>();

            foreach (var exam in allExams)
            {
                if (exam.IsPublished == true &&
                    exam.StartDate <= now &&
                    exam.EndDate >= now &&
                    !takenExams.Contains(exam.Id))
                {
                    foreach (var connection in examClassConnections)
                    {
                        if (connection.ExamId == exam.Id && studentClasses.Contains(connection.ClassHubId))
                        {
                            int totalQuestion = db.ExamQuestions.Count(q => q.ExamId == exam.Id);

                            results.Add(new ExamViewModel
                            {
                                Id = exam.Id,
                                Name = exam.Name,
                                Duration = exam.Duration,
                                StartDate = exam.StartDate,
                                EndDate = exam.EndDate,
                                TotalQuestions = totalQuestion
                            });
                            break;
                        }
                    }
                }
            }

            return View(results);
        }
        [HttpGet]
        public IActionResult UpcomingExam()
        {
            string currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return View(new List<ExamViewModel>());
            }

            List<string> studentClasses = db.StudentClasses
                .Where(a => a.StudentId == currentUserId)
                .Select(a => a.ClassId)
                .ToList();

            if (!studentClasses.Any())
            {
                return View(new List<ExamViewModel>());
            }

            DateTime? now = util.GetSystemTimeZoneDateTimeNow();
            var allExams = db.Exams.AsNoTracking().ToList();
            var examClassConnections = db.ExamClassHubs.AsNoTracking().ToList();

            var results = new List<ExamViewModel>();

            foreach (var exam in allExams)
            {
                if (exam.IsPublished == true && exam.StartDate > now)
                {
                    foreach (var connection in examClassConnections)
                    {
                        if (connection.ExamId == exam.Id && studentClasses.Contains(connection.ClassHubId))
                        {
                            int totalQuestion = db.ExamQuestions.Count(q => q.ExamId == exam.Id);

                            results.Add(new ExamViewModel
                            {
                                Id = exam.Id,
                                Name = exam.Name,
                                Duration = exam.Duration,
                                StartDate = exam.StartDate,
                                EndDate = exam.EndDate,
                                TotalQuestions = totalQuestion,
                                CreatedOn = exam.CreatedOn,
                                ModifiedOn = exam.ModifiedOn
                            });
                            break;
                        }
                    }
                }
            }

            return View(results);
        }
        [HttpGet]
        public IActionResult PastExam()
        {
            string currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return View(new List<ExamViewModel>());
            }

            List<string> studentClasses = db.StudentClasses
                .Where(a => a.StudentId == currentUserId)
                .Select(a => a.ClassId)
                .ToList();

            if (!studentClasses.Any())
            {
                return View(new List<ExamViewModel>());
            }

            DateTime? now = util.GetSystemTimeZoneDateTimeNow();
            var studentExams = db.StudentExams
                .Where(se => se.StudentId == currentUserId && se.EndedOn != null)
                .ToList();

            var results = new List<ExamViewModel>();

            foreach (var studentExam in studentExams)
            {
                var exam = db.Exams.AsNoTracking().FirstOrDefault(e => e.Id == studentExam.ExamId);
                if (exam != null)
                {
                    int totalQuestion = db.ExamQuestions.Count(q => q.ExamId == exam.Id);

                    results.Add(new ExamViewModel
                    {
                        Id = exam.Id,
                        Name = exam.Name,
                        Duration = exam.Duration,
                        StartDate = studentExam.StartedOn ?? exam.StartDate,
                        EndDate = studentExam.EndedOn ?? exam.EndDate,
                        TotalQuestions = totalQuestion,
                        Result = studentExam.Result,
                        CreatedOn = exam.CreatedOn,
                        ModifiedOn = exam.ModifiedOn,
                        StudentId = currentUserId
                    });
                }
            }

            return View(results);
        }

        public async Task<IActionResult> GetPartialViewListing(string status, string sort, string search, int? pg, int? size)
        {
            try
            {
                List<ColumnHeader> headers = null;
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
                _logger.LogError(ex, $"Error in GetPartialViewListing - Status: {status}, User: {_userManager.GetUserId(User)}");
                
                // Return a user-friendly error message as HTML content
                return Content(@"
                    <div class='alert alert-danger text-center mt-4'>
                        <strong>Error loading exam data</strong>
                        <p>We encountered a problem while retrieving your exams.</p>
                    </div>");
            }
        }

        public IQueryable<ExamViewModel> ReadStudentExams(string status)
        {
            string currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Enumerable.Empty<ExamViewModel>().AsQueryable();
            }
            
            List<string> studentClass = db.StudentClasses.Where(a => a.StudentId == currentUserId).Select(a => a.ClassId).ToList();
            DateTime? now = util.GetSystemTimeZoneDateTimeNow();
            List<string> studentTakenExams = new List<string>();

            // Create an empty result set
            IQueryable<ExamViewModel> list = Enumerable.Empty<ExamViewModel>().AsQueryable();

            try
            {
                if (status == "upcoming")
                {
                    // Get all exams
                    var exams = db.Exams.AsNoTracking().ToList();
                    
                    // Get all exam-class connections
                    var examClassConnections = db.ExamClassHubs.AsNoTracking().ToList();
                    
                    // Create a list to hold results
                    var results = new List<ExamViewModel>();
                    
                    // Filter manually to avoid complex LINQ-to-SQL translation issues
                    foreach (var exam in exams)
                    {
                        if (exam.IsPublished == true && exam.StartDate > now)
                        {
                            // Check if this exam is connected to any of student's classes
                            foreach (var connection in examClassConnections)
                            {
                                if (connection.ExamId == exam.Id && studentClass.Contains(connection.ClassHubId))
                                {
                                    // Count questions for this exam
                                    int totalQuestion = db.ExamQuestions.Count(q => q.ExamId == exam.Id);
                                    
                                    // This is an upcoming exam for this student
                                    results.Add(new ExamViewModel
                                    {
                                        Id = exam.Id,
                                        Name = exam.Name,
                                        Duration = exam.Duration,
                                        StartDate = exam.StartDate,
                                        StartDateIsoUtc = exam.IsoUtcStartDate,
                                        EndDate = exam.EndDate,
                                        EndDateIsoUtc = exam.IsoUtcEndDate,
                                        StudentExamStatus = "Upcoming",
                                        TotalQuestions = totalQuestion
                                    });
                                    break; // Found a connection, no need to check more
                                }
                            }
                        }
                    }
                    
                    // Convert to IQueryable
                    list = results.AsQueryable();
                }
                else if (status == "current")
                {
                    studentTakenExams = db.StudentExams.Where(a => a.EndedOn != null && a.StudentId == currentUserId).Select(a => a.ExamId).ToList();
                    
                    // Get all exams
                    var exams = db.Exams.ToList();
                    
                    // Get all exam-class connections
                    var examClassConnections = db.ExamClassHubs.ToList();
                    
                    // Create a list to hold results
                    var results = new List<ExamViewModel>();
                    
                    // Filter manually
                    foreach (var exam in exams)
                    {
                        if (exam.IsPublished == true && exam.StartDate <= now && exam.EndDate >= now && !studentTakenExams.Contains(exam.Id))
                        {
                            // Check if this exam is connected to any of student's classes
                            foreach (var connection in examClassConnections)
                            {
                                if (connection.ExamId == exam.Id && studentClass.Contains(connection.ClassHubId))
                                {
                                    // Count questions for this exam
                                    int totalQuestion = db.ExamQuestions.Count(q => q.ExamId == exam.Id);
                                    
                                    // This is a current exam for this student
                                    results.Add(new ExamViewModel
                                    {
                                        Id = exam.Id,
                                        Name = exam.Name,
                                        Duration = exam.Duration,
                                        StartDate = exam.StartDate,
                                        StartDateIsoUtc = exam.IsoUtcStartDate,
                                        EndDate = exam.EndDate,
                                        EndDateIsoUtc = exam.IsoUtcEndDate,
                                        StudentExamStatus = "Current",
                                        TotalQuestions = totalQuestion
                                    });
                                    break; // Found a connection, no need to check more
                                }
                            }
                        }
                    }
                    
                    // Convert to IQueryable
                    list = results.AsQueryable();
                }
                else if (status == "past")
                {
                    studentTakenExams = db.StudentExams.Where(a => a.EndedOn != null && a.StudentId == currentUserId).Select(a => a.ExamId).ToList();
                    
                    // Get all student exams for this user
                    var studentExams = db.StudentExams.Where(se => se.StudentId == currentUserId).ToList();
                    
                    // Create a list to hold results
                    var results = new List<ExamViewModel>();
                    
                    foreach (var studentExam in studentExams)
                    {
                        var exam = db.Exams.AsNoTracking().FirstOrDefault(e => e.Id == studentExam.ExamId);
                        if (exam != null && (exam.EndDate <= now || studentExam.EndedOn != null))
                        {
                            // Count questions for this exam
                            int totalQuestion = db.ExamQuestions.Count(q => q.ExamId == exam.Id);
                            
                            // This is a past exam for this student
                            results.Add(new ExamViewModel
                            {
                                Id = exam.Id,
                                Name = exam.Name,
                                Duration = exam.Duration,
                                StartDate = studentExam.StartedOn ?? exam.StartDate,
                                StartDateIsoUtc = studentExam.IsoUtcStartedOn ?? exam.IsoUtcStartDate,
                                EndDate = studentExam.EndedOn ?? exam.EndDate,
                                EndDateIsoUtc = studentExam.IsoUtcEndedOn ?? exam.IsoUtcEndDate,
                                StudentExamStatus = "Past",
                                TotalQuestions = totalQuestion,
                                Result = studentExam.Result
                            });
                        }
                    }
                    
                    // Convert to IQueryable
                    list = results.AsQueryable();
                }
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, $"{GetType().Name} Controller - ReadStudentExams Method");
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

        public IActionResult DiagnosticTest() 
        {
            try
            {
                var html = new System.Text.StringBuilder();
                html.AppendLine("<html><head><title>Database Diagnostic</title>");
                html.AppendLine("<style>body{font-family:Arial;margin:20px;} .section{margin-bottom:20px;padding:10px;border:1px solid #ccc;} h2{color:#333;} table{border-collapse:collapse;width:100%;} th,td{border:1px solid #ddd;padding:8px;text-align:left;} th{background-color:#f2f2f2;} .success{color:green;} .error{color:red;} .warning{color:orange;} .id{color:#666;font-size:0.9em;} .highlight{background-color:#ffffcc;}</style>");
                html.AppendLine("</head><body>");
                html.AppendLine("<h1>Database Diagnostic Results</h1>");
                
                // Get current user info
                string currentUserId = _userManager.GetUserId(User);
                html.AppendLine("<div class='section'>");
                html.AppendLine("<h2>User Information</h2>");
                
                try {
                    // Get basic user info
                    var user = db.AspNetUsers.Find(currentUserId);
                    var userProfile = db.UserProfiles.FirstOrDefault(p => p.AspNetUserId == currentUserId);
                    
                    html.AppendLine("<p>Current User: " + (userProfile?.FullName ?? "Unknown") + " <span class='id'>(" + currentUserId + ")</span></p>");
                    
                    if (user != null) {
                        html.AppendLine("<p>Username: " + user.UserName + "</p>");
                        html.AppendLine("<p>Email: " + user.Email + "</p>");
                    }
                    
                    if (userProfile != null) {
                        html.AppendLine("<p>Full Name: " + userProfile.FullName + "</p>");
                    }
                    
                    // Get user roles
                    try {
                        var userRoles = db.AspNetUserRoles
                            .Where(ur => ur.UserId == currentUserId)
                            .ToList();
                        
                        if (userRoles.Any()) {
                            html.AppendLine("<p>Roles: ");
                            foreach (var userRole in userRoles) {
                                var role = db.AspNetRoles.Find(userRole.RoleId);
                                if (role != null) {
                                    html.AppendLine(role.Name + " <span class='id'>(" + role.Id + ")</span>, ");
                                }
                            }
                            html.AppendLine("</p>");
                        }
                    }
                    catch (Exception ex) {
                        html.AppendLine("<p class='error'>Error getting user roles: " + ex.Message + "</p>");
                    }
                }
                catch (Exception ex) {
                    html.AppendLine("<p class='error'>Error getting user details: " + ex.Message + "</p>");
                }
                
                html.AppendLine("</div>");
                
                // SPECIFIC INVESTIGATION: Rami_st and Class 505
                html.AppendLine("<div class='section highlight'>");
                html.AppendLine("<h2>SPECIFIC INVESTIGATION: Rami_st and Class CS505</h2>");
                
                try {
                    // Find Rami_st user
                    var ramiUser = db.AspNetUsers.FirstOrDefault(u => u.UserName == "rami_st");
                    if (ramiUser != null) {
                        string ramiId = ramiUser.Id;
                        var ramiProfile = db.UserProfiles.FirstOrDefault(p => p.AspNetUserId == ramiId);
                        
                        html.AppendLine("<h3>Rami_st User Information</h3>");
                        html.AppendLine("<p>User ID: " + ramiId + "</p>");
                        html.AppendLine("<p>Username: " + ramiUser.UserName + "</p>");
                        html.AppendLine("<p>Email: " + ramiUser.Email + "</p>");
                        html.AppendLine("<p>Full Name: " + (ramiProfile?.FullName ?? "Unknown") + "</p>");
                        
                        // Check if Rami is in Class 505
                        var class505 = db.ClassHubs.FirstOrDefault(c => c.Name == "CS505" || c.Id == "CS505");
                        if (class505 == null) {
                            class505 = db.ClassHubs.FirstOrDefault(c => c.Id == "bc8c3509-aef1-488d-9e7d-e743b5cd4240");
                        }
                        
                        if (class505 != null) {
                            string class505Id = class505.Id;
                            html.AppendLine("<h3>Class CS505 Information</h3>");
                            html.AppendLine("<p>Class ID: " + class505Id + "</p>");
                            html.AppendLine("<p>Class Name: " + class505.Name + "</p>");
                            html.AppendLine("<p>Is Active: " + (class505.IsActive == true ? "Yes" : "No") + "</p>");
                            
                            // Check if Rami is enrolled in Class 505
                            bool isEnrolled = db.StudentClasses.Any(sc => sc.StudentId == ramiId && sc.ClassId == class505Id);
                            html.AppendLine("<p>Is Rami enrolled in Class CS505: <strong>" + (isEnrolled ? "Yes" : "No") + "</strong></p>");
                            
                            // Get all exams for Class 505
                            html.AppendLine("<h3>Exams for Class CS505</h3>");
                            var classExams = db.ExamClassHubs
                                .Where(ech => ech.ClassHubId == class505Id)
                                .ToList();
                            
                            if (classExams.Any()) {
                                html.AppendLine("<table>");
                                html.AppendLine("<tr><th>Exam</th><th>Start Date</th><th>End Date</th><th>Published</th><th>Status</th></tr>");
                                
                                DateTime? now = util.GetSystemTimeZoneDateTimeNow();
                                
                                foreach (var examClass in classExams) {
                                    var exam = db.Exams.Find(examClass.ExamId);
                                    if (exam != null) {
                                        string status = "";
                                        string statusClass = "";
                                        
                                        if (exam.IsPublished != true) {
                                            status = "Not Published";
                                            statusClass = "error";
                                        }
                                        else if (exam.StartDate > now) {
                                            status = "Upcoming";
                                            statusClass = "success";
                                        }
                                        else if (exam.StartDate <= now && exam.EndDate >= now) {
                                            status = "Current";
                                            statusClass = "warning";
                                        }
                                        else {
                                            status = "Past";
                                            statusClass = "";
                                        }
                                        
                                        // Check if Rami has taken this exam
                                        bool hasTaken = db.StudentExams.Any(se => se.StudentId == ramiId && se.ExamId == exam.Id && se.EndedOn != null);
                                        
                                        html.AppendLine("<tr>");
                                        html.AppendLine("<td>" + exam.Name + " <span class='id'>(" + exam.Id + ")</span></td>");
                                        html.AppendLine("<td>" + exam.StartDate + "</td>");
                                        html.AppendLine("<td>" + exam.EndDate + "</td>");
                                        html.AppendLine("<td>" + (exam.IsPublished == true ? "Yes" : "No") + "</td>");
                                        html.AppendLine("<td class='" + statusClass + "'>" + status + (hasTaken ? " (Taken)" : "") + "</td>");
                                        html.AppendLine("</tr>");
                                    }
                                }
                                
                                html.AppendLine("</table>");
                                
                                // Now test the ReadStudentExams method directly
                                html.AppendLine("<h3>Testing ReadStudentExams Method</h3>");
                                
                                try {
                                    // Get upcoming exams using direct query instead of the method
                                    List<string> studentClass = db.StudentClasses.Where(a => a.StudentId == ramiId).Select(a => a.ClassId).ToList();
                                    
                                    html.AppendLine("<p>Student Classes: " + string.Join(", ", studentClass) + "</p>");
                                    html.AppendLine("<p>Current Time: " + now + "</p>");
                                    
                                    // Use a simpler approach to avoid SQL syntax issues
                                    var upcomingExams = new List<dynamic>();
                                    
                                    // First get all exams
                                    var allExams = db.Exams.AsNoTracking().ToList();
                                    
                                    // Then get all exam-class connections
                                    var examClassConnections = db.ExamClassHubs.AsNoTracking().ToList();
                                    
                                    // Filter manually
                                    foreach (var exam in allExams) {
                                        if (exam.IsPublished == true && exam.StartDate > now) {
                                            // Check if this exam is connected to any of student's classes
                                            foreach (var connection in examClassConnections) {
                                                if (connection.ExamId == exam.Id && studentClass.Contains(connection.ClassHubId)) {
                                                    // This is an upcoming exam for this student
                                                    upcomingExams.Add(new {
                                                        Id = exam.Id,
                                                        Name = exam.Name,
                                                        StartDate = exam.StartDate,
                                                        EndDate = exam.EndDate,
                                                        ClassHubId = connection.ClassHubId
                                                    });
                                                    break; // Found a connection, no need to check more
                                                }
                                            }
                                        }
                                    }
                                    
                                    html.AppendLine("<p>Number of upcoming exams found: " + upcomingExams.Count + "</p>");
                                    
                                    if (upcomingExams.Any()) {
                                        html.AppendLine("<table>");
                                        html.AppendLine("<tr><th>Exam</th><th>Start Date</th><th>End Date</th><th>Class</th></tr>");
                                        
                                        foreach (var exam in upcomingExams) {
                                            var classHub = db.ClassHubs.Find(exam.ClassHubId);
                                            string className = classHub != null ? classHub.Name : "Unknown";
                                            
                                            html.AppendLine("<tr>");
                                            html.AppendLine("<td>" + exam.Name + " <span class='id'>(" + exam.Id + ")</span></td>");
                                            html.AppendLine("<td>" + exam.StartDate + "</td>");
                                            html.AppendLine("<td>" + exam.EndDate + "</td>");
                                            html.AppendLine("<td>" + className + " <span class='id'>(" + exam.ClassHubId + ")</span></td>");
                                            html.AppendLine("</tr>");
                                        }
                                        
                                        html.AppendLine("</table>");
                                    }
                                    else {
                                        html.AppendLine("<p class='error'>No upcoming exams found for Rami_st!</p>");
                                        
                                        // Debug each condition separately
                                        html.AppendLine("<h4>Debugging Conditions</h4>");
                                        
                                        var allExamsCount = db.Exams.Count();
                                        html.AppendLine("<p>Total exams in database: " + allExamsCount + "</p>");
                                        
                                        var publishedExamsCount = db.Exams.Count(e => e.IsPublished == true);
                                        html.AppendLine("<p>Published exams: " + publishedExamsCount + "</p>");
                                        
                                        var futureExamsCount = db.Exams.Count(e => e.StartDate > now);
                                        html.AppendLine("<p>Future exams: " + futureExamsCount + "</p>");
                                        
                                        var publishedFutureExamsCount = db.Exams.Count(e => e.IsPublished == true && e.StartDate > now);
                                        html.AppendLine("<p>Published future exams: " + publishedFutureExamsCount + "</p>");
                                        
                                        // Check class hub connections
                                        var examClassHubsCount = db.ExamClassHubs.Count();
                                        html.AppendLine("<p>Total exam-class connections: " + examClassHubsCount + "</p>");
                                        
                                        var class505ConnectionsCount = db.ExamClassHubs.Count(ech => ech.ClassHubId == class505Id);
                                        html.AppendLine("<p>Class 505 exam connections: " + class505ConnectionsCount + "</p>");
                                        
                                        // List all upcoming exams for Class 505
                                        html.AppendLine("<h4>Upcoming Exams for Class 505</h4>");
                                        var upcomingClass505Exams = new List<dynamic>();
                                        
                                        foreach (var exam in allExams) {
                                            if (exam.IsPublished == true && exam.StartDate > now) {
                                                foreach (var connection in examClassConnections) {
                                                    if (connection.ExamId == exam.Id && connection.ClassHubId == class505Id) {
                                                        upcomingClass505Exams.Add(new {
                                                            Id = exam.Id,
                                                            Name = exam.Name,
                                                            StartDate = exam.StartDate,
                                                            EndDate = exam.EndDate
                                                        });
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        
                                        if (upcomingClass505Exams.Any()) {
                                            html.AppendLine("<table>");
                                            html.AppendLine("<tr><th>Exam</th><th>Start Date</th><th>End Date</th></tr>");
                                            
                                            foreach (var exam in upcomingClass505Exams) {
                                                html.AppendLine("<tr>");
                                                html.AppendLine("<td>" + exam.Name + " <span class='id'>(" + exam.Id + ")</span></td>");
                                                html.AppendLine("<td>" + exam.StartDate + "</td>");
                                                html.AppendLine("<td>" + exam.EndDate + "</td>");
                                                html.AppendLine("</tr>");
                                            }
                                            
                                            html.AppendLine("</table>");
                                        }
                                        else {
                                            html.AppendLine("<p>No upcoming exams found for Class 505</p>");
                                        }
                                    }
                                }
                                catch (Exception ex) {
                                    html.AppendLine("<p class='error'>Error testing direct query: " + ex.Message + "</p>");
                                    html.AppendLine("<p class='error'>Stack trace: " + ex.StackTrace + "</p>");
                                }
                            }
                            else {
                                html.AppendLine("<p class='warning'>No exams found for Class 505.</p>");
                            }
                        }
                        else {
                            html.AppendLine("<p class='error'>Class 505 not found in the database!</p>");
                        }
                    }
                    else {
                        html.AppendLine("<p class='error'>User Rami_st not found in the database!</p>");
                    }
                }
                catch (Exception ex) {
                    html.AppendLine("<p class='error'>Error in specific investigation: " + ex.Message + "</p>");
                    html.AppendLine("<p class='error'>Stack trace: " + ex.StackTrace + "</p>");
                }
                
                html.AppendLine("</div>");
                
                // Basic database tests
                html.AppendLine("<div class='section'>");
                html.AppendLine("<h2>Basic Database Tests</h2>");
                
                try {
                    // Test simple count queries
                    int userCount = db.AspNetUsers.Count();
                    html.AppendLine("<p>Total Users: " + userCount + "</p>");
                    
                    int examCount = db.Exams.Count();
                    html.AppendLine("<p>Total Exams: " + examCount + "</p>");
                    
                    int questionCount = db.Questions.Count();
                    html.AppendLine("<p>Total Questions: " + questionCount + "</p>");
                    
                    int classCount = db.ClassHubs.Count();
                    html.AppendLine("<p>Total Classes: " + classCount + "</p>");
                    
                    // Get current time without string interpolation
                    DateTime? now = util.GetSystemTimeZoneDateTimeNow();
                    html.AppendLine("<p>Current System Time: " + now + "</p>");
                }
                catch (Exception ex) {
                    html.AppendLine("<p class='error'>Error in basic tests: " + ex.Message + "</p>");
                }
                html.AppendLine("</div>");
                
                // Test student classes with class names
                html.AppendLine("<div class='section'>");
                html.AppendLine("<h2>Student Classes</h2>");
                
                try {
                    // First get student classes
                    var studentClasses = db.StudentClasses
                        .Where(a => a.StudentId == currentUserId)
                        .ToList();
                        
                    html.AppendLine("<p>Number of classes: " + studentClasses.Count + "</p>");
                    
                    if (studentClasses.Any()) {
                        html.AppendLine("<table>");
                        html.AppendLine("<tr><th>Class</th><th>Active</th></tr>");
                        
                        foreach (var sc in studentClasses) {
                            // Get class details separately to avoid complex joins
                            var classHub = db.ClassHubs.Find(sc.ClassId);
                            string className = classHub != null ? classHub.Name : "Unknown";
                            bool? isActive = classHub != null ? classHub.IsActive : null;
                            
                            html.AppendLine("<tr>");
                            html.AppendLine("<td>" + className + " <span class='id'>(" + sc.ClassId + ")</span></td>");
                            html.AppendLine("<td>" + (isActive == true ? "Yes" : "No") + "</td>");
                            html.AppendLine("</tr>");
                        }
                        
                        html.AppendLine("</table>");
                    }
                    else {
                        html.AppendLine("<p class='warning'>No classes found for this student.</p>");
                    }
                }
                catch (Exception ex) {
                    html.AppendLine("<p class='error'>Error in student classes: " + ex.Message + "</p>");
                }
                html.AppendLine("</div>");
                
                // All Classes and Their Students
                html.AppendLine("<div class='section'>");
                html.AppendLine("<h2>All Classes and Their Students</h2>");
                
                try {
                    // Get all classes (limit to 10)
                    var allClasses = db.ClassHubs.Take(10).ToList();
                    
                    html.AppendLine("<p>Sample of classes (up to 10):</p>");
                    
                    if (allClasses.Any()) {
                        foreach (var classHub in allClasses) {
                            html.AppendLine("<h3>" + classHub.Name + " <span class='id'>(" + classHub.Id + ")</span></h3>");
                            
                            // Get students in this class
                            var classStudents = db.StudentClasses
                                .Where(sc => sc.ClassId == classHub.Id)
                                .ToList();
                            
                            if (classStudents.Any()) {
                                html.AppendLine("<table>");
                                html.AppendLine("<tr><th>Student</th><th>Email</th></tr>");
                                
                                foreach (var student in classStudents) {
                                    // Get student details
                                    string studentName = "Unknown";
                                    string studentEmail = "Unknown";
                                    
                                    var userProfile = db.UserProfiles
                                        .FirstOrDefault(up => up.AspNetUserId == student.StudentId);
                                    if (userProfile != null) {
                                        studentName = userProfile.FullName;
                                    }
                                    
                                    var aspNetUser = db.AspNetUsers.Find(student.StudentId);
                                    if (aspNetUser != null) {
                                        studentEmail = aspNetUser.Email;
                                    }
                                    
                                    html.AppendLine("<tr>");
                                    html.AppendLine("<td>" + studentName + " <span class='id'>(" + student.StudentId + ")</span></td>");
                                    html.AppendLine("<td>" + studentEmail + "</td>");
                                    html.AppendLine("</tr>");
                                }
                                
                                html.AppendLine("</table>");
                            }
                            else {
                                html.AppendLine("<p class='warning'>No students found in this class.</p>");
                            }
                        }
                    }
                    else {
                        html.AppendLine("<p class='warning'>No classes found.</p>");
                    }
                }
                catch (Exception ex) {
                    html.AppendLine("<p class='error'>Error in classes and students: " + ex.Message + "</p>");
                }
                html.AppendLine("</div>");
                
                // Classes and Their Related Exams
                html.AppendLine("<div class='section'>");
                html.AppendLine("<h2>Classes and Their Related Exams</h2>");
                
                try {
                    // Get all classes (limit to 10)
                    var allClasses = db.ClassHubs.Take(10).ToList();
                    
                    html.AppendLine("<p>Sample of classes (up to 10):</p>");
                    
                    if (allClasses.Any()) {
                        foreach (var classHub in allClasses) {
                            html.AppendLine("<h3>" + classHub.Name + " <span class='id'>(" + classHub.Id + ")</span></h3>");
                            
                            // Get exams for this class
                            var classExams = db.ExamClassHubs
                                .Where(ech => ech.ClassHubId == classHub.Id)
                                .ToList();
                            
                            if (classExams.Any()) {
                                html.AppendLine("<table>");
                                html.AppendLine("<tr><th>Exam</th><th>Start Date</th><th>End Date</th><th>Published</th><th>Created By</th></tr>");
                                
                                foreach (var examClass in classExams) {
                                    // Get exam details
                                    var exam = db.Exams.Find(examClass.ExamId);
                                    if (exam != null) {
                                        // Get creator name
                                        string creatorName = "Unknown";
                                        if (!string.IsNullOrEmpty(exam.CreatedBy)) {
                                            var creator = db.UserProfiles.FirstOrDefault(up => up.AspNetUserId == exam.CreatedBy);
                                            if (creator != null) {
                                                creatorName = creator.FullName;
                                            }
                                        }
                                        
                                        html.AppendLine("<tr>");
                                        html.AppendLine("<td>" + exam.Name + " <span class='id'>(" + exam.Id + ")</span></td>");
                                        html.AppendLine("<td>" + exam.StartDate + "</td>");
                                        html.AppendLine("<td>" + exam.EndDate + "</td>");
                                        html.AppendLine("<td>" + (exam.IsPublished == true ? "Yes" : "No") + "</td>");
                                        html.AppendLine("<td>" + creatorName + " <span class='id'>(" + exam.CreatedBy + ")</span></td>");
                                        html.AppendLine("</tr>");
                                    }
                                }
                                
                                html.AppendLine("</table>");
                            }
                            else {
                                html.AppendLine("<p class='warning'>No exams found for this class.</p>");
                            }
                        }
                    }
                    else {
                        html.AppendLine("<p class='warning'>No classes found.</p>");
                    }
                }
                catch (Exception ex) {
                    html.AppendLine("<p class='error'>Error in classes and exams: " + ex.Message + "</p>");
                }
                html.AppendLine("</div>");
                
                // Test simple exam queries
                html.AppendLine("<div class='section'>");
                html.AppendLine("<h2>Simple Exam Tests</h2>");
                
                try {
                    // Get all exams (limit to 10)
                    var allExams = db.Exams.Take(10).ToList();
                    
                    html.AppendLine("<p>Sample of exams (up to 10):</p>");
                    
                    if (allExams.Any()) {
                        html.AppendLine("<table>");
                        html.AppendLine("<tr><th>Exam</th><th>Start Date</th><th>End Date</th><th>Published</th><th>Created By</th></tr>");
                        
                        foreach (var exam in allExams) {
                            // Get creator name separately
                            string creatorName = "Unknown";
                            if (!string.IsNullOrEmpty(exam.CreatedBy)) {
                                var creator = db.UserProfiles.FirstOrDefault(up => up.AspNetUserId == exam.CreatedBy);
                                if (creator != null) {
                                    creatorName = creator.FullName;
                                }
                            }
                            
                            html.AppendLine("<tr>");
                            html.AppendLine("<td>" + exam.Name + " <span class='id'>(" + exam.Id + ")</span></td>");
                            html.AppendLine("<td>" + exam.StartDate + "</td>");
                            html.AppendLine("<td>" + exam.EndDate + "</td>");
                            html.AppendLine("<td>" + (exam.IsPublished == true ? "Yes" : "No") + "</td>");
                            html.AppendLine("<td>" + creatorName + " <span class='id'>(" + exam.CreatedBy + ")</span></td>");
                            html.AppendLine("</tr>");
                        }
                        
                        html.AppendLine("</table>");
                    }
                    else {
                        html.AppendLine("<p class='warning'>No exams found.</p>");
                    }
                }
                catch (Exception ex) {
                    html.AppendLine("<p class='error'>Error in exam tests: " + ex.Message + "</p>");
                }
                html.AppendLine("</div>");
                
                // Test student exams
                html.AppendLine("<div class='section'>");
                html.AppendLine("<h2>Student Exams</h2>");
                
                try {
                    var studentExams = db.StudentExams
                        .Where(a => a.StudentId == currentUserId)
                        .ToList();
                        
                    html.AppendLine("<p>Number of student exams: " + studentExams.Count + "</p>");
                    
                    if (studentExams.Any()) {
                        html.AppendLine("<table>");
                        html.AppendLine("<tr><th>Exam</th><th>Started On</th><th>Ended On</th><th>Result</th><th>Passed</th></tr>");
                        
                        foreach (var se in studentExams) {
                            // Get exam name separately
                            string examName = "Unknown";
                            var exam = db.Exams.Find(se.ExamId);
                            if (exam != null) {
                                examName = exam.Name;
                            }
                            
                            html.AppendLine("<tr>");
                            html.AppendLine("<td>" + examName + " <span class='id'>(" + se.ExamId + ")</span></td>");
                            html.AppendLine("<td>" + se.StartedOn + "</td>");
                            html.AppendLine("<td>" + se.EndedOn + "</td>");
                            html.AppendLine("<td>" + se.Result + "</td>");
                            
                            string passedClass = se.Passed == true ? "success" : "error";
                            string passedText = se.Passed == true ? "Yes" : "No";
                            html.AppendLine("<td><span class='" + passedClass + "'>" + passedText + "</span></td>");
                            
                            html.AppendLine("</tr>");
                        }
                        
                        html.AppendLine("</table>");
                    }
                    else {
                        html.AppendLine("<p class='warning'>No student exams found.</p>");
                    }
                }
                catch (Exception ex) {
                    html.AppendLine("<p class='error'>Error in student exams: " + ex.Message + "</p>");
                }
                html.AppendLine("</div>");
                
                html.AppendLine("</body></html>");
                
                return Content(html.ToString(), "text/html");
            }
            catch (Exception ex)
            {
                return Content("<html><body><h1>Error</h1><p>" + ex.Message + "</p><p>" + ex.StackTrace + "</p></body></html>", "text/html");
            }
        }

        // Helper class for testing
        private class MockUserManager
        {
            private string _userId;
            
            public MockUserManager(string userId)
            {
                _userId = userId;
            }
            
            public string GetUserId(object user)
            {
                return _userId;
            }
        }

        [HttpGet]
        public IActionResult CurrentExamDirect()
        {
            var html = new System.Text.StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<title>Current Exams</title>");
            html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1'>");
            html.AppendLine("<link rel='stylesheet' href='/lib/bootstrap/dist/css/bootstrap.min.css'>");
            html.AppendLine("<link rel='stylesheet' href='/lib/fontawesome/css/all.min.css'>");
            html.AppendLine("<link rel='stylesheet' href='/css/site.css'>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div class='container mt-4'>");
            html.AppendLine("<h2>Current Exams</h2>");
            html.AppendLine("<hr>");
            
            try {
                // Get current user ID
                string currentUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(currentUserId)) {
                    html.AppendLine("<div class='alert alert-danger'>User not authenticated</div>");
                } else {
                    // Get student classes
                    List<string> studentClasses = db.StudentClasses
                        .Where(a => a.StudentId == currentUserId)
                        .Select(a => a.ClassId)
                        .ToList();
                        
                    if (studentClasses.Count == 0) {
                        html.AppendLine("<div class='alert alert-info'>You are not enrolled in any classes</div>");
                    } else {
                        // Get current time
                        DateTime? now = util.GetSystemTimeZoneDateTimeNow();
                        
                        // Get taken exams
                        List<string> takenExams = db.StudentExams
                            .Where(a => a.EndedOn != null && a.StudentId == currentUserId)
                            .Select(a => a.ExamId)
                            .ToList();
                            
                        // Get all exams
                        var allExams = db.Exams.AsNoTracking().ToList();
                        
                        // Get exam-class connections
                        var examClassConnections = db.ExamClassHubs.AsNoTracking().ToList();
                        
                        // Find current exams for student
                        var results = new List<ExamViewModel>();
                        
                        foreach (var exam in allExams) {
                            if (exam.IsPublished == true && 
                                exam.StartDate <= now && 
                                exam.EndDate >= now && 
                                !takenExams.Contains(exam.Id)) {
                                
                                // Check if this exam is connected to any of student's classes
                                foreach (var connection in examClassConnections) {
                                    if (connection.ExamId == exam.Id && studentClasses.Contains(connection.ClassHubId)) {
                                        // Count questions for this exam
                                        int totalQuestion = db.ExamQuestions.Count(q => q.ExamId == exam.Id);
                                        
                                        // This is a current exam for this student
                                        results.Add(new ExamViewModel {
                                            Id = exam.Id,
                                            Name = exam.Name,
                                            Duration = exam.Duration,
                                            StartDate = exam.StartDate,
                                            EndDate = exam.EndDate,
                                            TotalQuestions = totalQuestion
                                        });
                                        break; // Found a connection, no need to check more
                                    }
                                }
                            }
                        }
                        
                        if (results.Count == 0) {
                            html.AppendLine("<div class='alert alert-info'>No current exams available for you at this time.</div>");
                        } else {
                            html.AppendLine("<div class='card'>");
                            html.AppendLine("<div class='card-body'>");
                            html.AppendLine("<div class='table-responsive'>");
                            html.AppendLine("<table class='table table-striped'>");
                            html.AppendLine("<thead class='thead-dark'>");
                            html.AppendLine("<tr>");
                            html.AppendLine("<th>Exam Name</th>");
                            html.AppendLine("<th>Start Date</th>");
                            html.AppendLine("<th>End Date</th>");
                            html.AppendLine("<th>Duration (min)</th>");
                            html.AppendLine("<th>Questions</th>");
                            html.AppendLine("<th>Actions</th>");
                            html.AppendLine("</tr>");
                            html.AppendLine("</thead>");
                            html.AppendLine("<tbody>");
                            
                            foreach (var exam in results) {
                                html.AppendLine("<tr>");
                                html.AppendLine("<td>" + exam.Name + "</td>");
                                html.AppendLine("<td>" + exam.StartDate + "</td>");
                                html.AppendLine("<td>" + exam.EndDate + "</td>");
                                html.AppendLine("<td>" + exam.Duration + "</td>");
                                html.AppendLine("<td>" + exam.TotalQuestions + "</td>");
                                html.AppendLine("<td>");
                                html.AppendLine("<a href='/studentexam/confirmtakeexam?eId=" + exam.Id + "' class='btn btn-primary btn-sm'>");
                                html.AppendLine("<i class='fas fa-edit'></i> Take Exam");
                                html.AppendLine("</a>");
                                html.AppendLine("</td>");
                                html.AppendLine("</tr>");
                            }
                            
                            html.AppendLine("</tbody>");
                            html.AppendLine("</table>");
                            html.AppendLine("</div>");
                            html.AppendLine("</div>");
                            html.AppendLine("</div>");
                        }
                    }
                }
            } catch (Exception ex) {
                html.AppendLine("<div class='alert alert-danger'>");
                html.AppendLine("<h4>Error</h4>");
                html.AppendLine("<p>" + ex.Message + "</p>");
                html.AppendLine("<p><small>" + ex.StackTrace + "</small></p>");
                html.AppendLine("</div>");
            }
            
            html.AppendLine("<div class='mt-3'>");
            html.AppendLine("<a href='/studentexam/currentexam' class='btn btn-secondary'>");
            html.AppendLine("<i class='fas fa-arrow-left'></i> Back to Regular View");
            html.AppendLine("</a>");
            html.AppendLine("</div>");
            
            html.AppendLine("</div>"); // container
            html.AppendLine("<script src='/lib/jquery/dist/jquery.min.js'></script>");
            html.AppendLine("<script src='/lib/bootstrap/dist/js/bootstrap.bundle.min.js'></script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return Content(html.ToString(), "text/html");
        }
    }
}
