﻿namespace ScTools.ScriptLang.Ast.Declarations
{
    using ScTools.ScriptLang.Ast.Expressions;
    using ScTools.ScriptLang.Ast.Statements;

    public enum VarKind
    {
        /// <summary>
        /// A static variable with the CONST modifier. Where used, it is replaced by its value at compile-time. Requires a non-null <see cref="VarDeclaration.Initializer"/>.
        /// </summary>
        Constant,
        /// <summary>
        /// A variable defined in a GLOBAL block, shared between script threads.
        /// </summary>
        Global,
        /// <summary>
        /// A variable defined outside any functions/procedures.
        /// </summary>
        Static,
        /// <summary>
        /// A static variable with the ARG modifier. Initialized by other script thread when starting
        /// this script with `START_NEW_SCRIPT_WITH_ARGS` or `START_NEW_SCRIPT_WITH_NAME_HASH_AND_ARGS`.
        /// </summary>
        StaticArg,
        /// <summary>
        /// A variable defined inside a function/procedure.
        /// </summary>
        Local,
        /// <summary>
        /// A local variable that represents a parameter of a function/procedure. <see cref="VarDeclaration.Initializer"/> must be null.
        /// </summary>
        Parameter,
    }

    public sealed class VarDeclaration : BaseValueDeclaration, IStatement
    {
        public VarKind Kind { get; set; }
        public IExpression? Initializer { get; set; }

        public VarDeclaration(SourceRange source, string name, VarKind kind) : base(source, name)
            => Kind = kind;

        public override TReturn Accept<TReturn, TParam>(IVisitor<TReturn, TParam> visitor, TParam param)
            => visitor.Visit(this, param);
    }
}
