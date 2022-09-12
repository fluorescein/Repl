using System;
using System.Collections.Generic;
using System.Text;

namespace Repl
{
    class Interpreter : Expression.IVisitor<object>, Statement.IVisitor<object>
    {
        private Environment environment = new Environment();

        public void Interpret(List<Statement> statements)
        {
            try
            {
                foreach (Statement stmt in statements)
                {
                    Execute(stmt);
                }
            }
            catch (RuntimeError error)
            {
                Repl.RuntimeError(error);
            }
        }

        // evaluating statements

        private void Execute(Statement stmt)
        {
            stmt.Accept(this);
        }

        // evaluate expression inside statement
        public object VisitExpressionStatement(ExpressionStatement stmt)
        {
            Evaluate(stmt.expr);
            return null;
        }

        // evaluate expression inside statement and display result
        public object VisitPrintStatement(PrintStatement stmt)
        {
            object value = Evaluate(stmt.expr);
            Console.WriteLine(PutString(value));
            return null;
        }

        public object VisitVariableStatement(VariableStatement stmt)
        {
            object value = null;

            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            environment.DefineVariable(stmt.name.lexeme, value);
            return null;
        }

        public object VisitIfStatement(IfStatement stmt)
        {
            if (IsTrue(stmt.condition))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }

            return null;
        }

        public object VisitForStatement(ForStatement stmt)
        {
            object left = Evaluate(stmt.left);
            object right = Evaluate(stmt.right);

            if (!(left is int) || !(right is int))
            {
                throw new RuntimeError("Range must be specified by numbers.");
            }

            int i = (int)left;
            int j = (int)right;

            while (i < j)
            {
                foreach (Statement statement in stmt.body)
                {
                    Execute(statement);
                }

                i++;
            }

            return null;
        }

        public object VisitWhileStatement(WhileStatement stmt)
        {
            while (IsTrue(Evaluate(stmt.condition)))
            {
                foreach (Statement statement in stmt.body)
                {
                    Execute(statement);
                }
            }

            return null;
        }


        // evaluating expressions

        private object Evaluate(Expression expr)
        {
            return expr.Accept(this);
        }

        // return literal value stored in the token
        public object VisitLiteralExpression(LiteralExpression expr)
        {
            return expr.value;
        }

        // return value bound to the name
        public object VisitVariableExpression(VariableExpression expr)
        {
            return environment.GetValue(expr.name);
        }

        // assign to variable and return the value
        public object VisitAssignmentExpression(AssignmentExpression expr)
        {
            object value = Evaluate(expr.value);
            environment.Assign(expr.name, value);
            return value;
        }

        // evaluate expression inside GroupingExpression
        public object VisitGroupingExpression(GroupingExpression expr)
        {
            return Evaluate(expr.expr);
        }

        public object VisitLogicalExpression(LogicalExpression expr)
        {
            // left operand is always evaluated
            object left = Evaluate(expr.left);

            if (expr.oper.type == TokenType.OR)
            {
                if (IsTrue(left))
                {
                    return left;
                }
            }
            else
            {
                if (!IsTrue(left))
                {
                    return left;
                }
            }

            // right operand is evaluated only if it needs to be
            return Evaluate(expr.right);
        }

        // logical NOT and arithmetic negation
        public object VisitUnaryExpression(UnaryExpression expr)
        {
            object right = Evaluate(expr.right);

            if (expr.oper.type == TokenType.BANG)
            {
                return !IsTrue(right);
            }
            else if (expr.oper.type == TokenType.MINUS)
            {
                CheckNumberOneOperand(expr.oper, right);
                return -(int)right;
            }

            return null;
        }

        // arithmetic, equality and comparison operations
        public object VisitBinaryExpression(BinaryExpression expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.oper.type)
            {
                case TokenType.PLUS:
                    if (left is string && right is string)          // "+" concatenates strings
                    {
                        return left.ToString() + right.ToString();
                    }
                    CheckNumberTwoOperands(expr.oper, left, right);
                    return (int)left + (int)right;
                case TokenType.MINUS:
                    CheckNumberTwoOperands(expr.oper, left, right);
                    return (int)left - (int)right;
                case TokenType.ASTERISK:
                    CheckNumberTwoOperands(expr.oper, left, right);
                    return (int)left * (int)right;
                case TokenType.SLASH:
                    CheckNumberTwoOperands(expr.oper, left, right);
                    if ((int)right == 0)
                    {
                        throw new RuntimeError(expr.oper, "Division by zero is undefined.");
                    }
                    return (int)left / (int)right;
                case TokenType.CARET:
                    CheckNumberTwoOperands(expr.oper, left, right);
                    return (int)Math.Pow(Convert.ToDouble(left), Convert.ToDouble(right));
                case TokenType.GREATER:
                    CheckNumberTwoOperands(expr.oper, left, right);
                    return (int)left > (int)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberTwoOperands(expr.oper, left, right);
                    return (int)left >= (int)right;
                case TokenType.LESS:
                    CheckNumberTwoOperands(expr.oper, left, right);
                    return (int)left < (int)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberTwoOperands(expr.oper, left, right);
                    return (int)left <= (int)right;
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case TokenType.NOT_EQUAL:
                    return !IsEqual(left, right);
            }

            return null;
        }

        // helper methods

        // if parameter is not an int, raise an error
        private void CheckNumberOneOperand(Token oper, object operand)
        {
            if (operand is int)
            {
                return;
            }

            throw new RuntimeError(oper, "Operand must be a number.");
        }

        // if either of two parameters are not int, raise an error
        private void CheckNumberTwoOperands(Token oper, object left, object right)
        {
            if (left is int && right is int)
            {
                return;
            }

            throw new RuntimeError(oper, "Operands must be numbers.");
        }

        private bool IsTrue(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is bool)
            {
                return (bool)obj;
            }

            return true;
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null)
            {
                if (b == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return a.Equals(b);
        }

        public string PutString(object obj)
        {
            return obj == null ? "nil" : obj.ToString();
        }
    }
}
