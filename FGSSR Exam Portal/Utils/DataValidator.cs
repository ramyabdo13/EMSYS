using System;
using System.Linq;

namespace EMSYS.Utils
{
    public static class DataValidator
    {
        public static bool HasNonLetterOrDigit(string value)
        {
            return value.Any(ch => !char.IsLetterOrDigit(ch));
        }
        public static bool HasDigit(string value)
        {
            return value.Any(ch => char.IsDigit(ch));
        }
        public static bool HasLowercase(string value)
        {
            return value.Any(ch => char.IsLower(ch));
        }
        public static bool HasUppercase(string value)
        {
            return value.Any(ch => char.IsUpper(ch));
        }
        public static bool ValidateImageFile(string fileName)
        {
            bool validated = false;
            if (fileName.Contains("jpg", StringComparison.OrdinalIgnoreCase) || fileName.Contains("jpeg", StringComparison.OrdinalIgnoreCase) || fileName.Contains("png", StringComparison.OrdinalIgnoreCase))
            {
                validated = true;
            }
            return validated;
        }

        public static bool IsEmptyArray(string[] arr)
        {
            bool allEmpty = false;
            if (arr == null)
            {
                allEmpty = true;
            }
            else
            {
                allEmpty = arr.All(str => str == "");
            }
            return allEmpty;
        }

        public static bool HaveDuplicatesInArray(string[] arr)
        {
            string[] withoutEmptyElement = arr.Where(str => str != "").ToArray();

            // Group the elements in the array by their values
            var groups = withoutEmptyElement.GroupBy(str => str);

            // Find groups that have more than one element
            var duplicates = groups.Where(group => group.Count() > 1);
            return duplicates.Any();
        }

    }
}
