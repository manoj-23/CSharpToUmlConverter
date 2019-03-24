using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    public class Generator
    {
        /// <summary>
        /// key is inheritor, value is list of parents
        /// </summary>
        private Dictionary<string, List<string>> inheritanceDictionary = new Dictionary<string, List<string>>();

        public void Start(string root, string outputPath)
        {
            var files = new List<string>();

            void AddFiles(string directory)
            {
                files.AddRange(Directory.GetFiles(directory));
                foreach (var subDir in Directory.GetDirectories(directory))
                {
                    AddFiles(subDir);
                }
            }
            AddFiles(root);

            Console.WriteLine($"found {files.Count} files");

            int index = 0;
            
            foreach (var file in files)
            {
                var stream = File.OpenText(file);
                var csFile = stream.ReadToEnd();
                var regularExpression = new Regex(Node.Template);
                var matches = regularExpression.Matches(csFile);

                var nodes = new List<Node>();
                for (var i = 0; i < matches.Count; i++)
                {
                    nodes.Add(Parse(matches[i].Value));
                }

                foreach (var node in nodes)
                {
                    Console.WriteLine($"{index}{new string('-', 200)}");
                    Console.WriteLine("Class:");
                    Console.WriteLine(node.Class.ClassName);
                    Console.WriteLine("Generics:");
                    Console.WriteLine(node.Class.ClassGenerics ?? "null");
                    Console.WriteLine("Parents:");
                    foreach (var nodeParent in node.Parents)
                    {
                        var generics = nodeParent.ClassGenerics != null
                            ? $"has generics {nodeParent.ClassGenerics}"
                            : "";
                        Console.WriteLine($"{nodeParent.ClassName} {generics}");
                    }
                    Console.WriteLine("Constraints:");
                    Console.WriteLine(node.Constraints ?? "null");
                    Console.WriteLine();

                    inheritanceDictionary[node.Class.ClassName] = node.Parents.Select(_ => _.ClassName).ToList();

                }

                index++;
            }

            var uml = "";

            foreach (var inheritor in inheritanceDictionary.Keys)
            {
                foreach (var parent in inheritanceDictionary[inheritor])
                {
                    uml += $"{inheritor}--|>{parent}\r\n";
                }
            }

            Console.WriteLine("Generating...");
            File.AppendAllLines(outputPath, new List<string> { uml });
            Console.WriteLine("Successful generated");
            Console.Clear();
        }

        private Node Parse(string a)
        {
            a = a.Replace("class", "").Trim('{').Replace("\r\n", "");
            var node = new Node();

            // class Abc<T, T1, T2> : Base1<T2, T3>, Base2<T2, T1> where T1 : Base where T2 : new()
            if (a.Contains("where")) //has constraint
            {
                if (a.IndexOf(':') < a.IndexOf("where", StringComparison.Ordinal)) //class has inheritance 
                {
                    if (a.Substring(0, a.IndexOf(':')).Contains('<') && a.Substring(0, a.IndexOf(':')).Contains('>')) // class has generics
                    {
                        node.Class.ClassName = a.Substring(0, a.IndexOf('<'));
                        a = a.Remove(0, a.IndexOf('<'));
                        node.Class.ClassGenerics = a.Substring(0, a.IndexOf('>') + 1);
                        a = a.Remove(0, a.IndexOf('>'));
                        a = a.Remove(0, a.IndexOf(':'));
                    }
                    else // class haven`t generics
                    {
                        node.Class.ClassName = a.Substring(0, a.IndexOf(':'));
                        a = a.Remove(0, a.IndexOf(':'));
                        node.Class.ClassGenerics = null;
                    }

                    node.ParentsFromString = a.Substring(0, a.IndexOf("where", StringComparison.Ordinal));
                    a = a.Remove(0, a.IndexOf("where", StringComparison.Ordinal));
                    node.Constraints = a;
                }
                else //class haven`t inheritance
                {
                    if (a.Substring(0, a.IndexOf("where", StringComparison.Ordinal)).Contains('<') && a.Substring(0, a.IndexOf("where", StringComparison.Ordinal)).Contains('>')
                    ) // class has generics
                    {
                        node.Class.ClassName = a.Substring(0, a.IndexOf('<'));
                        a = a.Remove(0, a.IndexOf('<'));
                        node.Class.ClassGenerics = a.Substring(0, a.IndexOf('>') + 1);
                        a = a.Remove(0, a.IndexOf('>'));
                        a = a.Remove(0, a.IndexOf("where", StringComparison.Ordinal));
                    }
                    else // class haven`t generics
                    {
                        node.Class.ClassName = a.Substring(0, a.IndexOf("where", StringComparison.Ordinal));
                        node.Class.ClassGenerics = null;
                        a = a.Remove(0, a.IndexOf("where", StringComparison.Ordinal));
                    }

                    node.Parents = null;
                    node.Constraints = a;
                }
            }
            else // haven`t constraint
            {
                if (a.Contains(':')) //class has inheritance 
                {
                    if (a.Substring(0, a.IndexOf(':')).Contains('<') && a.Substring(0, a.IndexOf(':')).Contains('>')) // class has generics
                    {
                        node.Class.ClassName = a.Substring(0, a.IndexOf('<'));
                        a = a.Remove(0, a.IndexOf('<'));
                        node.Class.ClassGenerics = a.Substring(0, a.IndexOf('>') + 1);
                        a = a.Remove(0, a.IndexOf('>'));
                        a = a.Remove(0, a.IndexOf(':'));
                    }
                    else // class haven`t generics
                    {
                        node.Class.ClassName = a.Substring(0, a.IndexOf(':'));
                        node.Class.ClassGenerics = null;
                        a = a.Remove(0, a.IndexOf(':'));
                    }

                    node.ParentsFromString = a;
                    node.Constraints = null;
                }
                else // class haven`t inheritance
                {
                    if (a.Contains('<') && a.Contains('>')) // class has generics
                    {
                        node.Class.ClassName = a.Substring(0, a.IndexOf('<'));
                        a = a.Remove(0, a.IndexOf('<'));
                        node.Class.ClassGenerics = a.Substring(0, a.IndexOf('>') + 1);
                    }
                    else // class haven`t generics
                    {
                        node.Class.ClassName = a;
                        node.Class.ClassGenerics = null;
                        node.Parents = null;
                        node.Constraints = null;
                    }
                }
            }

            return node;
        }
        public Node Parse1(string a)
        {
            a = a.Replace("class", "").Trim('{').Replace("\r\n", "");
            var node = new Node();



            // has constraint, class has inheritance, class has generics
            var assertion = "(?=^.*<.*> *:.*where).*";
            //  Abc<T>  :  Base1<T2, T3>, IBase2<T2, T1>, ICouple where T1 : IBase3 where T2 : new(), class
            var className = @"^.*?(?=<)";
            var classGenerics = "<.*?>(?= *?:)";
            var parrents = "(?<=> *:).*? (?=where)";
            var constraints = "(?<=^.*)(?=where).*";

            // has constraint, class has inheritance, class haven`t generics
            assertion = "(?=^.*:.*where).*";
            //  Abc  :  Base1<T2, T3>, IBase2<T2, T1>, ICouple where T1 : IBase3 where T2 : new(), class where T3 : IBase
            className = "^.*?(?=:)";
            classGenerics = "";
            parrents = "(?<=^:).*?(?=where)";
            constraints = "(?=^ *where).*";

            // has constraint, class haven`t inheritance, class has generics
            assertion = "";

            // has constraint, class haven`t inheritance, class haven`t generics
            // haven`t constraint, class has inheritance, class has generics
            // haven`t constraint, class has inheritance, class haven`t generics
            // haven`t constraint, class haven`t inheritance, class has generics
            // haven`t constraint, class haven`t inheritance, class haven`t generics

            return node;
        }
    }
}

