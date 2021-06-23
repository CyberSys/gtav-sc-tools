﻿namespace ScTools.ScriptLang.CodeGen
{
    using System.Linq;

    using ScTools.ScriptAssembly;
    using ScTools.ScriptLang.Ast;
    using ScTools.ScriptLang.Ast.Declarations;
    using ScTools.ScriptLang.Ast.Expressions;
    using ScTools.ScriptLang.Ast.Statements;
    using ScTools.ScriptLang.BuiltIns;

    /// <summary>
    /// Emits code to execute statements.
    /// </summary>
    public sealed class StatementEmitter : EmptyVisitor<Void, FuncDeclaration>
    {
        public CodeGenerator CG { get; }

        public StatementEmitter(CodeGenerator cg) => CG = cg;

        public override Void Visit(LabelDeclaration node, FuncDeclaration func)
        {
            // TODO: support local labels in assembler to prevent conflicts in assembly generated by the compiler
            CG.EmitLabel(node.Name);
            return default;
        }

        public override Void Visit(VarDeclaration node, FuncDeclaration func) => default;
        public override Void Visit(AssignmentStatement node, FuncDeclaration func) => default;

        public override Void Visit(BreakStatement node, FuncDeclaration func)
        {
            CG.EmitJump(node.EnclosingStatement!.ExitLabel!);
            return default;
        }

        public override Void Visit(ContinueStatement node, FuncDeclaration func)
        {
            CG.EmitJump(node.EnclosingLoop!.BeginLabel!);
            return default;
        }

        public override Void Visit(GotoStatement node, FuncDeclaration func)
        {
            CG.EmitJump(node.Label!.Name);
            return default;
        }

        public override Void Visit(IfStatement node, FuncDeclaration func)
        {
            // check condition
            CG.EmitValue(node.Condition);
            CG.EmitJumpIfZero(node.ElseLabel!);

            // then body
            node.Then.ForEach(stmt => stmt.Accept(this, func));
            if (node.Else.Any())
            {
                // jump over the else body
                CG.EmitJump(node.EndLabel!);
            }

            // else body
            CG.EmitLabel(node.ElseLabel!);
            node.Else.ForEach(stmt => stmt.Accept(this, func));

            CG.EmitLabel(node.EndLabel!);

            return default;
        }

        public override Void Visit(RepeatStatement node, FuncDeclaration func)
        {
            var intTy = BuiltInTypes.Int.CreateType(node.Source);
            var constantZero = new IntLiteralExpression(node.Source, 0) { Type = intTy, IsConstant = true, IsLValue = false };
            var constantOne = new IntLiteralExpression(node.Source, 1) { Type = intTy, IsConstant = true, IsLValue = false };

            // set counter to 0
            new AssignmentStatement(node.Source, compoundOperator: null,
                                    lhs: node.Counter, rhs: constantZero)
                .Accept(this, func);

            CG.EmitLabel(node.BeginLabel!);

            // check counter < limit
            CG.EmitValue(node.Counter);
            CG.EmitValue(node.Limit);
            CG.Emit(Opcode.ILT_JZ, node.ExitLabel!);

            // body
            node.Body.ForEach(stmt => stmt.Accept(this, func));

            // increment counter
            new AssignmentStatement(node.Source, compoundOperator: BinaryOperator.Add,
                                    lhs: node.Counter, rhs: constantOne)
                .Accept(this, func);

            // jump back to condition check
            CG.EmitJump(node.BeginLabel!);

            CG.EmitLabel(node.ExitLabel!);

            return default;
        }

        public override Void Visit(ReturnStatement node, FuncDeclaration func)
        {
            if (node.Expression is not null)
            {
                CG.EmitValue(node.Expression);
            }
            CG.Emit(Opcode.LEAVE, func.Prototype.ParametersSize, func.Prototype.ReturnType.SizeOf);
            return default;
        }

        public override Void Visit(SwitchStatement node, FuncDeclaration func)
        {
            CG.EmitValue(node.Expression);

            CG.EmitSwitch(node.Cases.OfType<ValueSwitchCase>());

            var defaultCase = node.Cases.OfType<DefaultSwitchCase>().SingleOrDefault();
            CG.EmitJump(defaultCase?.Label ?? node.ExitLabel!);

            node.Cases.ForEach(c => c.Accept(this, func));
            CG.EmitLabel(node.ExitLabel!);
            return default;
        }

        public override Void Visit(ValueSwitchCase node, FuncDeclaration func)
        {
            CG.EmitLabel(node.Label!);
            node.Body.ForEach(stmt => stmt.Accept(this, func));
            return default;
        }

        public override Void Visit(DefaultSwitchCase node, FuncDeclaration func)
        {
            CG.EmitLabel(node.Label!);
            node.Body.ForEach(stmt => stmt.Accept(this, func));
            return default;
        }

        public override Void Visit(WhileStatement node, FuncDeclaration func)
        {
            CG.EmitLabel(node.BeginLabel!);

            // check condition
            CG.EmitValue(node.Condition);
            CG.EmitJumpIfZero(node.ExitLabel!);

            // body
            node.Body.ForEach(stmt => stmt.Accept(this, func));

            // jump back to condition check
            CG.EmitJump(node.BeginLabel!);

            CG.EmitLabel(node.ExitLabel!);

            return default;
        }

        public override Void Visit(InvocationExpression node, FuncDeclaration func)
        {
            CG.EmitValue(node);
            var returnValueSize = node.Type!.SizeOf;
            for (int i = 0; i < returnValueSize; i++)
            {
                CG.Emit(Opcode.DROP);
            }
            return default;
        }
    }
}
