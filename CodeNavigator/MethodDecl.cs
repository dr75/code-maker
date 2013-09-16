using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator
{
    class MethodDecl : CodeFragment
    {
        protected String _retType;
        protected String _name;
        protected String _className;
        
        protected String _strArgs;
        protected List<Argument> _args = null;

        protected String _strModifiers;
        protected List<String> _modifiers = null;

        protected String _comment;  //comment in front of method or after method
        protected bool _bVirtual = false;
        protected bool _bAbstract = false;
        protected bool _bStatic = false;
        private bool _bConstructor = false;
        private bool _bDestructor = false;

        internal MethodDecl(String retType, String name, String args, 
            String modifiers)
        {
            _retType = retType.Trim();
            _name = name.Trim();
            _strModifiers = modifiers.Trim();

            if (_strModifiers.EndsWith("0"))
            {
                int p = _strModifiers.LastIndexOf('=');
                if (p != -1 && _strModifiers.Substring(p + 1, _strModifiers.Length - p - 2).Trim().Length == 0)
                {
                    _bAbstract = true;
                    if (p>0 && _strModifiers[p-1]!=' ')
                        _strModifiers = _strModifiers.Substring(0, p) + " =0";
                    else
                        _strModifiers = _strModifiers.Substring(0, p) + "=0";
                }
            }
            _strArgs = args.Trim();
        }

        internal MethodDecl(MethodDecl src)
            :this(src._retType, src._name, src._strArgs, src._strModifiers)
        {
            SetClassName(src._className);
            _bStatic = src._bStatic;
            _bVirtual = src._bVirtual;
            _bAbstract = src._bAbstract;
            _bConstructor = src._bConstructor;
            _bDestructor = src._bDestructor;
            _comment = src._comment;
        }

        internal void SetComment(String comment)
        {
            _comment = comment;
        }

        internal String GetName()
        {
            return _name;
        }

        internal void SetClassName(String name)
        {
            _className = name;
        }

        internal String GetClassName()
        {
            return _className;
        }

        //virtual
        internal void SetIsVirtual(bool bVirtual) { _bVirtual = bVirtual;  }
        internal bool IsVirtual() { return _bVirtual; }

        //abstract
        internal bool IsAbstract() { return _bAbstract; }

        //static
        internal void SetIsStatic(bool bStatic) { _bStatic = bStatic; }
        internal bool IsStatic() { return _bStatic; }

        internal bool DoArgumentsMatch(MethodDecl other)
        {
            //TODO: do a correct argument analysis
            return _args.Equals(other._args);
        }

        internal int StrLength()
        {
            return _retType.Length + 1
                 + _name.Length
                 + _strArgs.Length + 2
                 + _strModifiers.Length + 1;
        }

        protected List<Argument> GetArgs()
        {
            if (_args==null)
                _args = Argument.Parse(_strArgs);

            return _args;
        }

        protected List<String> GetModifiers()
        {
            if (_modifiers == null)
                _modifiers = _strModifiers.Split(' ').ToList();

            return _modifiers;
        }

        protected virtual void AppendSignature(StringBuilder res)
        {
            res.Append(_name);
            res.Append('(').Append(_strArgs).Append(')');

            if (_strModifiers.Length > 0)
                res.Append(' ').Append(_strModifiers);
        }

        internal String GetDeclAsString()
        {
            StringBuilder res = new StringBuilder();

            if (_bVirtual)
                res.Append("virtual ");

            if (_bStatic)
                res.Append("static ");

            res.Append(_retType).Append(' ');

            AppendSignature(res);

            res.Append(';');
            return res.ToString();
        }

        public override string ToString()
        {
            return GetDeclAsString();
        }

        internal string GetSignature(bool bIncludeReturnType = false)
        {
            StringBuilder ret = new StringBuilder();

            //append '('
            ret.Append(_name).Append('(');

            //append list of args
            List<Argument> args = GetArgs();
            int i = 0;
            foreach (Argument arg in _args)
            {
                if (i != 0)
                    ret.Append(',');

                ret.Append(arg.Type);
                i++;
            }

            //append ')'
            ret.Append(')');

            //append const
            if (GetModifiers().Contains("const"))
                ret.Append(" const");

            //append return type
            if (_retType.Length > 0 && bIncludeReturnType)
                ret.Append(':').Append(_retType);

            return ret.ToString();
        }

        internal void SetIsDestructor(bool bDtor)
        {
            _bDestructor = bDtor;
        }

        internal void SetIsConstructor(bool bCtor)
        {
            _bConstructor = bCtor;
        }

        internal bool IsDestructor()
        {
            return _bDestructor;
        }

        internal bool IsConstructor(bool bCtor)
        {
            return _bConstructor;
        }

        internal override bool UpdateOffsetFromMapping(CodeItem code)
        {
            if (   !base.UpdateOffsetFromMapping(code)
                || !Start.HasOffset())
                return false;

            //no decrement start as long as there are lines with comment
            String txt = code.GetContent(false);
            int pos = CodeAnalyzerCpp.SkipCommentLinesReverse(txt, Start.CharOffsetWithComments);
            int diff = Start.CharOffsetWithComments - pos;

            if (diff > 0)
                Start.CharOffsetDifference = diff;
                
            return true;
        }
    }
}
