namespace UmlGenerator
{
    public class ClassInfo
    {
        private string className;
        public string ClassName
        {
            get => className;
            set => className = value.TrimWhitespace();
        }

        private string classGenerics;
        public string ClassGenerics
        {
            get => classGenerics;
            set => classGenerics = value;
        }
    }
}