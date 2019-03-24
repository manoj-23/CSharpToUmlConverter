using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    public class Node
    {
        public static readonly string Template = "class(.|\n)*?{";


        public ClassInfo Class { get; set; }

        public List<ClassInfo> Parents { get; set; }

        public string ParentsFromString
        {
            set
            {
                var a = value.TrimColon()?.Trim(' ');

                Parents = a.Split(',').Select(_ =>
                {
                    if (_.Contains("<") && _.Contains(">"))
                    {

                        return new ClassInfo { ClassName = _.Substring(0, _.IndexOf('<')), ClassGenerics = _.Substring(_.IndexOf('<'), _.IndexOf('>') + 1 - _.IndexOf('<')) };
                    }
                    else
                    {
                        return new ClassInfo { ClassName = _ };
                    }
                }).ToList();
            }
        }


        private string constraints;
        public string Constraints
        {
            get => constraints;
            set => constraints = value?.Trim(' ');
        }

        public Node()
        {
            Class = new ClassInfo();
            Parents = new List<ClassInfo>();
        }
    }
}