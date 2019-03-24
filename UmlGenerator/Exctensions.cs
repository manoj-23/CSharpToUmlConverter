using System.Collections.Generic;

namespace UmlGenerator
{
    public static class Exctensions
    {
        public static string TrimWhitespace(this string text)
        {
            return text?.Replace(" ", string.Empty);
        }

        public static string TrimColon(this string text)
        {
            return text?.Replace(":", string.Empty);
        }

        public static string TrimWhere(this string text)
        {
            return text?.Replace("where", string.Empty);
        }

        public static string TrimAngleBrackets(this string text)
        {
            return text?.Replace("<", string.Empty).Replace(">", string.Empty);
        }




        public static string MyToString(this List<ClassInfo> infos)
        {
            var res = "";
            foreach (var info in infos)
            {
                var res1 = "";
                if (info.ClassGenerics != null)
                {
                    res1 = info.ClassGenerics;
                }

                res += $"{info.ClassName} {res1}, ";
            }

            return res;
        }

    }
}