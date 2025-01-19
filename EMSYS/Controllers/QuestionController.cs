using EMSYS.Models;
using EMSYS.Resources;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using EMSYS.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using EMSYS.Data;

namespace EMSYS.Controllers
{
    [Authorize(Roles = "System Admin, Instructor")]
    public class QuestionController : Controller
    {
        private EMSYSdbContext db;
        private Util util;
        private readonly UserManager<AspNetUsers> _userManager;
        private IWebHostEnvironment Environment;
        private ErrorLoggingService _logger;

        public QuestionController(EMSYSdbContext db, Util util, UserManager<AspNetUsers> userManager, IWebHostEnvironment _Environment, ErrorLoggingService logger)
        {
            this.db = db;
            this.util = util;
            _userManager = userManager;
            Environment = _Environment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            QuestionViewModel model = new QuestionViewModel();
            SetupSelectLists(model);
            QuestionListing listing = new QuestionListing();
            listing.SubjectIdSelectList = model.SubjectIdSelectList;
            listing.QuestionTypeIdSelectList = model.QuestionTypeIdSelectList;
            return View(listing);
        }

        public void SetupSelectLists(QuestionViewModel model)
        {
            model.SubjectIdSelectList = (from t1 in db.Subjects
                                         orderby t1.Name
                                         select new SelectListItem
                                         {
                                             Text = t1.Name,
                                             Value = t1.Id,
                                             Selected = t1.Id == model.SubjectId ? true : false
                                         }).ToList();
            model.QuestionTypeIdSelectList = (from t1 in db.QuestionTypes
                                              orderby t1.Name
                                              select new SelectListItem
                                              {
                                                  Text = t1.Name,
                                                  Value = t1.Id,
                                                  Selected = t1.Id == model.QuestionTypeId ? true : false
                                              }).ToList();
            model.ActiveInactiveSelectList = util.GetActiveInactiveDropDown(model.IsActive);
        }

