using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;
using System.Reflection;

namespace EMSYS.Utils
{
    public static class DataConverter
    {
        public static string GetEnumDisplayName(Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).First();
            var display = member.GetCustomAttribute<DisplayAttribute>();
            return display?.Name;
        }

        public static int[] GetNumberArrayInRandomOrder(int max)
        {
            int[] numbers = Enumerable.Range(1, max).ToArray();
            var random = new Random();
            var randomNumbers = numbers.OrderBy(x => random.Next()).ToArray();
            return randomNumbers;
        }

        public static string ConvertIntArrayToString(int[] array)
        {
            return string.Join(",", array);
        }
    }
}
