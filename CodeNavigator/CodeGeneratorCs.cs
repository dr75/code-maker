using EnvDTE;
namespace CodeNavigator
{

    class CodeGeneratorCs : CodeGenerator
    {
        public CodeGeneratorCs() 
        {
            AddCodeTemplate("class.cs");
        }

        internal override CodeItem CreateCodeItem(ProjectItem srcItem)
        {
            return new CodeItemCs(srcItem);
        }
    }
} //namespace CodeNavigator

