
using EMSYS.Resources;
using EMSYS.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using EMSYS.Data;
using EMSYS.Models;


namespace EMSYS.Controllers
{
    [Authorize(Roles = "System Admin, Instructor")]
    public class AnswerController : Controller
    {
        private EMSYSdbContext db;
        private Util util;
        private readonly UserManager<AspNetUsers> _userManager;
        private ErrorLoggingService _logger;

        public AnswerController(EMSYSdbContext db, Util util, UserManager<AspNetUsers> userManager, ErrorLoggingService logger)
        {
            this.db = db;
            this.util = util;
            _userManager = userManager;
            _logger = logger;
        }

        public ActionResult Edit(string Id)
        {
            AnswerViewModel model = new AnswerViewModel();
            try
            {
                if (Id != null)
                {
                    model = GetViewModel(Id, "Edit");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(AnswerViewModel model)
        {
            try
            {
                ValidateModel(model);

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                SaveRecord(model);

                List<StudentExamViewModel> studentExams = new List<StudentExamViewModel>();

                //update the marks obtained by student according to their chosen answer, because Instructor may changed the correct answer for the question
                List<StudentAnswerCloned> studentAnswers = db.StudentAnswerCloneds.Where(a => a.QuestionId == model.QuestionId).ToList();
                if (studentAnswers != null)
                {
                    foreach (var studentAnswer in studentAnswers)
                    {
                        bool isCorrect = db.Answers.Where(a => a.Id == studentAnswer.AnswerId).Select(a => a.IsCorrect).FirstOrDefault();
                        decimal? marks = db.ExamQuestions.Where(a => a.QuestionId == model.QuestionId && a.ExamId == studentAnswer.ExamId).Select(a => a.Mark).FirstOrDefault() ?? 0;
                        studentAnswer.MarksObtained = isCorrect ? marks : 0;
                        db.Entry(studentAnswer).State = EntityState.Modified;

                        //student exam that need to recalculate the result
                        StudentExamViewModel studentExam = new StudentExamViewModel { ExamId = studentAnswer.ExamId, StudentId = studentAnswer.StudentId };
                        studentExams.Add(studentExam);
                    }
                    db.SaveChanges();
                }
                if (studentExams != null)
                {
                    //re-calculate the student result for the related exams
                    foreach (var item in studentExams)
                    {
                        decimal? result = db.StudentAnswerCloneds.Where(a => a.StudentId == item.StudentId && a.ExamId == item.ExamId).Select(a => a.MarksObtained).Sum() ?? 0;
                        StudentExam studentExam = db.StudentExams.Where(a => a.ExamId == item.ExamId && a.StudentId == item.StudentId).FirstOrDefault();
                        studentExam.Result = result;
                        decimal? marksToPass = db.Exams.Where(a => a.Id == item.ExamId).Select(a => a.MarksToPass).FirstOrDefault() ?? 0;
                        studentExam.Passed = (result >= marksToPass) ? true : false;
                        db.Entry(studentExam).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }

                TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("edit", "answer", new { Id = model.QuestionId });
        }

        //Id = Question Id
        public AnswerViewModel GetViewModel(string Id, string type)
        {
            AnswerViewModel model = new AnswerViewModel();
            var answerList = db.Answers.Where(a => a.QuestionId == Id).OrderBy(a => a.AnswerOrder).Select(a => new { AnswerText = a.AnswerText, IsCorrect = a.IsCorrect, AnswerOrder = a.AnswerOrder }).ToList();
            model.AnswerOptions = new List<AnswerOption>();
            model.AnswerOptions = (from t1 in db.Answers
                                   where t1.QuestionId == Id
                                   orderby t1.AnswerOrder
                                   select new AnswerOption
                                   {
                                       Id = t1.Id,
                                       Text = t1.AnswerText,
                                       Order = t1.AnswerOrder,
                                       IsCorrect = t1.IsCorrect
                                   }).ToList();
            if (model.AnswerOptions.Count == 0)
            {
                AnswerOption answerOption = new AnswerOption();
                model.AnswerOptions.Add(answerOption);
            }
            if (answerList != null)
            {
                model.AnswerText = answerList.Select(a => a.AnswerText).ToArray();
            }
            model.QuestionId = Id;
            var questionDetail = db.Questions.Where(a => a.Id == Id).Select(a => new { Title = a.QuestionTitle, TypeId = a.QuestionTypeId }).FirstOrDefault();
            model.QuestionTitle = questionDetail.Title;
            var questionTypeDetail = db.QuestionTypes.Where(a => a.Id == questionDetail.TypeId).Select(a => new { Code = a.Code, Name = a.Name }).FirstOrDefault();
            model.QuestionTypeCode = questionTypeDetail.Code;
            model.QuestionTypeName = questionTypeDetail.Name;
            int? correctAnswerIndex = answerList.Where(a => a.IsCorrect == true).Select(a => a.AnswerOrder).FirstOrDefault();
            model.CorrectAnswer = correctAnswerIndex.ToString();
            return model;
        }

        public void ValidateModel(AnswerViewModel model)
        {
            if (model != null)
            {
                bool emptyArray = model.AnswerOptions.All(a => a.Text == "" || a.Text == null);
                if (emptyArray)
                {
                    ModelState.AddModelError("AnswerOptions", Resource.FieldIsRequired);
                }
                if (model.CorrectAnswer == null && model.QuestionTypeCode == "MCQ")
                {
                    ModelState.AddModelError("CorrectAnswer", Resource.PleaseSelectACorrectAnswer);
                }
                else
                {
                    if (model.QuestionTypeCode == "MCQ" && model.AnswerOptions.Where(a => a.Text != "" && a.Text != null).Count() <= 1)
                    {
                        ModelState.AddModelError("AnswerOptions", Resource.MCQMustHaveAtLeast2AnswerOptions);
                    }
                    if (model.AnswerOptions.Where(a => a.Text != null && a.Text != "").Select(a => a.Text).Count() != model.AnswerOptions.Where(a => a.Text != null && a.Text != "").Select(a => a.Text).Distinct().Count())
                    {
                        ModelState.AddModelError("Id", Resource.SomeAnswerOptionsAreDuplicated);
                    }
                    if (model.QuestionTypeCode == "MCQ")
                    {
                        int index = Convert.ToInt32(model.CorrectAnswer);
                        if (string.IsNullOrEmpty(model.AnswerOptions[index].Text))
                        {
                            ModelState.AddModelError("CorrectAnswer", Resource.CannotChooseEmptyAnswerAsCorrectAnswer);
                        }
                    }
                }
            }
        }

        public void SaveRecord(AnswerViewModel model)
        {
            if (model != null)
            {
                List<Answer> existingAnswers = db.Answers.Where(a => a.QuestionId == model.QuestionId).ToList();
                int count = 0;
                if (model.AnswerOptions != null)
                {
                    foreach (AnswerOption answerOption in model.AnswerOptions)
                    {
                        if (!string.IsNullOrEmpty(answerOption.Id))
                        {
                            if (string.IsNullOrEmpty(answerOption.Text))
                            {
                                Answer answer = db.Answers.Find(answerOption.Id);
                                db.Answers.Remove(answer);
                            }
                            else
                            {
                                Answer answer = db.Answers.Find(answerOption.Id);
                                answer.AnswerText = answerOption.Text;
                                if (model.QuestionTypeCode == "MCQ")
                                {
                                    answer.IsCorrect = (count.ToString() == model.CorrectAnswer) ? true : false;
                                }
                                else if (model.QuestionTypeCode == "SA")
                                {
                                    //for short answer questions, admin/Instructor will add a bunch of possible correct answers
                                    answer.IsCorrect = true;
                                }
                                answer.ModifiedBy = _userManager.GetUserId(User);
                                answer.ModifiedOn = util.GetSystemTimeZoneDateTimeNow();
                                answer.IsoUtcModifiedOn = util.GetIsoUtcNow();
                                db.Entry(answer).State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(answerOption.Text))
                            {
                                Answer answer = new Answer();
                                answer.Id = Guid.NewGuid().ToString();
                                answer.AnswerText = answerOption.Text;
                                answer.QuestionId = model.QuestionId;
                                if (model.QuestionTypeCode == "MCQ")
                                {
                                    answer.IsCorrect = (count.ToString() == model.CorrectAnswer) ? true : false;
                                }
                                else if (model.QuestionTypeCode == "SA")
                                {
                                    //for short answer questions, admin/Instructor will add a bunch of possible correct answers
                                    answer.IsCorrect = true;
                                }
                                answer.AnswerOrder = count;
                                answer.CreatedBy = _userManager.GetUserId(User);
                                answer.CreatedOn = util.GetSystemTimeZoneDateTimeNow();
                                answer.IsoUtcCreatedOn = util.GetIsoUtcNow();
                                db.Answers.Add(answer);
                            }
                        }
                        count++;
                    }
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
