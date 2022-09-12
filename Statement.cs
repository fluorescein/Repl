using System;
using System.Collections.Generic;
using System.Text;

namespace Repl
{
    abstract class Statement
    {
        public interface IVisitor<T>
        {
            T VisitExpressionStatement(ExpressionStatement stmt);
            T VisitPrintStatement(PrintStatement stmt);
            T VisitVariableStatement(VariableStatement stmt);
            T VisitIfStatement(IfStatement stmt);
            T VisitForStatement(ForStatement stmt);
            T VisitWhileStatement(WhileStatement stmt);
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }

    class ExpressionStatement : Statement
    {
        public Expression expr;

        public ExpressionStatement(Expression expr)
        {
            this.expr = expr;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }

    class PrintStatement : Statement
    {
        public Expression expr;

        public PrintStatement(Expression expr)
        {
            this.expr = expr;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitPrintStatement(this);
        }
    }

    class VariableStatement : Statement
    {
        public Token name;
        public Expression initializer;

        public VariableStatement(Token name, Expression initializer)
        {
            this.name = name;
            this.initializer = initializer;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitVariableStatement(this);
        }        
    }

    class IfStatement : Statement
    {
        public Expression condition;
        public Statement thenBranch;
        public Statement elseBranch;

        public IfStatement(Expression condition, Statement thenBranch, Statement elseBranch)
        {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitIfStatement(this);
        }
    }


    class ForStatement : Statement
    {
        public Expression left;
        public Expression right;
        public List<Statement> body;

        public ForStatement(Expression left, Expression right, List<Statement> body)
        {
            this.left = left;
            this.right = right;
            this.body = body;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitForStatement(this);
        }
    }


    class WhileStatement : Statement
    {
        public Expression condition;
        public List<Statement> body;

        public WhileStatement(Expression condition, List<Statement> body)
        {
            this.condition = condition;
            this.body = body;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitWhileStatement(this);
        }
    }
}
