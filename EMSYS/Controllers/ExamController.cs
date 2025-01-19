using EMSYS.Models;
using EMSYS.Resources;
using EMSYS.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static EMSYS.Models.ProjectEnum;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using EMSYS.Data;

namespace EMSYS.Controllers
{
    [Authorize(Roles = "System Admin, Instructor")]
    public class ExamController : Controller
    {
        private EMSYSdbContext db;
        private Util util;
        private readonly UserManager<AspNetUsers> _userManager;
        private ErrorLoggingService _logger;

        public ExamController(EMSYSdbContext db, Util util, UserManager<AspNetUsers> userManager, ErrorLoggingService logger)
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

        public void SetupSelectLists(ExamViewModel model)
        {
            model.ClassIdSelectList = util.GetDataForMultiSelect(model.ClassIdList, db.ClassHubs, a => a.Name, a => a.Id);
            model.SubjectIdSelectList = util.GetDataForMultiSelect(model.SubjectIdList, db.Subjects, a => a.Name, a => a.Id);
            model.ActiveInactiveSelectlist = util.GetActiveInactiveDropDown(model.IsActive ?? "Active");
        }

        public IActionResult AllocateMarks(string Id)
        {
            ExamViewModel model = new ExamViewModel();
            if (Id != null)
            {
                model.Id = Id;
                var exam = db.Exams.Where(a => a.Id == Id).Select(a => new { a.Name, a.RandomizeQuestions, a.IsPublished, a.StartDate, a.EndDate }).FirstOrDefault();
                model.Name = exam.Name;
                model.RandomizeQuestions = exam.RandomizeQuestions;
                model.QuestionListing = new QuestionListing();
                model.QuestionListing.Listing = new List<QuestionViewModel>();
                model.QuestionListing.Listing = ReadSelectedQuestions(Id);

                bool studentTakenExam = db.StudentExams.Where(a => a.ExamId == model.Id).Any();
                model.AlreadyStarted = studentTakenExam;

                DateTime? now = util.GetSystemTimeZoneDateTimeNow();
                if (exam.StartDate <= now && exam.EndDate >= now && exam.IsPublished == true)
                {
                    model.ExamStatus = DataConverter.GetEnumDisplayName(ExamStatus.OnGoing);
                }
                else if (exam.StartDate < now && exam.EndDate < now)
                {
                    model.ExamStatus = ExamStatus.Ended.ToString();
                }
                else if (exam.StartDate > now && exam.EndDate > now)
                {
                    model.ExamStatus = exam.IsPublished == true ? ExamStatus.Published.ToString() : ExamStatus.Draft.ToString();
                }
                else
                {
                    model.ExamStatus = ExamStatus.Draft.ToString();
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult AllocateMarks(ExamViewModel model)
        {
            try
            {
                if (model.QuestionIds != null)
                {
                    for (int i = 0; i < model.QuestionIds.Length; i++)
                    {
                        string questionid = model.QuestionIds[i];
                        ExamQuestion examQuestion = db.ExamQuestions.Where(a => a.QuestionId == questionid && a.ExamId == model.Id).FirstOrDefault();
                        if (examQuestion != null && model.MarkForEachQuestions[i] != null)
                        {
                            if (examQuestion.Mark != model.MarkForEachQuestions[i])
                            {
                                examQuestion.Mark = model.MarkForEachQuestions[i];
                                db.Entry(examQuestion).State = EntityState.Modified;

                                //check if there is any student took the exam already, re-calculate their marks obtained
                                List<StudentAnswerCloned> studentAnswers = db.StudentAnswerCloneds.Where(a => a.QuestionId == examQuestion.QuestionId && a.ExamId == examQuestion.ExamId).ToList();
                                if (studentAnswers != null)
                                {
                                    if (studentAnswers.Count > 0)
                                    {
                                        List<StudentExam> studentExams = new List<StudentExam>();
                                        foreach (StudentAnswerCloned studentAnswer in studentAnswers)
                                        {
                                            if (studentAnswer.MarksObtained != 0)
                                            {
                                                studentAnswer.MarksObtained = model.MarkForEachQuestions[i];
                                                db.Entry(studentAnswer).State = EntityState.Modified;

                                                StudentExam studentExam = db.StudentExams.Where(a => a.StudentId == studentAnswer.StudentId && a.ExamId == studentAnswer.ExamId).FirstOrDefault();
                                                studentExams.Add(studentExam);
                                            }
                                        }
                                        db.SaveChanges();
                                        if (studentExams != null)
                                        {
                                            //re-calculate student result
                                            foreach (StudentExam stuExam in studentExams)
                                            {
                                                stuExam.Result = db.StudentAnswerCloneds.Where(a => a.StudentId == stuExam.StudentId && a.ExamId == stuExam.ExamId).Select(a => a.MarksObtained).Sum();
                                                db.Entry(stuExam).State = EntityState.Modified;
                                            }
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    db.SaveChanges();
                }
                TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.Anerroroccurred;
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("allocatemarks", new { Id = model.Id });
        }

        public List<QuestionViewModel> ReadSelectedQuestions(string id)
        {
            List<QuestionViewModel> list = new List<QuestionViewModel>();
            list = (from t1 in db.ExamQuestions
                    join t2 in db.Questions on t1.QuestionId equals t2.Id
                    join t3 in db.QuestionTypes on t2.QuestionTypeId equals t3.Id into g2
                    from t4 in g2.DefaultIfEmpty()
                    where t1.ExamId == id
                    orderby t1.QuestionOrder
                    select new QuestionViewModel
                    {
                        Id = t1.QuestionId,
                        QuestionTitle = t2.QuestionTitle,
                        QuestionTypeName = t4 == null ? "" : t4.Name,
                        QuestionTypeCode = t4 == null ? "" : t4.Code,
                        ExamQuestionMark = t1.Mark
                    }).ToList();
            return list;
        }

        public IActionResult AddQuestion(string Id)
        {
            ExamViewModel model = new ExamViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "Edit");
            }
            model.SelectedQuestions = new List<string>();
            model.SelectedQuestions = db.ExamQuestions.Where(a => a.ExamId == model.Id).Select(a => a.QuestionId).ToList();
            SetupFilterQuestion(model);
            return View(model);
        }

        public void SetupFilterQuestion(ExamViewModel model)
        {
            model.QuestionListing = new QuestionListing();
            model.QuestionListing.SubjectIdSelectList = (from t1 in db.ExamSubjects
                                                         join t2 in db.Subjects on t1.SubjectId equals t2.Id
                                                         where t1.ExamId == model.Id
                                                         orderby t2.Name
                                                         select new SelectListItem
                                                         {
                                                             Text = t2.Name,
                                                             Value = t2.Id,
                                                             Selected = false
                                                         }).Distinct().ToList();
            model.QuestionListing.QuestionTypeIdSelectList = (from t1 in db.QuestionTypes
                                                              orderby t1.Name
                                                              select new SelectListItem
                                                              {
                                                                  Text = t1.Name,
                                                                  Value = t1.Id,
                                                                  Selected = false
                                                              }).ToList();
        }

        [HttpPost]
        public IActionResult AddQuestion(ExamViewModel model)
        {
            try
            {
                bool isValid = true;
                if (model.SelectedQuestions == null)
                {
                    isValid = false;
                    TempData["ErrorMessage"] = Resource.PleaseSelectAtLeastOneQuestion;
                }
                else
                {
                    model.SelectedQuestions.RemoveAll(x => x == "all"); //if user selected the checkbox in <th>, remove it from the array

                    //when student already took exam, Instructor or admin can only exclude a question from the exam, they cannot add new question to it
                    bool? studentTakenExam = db.StudentExams.Where(a => a.ExamId == model.Id).Any();
                    if (studentTakenExam == true)
                    {
                        isValid = false;
                        TempData["ErrorMessage"] = Resource.CannotEditQuestion;
                    }
                }

                if (isValid)
                {
                    if (model.RandomizeQuestions == true)
                    {
                        UpdateExamQuestion(model, true);
                    }
                    else
                    {
                        UpdateExamQuestion(model, false);
                    }

                    TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
                    return RedirectToAction("addquestion", "exam", new { Id = model.Id });
                }
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }

            SetupFilterQuestion(model);
            return View(model);
        }

        public IActionResult SortQuestion(string Id)
        {
            ExamViewModel model = new ExamViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "Edit");
            }
            model.QuestionListing = new QuestionListing();
            model.QuestionListing.Listing = new List<QuestionViewModel>();
            model.QuestionListing.Listing = ReadSelectedQuestions(Id);
            return View(model);
        }

        [HttpPost]
        public IActionResult ProcessSortQuestion(string[] itemIds, string examId)
        {
            try
            {
                for (int i = 0; i < itemIds.Length; i++)
                {
                    string questionid = itemIds[i];
                    ExamQuestion examQuestion = db.ExamQuestions.Where(a => a.QuestionId == questionid && a.ExamId == examId).FirstOrDefault();
                    if (examQuestion != null)
                    {
                        examQuestion.QuestionOrder = i + 1;
                        db.Entry(examQuestion).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
                TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
                return Ok(new { Message = "Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return Ok(new { Message = "Failed" });
        }

        public IActionResult Publish(string Id)
        {
            try
            {
                Exam exam = db.Exams.Find(Id);
                if (exam != null)
                {
                    exam.IsPublished = true;
                    exam.ModifiedBy = _userManager.GetUserId(User);
                    exam.ModifiedOn = util.GetSystemTimeZoneDateTimeNow();
                    db.Entry(exam).State = EntityState.Modified;
                    db.SaveChanges();

                    if (exam.RandomizeQuestions == true)
                    {
                        UpdateStudentQuestionOrder(exam.Id);
                    }

                    TempData["NotifySuccess"] = Resource.PublishedSuccessfully;
                }
                else
                {
                    TempData["ErrorMessage"] = Resource.Examnotfound;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = Resource.FailedExceptionError;
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("viewrecord", new { Id = Id });
        }

        public IActionResult Unpublish(string Id)
        {
            try
            {
                Exam exam = db.Exams.Find(Id);
                if (exam != null)
                {
                    exam.IsPublished = false;
                    exam.ModifiedBy = _userManager.GetUserId(User);
                    exam.ModifiedOn = util.GetSystemTimeZoneDateTimeNow();
                    db.Entry(exam).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["NotifySuccess"] = Resource.PublishedSuccessfully;
                }
                else
                {
                    TempData["ErrorMessage"] = Resource.Examnotfound;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = Resource.FailedExceptionError;
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("viewrecord", new { Id = Id });
        }


        public IActionResult ViewRecord(string Id)
        {
            ExamViewModel model = new ExamViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "View");
            }
            return View(model);
        }

        public IActionResult Edit(string Id)
        {
            ExamViewModel model = new ExamViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "Edit");
            }
            SetupSelectLists(model);
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(ExamViewModel model)
        {
            try
            {
                ValidateModel(model);

                if (!ModelState.IsValid)
                {
                    SetupSelectLists(model);
                    return View(model);
                }

                SaveRecord(model);
                TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("edit", new { Id = model.Id });
        }

        public IActionResult Delete(string Id)
        {
            try
            {
                if (Id != null)
                {
                    Exam exam = db.Exams.Where(a => a.Id == Id).FirstOrDefault();
                    if (exam != null)
                    {
                        db.Exams.Remove(exam);
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
                    sort = ExamListConfig.DefaultSortOrder;
                }
                headers = ListUtil.GetColumnHeaders(ExamListConfig.DefaultColumnHeaders, sort);
                var list = ReadExams();
                string searchMessage = ExamListConfig.SearchMessage;
                list = ExamListConfig.PerformSearch(list, search);
                list = ExamListConfig.PerformSort(list, sort);
                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = pg ?? 1;
                ViewData["CurrentSearch"] = search;
                int? total = list.Count();
                int? defaultSize = ExamListConfig.DefaultPageSize;
                size = size == 0 || size == null ? (defaultSize != -1 ? defaultSize : total) : size == -1 ? total : size;
                ViewData["CurrentSize"] = size;
                PaginatedList<ExamViewModel> result = await PaginatedList<ExamViewModel>.CreateAsync(list, pg ?? 1, size.Value, total.Value, headers, searchMessage);
                return PartialView("~/Views/Exam/_MainList.cshtml", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return PartialView("~/Views/Shared/Error.cshtml", null);
        }

        public IQueryable<ExamViewModel> ReadExams()
        {
            var currentUserId = _userManager.GetUserId(User);
            DateTime? now = util.GetSystemTimeZoneDateTimeNow();
            var exams = from t1 in db.Exams
                        select new ExamViewModel
                        {
                            Id = t1.Id,
                            Name = t1.Name,
                            Duration = t1.Duration,
                            IsPublished = t1.IsPublished,
                            StartDate = t1.StartDate,
                            StartDateIsoUtc = t1.IsoUtcStartDate,
                            EndDate = t1.EndDate,
                            EndDateIsoUtc = t1.IsoUtcEndDate,
                            CreatedBy = t1.CreatedBy,
                            CreatedOn = t1.CreatedOn,
                            CreatedOnIsoUtc = t1.IsoUtcCreatedOn,
                            TotalQuestions = db.ExamQuestions.Count(eq => eq.ExamId == t1.Id),
                            TotalMark = db.ExamQuestions.Where(eq => eq.ExamId == t1.Id).Sum(eq => eq.Mark),
                            ExamStatus = (t1.StartDate <= now && t1.EndDate >= now && t1.IsPublished == true) ? "On Going" :
                            (t1.StartDate < now && t1.EndDate < now) ? "Ended" :
                            (t1.StartDate > now && t1.EndDate > now && t1.IsPublished == true) ? "Published" :
                            (t1.StartDate > now && t1.EndDate > now && t1.IsPublished == false) ? "Draft" : "Draft"
                        };
            exams = User.IsInRole("Instructor") ? exams.Where(a => a.CreatedBy == currentUserId) : exams;
            return exams; 

        }

        public ExamViewModel GetViewModel(string Id, string type)
        {
            ExamViewModel model = new ExamViewModel();
            Exam exam = db.Exams.Where(a => a.Id == Id).FirstOrDefault();
            model.Id = exam.Id;
            model.Name = exam.Name;

            model.StartDate = exam.StartDate.Value.ToLocalTime();
            model.StartDateIsoUtc = exam.IsoUtcStartDate;
            model.StartDateIsoString = exam.IsoUtcStartDate;

            model.EndDate = exam.StartDate.Value.ToLocalTime();
            model.EndDateIsoUtc = exam.IsoUtcEndDate;
            model.EndDateIsoString = exam.IsoUtcEndDate;

            model.ClassIdList = db.ExamClassHubs.Where(a => a.ExamId == model.Id).Select(a => a.ClassHubId).ToList();
            var classNameList = (from t1 in model.ClassIdList
                                 join t2 in db.ClassHubs on t1 equals t2.Id
                                 select t2.Name).OrderBy(o => o).ToList();
            model.ClassName = String.Join(", ", classNameList);

            model.SubjectIdList = db.ExamSubjects.Where(a => a.ExamId == model.Id).Select(a => a.SubjectId).ToList();
            var subjectNameList = (from t1 in model.SubjectIdList
                                   join t2 in db.Subjects on t1 equals t2.Id
                                   select t2.Name).OrderBy(o => o).ToList();
            model.SubjectName = String.Join(", ", subjectNameList);

            model.MarksToPass = exam.MarksToPass;
            model.TotalMark = db.ExamQuestions.Where(a => a.ExamId == Id).Select(a => a.Mark).Sum();
            model.Duration = exam.Duration;
            model.RandomizeQuestions = exam.RandomizeQuestions;
            model.Description = exam.Description;
            model.ReleaseAnswer = exam.ReleaseAnswer;
            model.IsActive = exam.IsActive == true ? "Active" : "Inactive";
            model.TotalQuestions = db.ExamQuestions.Where(a => a.ExamId == Id).Count();
            model.ExamStatus = GetExamStatus(exam);
            int answers = (from t1 in db.ExamQuestions
                           join t2 in db.Answers on t1.QuestionId equals t2.QuestionId
                           select t2.Id).Count();
            bool? orderNotSave = db.ExamQuestions.Where(a => a.ExamId == Id && a.QuestionOrder == null).Any();
            bool? marksNotAllocated = db.ExamQuestions.Where(a => a.ExamId == Id && a.Mark == null).Any();
            if ((answers >= model.TotalQuestions) && orderNotSave == false && model.TotalQuestions > 0 && marksNotAllocated == false)
            {
                model.CanPublishNow = true;
            }
            else
            {
                model.CanPublishNow = false;
            }

            bool studentTakenExam = db.StudentExams.Where(a => a.ExamId == exam.Id).Any();
            model.AlreadyStarted = studentTakenExam;
            model.ExamStatus = GetExamStatus(exam);
            if (type == "View")
            {
                model.CreatedAndModified = util.GetCreatedAndModified(exam.CreatedBy, exam.IsoUtcCreatedOn, exam.ModifiedBy, exam.IsoUtcModifiedOn);
            }
            return model;
        }

        public string GetExamStatus(Exam exam)
        {
            string status = "";
            DateTime? now = util.GetSystemTimeZoneDateTimeNow();
            if (exam.StartDate <= now && exam.EndDate >= now && exam.IsPublished == true)
            {
                status = DataConverter.GetEnumDisplayName(ExamStatus.OnGoing);
            }
            else if (exam.StartDate < now && exam.EndDate < now)
            {
                status = ExamStatus.Ended.ToString();
            }
            else if (exam.StartDate > now && exam.EndDate > now)
            {
                status = exam.IsPublished == true ? ExamStatus.Published.ToString() : ExamStatus.Draft.ToString();
            }
            else
            {
                status = ExamStatus.Draft.ToString();
            }
            return status;
        }

        public void ValidateModel(ExamViewModel model)
        {
            if (model != null)
            {
                bool duplicated = false;
                if (model.Id != null)
                {
                    duplicated = db.Exams.Where(a => a.Name == model.Name && a.Id != model.Id).Any();
                }
                else
                {
                    duplicated = db.Exams.Where(a => a.Name == model.Name).Select(a => a.Id).Any();
                }
                if (duplicated == true)
                {
                    ModelState.AddModelError("Name", Resource.ThisIsADuplicatedValue);
                }
                if (!string.IsNullOrEmpty(model.StartDateIsoUtc) && !string.IsNullOrEmpty(model.EndDateIsoUtc))
                {
                    model.StartDate = util.ConvertToSystemTimeZoneDateTime(model.StartDateIsoUtc);
                    model.EndDate = util.ConvertToSystemTimeZoneDateTime(model.EndDateIsoUtc);

                    if (model.EndDate.Value <= model.StartDate.Value)
                    {
                        ModelState.AddModelError("EndDate", Resource.EndDateTimeMustBeGreater);
                    }
                    TimeSpan timeSpan = model.EndDate.Value.Subtract(model.StartDate.Value);
                    if (model.Duration > timeSpan.TotalMinutes)
                    {
                        ModelState.AddModelError("Duration", Resource.InvalidDuration);
                    }
                }
            }
        }

        public void AssignModelValues(Exam exam, ExamViewModel model)
        {
            exam.Name = model.Name;
            exam.Description = model.Description;
            exam.IsoUtcStartDate = model.StartDateIsoUtc;
            exam.IsoUtcEndDate = model.EndDateIsoUtc;
            exam.StartDate = util.ConvertToSystemTimeZoneDateTime(model.StartDateIsoUtc);
            exam.EndDate = util.ConvertToSystemTimeZoneDateTime(model.EndDateIsoUtc);
            exam.Duration = model.Duration;
            exam.MarksToPass = model.MarksToPass;
            exam.RandomizeQuestions = model.RandomizeQuestions;
            exam.IsActive = model.IsActive == "Active" ? true : false;
            exam.ReleaseAnswer = model.ReleaseAnswer;
        }

        public void SaveRecord(ExamViewModel model)
        {
            if (model != null)
            {
                if (model.Id == null)
                {
                    Exam exam = new Exam();
                    exam.Id = Guid.NewGuid().ToString();
                    AssignModelValues(exam, model);
                    exam.CreatedBy = _userManager.GetUserId(User);
                    exam.CreatedOn = util.GetSystemTimeZoneDateTimeNow();
                    exam.IsoUtcCreatedOn = util.GetIsoUtcNow();
                    db.Exams.Add(exam);
                    db.SaveChanges();
                    model.Id = exam.Id;
                }
                else
                {
                    Exam exam = db.Exams.Where(a => a.Id == model.Id).FirstOrDefault();

                    //if user change the exam from 'randomize question' to 'not randomize question', then remove the student question order records
                    if (model.RandomizeQuestions == false && exam.RandomizeQuestions == true)
                    {
                        RemoveStudentQuestionOrder(exam.Id);
                    }
                    //if user change the exam from 'not randomize question' to 'randomize question', then add student question order records
                    else if (model.RandomizeQuestions == true && exam.RandomizeQuestions == false)
                    {
                        UpdateStudentQuestionOrder(exam.Id);
                    }

                    //update StudentExam bool Passed column value
                    bool? studentTakenExam = db.StudentExams.Where(a => a.ExamId == exam.Id).Select(a => a.Id).Any();
                    if (model.MarksToPass != exam.MarksToPass && studentTakenExam == true)
                    {
                        List<StudentExam> studentExams = db.StudentExams.Where(a => a.ExamId == exam.Id).ToList();
                        foreach (StudentExam studentExam in studentExams)
                        {
                            studentExam.Passed = studentExam.Result >= model.MarksToPass ? true : false;
                            db.Entry(studentExam).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }

                    AssignModelValues(exam, model);
                    exam.ModifiedBy = _userManager.GetUserId(User);
                    exam.ModifiedOn = util.GetSystemTimeZoneDateTimeNow();
                    exam.IsoUtcModifiedOn = util.GetIsoUtcNow();
                    db.Entry(exam).State = EntityState.Modified;
                    db.SaveChanges();
                }

                UpdateExamClass(model);
                UpdateExamSubject(model);

            }
        }

        public void UpdateExamClass(ExamViewModel model)
        {
            List<string> existingExamClassId = db.ExamClassHubs.Where(a => a.ExamId == model.Id).Select(a => a.ClassHubId).ToList();
            //if user changed the class, then proceed, else, do nothing
            if (existingExamClassId.Count != model.ClassIdList.Count || existingExamClassId.Count == 0)
            {
                List<ExamClassHub> existingExamClasses = db.ExamClassHubs.Where(a => a.ExamId == model.Id).ToList();
                if (existingExamClasses.Count > 0)
                {
                    db.ExamClassHubs.RemoveRange(existingExamClasses);
                    db.SaveChanges();
                }

                List<ExamClassHub> toAdd = new List<ExamClassHub>();
                if (model.ClassIdList != null)
                {
                    foreach (string classid in model.ClassIdList)
                    {
                        ExamClassHub examClass = new ExamClassHub();
                        examClass.ExamId = model.Id;
                        examClass.ClassHubId = classid;
                        toAdd.Add(examClass);
                    }
                }
                if (toAdd.Count > 0)
                {
                    db.ExamClassHubs.AddRange(toAdd);
                    db.SaveChanges();
                }

                //changing class means student question order need to be changed too
                List<StudentQuestionOrder> existingStudentQuestionOrder = db.StudentQuestionOrders.Where(a => a.ExamId == model.Id).ToList();
                if (existingStudentQuestionOrder.Count > 0)
                {
                    UpdateStudentQuestionOrder(model.Id);
                }
            }
        }

        public void UpdateExamSubject(ExamViewModel model)
        {
            List<string> existingExamSubjectId = db.ExamSubjects.Where(a => a.ExamId == model.Id).Select(a => a.SubjectId).ToList();
            //if user changed the subject, then proceed, else, do nothing
            if (existingExamSubjectId.Count != model.SubjectIdList.Count || existingExamSubjectId.Count == 0)
            {
                List<ExamSubject> existingExamSubjects = db.ExamSubjects.Where(a => a.ExamId == model.Id).ToList();
                if (existingExamSubjects.Count > 0)
                {
                    db.ExamSubjects.RemoveRange(existingExamSubjects);
                    db.SaveChanges();
                }

                List<ExamSubject> toAdd = new List<ExamSubject>();
                if (model.SubjectIdList != null)
                {
                    foreach (string subjectid in model.SubjectIdList)
                    {
                        ExamSubject examSubject = new ExamSubject();
                        examSubject.ExamId = model.Id;
                        examSubject.SubjectId = subjectid;
                        toAdd.Add(examSubject);
                    }
                }
                if (toAdd.Count > 0)
                {
                    db.ExamSubjects.AddRange(toAdd);
                    db.SaveChanges();
                }
            }
        }

        public void RemoveStudentQuestionOrder(string examId)
        {
            List<StudentQuestionOrder> existingStudentQuestionOrder = db.StudentQuestionOrders.Where(a => a.ExamId == examId).ToList();
            if (existingStudentQuestionOrder.Count > 0)
            {
                db.StudentQuestionOrders.RemoveRange(existingStudentQuestionOrder);
                db.SaveChanges();
            }
        }

        public void UpdateStudentQuestionOrder(string examId)
        {
            RemoveStudentQuestionOrder(examId);

            int totalQuestions = db.ExamQuestions.Where(a => a.ExamId == examId).Count();
            List<string> classIds = db.ExamClassHubs.Where(a => a.ExamId == examId).Select(a => a.ClassHubId).ToList();
            if (classIds != null)
            {
                foreach (string classid in classIds)
                {
                    List<string> studentIds = db.StudentClasses.Where(a => a.ClassId == classid).Select(a => a.StudentId).ToList();
                    List<StudentQuestionOrder> updatedStudentQuestionOrders = new List<StudentQuestionOrder>();
                    foreach (string studentId in studentIds)
                    {
                        StudentQuestionOrder studentQuestionOrder = new StudentQuestionOrder();
                        studentQuestionOrder.Id = Guid.NewGuid().ToString();
                        studentQuestionOrder.StudentId = studentId;
                        studentQuestionOrder.ExamId = examId;
                        int[] randomNumbers = DataConverter.GetNumberArrayInRandomOrder(totalQuestions);
                        studentQuestionOrder.QuestionOrder = DataConverter.ConvertIntArrayToString(randomNumbers);
                        updatedStudentQuestionOrders.Add(studentQuestionOrder);
                    }
                    if (updatedStudentQuestionOrders.Count > 0)
                    {
                        db.StudentQuestionOrders.AddRange(updatedStudentQuestionOrders);
                        db.SaveChanges();
                    }
                }
            }
        }

        public void UpdateExamQuestion(ExamViewModel model, bool? updateOrder)
        {
            List<ExamQuestion> existingExamQuestions = db.ExamQuestions.Where(a => a.ExamId == model.Id).ToList();
            db.ExamQuestions.RemoveRange(existingExamQuestions);
            db.SaveChanges();

            List<ExamQuestion> toAdd = new List<ExamQuestion>();
            int count = 1;
            if (model.SelectedQuestions != null)
            {
                foreach (var questionid in model.SelectedQuestions)
                {
                    ExamQuestion examQuestion = new ExamQuestion();
                    examQuestion.ExamId = model.Id;
                    examQuestion.QuestionId = questionid;
                    if (updateOrder == true)
                    {
                        examQuestion.QuestionOrder = count;
                    }
                    toAdd.Add(examQuestion);
                    count++;
                }
                db.ExamQuestions.AddRange(toAdd);
                db.SaveChanges();
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
