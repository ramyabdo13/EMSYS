using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using EMSYS.Resources;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using EMSYS.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace EMSYS.Models
{
    public static class Extensions
    {
        public static TSource _DefaultIfEmpty<TSource>(this TSource source, TSource defaultValue)
        {
            if (typeof(TSource) == typeof(string) && source == null)
            {
                return defaultValue;
            }
            else if (typeof(TSource) == typeof(int) && source == null)
            {
                return defaultValue;
            }
            else
                return source;
        }
    }

    public static class ModelBuilderExtensions
    {
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.DisplayName());
            }
        }
    }

    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static string ReplaceWhitespace(string input, string replacement)
        {
            Regex sWhitespace = new Regex(@"\s+");
            return sWhitespace.Replace(input, replacement);
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }

    public class UserProfilePictureActionFilter : ActionFilterAttribute
    {
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var acc = (Util)context.HttpContext.RequestServices.
            GetService(typeof(Util));

            ClaimsPrincipal currentUser = context.HttpContext.User;
            if (currentUser != null)
            {
                var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string userid = currentUserID;
                if (!string.IsNullOrEmpty(userid))
                {
                    var controller = context.Controller as Controller;
                    if (controller != null)
                    {
                        controller.ViewBag.Avatar = acc.GetMyProfilePictureName(userid);
                    }
                }
            }
            await next();
        }
    }

    public class PasswordValidation : RequiredAttribute
    {
        public PasswordValidation()
        {
            this.ErrorMessage = Resource.InvalidPassword;
        }

        public override bool IsValid(object value)
        {
            string passwordValue = value as string;
            if (!string.IsNullOrEmpty(passwordValue))
            {
                bool hasNonLetterOrDigit, hasDigit, hasUppercase, hasLowercase = false;
                hasNonLetterOrDigit = DataValidator.HasNonLetterOrDigit(passwordValue);
                hasDigit = DataValidator.HasDigit(passwordValue);
                hasUppercase = DataValidator.HasUppercase(passwordValue);
                hasLowercase = DataValidator.HasLowercase(passwordValue);

                if (passwordValue.Length < 6 || hasNonLetterOrDigit == false || hasDigit == false || hasUppercase == false || hasLowercase == false)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
    }

    public class Max5MBAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            var file = value as IFormFile;
            //file is null means it's an optional field, no file to validate, return true
            if (file == null)
            {
                return true;
            }
            //file more than 5 mb
            if (file.Length > 5242880)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public class Max50MBAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            var file = value as IFormFile;
            //file is null means it's an optional field, no file to validate, return true
            if (file == null)
            {
                return true;
            }
            //file more than 50 mb
            if (file.Length > 52428800)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}


