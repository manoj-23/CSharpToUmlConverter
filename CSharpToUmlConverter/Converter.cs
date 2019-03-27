using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpToUmlConverter
{
    public class Converter
    {
        public void Start(string root, string outputPath)
        {
            var files = new List<string>();

            void AddFiles(string directory)
            {
                files.AddRange(Directory.GetFiles(directory).Where(_ => _.EndsWith(".cs")));
                foreach (var subDir in Directory.GetDirectories(directory))
                {
                    AddFiles(subDir);
                }
            }
            AddFiles(root);

            Console.WriteLine($"found {files.Count} files");

            var index = 0;
            var nodes = new List<Node>();

           // files = new List<string>() { @"C:\Work\Leapwork\Main\LeapTest.AutomationStudio.Common\Interop\user32.cs" };

            Console.WriteLine("Generating...");
            foreach (var file in files)
            {
                var stream = File.OpenText(file);
                var csFile = stream.ReadToEnd();
                if (csFile.Contains("//"))
                {
                    csFile = Regex.Replace(csFile, @"(?=//).*(\r\n)", String.Empty);
                }
                if (csFile.Contains("/*") && csFile.Contains("*/"))
                {
                    csFile = Regex.Replace(csFile, @"/\*(.|\r\n)*?(?<=\*/)", String.Empty);
                }

                var regularExpression = new Regex(Node.Template);
                var matches = regularExpression.Matches(csFile);

                for (var i = 0; i < matches.Count; i++)
                {
                    nodes.Add(Parse(matches[i].Value));
                }

#if verbose
                Output(nodes, index);
#endif
                index++;
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            var uml = BuildUml(nodes);
            File.AppendAllLines(outputPath, new List<string> { uml });
            Console.WriteLine("Successful generated");
        }

        private string BuildUml(IEnumerable<Node> nodes)
        {
            var uml = string.Empty;
            foreach (var node in nodes)
            {
                if (node.Parents != null)
                {
                    foreach (var nodeParent in node.Parents)
                    {
                        var generics = nodeParent.ClassGenerics != null ? $":{nodeParent.ClassGenerics}" : string.Empty;
                        uml += $"{node.Class.ClassName}--|>{nodeParent.ClassName}{generics}{Environment.NewLine}";
                    }
                }
            }
            return uml;
        }

        private void Output(List<Node> nodes, int index)
        {
            foreach (var node in nodes)
            {
                Console.WriteLine($"{index}{new string('-', 200)}");
                Console.WriteLine("Class:");
                Console.WriteLine(node.Class.ClassName);
                Console.WriteLine("Generics:");
                Console.WriteLine(node.Class.ClassGenerics ?? "null");
                Console.WriteLine("Parents:");
                if (node.Parents != null)
                {
                    foreach (var nodeParent in node.Parents)
                    {
                        var generics = nodeParent.ClassGenerics != null
                            ? $"has generics {nodeParent.ClassGenerics}"
                            : "";
                        Console.WriteLine($"{nodeParent.ClassName} {generics}");
                    }
                }

                Console.WriteLine("Constraints:");
                Console.WriteLine(node.Constraints ?? "null");
                Console.WriteLine();
            }
        }
        

        private Node Parse(string a)
        {
            a = a.Replace("class", string.Empty).Replace("\r\n", string.Empty).Replace("\n\t", String.Empty);
            a = a.Trim("\r").Trim("\n").Trim("\t");
            var node = new Node();

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

