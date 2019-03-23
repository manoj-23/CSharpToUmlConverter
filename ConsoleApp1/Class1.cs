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
            string directory = @"C:\Work\Leapwork\Main\LeapTest.AutomationStudio\ViewModels\BuildingBlocks";
            string fileName = @"FlowProperties\BaseBlockFlowPropertyViewModel.cs";
            string pathFile = Path.Combine(directory, fileName);
            var stream = File.OpenText(pathFile);
            var csFile = stream.ReadToEnd();
            string pattern = "class(.|\n)*?{";
            var regularExpression = new Regex(pattern);
            var matches = regularExpression.Matches(csFile);

            for (var i = 0; i < matches.Count; i++)
            {
                var node = new Node();

                //" BaseBlockFlowPropertyViewModel<TFlowProperty, TInnerFlowProperty> : FlowPropertyViewModel, IBaseBlockFlowPropertyViewModel        where TFlowProperty : IBlockFlowPropertyM<TInnerFlowProperty>        where TInnerFlowProperty : IFlowPropertyM    "
                var a = matches[i].Value.Replace("class", "").Trim('{').Replace("\r\n", "");
                if (a.Contains("where"))
                {
                    if (a.Contains(':'))
                    {
                        if (a.IndexOf("where") > a.IndexOf(':'))
                        {
                            node.ClassName = a.Substring(0, a.IndexOf(':'));
                            a = a.Remove(0, a.IndexOf(':'));

                            node.ParentClasses = a.Substring(0, a.IndexOf("where")).Split(',');
                            a = a.Remove(0, a.IndexOf("where"));
                            node.Constraints = a.Split(new []{"where"}, StringSplitOptions.None);
                        }
                        else
                        {
                            node.ClassName = a.Substring(0, a.IndexOf("where"));
                            a = a.Remove(0, a.IndexOf("where"));
                            node.ParentClasses = null;
                            node.Constraints = a.Split(new[] { "where" }, StringSplitOptions.None);
                        }
                    }
                    else
                    {
                        node.ClassName = a.Substring(0, a.IndexOf("where"));
                        a = a.Remove(0, a.IndexOf("where"));
                        node.ParentClasses = null;
                        node.Constraints = a.Split(new[] { "where" }, StringSplitOptions.None);
                    }
                }
                else
                {
                  
                    if (a.Contains(':'))
                    {
                        node.ClassName = a.Substring(0, a.IndexOf(':'));
                        a = a.Remove(0, a.IndexOf(':'));
                        node.ParentClasses = a.Split(',');
                        node.Constraints = null;
                    }
                    else
                    {
                        node.ClassName = a;
                        node.ParentClasses = null;
                        node.Constraints = null;
                    }
                }
            }
        }
    }

    public class Node
    {
        public string ClassName { get; set; }
        public string[] ParentClasses { get; set; }
        public string[] Constraints { get; set; }
    }
}


//C:\Work\Leapwork\Main\LeapTest.AutomationStudio\ViewModels\BuildingBlocks\FlowPropertyViewModel.cs
//C:\Work\Leapwork\Main\LeapTest.AutomationStudio\ViewModels\BuildingBlocks\CompareBaseBlockViewModel.cs
//C:\Work\Leapwork\Main\LeapTest.AutomationStudio\ViewModels\BuildingBlocks\FlowProperties\BaseBlockFlowPropertyViewModel.cs