using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                string pattern = "class(.|\n)*?{";
                var regularExpression = new Regex(pattern);
                var matches = regularExpression.Matches(csFile);


                var nodes = new List<Node>();
                for (var i = 0; i < matches.Count; i++)
                {
                    var node = new Node();
                    var a = matches[i].Value.Replace("class", "").Trim('{').Replace("\r\n", "");
                    if (a.Contains("where"))
                    {
                        if (a.Contains(':'))
                        {
                            if (a.IndexOf("where") > a.IndexOf(':'))
                            {
                                node.ClassName = a.Substring(0, a.IndexOf(':'));
                                a = a.Remove(0, a.IndexOf(':'));

                                node.ParentClasses = a.Substring(0, a.IndexOf("where"));
                                a = a.Remove(0, a.IndexOf("where"));
                                node.Constraints = a;
                            }
                            else
                            {
                                node.ClassName = a.Substring(0, a.IndexOf("where"));
                                a = a.Remove(0, a.IndexOf("where"));
                                node.ParentClasses = null;
                                node.Constraints = a;
                            }
                        }
                        else
                        {
                            node.ClassName = a.Substring(0, a.IndexOf("where"));
                            a = a.Remove(0, a.IndexOf("where"));
                            node.ParentClasses = null;
                            node.Constraints = a;
                        }
                    }
                    else
                    {

                        if (a.Contains(':'))
                        {
                            node.ClassName = a.Substring(0, a.IndexOf(':'));
                            a = a.Remove(0, a.IndexOf(':'));
                            node.ParentClasses = a;
                            node.Constraints = null;
                        }
                        else
                        {
                            node.ClassName = a;
                            node.ParentClasses = null;
                            node.Constraints = null;
                        }
                    }

                    nodes.Add(node);
                }

                foreach (var node in nodes)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(node.ClassName);
                    Console.WriteLine(node.ParentClasses);
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
        public string ClassName { get; set; }
        public string Generics { get; set; }
        public string ParentClasses { get; set; }
        public string Constraints { get; set; }

        public Node()
        {
            ClassName = string.Empty;
            Generics = String.Empty;
            ParentClasses = string.Empty;
            Constraints = string.Empty;
        }
    }
}


//C:\Work\Leapwork\Main\LeapTest.AutomationStudio\ViewModels\BuildingBlocks\FlowPropertyViewModel.cs
//C:\Work\Leapwork\Main\LeapTest.AutomationStudio\ViewModels\BuildingBlocks\CompareBaseBlockViewModel.cs
//C:\Work\Leapwork\Main\LeapTest.AutomationStudio\ViewModels\BuildingBlocks\FlowProperties\BaseBlockFlowPropertyViewModel.cs