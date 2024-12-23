using ExcelDataReader;
using EMSYS.Models;
using System.Data;
using System.Globalization;
using EMSYS.Resources;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMSYS.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using EMSYS.Data;

namespace EMSYS.Controllers
{
    [Authorize(Roles = "System Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AspNetUsers> _userManager;
        private readonly SignInManager<AspNetUsers> _signInManager;
        private EMSYSdbContext db;
        private Util util;
        private IWebHostEnvironment Environment;
        private ErrorLoggingService _logger;

        public UserController(EMSYSdbContext _db, UserManager<AspNetUsers> userManager,
                              SignInManager<AspNetUsers> signInManager, Util _util, IWebHostEnvironment _Environment, ErrorLoggingService logger)
        {
            db = _db;
            _userManager = userManager;
            _signInManager = signInManager;
            util = _util;
            Environment = _Environment;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            UserProfileListing model = new UserProfileListing();
            model.RoleSelectList = await db.AspNetRoles.Select(a => new SelectListItem { Text = a.Name, Value = a.Name, Selected = false }).ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> GetPartialViewUser(string sort, string search, int? pg, int? size)
        {
            try
            {
                List<ColumnHeader> headers = new List<ColumnHeader>();
                if (string.IsNullOrEmpty(sort))
                {
                    sort = UserListConfig.DefaultSortOrder;
                }
                headers = ListUtil.GetColumnHeaders(UserListConfig.DefaultColumnHeaders, sort);
                var list = ReadUserProfileList();
                string searchMessage = UserListConfig.SearchMessage;
                list = UserListConfig.PerformSearch(list, search);
                list = UserListConfig.PerformSort(list, sort);
                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = pg ?? 1;
                ViewData["CurrentSearch"] = search;
                int? total = list.Count();
                int? defaultSize = UserListConfig.DefaultPageSize;
                size = size == 0 || size == null ? (defaultSize != -1 ? defaultSize : total) : size == -1 ? total : size;
                ViewData["CurrentSize"] = size;
                PaginatedList<UserProfileViewModel> result = await PaginatedList<UserProfileViewModel>.CreateAsync(list, pg ?? 1, size.Value, total.Value, headers, searchMessage);
                return PartialView("~/Views/User/_MainList.cshtml", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return PartialView("~/Views/Shared/Error.cshtml", null);
        }

        public IQueryable<UserProfileViewModel> ReadUserProfileList()
        {
            var userList = from t1 in db.UserProfiles.AsNoTracking()
                           let t2 = db.AspNetUsers.Where(u => u.Id == t1.AspNetUserId).SingleOrDefault()
                           let t3 = db.GlobalOptionSets.Where(g => g.Id == t1.UserStatusId).SingleOrDefault()
                           let t5 = db.AspNetUserRoles.Where(ur => ur.UserId == t1.AspNetUserId).SingleOrDefault()
                           let t6 = db.AspNetRoles.Where(r => r.Id == t5.RoleId).SingleOrDefault()
                           select new UserProfileViewModel
                           {
                               Id = t1.Id,
                               FullName = t1.FullName,
                               Username = t2.UserName,
                               AspNetUserId = t1.AspNetUserId,
                               UserStatusName = t3 == null ? "" : t3.DisplayName,
                               EmailAddress = t2.Email,
                               PhoneNumber = t1.PhoneNumber,
                               CountryName = t1.CountryName,
                               Address = t1.Address,
                               CreatedOn = t1.CreatedOn,
                               IsoUtcCreatedOn = t1.IsoUtcCreatedOn,
                               UserRoleName = t6 == null ? "" : t6.Name
                           };
            return userList;
        }

        public UserProfileViewModel GetUserProfileViewModel(string Id, string type)
        {
            UserProfileViewModel model = new UserProfileViewModel();
            string profilePicTypeId = util.GetGlobalOptionSetId(ProjectEnum.UserAttachment.ProfilePicture.ToString(), "UserAttachment");
            model = (from t1 in db.UserProfiles
                     join t2 in db.AspNetUsers on t1.AspNetUserId equals t2.Id
                     where t1.Id == Id
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
                         CountryName = t1.CountryName,
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
            model.ClassIdList = db.StudentClasses.Where(a => a.StudentId == model.AspNetUserId).Select(a => a.ClassId).ToList();
            model.UserStatusName = db.GlobalOptionSets.Where(a => a.Id == model.UserStatusId).Select(a => a.DisplayName).FirstOrDefault();
            var user = db.AspNetUsers.Where(a => a.Id == model.AspNetUserId).FirstOrDefault();
            model.UserRoleName = (from t1 in db.AspNetUserRoles
                                  join t2 in db.AspNetRoles on t1.RoleId equals t2.Id
                                  where t1.UserId == model.AspNetUserId
                                  select t2.Name).ToList().FirstOrDefault();
            model.GenderName = db.GlobalOptionSets.Where(a => a.Id == model.GenderId).Select(a => a.DisplayName).FirstOrDefault();
            model.ProfilePictureFileName = db.UserAttachments.Where(a => a.UserProfileId == model.Id && a.AttachmentTypeId == profilePicTypeId).OrderByDescending(a => a.CreatedOn).Select(a => a.UniqueFileName).FirstOrDefault();
            if (type == "View")
            {
                model.CreatedAndModified = util.GetCreatedAndModified(model.CreatedBy, model.IsoUtcCreatedOn, model.ModifiedBy, model.IsoUtcModifiedOn);
            }
            return model;
        }

        public void SetupSelectLists(UserProfileViewModel model)
        {
            model.GenderSelectList = util.GetGlobalOptionSets("Gender", model.GenderId);
            model.UserStatusSelectList = util.GetGlobalOptionSets("UserStatus", model.UserStatusId);
            model.UserRoleSelectList = util.GetDataForDropDownList(model.UserRoleName, db.AspNetRoles, a => a.Name, a => a.Name);
            model.CountrySelectList = util.GetCountryList(model.CountryName);
            model.ClassSelectList = util.GetDataForMultiSelect(model.ClassIdList, db.ClassHubs, a => a.Name, a => a.Id);
        }

        public List<string> ValidateImportUserFromExcel(UserProfileViewModel model)
        {
            List<string> errors = new List<string>();
            if (model != null)
            {
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.EmailAddress)
                    || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword)
                    || string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.PhoneNumber)
                    || string.IsNullOrEmpty(model.CountryName) || string.IsNullOrEmpty(model.UserRoleName))
                {
                    errors.Add(Resource.SomeRequiredFieldsAreEmpty);
                }
                else
                {
                    if (model.Password != model.ConfirmPassword)
                    {
                        errors.Add(Resource.PasswordNotMatch);
                    }
                    PasswordValidation passwordValidation = new PasswordValidation();
                    if (passwordValidation.IsValid(model.Password) == false)
                    {
                        errors.Add(passwordValidation.ErrorMessage);
                    }
                    string phonePattern = @"^\+?\d{1,4}?[-.\s]?\(?\d{1,3}?\)?[-.\s]?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,9}$";
                    if (model.PhoneNumber != null)
                    {
                        if (Regex.Match(model.PhoneNumber, phonePattern).Success == false)
                        {
                            errors.Add(Resource.InvalidPhoneNumber);
                        }
                    }
                    EmailAddressAttribute emailAddressAttribute = new EmailAddressAttribute();
                    bool emailValid = emailAddressAttribute.IsValid(model.EmailAddress);
                    if (emailValid == false)
                    {
                        errors.Add(Resource.InvalidEmailAddress);
                    }
                    string usernamePattern = @"^[A-Za-z]\w{3,29}$";
                    if (model.Username != null)
                    {
                        if (Regex.Match(model.Username, usernamePattern).Success == false)
                        {
                            errors.Add(Resource.InvalidUsername);
                        }
                    }
                    if (db.AspNetRoles.Where(a => a.Name == model.UserRoleName).Any() == false)
                    {
                        errors.Add(Resource.RoleNotFound);
                    }
                    List<SelectListItem> countries = util.GetCountryList("");
                    if (countries?.Where(a => a.Text == model.CountryName).Any() == false)
                    {
                        errors.Add(Resource.CountryNotFound);
                    }
                    var _user = db.AspNetUsers.FirstOrDefault(a => a.UserName == model.Username || a.Email == model.EmailAddress);
                    if (_user != null)
                    {
                        errors.Add($"{Resource.User} {model.Username} {Resource.alreadyexists}.");
                    }
                }
            }
            return errors;
        }

        public IActionResult DownloadImportTemplate()
        {
            var path = Path.Combine(this.Environment.WebRootPath, "Assets", "UserExcelTemplate.xlsx");
            var tempFilePath = Path.Combine(this.Environment.WebRootPath, "Assets", "ImportUsersFromExcel.xlsx");
            List<string> countries = db.Countries.Select(a => a.Name).ToList();
            byte[] fileBytes = util.CreateDropDownListValueInExcel(path, tempFilePath, countries, "Country");
            if (fileBytes == null)
            {
                return null;
            }
            string dtnow = util.GetIsoUtcNow();
            dtnow = dtnow.Replace("-", "");
            dtnow = dtnow.Replace(":", "");
            dtnow = dtnow.Replace(".", "");
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, $"ImportUsersFromExcel{dtnow}.xlsx");
        }

        public IActionResult Import()
        {
            ImportFromExcel importFromExcel = new ImportFromExcel();
            return View(importFromExcel);
        }

        [HttpPost]
        public async Task<IActionResult> Import(ImportFromExcel model, IFormFile File)
        {
            try
            {
                List<string> errors = new List<string>();
                List<ImportFromExcelError> errorsList = new List<ImportFromExcelError>();
                var users = new List<AspNetUsers>();

                UserProfileViewModel upModel = new UserProfileViewModel();

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
                        "Username","Email Address","Password",
                        "Confirm Password","Full Name","First Name",
                        "Last Name","Phone Number","Country","Role"
                    });
                        //if all columns validated
                        if (errors.Count == 0)
                        {
                            for (int i = 0; i < dtRowsCount; i++)
                            {
                                try
                                {
                                    string userName = dt.Rows[i].Field<string>("Username");
                                    string email = dt.Rows[i].Field<string>("Email Address");
                                    string pass = dt.Rows[i].Field<string>("Password");
                                    string confirm_pass = dt.Rows[i].Field<string>("Confirm Password");
                                    string fullName = dt.Rows[i].Field<string>("Full Name");
                                    string phone = dt.Rows[i].Field<string>("Phone Number");
                                    string country = dt.Rows[i].Field<string>("Country");
                                    string firstName = dt.Rows[i].Field<string>("First Name");
                                    string lastName = dt.Rows[i].Field<string>("Last Name");
                                    string role = dt.Rows[i].Field<string>("Role");

                                    upModel.FullName = fullName;
                                    upModel.PhoneNumber = phone;
                                    upModel.Password = pass;
                                    upModel.ConfirmPassword = confirm_pass;
                                    upModel.EmailAddress = email;
                                    upModel.Username = userName;
                                    upModel.CountryName = country;
                                    upModel.FirstName = firstName;
                                    upModel.LastName = lastName;
                                    upModel.UserRoleName = role;

                                    errors = ValidateImportUserFromExcel(upModel);

                                    if (errors.Count() > 0)
                                    {
                                        ImportFromExcelError importFromExcelError = new ImportFromExcelError();
                                        importFromExcelError.Row = $"At Row {i + 2}";
                                        importFromExcelError.Errors = errors;
                                        errorsList.Add(importFromExcelError);
                                        continue;
                                    }

                                    //after finish assign value to upModel, create user here
                                    var user = new AspNetUsers { UserName = upModel.Username, Email = upModel.EmailAddress };
                                    var creationResult = await _userManager.CreateAsync(user, upModel.Password); //create user and save in db
                                    if (creationResult.Succeeded)
                                    {
                                        //when create user success, add user detail in UserProfile table
                                        UserProfile userProfile = new UserProfile();
                                        userProfile.Id = Guid.NewGuid().ToString();

                                        //write other things like Full name, First name, Last name etc...
                                        userProfile.AspNetUserId = user.Id;
                                        userProfile.FullName = upModel.FullName;
                                        userProfile.FirstName = upModel.FirstName;
                                        userProfile.LastName = upModel.LastName;
                                        userProfile.PhoneNumber = upModel.PhoneNumber;
                                        userProfile.CountryName = upModel.CountryName;
                                        userProfile.UserStatusId = util.GetGlobalOptionSetId(ProjectEnum.UserStatus.Registered.ToString(), "UserStatus");
                                        userProfile.CreatedBy = _userManager.GetUserId(User);
                                        userProfile.CreatedOn = util.GetSystemTimeZoneDateTimeNow();
                                        userProfile.IsoUtcCreatedOn = util.GetIsoUtcNow();
                                        db.UserProfiles.Add(userProfile);
                                        db.SaveChanges();

                                        List<string> roles = new List<string>();
                                        roles.Add(role);
                                        var roleResult = await _userManager.AddToRolesAsync(user, roles);

                                        successCount++;
                                    }
                                    else
                                    {
                                        var error = creationResult.Errors.FirstOrDefault();
                                        if (error != null)
                                        {
                                            var errorMessage = error.Description;
                                            errors.Add(errorMessage);
                                            ImportFromExcelError importFromExcelError = new ImportFromExcelError();
                                            importFromExcelError.Row = $"At Row {i + 2}";
                                            importFromExcelError.Errors = errors;
                                            errorsList.Add(importFromExcelError);
                                        }
                                    }
                                    ModelState.Clear();
                                }
                                catch (Exception ex)
                                {
                                    errors.Add(ex.InnerException.Message);
                                    ImportFromExcelError importFromExcelError = new ImportFromExcelError();
                                    importFromExcelError.Row = $"At Row {i + 2}";
                                    importFromExcelError.Errors = errors;
                                    errorsList.Add(importFromExcelError);
                                }
                            }
                        }
                        else
                        {
                            ImportFromExcelError importFromExcelError = new ImportFromExcelError();
                            importFromExcelError.Row = Resource.InvalidUserTemplate;
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

        public IActionResult Edit(string Id)
        {
            UserProfileViewModel model = new UserProfileViewModel();
            if (Id != null)
            {
                model = GetUserProfileViewModel(Id, "Edit");
            }
            SetupSelectLists(model);
            return View(model);
        }

        public IActionResult ViewRecord(string Id)
        {
            UserProfileViewModel model = new UserProfileViewModel();
            if (Id != null)
            {
                model = GetUserProfileViewModel(Id, "View");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserProfileViewModel model)
        {
            try
            {
                ValidateModel(model);

                if (!ModelState.IsValid)
                {
                    SetupSelectLists(model);
                    return View(model);
                }

                bool result = await SaveRecord(model);
                if (result == false)
                {
                    TempData["NotifyFailed"] = Resource.FailedExceptionError;
                }
                else
                {
                    ModelState.Clear();
                    TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }

            return RedirectToAction("index", "user");
        }

        public void ValidateModel(UserProfileViewModel model)
        {
            if (model != null)
            {
                bool usernameExist = util.UsernameExists(model.Username, model.AspNetUserId);
                bool emailExist = util.EmailExists(model.EmailAddress, model.AspNetUserId);
                if (usernameExist)
                {
                    ModelState.AddModelError("UserName", Resource.UsernameTaken);
                }
                if (emailExist)
                {
                    ModelState.AddModelError("EmailAddress", Resource.EmailAddressTaken);
                }
                if (model.Id == null)
                {
                    if (model.Password == null)
                    {
                        ModelState.AddModelError("Password", Resource.PasswordRequired);
                    }
                    if (model.ConfirmPassword == null)
                    {
                        ModelState.AddModelError("ConfirmPassword", Resource.ConfirmPasswordRequired);
                    }
                }
                else
                {
                    ModelState.Remove("Password");
                    ModelState.Remove("ConfirmPassword");
                }
                if (string.IsNullOrEmpty(model.UserStatusId))
                {
                    ModelState.AddModelError("UserStatusId", Resource.StatusRequired);
                }
                if (string.IsNullOrEmpty(model.UserRoleName))
                {
                    ModelState.AddModelError("UserRoleName", Resource.RoleRequired);
                }
                else
                {
                    if (model.UserRoleName == "Student" && model.ClassIdList == null)
                    {
                        ModelState.AddModelError("ClassIdList", Resource.FieldIsRequired);
                    }
                }
                if (string.IsNullOrEmpty(model.CountryName))
                {
                    ModelState.AddModelError("CountryName", Resource.CountryRequired);
                }
                else
                {
                    List<SelectListItem> countries = util.GetCountryList("");
                    if (countries.Where(a => a.Text == model.CountryName).Any() == false)
                    {
                        ModelState.AddModelError("CountryName", Resource.CountryNotFound);
                    }
                }
                if (model.ProfilePicture != null)
                {

                    bool validatedImage = DataValidator.ValidateImageFile(model.ProfilePicture.FileName);
                    if (!validatedImage)
                    {
                        ModelState.AddModelError("ProfilePicture", Resource.FailedOnlyJpgJpegPngCanBeSetAsProfilePicture);
                    }
                }
            }
        }

        public void AssignUserProfileValues(UserProfile userProfile, UserProfileViewModel model)
        {
            userProfile.FullName = model.FullName;
            userProfile.FirstName = model.FirstName;
            userProfile.LastName = model.LastName;
            userProfile.DateOfBirth = model.DateOfBirth;
            if (model.DateOfBirth != null)
            {
                userProfile.IsoUtcDateOfBirth = model.DateOfBirth.Value.ToString("o", CultureInfo.InvariantCulture);
            }
            userProfile.PhoneNumber = model.PhoneNumber;
            userProfile.IDCardNumber = model.IDCardNumber;
            userProfile.GenderId = model.GenderId;
            userProfile.CountryName = model.CountryName;
            userProfile.PostalCode = model.PostalCode;
            userProfile.Address = model.Address;
            userProfile.UserStatusId = model.UserStatusId;
            userProfile.IntakeYear = model.IntakeYear;
            userProfile.Code = model.Code;
            if (model.Id == null)
            {
                userProfile.CreatedBy = model.CreatedBy;
                userProfile.CreatedOn = util.GetSystemTimeZoneDateTimeNow();
                userProfile.IsoUtcCreatedOn = util.GetIsoUtcNow();
            }
            else
            {
                userProfile.ModifiedBy = model.ModifiedBy;
                userProfile.ModifiedOn = util.GetSystemTimeZoneDateTimeNow();
                userProfile.IsoUtcModifiedOn = util.GetIsoUtcNow();
            }
        }

        public async Task<bool> SaveRecord(UserProfileViewModel model)
        {
            bool result = true;
            if (model != null)
            {
                string userId = "";
                string userProfileId = "";
                string profilePictureId = "";
                string type = "";
                try
                {
                    model.CreatedBy = _userManager.GetUserId(User);
                    model.ModifiedBy = _userManager.GetUserId(User);
                    //edit
                    if (model.Id != null)
                    {
                        type = "update";
                        //if change username & email, need to change in AspNetUsers table
                        AspNetUsers aspNetUsers = db.AspNetUsers.FirstOrDefault(a => a.Id == model.AspNetUserId);
                        aspNetUsers.UserName = model.Username;
                        aspNetUsers.Email = model.EmailAddress;
                        db.Entry(aspNetUsers).State = EntityState.Modified;

                        //save user profile
                        UserProfile userProfile = db.UserProfiles.FirstOrDefault(a => a.Id == model.Id);
                        AssignUserProfileValues(userProfile, model);
                        db.Entry(userProfile).State = EntityState.Modified;

                        //save AspNetUsers and UserProfile
                        db.SaveChanges();

                        userProfileId = userProfile.Id;

                    }
                    //register new user record
                    else
                    {
                        type = "create";
                        var user = new AspNetUsers { UserName = model.Username, Email = model.EmailAddress };
                        //var creationResult = account.CreateNewUserIdentity(user, model.Password);
                        var creationResult = await _userManager.CreateAsync(user, model.Password); //create user and save in db
                        if (creationResult.Succeeded)
                        {
                            userId = user.Id;
                            //save user profile
                            UserProfile userProfile = new UserProfile();
                            AssignUserProfileValues(userProfile, model);
                            userProfile.Id = Guid.NewGuid().ToString();
                            userProfile.AspNetUserId = user.Id;
                            userProfile.UserStatusId = util.GetGlobalOptionSetId(ProjectEnum.UserStatus.Registered.ToString(), "UserStatus");
                            db.UserProfiles.Add(userProfile);
                            db.SaveChanges();
                            userProfileId = userProfile.Id;
                            model.AspNetUserId = user.Id;

                            // Send an email with this link
                            if (util.GetAppSettingsValue("confirmEmailToLogin") == "true")
                            {
                                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);
                                EmailTemplate emailTemplate = util.EmailTemplateForConfirmEmail(user.UserName, callbackUrl);
                                util.SendEmail(user.Email, emailTemplate.Subject, emailTemplate.Body);
                            }
                        }
                    }

                    //roles
                    if (!string.IsNullOrEmpty(model.UserRoleName))
                    {
                        string roleName = db.AspNetRoles.Where(a => a.Name == model.UserRoleName).Select(a => a.Name).FirstOrDefault(); //just to make sure the role name is valid
                        var user = await _userManager.FindByIdAsync(model.AspNetUserId);
                        var existingRoles = await _userManager.GetRolesAsync(user);
                        string[] removeRoles = existingRoles.ToArray();
                        if (DataValidator.IsEmptyArray(removeRoles) == false)
                        {
                            var removeRole = await _userManager.RemoveFromRolesAsync(user, removeRoles);
                        }
                        var assignRole = await _userManager.AddToRolesAsync(user, new[] { roleName });
                    }

                    if (model.ClassIdList != null)
                    {
                        List<StudentClass> studentClasses = db.StudentClasses.Where(a => a.StudentId == model.AspNetUserId).ToList();
                        if (studentClasses != null)
                        {
                            db.StudentClasses.RemoveRange(studentClasses);
                            db.SaveChanges();
                        }
                        List<StudentClass> updatedStudentClasses = new List<StudentClass>();
                        foreach (string classId in model.ClassIdList)
                        {
                            StudentClass studentClass = new StudentClass();
                            studentClass.StudentId = model.AspNetUserId;
                            studentClass.ClassId = classId;
                            updatedStudentClasses.Add(studentClass);
                        }
                        db.StudentClasses.AddRange(updatedStudentClasses);
                        db.SaveChanges();
                    }

                    if (model.ProfilePicture != null)
                    {
                        string profilePicture = util.GetGlobalOptionSetId(ProjectEnum.UserAttachment.ProfilePicture.ToString(), "UserAttachment");
                        util.SaveUserAttachment(model.ProfilePicture, userProfileId, profilePicture, _userManager.GetUserId(User));
                    }
                }
                catch (Exception ex)
                {
                    //Exception when creating new record, means record creation incomplete due to error, undo the record creation
                    if (type == "create")
                    {
                        if (!string.IsNullOrEmpty(userId))
                        {
                            AspNetUsers aspNetUsers = db.AspNetUsers.FirstOrDefault(a => a.Id == userId);
                            if (aspNetUsers != null)
                            {
                                db.AspNetUsers.Remove(aspNetUsers);
                                db.SaveChanges();
                            }
                        }
                        if (!string.IsNullOrEmpty(userProfileId))
                        {
                            UserProfile userProfile = db.UserProfiles.FirstOrDefault(a => a.Id == userProfileId);
                            if (userProfile != null)
                            {
                                db.UserProfiles.Remove(userProfile);
                                db.SaveChanges();
                            }
                        }
                        if (!string.IsNullOrEmpty(profilePictureId))
                        {
                            UserAttachment userAttachment = db.UserAttachments.FirstOrDefault(a => a.Id == profilePictureId);
                            if (userAttachment != null)
                            {
                                db.UserAttachments.Remove(userAttachment);
                                db.SaveChanges();
                            }
                        }
                    }
                    return false;
                }
            }
            return result;
        }

        public IActionResult AdminChangePassword(string Id)
        {
            AdminChangePasswordViewModel model = new AdminChangePasswordViewModel();
            if (Id != null)
            {
                model = GetAdminChangePasswordViewModel(Id);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AdminChangePassword(AdminChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                //Not allowed to change demo account's password
                string username = db.AspNetUsers.Where(a => a.Id == model.AspNetUserId).Select(a => a.UserName).FirstOrDefault();
                if (util.GetAppSettingsValue("environment") == "prod" && username == "nsadmin")
                {
                    TempData["NotifyFailed"] = Resource.NotAllowToChangePasswordForDemoAccount;
                    return RedirectToAction("index", "dashboard");
                }

                var userExists = await _userManager.FindByIdAsync(model.AspNetUserId);
                if (userExists != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(userExists);
                    var result = await _userManager.ResetPasswordAsync(userExists, token, model.NewPassword);
                    if (result.Succeeded)
                    {
                        ModelState.Clear();
                        TempData["NotifySuccess"] = Resource.PasswordChangedSuccessfully;

                        //send email to notify the user
                        string userEmail = db.AspNetUsers.Where(a => a.Id == model.AspNetUserId).Select(a => a.Email).FirstOrDefault();
                        string resetById = _userManager.GetUserId(User);
                        string resetByName = db.UserProfiles.Where(a => a.AspNetUserId == resetById).Select(a => a.FullName).FirstOrDefault();
                        EmailTemplate emailTemplate = util.EmailTemplateForPasswordResetByAdmin(model.Username, resetByName, model.NewPassword);
                        util.SendEmail(userEmail, emailTemplate.Subject, emailTemplate.Body);
                    }
                    else
                    {
                        TempData["NotifyFailed"] = Resource.FailedToResetPassword;
                    }
                    return RedirectToAction("index", "user");
                }
                else
                {
                    TempData["NotifyFailed"] = Resource.UserNotExist;
                    return RedirectToAction("index", "user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("index", "user");
        }

        public AdminChangePasswordViewModel GetAdminChangePasswordViewModel(string Id)
        {
            AdminChangePasswordViewModel model = new AdminChangePasswordViewModel();
            model = (from t1 in db.UserProfiles
                     join t2 in db.AspNetUsers on t1.AspNetUserId equals t2.Id
                     where t1.Id == Id
                     select new AdminChangePasswordViewModel
                     {
                         Id = Id,
                         FullName = t1.FullName,
                         Username = t2.UserName,
                         EmailAddress = t2.Email,
                         AspNetUserId = t1.AspNetUserId
                     }).FirstOrDefault();
            return model;
        }

        public IActionResult Delete(string Id)
        {
            try
            {
                if (Id != null)
                {
                    AspNetUsers users = db.AspNetUsers.Where(a => a.Id == Id).FirstOrDefault();
                    if (users != null)
                    {
                        if (util.GetAppSettingsValue("environment") == "prod" && users.UserName == "nsadmin")
                        {
                            TempData["NotifyFailed"] = Resource.DemoAccountCannotBeDeleted;
                            return RedirectToAction("index");
                        }
                        db.AspNetUsers.Remove(users);
                        db.SaveChanges();
                    }
                }
                TempData["NotifySuccess"] = Resource.RecordDeletedSuccessfully;
            }
            catch (Exception ex)
            {
                AspNetUsers users = db.AspNetUsers.Where(a => a.Id == Id).FirstOrDefault();
                if (users == null)
                {
                    TempData["NotifySuccess"] = Resource.RecordDeletedSuccessfully;
                }
                else
                {
                    TempData["NotifyFailed"] = Resource.FailedExceptionError;
                }
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("index");
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