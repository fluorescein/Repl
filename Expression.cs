using System;
using System.Collections.Generic;
using System.Text;

namespace Repl
{
    // base class representing possible (supported) expressions
    abstract class Expression
    {
        public interface IVisitor<T>
        {
            T VisitLiteralExpression(LiteralExpression expr);
            T VisitBinaryExpression(BinaryExpression expr);
            T VisitUnaryExpression(UnaryExpression expr);
            T VisitLogicalExpression(LogicalExpression expr);
            T VisitGroupingExpression(GroupingExpression expr);
            T VisitVariableExpression(VariableExpression expr);
            T VisitAssignmentExpression(AssignmentExpression expr);
        }

        // using Visitor pattern to call correct evaluation method for each kind of expression
        public abstract T Accept<T>(IVisitor<T> Visitor);
    }


    class BinaryExpression : Expression
    {
        public Expression left;
        public Expression right;
        public Token oper;          // operator

        public BinaryExpression(Expression left, Token oper, Expression right)
        {
            this.oper = oper;
            this.left = left;
            this.right = right;
        }

        public override T Accept<T>(IVisitor<T> Visitor)
        {
            return Visitor.VisitBinaryExpression(this);
        }
    }

    class LiteralExpression : Expression
    {
        public object value;

        public LiteralExpression(object value)
        {
            this.value = value;
        }

        public override T Accept<T>(IVisitor<T> Visitor)
        {
            return Visitor.VisitLiteralExpression(this);
        }
    }


    class UnaryExpression : Expression
    {
        public Expression right;
        public Token oper;              // "!" or "-" operator

        public UnaryExpression(Token oper, Expression right)
        {
            this.oper = oper;
            this.right = right;
        }

        public override T Accept<T>(IVisitor<T> Visitor)
        {
            return Visitor.VisitUnaryExpression(this);
        }
    }

    class LogicalExpression : Expression
    {
        public Expression left;
        public Expression right;
        public Token oper;

        public LogicalExpression(Expression left, Token oper, Expression right)
        {
            this.oper = oper;
            this.left = left;
            this.right = right;
        }

        public override T Accept<T>(IVisitor<T> Visitor)
        {
            return Visitor.VisitLogicalExpression(this);
        }
    }


    class GroupingExpression : Expression
    {
        public Expression expr;

        public GroupingExpression(Expression expr)
        {
            this.expr = expr;
        }

        public override T Accept<T>(IVisitor<T> Visitor)
        {
            return Visitor.VisitGroupingExpression(this);
        }
    }

    class VariableExpression : Expression
    {
        public Token name;

        public VariableExpression(Token name)
        {
            this.name = name;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitVariableExpression(this);
        }
    }

    class AssignmentExpression : Expression
    {
        public Token name;
        public Expression value;

        public AssignmentExpression(Token name, Expression value)
        {
            this.name = name;
            this.value = value;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitAssignmentExpression(this);
        }
    }
}
