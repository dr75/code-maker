using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator
{
    class MethodImpl : MethodDecl
    {
        internal CodeFragment Init { get; set; }
        internal CodeFragment Body { get; set; }

        /// <summary>
        /// Create a method impl.
        /// </summary>
        /// <param name="retType">return type (TODO: currently includes templ. def)</param>
        /// <param name="name">name of method (excluding class name)</param>
        /// <param name="args">arguments (excluding '(' and ')')</param>
        /// <param name="modifiers">list of modifiers</param>
        /// <param name="init">initializer in ctors</param>
        /// <param name="body">method body (excluding '{' and '}')</param>
        public MethodImpl(String retType, String name, String args, 
            String modifiers, String init, String body)
            : base(retType, name, args, modifiers)
        {   
            Init = new CodeFragment(init);
            Body = new CodeFragment(body);
        }

        public MethodImpl(MethodDecl src)
            : base(src)
        {
            Init = null;
            Body = null;
        }

        private void AppendArgs(StringBuilder res)
        {
            res.Append('(');

            List<Argument> args = GetArgs();
            bool start = true;
            foreach (Argument arg in args)
            {
                if (start)
                    start = false;
                else
                    res.Append(", ");

                arg.Append(res, true);
            }
            res.Append(')');
        }

        protected override void AppendSignature(StringBuilder res)
        {
            //name
            res.Append(_name);
            
            //args
            AppendArgs(res);
            
            //const
            if (_strModifiers.Length > 0)
            {
                List<String> modList = GetModifiers();

                //only append const in impl
                if (modList.Contains("const"))
                    res.Append(' ').Append("const");
            }
        }

        public override string ToString()
        {
            return GetImplAsString();
        }

        internal string GetImplAsString(String className = null)
        {
            if (className == null)
                className = _className;

            StringBuilder res = new StringBuilder();

            //if static, then prepend static as comment
            if (IsStatic())
                res.Append("//static\n");

            //type 
            res.Append(_retType).Append(' ');

            //class::
            res.Append(_className).Append("::");

            //name(args) [const]
            AppendSignature(res);

            //[override][:init]
            if (Init != null && Init.Length>0)
            {
                res.Append("\n:");
                res.AppendLine(Init.Code);
            }

            //{ body }
            res.Append("\n{").Append(Body).Append('}');
            return res.ToString();
        }
    }
}
