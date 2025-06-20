using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using EMSYS.Models;
using EMSYS.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using EMSYS.Data;

namespace EMSYS.Utils
{
    public class Util : IDisposable
    {
        private EMSYSdbContext db;
        private IConfiguration Configuration;
        private IWebHostEnvironment Environment;
        public Util(EMSYSdbContext _db, IConfiguration _Configuration, IWebHostEnvironment _Environment)
        {
            db = _db;
            Configuration = _Configuration;
            Environment = _Environment;
        }

        public List<string> GetCurrentUserRoleIdList(string aspnetUserId)
        {
            List<string> roles = new List<string>();
            roles = db.AspNetUserRoles.Where(a => a.UserId == aspnetUserId).Select(a => a.RoleId).ToList();
            return roles;
        }

        public List<string> GetCurrentUserRoleNameList(string aspnetUserId)
        {
            List<string> nameList = new List<string>();
            List<string> roleIds = GetCurrentUserRoleIdList(aspnetUserId);
            if (roleIds != null)
            {
                foreach (var id in roleIds)
                {
                    string name = db.AspNetRoles.Where(a => a.Id == id).Select(a => a.Name).FirstOrDefault();
                    nameList.Add(name);
                }
            }
            return nameList;
        }

        public string GetAppSettingsValue(string key)
        {
            switch (key)
            {
                case "demoData": return Configuration.GetValue<bool>("demoData").ToString().ToLower();
                // smtp setting
                case "smtpUserName": return Configuration.GetValue<string>("smtpUserName");
                case "smtpPassword": return Configuration.GetValue<string>("smtpPassword");
                case "smtpHost": return Configuration.GetValue<string>("smtpHost");
                case "smtpPort": return Configuration.GetValue<string>("smtpPort");
                //portal general setting
                case "portalName": return Configuration.GetValue<string>("portalName");
                case "timeZone": return Configuration.GetValue<string>("timeZone");
                case "confirmEmailToLogin": return Configuration.GetValue<bool>("confirmEmailToLogin").ToString().ToLower(); // Fetch as bool and convert to string
                case "environment": return Configuration.GetValue<string>("environment");
                default: break;
            }
            return "";
        }

        public string GetGlobalOptionSetCode(string Id)
        {
            string id = db.GlobalOptionSets.Where(a => a.Id == Id && a.Status == "Active").Select(a => a.Code).FirstOrDefault();
            return id;
        }

        public CreatedAndModifiedViewModel GetCreatedAndModified(string createdBy, string isoUtcCreatedOn, string modifiedBy, string isoUtcModifiedOn)
        {
            CreatedAndModifiedViewModel model = new CreatedAndModifiedViewModel();
            model.CreatedByName = string.IsNullOrEmpty(createdBy) ? "" : db.UserProfiles.Where(a => a.AspNetUserId == createdBy).Select(a => a.FullName).FirstOrDefault();
            model.ModifiedByName = string.IsNullOrEmpty(modifiedBy) ? "" : db.UserProfiles.Where(a => a.AspNetUserId == modifiedBy).Select(a => a.FullName).FirstOrDefault();
            model.FormattedCreatedOn = "";
            model.FormattedModifiedOn = "";
            if (string.IsNullOrEmpty(isoUtcCreatedOn) == false)
            {
                model.FormattedCreatedOn = isoUtcCreatedOn;
            }
            if (string.IsNullOrEmpty(isoUtcModifiedOn) == false)
            {
                model.FormattedModifiedOn = isoUtcModifiedOn;
            }
            return model;
        }

        public DateTime? GetSystemTimeZoneDateTimeNow()
        {
            string timeZone = GetAppSettingsValue("timeZone");
            DateTime dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, timeZone);
            return dateTime;
        }