        public IActionResult ViewRecord(string Id)
        {
            QuestionViewModel model = new QuestionViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "View");
            }
            SetupSelectLists(model);
            return View(model);
        }

        public IActionResult Edit(string Id)
        {
            QuestionViewModel model = new QuestionViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "Edit");
            }
            SetupSelectLists(model);
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(QuestionViewModel model)
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

                string questionType = db.QuestionTypes.Where(a => a.Id == model.QuestionTypeId).Select(a => a.Code).FirstOrDefault();
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.Anerroroccurred;
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }

            return RedirectToAction("edit", "answer", new { Id = model.Id });
        }

        public IActionResult Delete(string Id)
        {
            try
            {
                if (db.StudentAnswerCloneds.Where(a => a.QuestionId == Id).Any() == true)
                {
                    TempData["NotifyFailed"] = Resource.QuestionInUsedCannotBeDeleted;
                }
                else
                {
                    Question question = db.Questions.Where(a => a.Id == Id).FirstOrDefault();
                    if (question != null)
                    {
                        db.Questions.Remove(question);
                        db.SaveChanges();
                    }
                    TempData["NotifySuccess"] = Resource.RecordDeletedSuccessfully;
                }
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("index");
        }

        public IActionResult DeleteImage(string Id)
        {
            try
            {
                if (Id != null)
                {
                    QuestionAttachment questionAttachment = db.QuestionAttachments.Where(a => a.QuestionId == Id).FirstOrDefault();
                    if (questionAttachment != null)
                    {
                        db.QuestionAttachments.Remove(questionAttachment);
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
            return RedirectToAction("edit", new { Id = Id });
        }

        public async Task<IActionResult> GetPartialViewListing(string sort, string search, int? pg, int? size)
        {
            try
            {
                List<ColumnHeader> headers = new List<ColumnHeader>();
                if (string.IsNullOrEmpty(sort))
                {
                    sort = QuestionListConfig.DefaultSortOrder;
                }
                headers = ListUtil.GetColumnHeaders(QuestionListConfig.DefaultColumnHeaders, sort);
                var list = ReadQuestions();
                string searchMessage = QuestionListConfig.SearchMessage;
                list = QuestionListConfig.PerformSearch(list, search);
                list = QuestionListConfig.PerformSort(list, sort);
                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = pg ?? 1;
                ViewData["CurrentSearch"] = search;
                int? total = list.Count();
                int? defaultSize = QuestionListConfig.DefaultPageSize;
                size = size == 0 || size == null ? (defaultSize != -1 ? defaultSize : total) : size == -1 ? total : size;
                ViewData["CurrentSize"] = size;
                PaginatedList<QuestionViewModel> result = await PaginatedList<QuestionViewModel>.CreateAsync(list, pg ?? 1, size.Value, total.Value, headers, searchMessage);
                return PartialView("~/Views/Question/_MainList.cshtml", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return PartialView("~/Views/Shared/Error.cshtml", null);
        }

        public async Task<IActionResult> ExamAddQuestionListing(string id, string sort, string search, int? pg, int? size)
        {
            try
            {
                List<ColumnHeader> headers = new List<ColumnHeader>();
                if (string.IsNullOrEmpty(sort))
                {
                    sort = AddQuestionInExamListConfig.DefaultSortOrder;
                }
                headers = ListUtil.GetColumnHeaders(AddQuestionInExamListConfig.DefaultColumnHeaders, sort);
                var list = ReadQuestions(id);
                string searchMessage = AddQuestionInExamListConfig.SearchMessage;
                list = AddQuestionInExamListConfig.PerformSearch(list, search);
                list = AddQuestionInExamListConfig.PerformSort(list, sort);
                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = pg ?? 1;
                ViewData["CurrentSearch"] = search;
                int? total = list.Count();
                int? defaultSize = AddQuestionInExamListConfig.DefaultPageSize;
                size = size == 0 || size == null ? (defaultSize != -1 ? defaultSize : total) : size == -1 ? total : size;
                ViewData["CurrentSize"] = size;
                PaginatedList<QuestionViewModel> result = await PaginatedList<QuestionViewModel>.CreateAsync(list, pg ?? 1, size.Value, total.Value, headers, searchMessage);
                return PartialView("~/Views/Exam/_QuestionList.cshtml", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return PartialView("~/Views/Shared/Error.cshtml", null);
        }

        public IQueryable<QuestionViewModel> ReadQuestions(string id = "")
        {
            try
            {
                List<string> subjectsForExam = new List<string>();
                if (!string.IsNullOrEmpty(id))
                {
                    subjectsForExam = db.ExamSubjects.Where(a => a.ExamId == id).Select(a => a.SubjectId).ToList();
                }
                string userid = _userManager.GetUserId(User);
                var query = from t1 in db.Questions
                            join t2 in db.Subjects on t1.SubjectId equals t2.Id into g1
                            from t3 in g1.DefaultIfEmpty()
                            join t4 in db.QuestionTypes on t1.QuestionTypeId equals t4.Id into g2
                            from t5 in g2.DefaultIfEmpty()
                            join t6 in db.QuestionAttachments on t1.Id equals t6.QuestionId into g3
                            from t7 in g3.DefaultIfEmpty()
                            let t8 = db.Answers.Where(a => a.QuestionId == t1.Id).Any()
                            let t9 = db.StudentAnswerCloneds.Where(a => a.QuestionId == t1.Id).Any()
                            where (User.IsInRole("System Admin") == true || User.IsInRole("Instructor") == true) ? t1.CreatedBy == userid : t1.Id != null &&
                            (!string.IsNullOrEmpty(id) ? subjectsForExam.Contains(t3.Id) == true : t1.Id != null)
                            orderby t1.CreatedOn
                            select new QuestionViewModel
                            {
                                Id = t1.Id,
                                QuestionTitle = t1.QuestionTitle,
                                SubjectName = t3 == null ? "" : t3.Name,
                                QuestionTypeName = t5 == null ? "" : t5.Name,
                                QuestionTypeCode = t5 == null ? "" : t5.Code,
                                IsActive = t1.IsActive == true ? "Active" : "Inactive",
                                ImageUniqueFileName = t7 == null ? "" : t7.UniqueFileName,
                                CreatedOn = t1.CreatedOn,
                                IsoUtcCreatedOn = t1.IsoUtcCreatedOn,
                                AnswerSaved = t8,
                                CanDelete = t9 == true ? false : true
                            };
                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return null;
        }

        public QuestionViewModel GetViewModel(string Id, string type)
        {
            QuestionViewModel model = new QuestionViewModel();
            Question question = db.Questions.Where(a => a.Id == Id).FirstOrDefault();
            model.Id = question.Id;
            model.QuestionTitle = question.QuestionTitle;
            model.QuestionTypeId = question.QuestionTypeId;
            model.SubjectId = question.SubjectId;
            model.IsActive = question.IsActive == true ? "Active" : "Inactive";
            var questionAttachment = db.QuestionAttachments.Where(a => a.QuestionId == question.Id).Select(a => new { FileName = a.FileName, UniqueFileName = a.UniqueFileName, FileUrl = a.FileUrl }).FirstOrDefault();
            model.ImageFileName = questionAttachment?.FileName;
            model.ImageUniqueFileName = questionAttachment?.UniqueFileName;
            model.ImageFileUrl = questionAttachment?.FileUrl;
            if (type == "View")
            {
                model.CreatedAndModified = util.GetCreatedAndModified(question.CreatedBy, question.IsoUtcCreatedOn, question.ModifiedBy, question.IsoUtcModifiedOn);
                var questionTypeDetail = db.QuestionTypes.Where(a => a.Id == question.QuestionTypeId).Select(a => new { Code = a.Code, Name = a.Name }).FirstOrDefault();
                model.QuestionTypeName = questionTypeDetail == null ? "" : questionTypeDetail.Name;
                model.QuestionTypeCode = questionTypeDetail == null ? "" : questionTypeDetail.Code;
                model.SubjectName = db.Subjects.Where(a => a.Id == question.SubjectId).Select(a => a.Name).FirstOrDefault();
            }
            else
            {
                string code = db.QuestionTypes.Where(a => a.Id == question.QuestionTypeId).Select(a => a.Code).FirstOrDefault();
                model.QuestionTypeCode = code == null ? "" : code;
            }
            return model;
        }

        public void ValidateModel(QuestionViewModel model)
        {
            if (model != null)
            {
                bool duplicated = false;
                //cannot have same question for the same subject & same question type
                if (model.Id != null)
                {
                    duplicated = db.Questions.Where(a => a.QuestionTitle == model.QuestionTitle && a.SubjectId == model.SubjectId && a.QuestionTypeId == model.QuestionTypeId && a.Id != model.Id).Any();
                }
                else
                {
                    duplicated = db.Questions.Where(a => a.QuestionTitle == model.QuestionTitle && a.SubjectId == model.SubjectId && a.QuestionTypeId == model.QuestionTypeId).Select(a => a.Id).Any();
                }
                if (duplicated == true)
                {
                    ModelState.AddModelError("QuestionTitle", Resource.ThisIsADuplicatedValue);
                }
            }
        }

        public void SaveRecord(QuestionViewModel model)
        {
            if (model != null)
            {
                if (model.Id == null)
                {
                    Question question = new Question();
                    question.Id = Guid.NewGuid().ToString();
                    AssignModelValue(question, model);
                    question.CreatedBy = _userManager.GetUserId(User);
                    question.CreatedOn = util.GetSystemTimeZoneDateTimeNow();
                    question.IsoUtcCreatedOn = util.GetIsoUtcNow();
                    db.Questions.Add(question);
                    db.SaveChanges();
                    model.Id = question.Id;
                }
                else
                {
                    Question question = db.Questions.Where(a => a.Id == model.Id).FirstOrDefault();
                    AssignModelValue(question, model);
                    question.ModifiedBy = _userManager.GetUserId(User);
                    question.ModifiedOn = util.GetSystemTimeZoneDateTimeNow();
                    question.IsoUtcModifiedOn = util.GetIsoUtcNow();
                    db.Entry(question).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (model.ImageFile != null)
                {
                    QuestionAttachment attachment = db.QuestionAttachments.Where(a => a.QuestionId == model.Id).FirstOrDefault();
                    FileModel fileModel = util.SaveFile(model.ImageFile);
                    if (attachment != null)
                    {
                        attachment.FileName = fileModel.FileName;
                        attachment.UniqueFileName = fileModel.UniqueFileName;
                        attachment.FileUrl = fileModel.FileUrl;
                        attachment.ModifiedBy = _userManager.GetUserId(User);
                        attachment.ModifiedOn = util.GetSystemTimeZoneDateTimeNow();
                        attachment.IsoUtcModifiedOn = util.GetIsoUtcNow();
                        db.Entry(attachment).State = EntityState.Modified;
                    }
                    else
                    {
                        attachment = new QuestionAttachment();
                        attachment.Id = Guid.NewGuid().ToString();
                        attachment.QuestionId = model.Id;
                        attachment.FileName = fileModel.FileName;
                        attachment.UniqueFileName = fileModel.UniqueFileName;
                        attachment.FileUrl = fileModel.FileUrl;
                        attachment.CreatedBy = _userManager.GetUserId(User);
                        attachment.CreatedOn = util.GetSystemTimeZoneDateTimeNow();
                        attachment.IsoUtcCreatedOn = util.GetIsoUtcNow();
                        db.QuestionAttachments.Add(attachment);
                    }
                    db.SaveChanges();
                }
            }
        }

        public void AssignModelValue(Question question, QuestionViewModel model)
        {
            question.QuestionTitle = model.QuestionTitle;
            question.QuestionTypeId = model.QuestionTypeId;
            question.SubjectId = model.SubjectId;
            question.IsActive = model.IsActive == "Active" ? true : false;
        }

        public IActionResult Import()
        {
            ImportFromExcel importFromExcel = new ImportFromExcel();
            return View(importFromExcel);
        }

        public IActionResult DownloadImportTemplate()
        {
            var path = Path.Combine(this.Environment.WebRootPath, "Assets", "QuestionTemplate.xlsx");
            var tempFilePath = Path.Combine(this.Environment.WebRootPath, "Assets", "ImportQuestionFromExcel.xlsx");
            List<string> subjectNames = db.Subjects.Select(a => a.Name).ToList();
            byte[] fileBytes = util.CreateDropDownListValueInExcel(path, tempFilePath, subjectNames, "Subject");
            if (fileBytes == null)
            {
                return null;
            }
            string dtnow = util.GetIsoUtcNow();
            dtnow = dtnow.Replace("-", "");
            dtnow = dtnow.Replace(":", "");
            dtnow = dtnow.Replace(".", "");
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, $"ImportQuestionFromExcel{dtnow}.xlsx");
        }

        [HttpPost]
        public IActionResult Import(ImportFromExcel model, IFormFile File)
        {
            try
            {
                List<string> errors = new List<string>();
                List<ImportFromExcelError> errorsList = new List<ImportFromExcelError>();

                QuestionViewModel questionViewModel = new QuestionViewModel();

                int successCount = 0;
                int dtRowsCount = 0;
                List<string> columns = new List<string>();

                using (var memoryStream = new MemoryStream())
                {
                    File.CopyTo(memoryStream);
                    using (var reader = ExcelReaderFactory.CreateReader(memoryStream))
                    {
                        var ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        });

                        var dt = ds.Tables[0];

                        foreach (var col in dt.Columns.Cast<DataColumn>())
                        {
                            col.ColumnName = col.ColumnName.Replace("*", "");
                            columns.Add(col.ColumnName);
                        }
                        dtRowsCount = dt.Rows.Count;

                        errors = util.ValidateColumns(columns, new List<string>
                    {
                        "Question Title","Subject",
                        "Question Type","Status"
                    });

                        //if all columns validated
                        if (errors.Count == 0)
                        {
                            for (int i = 0; i < dtRowsCount; i++)
                            {
                                try
                                {
                                    //decimal markValue = 0;
                                    string questionTitle = dt.Rows[i].Field<string>("Question Title");
                                    string subject = dt.Rows[i].Field<string>("Subject");
                                    string questionType = dt.Rows[i].Field<string>("Question Type");
                                    string status = dt.Rows[i].Field<string>("Status");

                                    string subjectId = db.Subjects.Where(a => a.Name == subject).Select(a => a.Id).FirstOrDefault();
                                    string questionTypeId = db.QuestionTypes.Where(a => a.Name == questionType).Select(a => a.Id).FirstOrDefault();

                                    questionViewModel.QuestionTitle = questionTitle;
                                    questionViewModel.SubjectId = subjectId;
                                    questionViewModel.QuestionTypeId = questionTypeId;

                                    errors = ValidateImportRow(questionViewModel);

                                    if (errors.Count() > 0)
                                    {
                                        ImportFromExcelError importFromExcelError = new ImportFromExcelError();
                                        importFromExcelError.Row = $"At Row {i + 2}";
                                        importFromExcelError.Errors = errors;
                                        errorsList.Add(importFromExcelError);
                                        continue;
                                    }

                                    Question question = new Question();
                                    question.Id = Guid.NewGuid().ToString();
                                    question.QuestionTitle = questionTitle;
                                    question.SubjectId = subjectId;
                                    question.QuestionTypeId = questionTypeId;
                                    question.IsActive = status == "Active" ? true : false;
                                    question.CreatedBy = _userManager.GetUserId(User);
                                    question.CreatedOn = util.GetSystemTimeZoneDateTimeNow();
                                    question.IsoUtcCreatedOn = util.GetIsoUtcNow();
                                    db.Questions.Add(question);
                                    db.SaveChanges();
                                    successCount++;
                                    ModelState.Clear();
                                }
                                catch (Exception ex)
                                {
                                    errors.Add($"{ex.Message} - Row: {i + 2}");
                                }
                            }
                        }
                        else
                        {
                            ImportFromExcelError importFromExcelError = new ImportFromExcelError();
                            importFromExcelError.Row = Resource.InvalidTemplate;
                            importFromExcelError.Errors = errors;
                            errorsList.Add(importFromExcelError);
                        }
                    }
                }

                if (errorsList.Count > 0)
                {
                    model.ErrorList = errorsList;
                    model.UploadResult = $"{successCount} {Resource.outof} {dtRowsCount} {Resource.recordsuploaded}";
                    return View("import", model);
                }
                TempData["NotifySuccess"] = Resource.RecordsImportedSuccessfully;
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("index");
        }

        public List<string> ValidateImportRow(QuestionViewModel model)
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrEmpty(model.QuestionTitle) || string.IsNullOrEmpty(model.SubjectId) || string.IsNullOrEmpty(model.QuestionTypeId))
            {
                errors.Add(Resource.SomeRequiredFieldsAreEmpty);
            }
            else
            {
                if (string.IsNullOrEmpty(model.SubjectId))
                {
                    errors.Add(Resource.InvalidSubject);
                }
                if (string.IsNullOrEmpty(model.QuestionTypeId))
                {
                    errors.Add(Resource.InvalidQuestionType);
                }
                bool existed = db.Questions.Where(a => a.QuestionTitle == model.QuestionTitle).Any();
                if (existed)
                {
                    errors.Add($"{Resource.QuestionTitle} {Resource.alreadyexists}");
                }
            }

            return errors;
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
