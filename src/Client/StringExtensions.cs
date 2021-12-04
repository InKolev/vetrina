using System.Text.RegularExpressions;

namespace Vetrina.Client
{
    public static class StringExtensions
    {
        public static double ToDouble(this string text)
        {
            return double.Parse(text.Trim().Replace(',', '.'));
        }

        public static string ReplaceNewLinesWithSingleWhitespace(this string text)
        {
            return Regex.Replace(text, @"\t|\n|\r", " ");
        }
    }
}