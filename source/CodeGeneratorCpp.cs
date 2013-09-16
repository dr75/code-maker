using EnvDTE;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeNavigator
{
    class CodeGeneratorCpp : CodeGenerator
    {
        public CodeGeneratorCpp()
        {
            //add cpp first to execute it before the h, which is the window
            //that stays active after execution; otherwise the editor would 
            //flicker a bit while switching active windows
            CodeTemplate classImpl = AddCodeTemplate("class.cpp");
            CodeTemplate classDecl = AddCodeTemplate("class.h");

            /*
            classDecl.AddCodeTemplate("ctor_decl.h");
            classDecl.AddCodeTemplate("dtor_decl.h");

            classImpl.AddCodeTemplate("ctor_impl.cpp");
            classImpl.AddCodeTemplate("dtor_impl.cpp");*/

            _codeTemplateClassDef = new CodeTemplate("classDef.h");
        }

        internal override CodeItem CreateCodeItem(ProjectItem srcItem)
        {
            return CodeItemCpp.Create(srcItem);
        }
    }
} //namespace CodeNavigator

