using System.Collections.Generic;
using System.Linq;

namespace UmlGenerator
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
                var parents = new List<ClassInfo>();
                void parse()
                {
                    var info = new ClassInfo();
                    if (a.Contains('<') && a.Contains('>')) //has generics
                    {
                        if (a.Contains(',')) //has colons
                        {
                            if (a.IndexOf('<') < a.IndexOf(',')) // first parent has generics
                            {
                                info.ClassName = a.Substring(0, a.IndexOf('<'));
                                info.ClassGenerics = a.Substring(a.IndexOf('<'), a.IndexOf('>') + 1 - a.IndexOf('<'));
                                a = a.Remove(0, a.IndexOf('>') + 1);
                                a = a.Remove(0, a.IndexOf(',') + 1);
                            }
                            else // first parent haven`t generics
                            {
                                info.ClassName = a.Substring(0, a.IndexOf(','));
                                info.ClassGenerics = null;
                                a = a.Remove(0, a.IndexOf(',') +1 );
                            }
                        }
                        else // haven`t colons
                        {
                            info.ClassName = a.Substring(0, a.IndexOf('<'));
                            info.ClassGenerics = a.Substring(a.IndexOf('<'), a.IndexOf('>') - a.IndexOf('<'));
                            a = a.Remove(0, a.IndexOf('>') + 1);
                        }
                    } // haven`t generics
                    else
                    {
                        if (a.Contains(','))
                        {
                            info.ClassName = a.Substring(0, a.IndexOf(','));
                            info.ClassGenerics = null;
                            a = a.Remove(0, a.IndexOf(',') + 1);
                        }
                        else
                        {
                            info.ClassName = a;
                            a = string.Empty;
                            info.ClassGenerics = null;
                        }
                    }
                    parents.Add(info);
                    if (a.Length > 0)
                        parse();
                }
                parse();
                Parents = parents;
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