        public string GetIsoUtcNow()
        {
            return DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture).Substring(0, 16);
        }

        public List<SelectListItem> GetGlobalOptionSets(string type, string selectedId)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = (from t1 in db.GlobalOptionSets
                    where t1.Type == type && t1.Status == "Active"
                    orderby t1.OptionOrder
                    select new SelectListItem
                    {
                        Value = t1.Id,
                        Text = t1.DisplayName,
                        Selected = t1.Id == selectedId ? true : false
                    }).ToList();
            return list;
        }

        public void SaveUserAttachment(IFormFile file, string userProfileId, string attachmentTypeId, string uploadedById)
        {
            var fileNameWithExtension = Path.GetFileName(file.FileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            string shortUniqueId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            string uniqueid = StringExtensions.RemoveSpecialCharacters(shortUniqueId);
            fileNameWithoutExtension = fileNameWithoutExtension.Replace(" ", "");//remove space to form a valid url
            string uniqueFileName = fileNameWithoutExtension + "_" + uniqueid + extension;
            var path = Path.Combine(this.Environment.WebRootPath, "UploadedFiles", uniqueFileName);
            string relativePath = "\\" + Path.Combine("UploadedFiles", uniqueFileName);
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            UserAttachment userAttachment = new UserAttachment();
            userAttachment.Id = Guid.NewGuid().ToString();
            userAttachment.UserProfileId = userProfileId;
            userAttachment.FileName = fileNameWithExtension;
            userAttachment.UniqueFileName = uniqueFileName;
            userAttachment.FileUrl = relativePath;
            userAttachment.AttachmentTypeId = attachmentTypeId;
            userAttachment.CreatedBy = uploadedById;
            userAttachment.CreatedOn = GetSystemTimeZoneDateTimeNow();
            userAttachment.IsoUtcCreatedOn = GetIsoUtcNow();
            db.UserAttachments.Add(userAttachment);
            db.SaveChanges();
        }

        public FileModel SaveFile(IFormFile file)
        {
            FileModel model = new FileModel();
            var fileNameWithExtension = Path.GetFileName(file.FileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            string shortUniqueId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            string uniqueid = StringExtensions.RemoveSpecialCharacters(shortUniqueId);
            fileNameWithoutExtension = fileNameWithoutExtension.Replace(" ", "");//remove space to form a valid url
            string uniqueFileName = fileNameWithoutExtension + "_" + uniqueid + extension;
            var path = Path.Combine(this.Environment.WebRootPath, "UploadedFiles", uniqueFileName);
            string relativePath = "\\" + Path.Combine("UploadedFiles", uniqueFileName);
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            model.FileName = fileNameWithExtension;
            model.UniqueFileName = uniqueFileName;
            model.FileUrl = relativePath;
            return model;
        }

        public WorksheetPart GetWorksheetPart(WorkbookPart workbookPart, string sheetName)
        {
            string relId = workbookPart.Workbook.Descendants<Sheet>().First(s => sheetName.Equals(s.Name)).Id;
            return (WorksheetPart)workbookPart.GetPartById(relId);
        }

        // Given text and a SharedStringTablePart, creates a SharedStringItem with the specified text 
        // and inserts it into the SharedStringTablePart. If the item already exists, returns its index.
        public int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
            shareStringPart.SharedStringTable.Save();

            return i;
        }

        // Given a column name, a row index, and a WorksheetPart, inserts a cell into the worksheet. 
        // If the cell already exists, returns it. 
        public Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            string cellReference = columnName + rowIndex;

            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row;
            if (sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() != 0)
            {
                row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
            }
            else
            {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            // If there is not a cell with the specified column name, insert one.  
            if (row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
            {
                return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                Cell refCell = null;
                foreach (Cell cell in row.Elements<Cell>())
                {
                    if (cell.CellReference.Value.Length == cellReference.Length)
                    {
                        if (string.Compare(cell.CellReference.Value, cellReference, true) > 0)
                        {
                            refCell = cell;
                            break;
                        }
                    }
                }

                Cell newCell = new Cell() { CellReference = cellReference };
                row.InsertBefore(newCell, refCell);

                worksheet.Save();
                return newCell;
            }
        }
        public byte[] CreateDropDownListValueInExcel(string path, string tempFilePath, List<string> valuesToInsert, string excelSheetName)
        {
            if (System.IO.File.Exists(path))
            {
                Workbook wb = new Workbook();

                //clone the question excel template
                byte[] byteArray = System.IO.File.ReadAllBytes(path);
                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(byteArray, 0, (int)byteArray.Length);
                    System.IO.File.WriteAllBytes(tempFilePath, stream.ToArray());
                }

                // Open cloned excel document
                using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(tempFilePath, true))
                {
                    // Get the SharedStringTablePart. If it does not exist, create a new one.
                    SharedStringTablePart shareStringPart;
                    if (spreadSheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
                    {
                        shareStringPart = spreadSheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
                    }
                    else
                    {
                        shareStringPart = spreadSheet.WorkbookPart.AddNewPart<SharedStringTablePart>();
                    }

                    //get the hidden Subject sheet
                    WorksheetPart worksheetPart = GetWorksheetPart(spreadSheet.WorkbookPart, excelSheetName);

                    uint count = 1;

                    //insert subject name inside the hidden Subject sheet
                    foreach (var value in valuesToInsert)
                    {
                        int index = InsertSharedStringItem(value, shareStringPart);
                        Cell cell = InsertCellInWorksheet("A", count, worksheetPart);
                        cell.CellValue = new CellValue(index.ToString());
                        cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                        worksheetPart.Worksheet.Save();
                        count++;
                    }
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(tempFilePath);

                //delete the cloned excel file from device
                System.IO.File.Delete(tempFilePath);

                return fileBytes;
            }

            return null;
        }

        public string GetGlobalOptionSetDisplayName(string Id)
        {
            string id = db.GlobalOptionSets.Where(a => a.Id == Id && a.Status == "Active").Select(a => a.DisplayName).FirstOrDefault();
            return id;
        }

        public UserProfileViewModel GetCurrentUserProfile(string currentUserId)
        {
            UserProfileViewModel model = new UserProfileViewModel();
            string profilePicTypeId = GetGlobalOptionSetId(ProjectEnum.UserAttachment.ProfilePicture.ToString(), "UserAttachment");
            model = (from t1 in db.UserProfiles
                     join t2 in db.AspNetUsers on t1.AspNetUserId equals t2.Id
                     where t2.Id == currentUserId
                     select new UserProfileViewModel
                     {
                         Id = t1.Id,
                         AspNetUserId = t1.AspNetUserId,
                         FullName = t1.FullName,
                         IDCardNumber = t1.IDCardNumber,
                         FirstName = t1.FirstName,
                         LastName = t1.LastName,
                         DateOfBirth = t1.DateOfBirth,
                         PhoneNumber = t1.PhoneNumber,
                         Address = t1.Address,
                         PostalCode = t1.PostalCode,
                         Username = t2.UserName,
                         EmailAddress = t2.Email,
                         UserStatusId = t1.UserStatusId,
                         GenderId = t1.GenderId,
                         GovernateName = t1.GovernateName,
                         IntakeYear = t1.IntakeYear,
                         Code = t1.Code,
                         CreatedBy = t1.CreatedBy,
                         ModifiedBy = t1.ModifiedBy,
                         CreatedOn = t1.CreatedOn,
                         ModifiedOn = t1.ModifiedOn,
                         IsoUtcCreatedOn = t1.IsoUtcCreatedOn,
                         IsoUtcModifiedOn = t1.IsoUtcModifiedOn,
                         IsoUtcDateOfBirth = t1.IsoUtcDateOfBirth
                     }).FirstOrDefault();
            model.UserStatusName = db.GlobalOptionSets.Where(a => a.Id == model.UserStatusId).Select(a => a.DisplayName).FirstOrDefault();
            model.UserRoleName = (from t1 in db.AspNetUserRoles
                                  join t2 in db.AspNetRoles on t1.RoleId equals t2.Id
                                  where t1.UserId == model.AspNetUserId
                                  select t2.Name).FirstOrDefault();
            model.GenderName = db.GlobalOptionSets.Where(a => a.Id == model.GenderId).Select(a => a.DisplayName).FirstOrDefault();
            model.ProfilePictureFileName = db.UserAttachments.Where(a => a.UserProfileId == model.Id && a.AttachmentTypeId == profilePicTypeId).OrderByDescending(o => o.CreatedOn).Select(a => a.UniqueFileName).FirstOrDefault();
            model.CreatedAndModified = GetCreatedAndModified(model.CreatedBy, model.IsoUtcCreatedOn, model.ModifiedBy, model.IsoUtcModifiedOn);
            return model;
        }

       

        public List<SelectListItem> GetActiveInactiveDropDown(string selectedName)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (string.IsNullOrEmpty(selectedName))
            {
                list = new List<SelectListItem>() {
                    new SelectListItem { Text = "Active", Value = "Active", Selected = false },
                    new SelectListItem { Text = "Inactive", Value = "Inactive", Selected = false }
                };
            }
            else if (selectedName == "Active")
            {
                list = new List<SelectListItem>() {
                    new SelectListItem { Text = "Active", Value = "Active", Selected = true },
                    new SelectListItem { Text = "Inactive", Value = "Inactive", Selected = false }
                };
            }
            else
            {
                list = new List<SelectListItem>() {
                    new SelectListItem { Text = "Active", Value = "Active", Selected = false },
                    new SelectListItem { Text = "Inactive", Value = "Inactive", Selected = true }
                };
            }
            return list;
        }

        public string GetGlobalOptionSetId(string code, string type)
        {
            string id = db.GlobalOptionSets.Where(a => a.Code == code && a.Type == type && a.Status == "Active").Select(a => a.Id).FirstOrDefault();
            return id;
        }
        public List<SelectListItem> GetGovernateList(string selectedName)
        {
            List<SelectListItem> governateList = new List<SelectListItem>();
            List<string> governates = db.Governates.Select(a => a.Name).ToList();
            foreach (string governate in governates)
            {
                SelectListItem selectListItem = new SelectListItem();
                selectListItem.Text = governate;
                selectListItem.Value = governate;
                selectListItem.Selected = selectedName == governate ? true : false;
                governateList.Add(selectListItem);
            }
            return governateList.OrderBy(a => a.Text).ToList();
        }
        public bool UsernameExists(string username, string currentRecordId)
        {
            bool usernameExist = false;
            if (string.IsNullOrEmpty(currentRecordId))
            {
                usernameExist = db.AspNetUsers.Where(a => a.UserName == username).Any();
            }
            else
            {
                //user is editing record, need to add one more filter "a.Id != currentRecordId"
                usernameExist = db.AspNetUsers.Where(a => a.UserName == username && a.Id != currentRecordId).Any();
            }
            return usernameExist;
        }

        public bool EmailExists(string email, string currentRecordId)
        {
            bool emailExist = false;
            if (string.IsNullOrEmpty(currentRecordId))
            {
                emailExist = db.AspNetUsers.Where(a => a.Email == email).Any();
            }
            else
            {
                //user is editing his/her own record, need to add one more filter "a.Id != currentRecordId"
                emailExist = db.AspNetUsers.Where(a => a.Email == email && a.Id != currentRecordId).Any();
            }
            return emailExist;
        }

        public string GetUserProfileId(string aspNetUserId)
        {
            string id = db.UserProfiles.Where(a => a.AspNetUserId == aspNetUserId).Select(a => a.Id).FirstOrDefault();
            return id;
        }

        public EmailTemplate EmailTemplateForConfirmEmail(string Username, string callbackUrl)
        {
            string websiteName = GetAppSettingsValue("portalName");
            EmailTemplate emailTemplate = db.EmailTemplates.Where(a => a.Type == "ConfirmEmail").FirstOrDefault();
            string subject = emailTemplate.Subject;
            string body = emailTemplate.Body;
            subject = ReplaceWords(subject, "[WebsiteName]", websiteName);
            body = ReplaceWords(body, "[Username]", Username);
            body = ReplaceWords(body, "[WebsiteName]", websiteName);
            body = ReplaceWords(body, "[Url]", callbackUrl);
            emailTemplate.Subject = subject;
            emailTemplate.Body = body;
            return emailTemplate;
        }

        public EmailTemplate EmailTemplateForPasswordResetByAdmin(string Username = "", string ResetByName = "", string NewPassword = "")
        {
            string websiteName = GetAppSettingsValue("portalName");
            EmailTemplate emailTemplate = db.EmailTemplates.Where(a => a.Type == ProjectEnum.EmailTemplate.PasswordResetByAdmin.ToString()).FirstOrDefault();
            string subject = emailTemplate.Subject;
            string body = emailTemplate.Body;
            subject = ReplaceWords(subject, "[WebsiteName]", websiteName);
            body = ReplaceWords(body, "[WebsiteName]", websiteName);
            body = ReplaceWords(body, "[Username]", Username);
            body = ReplaceWords(body, "[ResetByName]", ResetByName);
            body = ReplaceWords(body, "[NewPassword]", NewPassword);
            emailTemplate.Subject = subject;
            emailTemplate.Body = body;
            return emailTemplate;
        }

        public EmailTemplate EmailTemplateForForgotPassword(string Username, string callbackUrl)
        {
            string websiteName = GetAppSettingsValue("portalName");
            EmailTemplate emailTemplate = db.EmailTemplates.Where(a => a.Type == "ForgotPassword").FirstOrDefault();
            string subject = emailTemplate.Subject;
            string body = emailTemplate.Body;
            subject = ReplaceWords(subject, "[WebsiteName]", websiteName);
            body = ReplaceWords(body, "[Username]", Username);
            body = ReplaceWords(body, "[WebsiteName]", websiteName);
            body = ReplaceWords(body, "[Url]", callbackUrl);
            emailTemplate.Subject = subject;
            emailTemplate.Body = body;
            return emailTemplate;
        }

        public string GetMyProfilePictureName(string userid)
        {
            string fileName = "";
            if (!string.IsNullOrEmpty(userid))
            {
                string upId = GetUserProfileId(userid);
                string profilePictureTypeId = GetGlobalOptionSetId(ProjectEnum.UserAttachment.ProfilePicture.ToString(), "UserAttachment");
                fileName = db.UserAttachments.Where(a => a.UserProfileId == upId && a.AttachmentTypeId == profilePictureTypeId).OrderByDescending(o => o.CreatedOn).Select(a => a.UniqueFileName).FirstOrDefault();
            }
            return fileName;
        }

        public string ReplaceWords(string sentence, string target, string replaceWith)
        {
            string result = sentence.Replace(target, replaceWith);
            return result;
        }

        public void SendEmail(string email, string subject, string body)
        {
            string host = GetAppSettingsValue("smtpHost");
            string strPort = GetAppSettingsValue("smtpPort");
            int port = Int32.Parse(strPort);
            string userName = GetAppSettingsValue("smtpUserName");
            string password = GetAppSettingsValue("smtpPassword");
            var client = new SmtpClient(host, port);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(userName, password);
            client.EnableSsl = true;

            MailMessage mail = new MailMessage(userName, email, subject, body);
            mail.IsBodyHtml = true;
            client.Send(mail);
        }

        public List<string> ValidateColumns(List<string> dtColumns, List<string> columns)
        {
            var errors = new List<string>();
            //check either the provided columns and required columns length same
            if (dtColumns.Count != columns.Count)
            {
                errors.Add(Resource.ColumnsCountMismatch);
            }
            else
            {
                //check either the required columns exists in provided columns through excel
                foreach (var column in columns)
                {
                    if (!dtColumns.Contains(column))
                        errors.Add($"{Resource.Column} {column} {Resource.notfoundormismatch}");
                }
            }
            return errors;
        }

        public DateTime? ConvertToSystemTimeZoneDateTime(string isoUtc)
        {
            DateTime dateTimeUtc = DateTime.Parse(isoUtc);
            string timeZone = GetAppSettingsValue("timeZone");
            DateTime result = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTimeUtc, TimeZoneInfo.Utc.Id, timeZone);
            return result;
        }

        public string ConvertToIsoUtcDate(DateTime? dateTime)
        {
            if (dateTime != null)
            {
                DateTime dateTimeUtc = dateTime.Value.ToUniversalTime();
                return dateTimeUtc.ToString("o", CultureInfo.InvariantCulture);
            }
            return "";
        }

        //call this method like this:
        //GetDataForMultiSelect(model.SubjectIdList, db.Subjects, a => a.Name, a => a.Id);
        //selectedIds = the selected values
        //dbSet = db.Subjects or other entity
        //a => a.Name = the field in db.Subjects that is relevant to the 'Text' of SelectListItem
        //a => a.Id = the field in db.Subjects that is relevant to the 'Value' of SelectListItem
        public List<SelectListItem> GetDataForMultiSelect<T>(List<string> selectedIds, DbSet<T> dbSet, Expression<Func<T, string>> fieldNameForText, Expression<Func<T, string>> fieldNameForValue) where T : class
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (selectedIds != null)
            {
                list = dbSet.AsEnumerable().OrderBy(fieldNameForText.Compile()).Select(x => new SelectListItem
                {
                    Text = fieldNameForText.Compile()(x),
                    Value = fieldNameForValue.Compile()(x),
                    Selected = selectedIds?.Contains(fieldNameForValue.Compile()(x)) ?? false
                }).ToList();
            }
            else
            {
                list = dbSet.AsEnumerable().OrderBy(fieldNameForText.Compile()).Select(x => new SelectListItem
                {
                    Text = fieldNameForText.Compile()(x),
                    Value = fieldNameForValue.Compile()(x),
                    Selected = false
                }).ToList();
            }
            return list;
        }

  
        public List<SelectListItem> GetDataForDropDownList<T>(string selectedId, DbSet<T> dbSet, Expression<Func<T, string>> fieldNameForText, Expression<Func<T, string>> fieldNameForValue) where T : class
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (selectedId != null)
            {
                list = dbSet.AsEnumerable().OrderBy(fieldNameForText.Compile()).Select(x => new SelectListItem
                {
                    Text = fieldNameForText.Compile()(x),
                    Value = fieldNameForValue.Compile()(x),
                    Selected = (selectedId == fieldNameForValue.Compile()(x)) ? true : false
                }).ToList();
            }
            else
            {
                list = dbSet.AsEnumerable().OrderBy(fieldNameForText.Compile()).Select(x => new SelectListItem
                {
                    Text = fieldNameForText.Compile()(x),
                    Value = fieldNameForValue.Compile()(x),
                    Selected = false
                }).ToList();
            }
            return list;
        }

        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }
    }
}
