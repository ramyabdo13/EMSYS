using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMSYS.Models;
using EMSYS.Resources;
using EMSYS.Utils;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using EMSYS.Data;


namespace EMSYS.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<AspNetUsers> _userManager;
        private readonly SignInManager<AspNetUsers> _signInManager;
        private Util util;
        private EMSYSdbContext db;
        private ErrorLoggingService _logger;

        public AccountController(EMSYSdbContext _db, UserManager<AspNetUsers> userManager,
                              SignInManager<AspNetUsers> signInManager, Util _util, ErrorLoggingService logger)
        {
            db = _db;
            _userManager = userManager;
            _signInManager = signInManager;
            util = _util;
            _logger = logger;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                string userid = "";

                // if username input contains @ sign, means that user use email to login
                if (model.UserName.Contains("@"))
                {
                    // select the UserName of the user from AspNetUsers table and assign to model.UserName because instead of email, SignInManager use username to sign in 
                    string userName = db.AspNetUsers.Where(a => a.Email == model.UserName).Select(a => a.UserName).FirstOrDefault();
                    model.UserName = userName ?? "";
                    userid = db.AspNetUsers.Where(a => a.Email == model.UserName).Select(a => a.Id).FirstOrDefault();
                }
                else
                {
                    userid = db.AspNetUsers.Where(a => a.UserName == model.UserName).Select(a => a.Id).FirstOrDefault();
                }

                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    SaveLoginHistory(userid);
                    return RedirectToAction("index", "dashboard");
                }
                if (result.IsLockedOut)
                {
                    return View("Lockout");
                }
                ModelState.AddModelError("", Resource.InvalidLoginAttempt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return View(model);
        }

        public void SaveLoginHistory(string userId)
        {
            LoginHistory loginHistory = new LoginHistory();
            loginHistory.Id = Guid.NewGuid().ToString();
            loginHistory.AspNetUserId = userId;
            loginHistory.LoginDateTime = util.GetSystemTimeZoneDateTimeNow();
            loginHistory.IsoUtcLoginDateTime = util.GetIsoUtcNow();
            db.LoginHistories.Add(loginHistory);
            db.SaveChanges();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                if (util.GetAppSettingsValue("environment") == "prod" && _userManager.GetUserName(User) == "nsadmin")
                {
                    TempData["NotifyFailed"] = Resource.NotAllowToChangePasswordForDemoAccount;
                    return RedirectToAction("index", "dashboard");
                }
                var user = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    if (user != null)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                    }
                    TempData["NotifySuccess"] = Resource.PasswordChangedSuccessfully;
                    return RedirectToAction("index", "dashboard");
                }
                AddErrors(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }

            return View(model);
        }

        public IActionResult MyProfile()
        {
            UserProfileViewModel model = new UserProfileViewModel();
            string currentUserId = _userManager.GetUserId(User);
            model = util.GetCurrentUserProfile(currentUserId);
            return View(model);
        }

        public IActionResult EditMyProfile()
        {
            UserProfileViewModel model = new UserProfileViewModel();
            string currentUserId = _userManager.GetUserId(User);
            model = util.GetCurrentUserProfile(currentUserId);
            SetupSelectLists(model);
            return View(model);
        }

        public void SetupSelectLists(UserProfileViewModel model)
        {
            model.GenderSelectList = util.GetGlobalOptionSets("Gender", model.GenderId);
            model.GovernateSelectList = util.GetGovernateList(model.GovernateName);
        }

        [HttpPost]
        public IActionResult EditMyProfile(UserProfileViewModel model, IFormFile ProfilePicture)
        {
            try
            {
                ValidateEditMyProfile(model);

                //These 2 fields can only be edited by system admin in user management section. When normal user edit profile from here, these 2 fields are not required.
                if (User.IsInRole("Instructor") || User.IsInRole("Student"))
                {
                    ModelState.Remove("UserStatusId");
                    ModelState.Remove("UserRoleIdList");
                    ModelState.Remove("UserRoleName");
                    ModelState.Remove("Password");
                }

                if (!ModelState.IsValid)
                {
                    SetupSelectLists(model);
                    return View(model);
                }

                bool result = SaveMyProfile(model);
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
            return RedirectToAction("myprofile");
        }

        public void ValidateEditMyProfile(UserProfileViewModel model)
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
            }
        }

        public void AssignUserProfileValues(UserProfile userProfile, UserProfileViewModel model)
        {
            userProfile.FullName = model.FullName;
            userProfile.FirstName = model.FirstName;
            userProfile.LastName = model.LastName;
            userProfile.DateOfBirth = model.DateOfBirth;
            userProfile.PhoneNumber = model.PhoneNumber;
            userProfile.IDCardNumber = model.IDCardNumber;
            userProfile.GenderId = model.GenderId;
            userProfile.GovernateName = model.GovernateName;
            userProfile.PostalCode = model.PostalCode;
            userProfile.Address = model.Address;
            userProfile.IntakeYear = model.IntakeYear;
            userProfile.Code = model.Code;
            userProfile.ModifiedBy = model.ModifiedBy;
            userProfile.ModifiedOn = util.GetSystemTimeZoneDateTimeNow();
            userProfile.IsoUtcModifiedOn = util.GetIsoUtcNow();
        }

        public bool SaveMyProfile(UserProfileViewModel model)
        {
            bool result = true;
            if (model != null)
            {
                try
                {
                    model.ModifiedBy = _userManager.GetUserId(User);
                    //edit
                    if (model.Id != null)
                    {
                        //save user profile
                        UserProfile userProfile = db.UserProfiles.FirstOrDefault(a => a.Id == model.Id);
                        AssignUserProfileValues(userProfile, model);
                        db.Entry(userProfile).State = EntityState.Modified;
                        //save AspNetUsers and UserProfile
                        db.SaveChanges();
                        if (model.ProfilePicture != null)
                        {
                            string profilePicture = util.GetGlobalOptionSetId(ProjectEnum.UserAttachment.ProfilePicture.ToString(), "UserAttachment");
                            util.SaveUserAttachment(model.ProfilePicture, userProfile.Id, profilePicture, _userManager.GetUserId(User));
                        }
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return result;
        }

        //
        // GET: /Account/Register
        //[AllowAnonymous]
        //public IActionResult Register()
        //{
        //    RegisterViewModel model = new RegisterViewModel();
        //    string id = db.AspNetUsers.Select(a => a.Id).FirstOrDefault();
        //    model.NoUserYet = string.IsNullOrEmpty(id) ? true : false;
        //    model.RoleNameSelectList = db.AspNetRoles.Select(a => new SelectListItem { Text = a.Name, Value = a.Name }).ToList();
        //    return View(model);
        //}

        //
        // POST: /Account/Register
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Register(RegisterViewModel model)
        //{
        //    try
        //    {
        //        bool usernameExist = util.UsernameExists(model.UserName, null);
        //        bool emailExist = util.EmailExists(model.Email, null);
        //        if (usernameExist)
        //        {
        //            ModelState.AddModelError("UserName", Resource.UsernameTaken);
        //        }
        //        if (emailExist)
        //        {
        //            ModelState.AddModelError("Email", Resource.EmailAddressTaken);
        //        }
        //        bool haveUsersInSystem = db.AspNetUsers.Select(a => a.Id).Any();
        //        if (!haveUsersInSystem)
        //        {
        //            ModelState.Remove("RoleName");
        //        }
        //        if (ModelState.IsValid)
        //        {
        //            var user = new AspNetUsers { UserName = model.UserName, Email = model.Email };

        //            var result = await _userManager.CreateAsync(user, model.Password); //create user and save in db
        //            if (result.Succeeded)
        //            {
        //                //create user profile
        //                RegisterUserProfile(user.Id);

        //                //if don't have any user yet in the system, this is the first registered user, assign system admin to this user
        //                //assumming the first user who access the system is the system admin
        //                if (haveUsersInSystem == false)
        //                {
        //                    var assignSystemAdminRole = await _userManager.AddToRoleAsync(user, "System Admin");
        //                }
        //                else
        //                {
        //                    if (!string.IsNullOrEmpty(model.RoleName))
        //                    {
        //                        var assignNormalUserRole = await _userManager.AddToRoleAsync(user, model.RoleName);
        //                    }
        //                }

        //                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
        //                // Send an email with this link
        //                if (util.GetAppSettingsValue("confirmEmailToLogin") == "true")
        //                {
        //                    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);
        //                    EmailTemplate emailTemplate = util.EmailTemplateForConfirmEmail(user.UserName, callbackUrl);
        //                    util.SendEmail(user.Email, emailTemplate.Subject, emailTemplate.Body);
        //                }

        //                ModelState.Clear();
        //                TempData["NotifySuccess"] = Resource.RegisterSuccessLoginNow;
        //                return RedirectToAction("login", "account");
        //            }

        //            string errorMessage = "";
        //            if (result.Errors != null)
        //            {
        //                foreach (var message in result.Errors)
        //                {
        //                    if (message.Description.Contains("Email"))
        //                    {
        //                        ModelState.AddModelError("Email", message.Description);
        //                    }
        //                    if (message.Description.Contains("UserName"))
        //                    {
        //                        ModelState.AddModelError("UserName", message.Description);
        //                    }
        //                    if (message.Description.Contains("Password"))
        //                    {
        //                        ModelState.AddModelError("Password", message.Description);
        //                    }
        //                    if (message.Description.Contains("ConfirmPassword"))
        //                    {
        //                        ModelState.AddModelError("ConfirmPassword", message.Description);
        //                    }
        //                    errorMessage += message + "\n";
        //                }
        //            }
        //            AddErrors(result);
        //        }
        //        model.NoUserYet = haveUsersInSystem ? false : true;
        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["NotifyFailed"] = Resource.FailedExceptionError;
        //        _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
        //        return RedirectToAction("register");
        //    }
        //}

        public void RegisterUserProfile(string userId)
        {
            UserProfile userProfile = new UserProfile();
            userProfile.Id = Guid.NewGuid().ToString();
            userProfile.AspNetUserId = userId;
            userProfile.UserStatusId = util.GetGlobalOptionSetId(ProjectEnum.UserStatus.Registered.ToString(), "UserStatus");
            userProfile.CreatedBy = userId;
            userProfile.CreatedOn = util.GetSystemTimeZoneDateTimeNow();
            userProfile.IsoUtcCreatedOn = util.GetIsoUtcNow();
            db.UserProfiles.Add(userProfile);
            db.SaveChanges();
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        // Don't reveal that the user does not exist or is not confirmed
                        ModelState.Clear();
                        TempData["NotifySuccess"] = Resource.ResetPasswordEmailSent;
                        return RedirectToAction("login", "account");
                    }

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);

                    EmailTemplate emailTemplate = util.EmailTemplateForForgotPassword(user.UserName, callbackUrl);
                    util.SendEmail(user.Email, emailTemplate.Subject, emailTemplate.Body);

                    ModelState.Clear();
                    TempData["NotifySuccess"] = Resource.ResetPasswordEmailSent;
                    return RedirectToAction("login", "account");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public IActionResult ResetPassword(string code)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel { Code = code };
            return string.IsNullOrEmpty(code) ? View("Error") : View(model);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    ModelState.Clear();
                    TempData["NotifySuccess"] = Resource.YourPasswordResetSuccessfully;
                    return RedirectToAction("login", "account");
                }
                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    ModelState.Clear();
                    TempData["NotifySuccess"] = Resource.YourPasswordResetSuccessfully;
                    return RedirectToAction("login", "account");
                }
                TempData["NotifyFailed"] = Resource.FailedToResetPassword;
                AddErrors(result);
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            try
            {
                await _signInManager.SignOutAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} Controller - {MethodBase.GetCurrentMethod().Name} Method");
            }
            return RedirectToAction("index", "home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public IActionResult ExternalLoginFailure()
        {
            return View();
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

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : UnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }
        }
        #endregion
    }
}