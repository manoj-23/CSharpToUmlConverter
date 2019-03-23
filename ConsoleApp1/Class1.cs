using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    public class Class1
    {
        /// <summary>
        /// key is inheritor, value is parent
        /// </summary>
        private Dictionary<string, string> inheritanceDictionary = new Dictionary<string, string>();

        public void Start()
        {
            var files = new List<string>();
            string root = @"C:\Work\Leapwork\Main\LeapTest.AutomationStudio\ViewModels\BuildingBlocks";

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
                    nodes.Add(Node.Parse(matches[i].Value));
                }

                foreach (var node in nodes)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(node.ClassName);
                    Console.WriteLine(node.ClassGenerics);
                    Console.WriteLine(node.Parents);
                    Console.WriteLine(node.Constraints);

                    Console.WriteLine($"{index}{new string('-', 30)}");
                }

                index++;
            }


            Console.ReadKey();
        }
    }

    public class Node
    {
        public static readonly string Template = "class(.|\n)*?{";

        public string ClassName { get; set; }
        public string ClassGenerics { get; set; }
        public string Parents { get; set; }
        public string Constraints { get; set; }

        public static Node Parse(string a)
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
                        node.ClassName = a.Substring(0, a.IndexOf('<'));
                        a = a.Remove(0, a.IndexOf('<'));
                        node.ClassGenerics = a.Substring(0, a.IndexOf('>'));
                        a = a.Remove(0, a.IndexOf('>'));
                        a = a.Remove(0, a.IndexOf(':'));
                    }
                    else // class haven`t generics
                    {
                        node.ClassName = a.Substring(0, a.IndexOf(':'));
                        a = a.Remove(0, a.IndexOf(':'));
                        node.ClassGenerics = null;
                    }

                    node.Parents = a.Substring(0, a.IndexOf("where", StringComparison.Ordinal));
                    a = a.Remove(0, a.IndexOf("where", StringComparison.Ordinal));
                    node.Constraints = a;
                }
                else //class haven`t inheritance
                {
                    if (a.Substring(0, a.IndexOf("where", StringComparison.Ordinal)).Contains('<') && a.Substring(0, a.IndexOf("where", StringComparison.Ordinal)).Contains('>')) // class has generics
                    {
                        node.ClassName = a.Substring(0, a.IndexOf('<'));
                        a = a.Remove(0, a.IndexOf('<'));
                        node.ClassGenerics = a.Substring(0, a.IndexOf('>'));
                        a = a.Remove(0, a.IndexOf('>'));
                        a = a.Remove(0, a.IndexOf("where", StringComparison.Ordinal));
                    }
                    else // class haven`t generics
                    {
                        node.ClassName = a.Substring(0, a.IndexOf("where", StringComparison.Ordinal));
                        node.ClassGenerics = null;
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
                        node.ClassName = a.Substring(0, a.IndexOf('<'));
                        a = a.Remove(0, a.IndexOf('<'));
                        node.ClassGenerics = a.Substring(0, a.IndexOf('>'));
                        a = a.Remove(0, a.IndexOf('>'));
                        a = a.Remove(0, a.IndexOf(':'));
                    }
                    else // class haven`t generics
                    {
                        node.ClassName = a.Substring(0, a.IndexOf(':'));
                        node.ClassGenerics = null;
                        a = a.Remove(0, a.IndexOf(':'));
                    }

                    node.Parents = a;
                    node.Constraints = null;
                }
                else // class haven`t inheritance
                {
                    if (a.Contains('<') && a.Contains('>')) // class has generics
                    {
                        node.ClassName = a.Substring(0, a.IndexOf('<'));
                        a = a.Remove(0, a.IndexOf('<'));
                        node.ClassGenerics = a.Substring(0, a.IndexOf('>'));
                    }
                    else // class haven`t generics
                    {
                        node.ClassName = a;
                        node.ClassGenerics = null;
                        node.Parents = null;
                        node.Constraints = null;
                    }
                }
            }

            return node;
        }
    }
}

