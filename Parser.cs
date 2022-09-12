using System;
using System.Collections.Generic;
using System.Text;

namespace Repl
{
    // recursive descent parser
    class Parser
    {
        private List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }


        public List<Statement> Parse()
        {
            List<Statement> statements = new List<Statement>();

            // program is a sequence of statements
            // we start parsing from declarations
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Statement Declaration()
        {
            try
            {
                if (MatchesNext(TokenType.LET))
                {
                    return VariableDeclaration();
                }

                // declaration not found, parse other statements
                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        // parse statement according to keyword
        private Statement Statement()
        {
            if (MatchesNext(TokenType.IF))
            {
                return IfStatement();
            }

            if (MatchesNext(TokenType.PRINT))
            {
                return PrintStatement();
            }

            if (MatchesNext(TokenType.FOR))
            {
                return ForStatement();
            }

            if (MatchesNext(TokenType.WHILE))
            {
                return WhileStatement();
            }

            return ExpressionStatement();
        }

        // print result of an expression
        private Statement PrintStatement()
        {
            Expression value = Expression();
            Consume(TokenType.SEMICOLON, "Expected ';' after expression.");
            return new PrintStatement(value);
        }

        private Statement ExpressionStatement()
        {
            Expression expr = Expression();
            Consume(TokenType.SEMICOLON, "Expected ';' after expression.");
            return new ExpressionStatement(expr);
        }

        private Statement VariableDeclaration()
        {
            // "let" <identifier> "=" <initializer> ";"
            Token name = Consume(TokenType.IDENTIFIER, "Expected variable name.");      // let is already consumed
            Expression initializer = null;

            // variable is initialized with the result of expression
            if (MatchesNext(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expected ';' after declaration.");
            return new VariableStatement(name, initializer);
        }

        private Statement IfStatement()
        {
            Expression condition = Expression();
            Consume(TokenType.THEN, "Expected 'then' after condition.");

            Statement thenBranch = Statement();
            Statement elseBranch = null;

            if (MatchesNext(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new IfStatement(condition, thenBranch, elseBranch);
        }


        private Statement ForStatement()
        {
            Advance();                                  // skip loop "identifier"
            Consume(TokenType.IN, "Expected 'in'.");
            Expression left = AddSub();
            Consume(TokenType.DOUBLE_DOT, "Expected '..' after first expression.");
            Expression right = AddSub();
            Consume(TokenType.BEGIN, "Expected 'begin' after range.");

            List<Statement> body = new List<Statement>();

            while (Peek().type != TokenType.END)
            {
                body.Add(Declaration());
            }

            Consume(TokenType.END, "Expected 'end' after statement.");
            return new ForStatement(left, right, body);
        }

        private Statement WhileStatement()
        {
            Expression condition = Expression();
            Consume(TokenType.BEGIN, "Expected 'begin' after condition.");

            List<Statement> body = new List<Statement>();

            while (Peek().type != TokenType.END)
            {
                body.Add(Declaration());
            }

            Consume(TokenType.END, "Expected 'end' after statement.");
            return new WhileStatement(condition, body);
        }


        // parsing expressions

        private Expression Expression() => Comma();

        // operator comma evaluates expressions separated by comma and returns the last value
        private Expression Comma()
        {
            Expression expr = Assignment();

            while (MatchesNext(TokenType.COMMA))
            {
                expr = Assignment();
            }

            return expr;
        }

        private Expression Assignment()
        {
            Expression expr = LogicalOr();               // parse assignment as if it was an expression (e.g. left side of "x = 10;" looks like expression)

            if (MatchesNext(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expression value = Assignment();        // assignment is right-associative, we parse next assignment

                if (expr is VariableExpression)
                {
                    Token name = ((VariableExpression)expr).name;
                    return new AssignmentExpression(name, value);
                }

                Error(equals, "Invalid assignment.");
            }

            return expr;
        }

        private Expression LogicalOr()
        {
            Expression expr = LogicalAnd();

            while (MatchesNext(TokenType.OR))
            {
                Token oper = Previous();
                Expression right = LogicalAnd();
                expr = new LogicalExpression(expr, oper, right);
            }

            return expr;
        }

        private Expression LogicalAnd()
        {
            Expression expr = Equality();

            while (MatchesNext(TokenType.AND))
            {
                Token oper = Previous();
                Expression right = Equality();
                expr = new BinaryExpression(expr, oper, right);
            }

            return expr;
        }


        private Expression Equality()
        {
            Expression expr = comparison();

            while (MatchesNext(TokenType.EQUAL_EQUAL, TokenType.NOT_EQUAL))
            {
                Token oper = Previous();
                Expression right = comparison();
                expr = new BinaryExpression(expr, oper, right);
            }

            return expr;
        }

        private Expression comparison()
        {
            Expression expr = AddSub();

            while (MatchesNext(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token oper = Previous();
                Expression right = AddSub();
                expr = new BinaryExpression(expr, oper, right);
            }

            return expr;
        }

        private Expression AddSub()
        {
            Expression expr = MulDiv();

            while (MatchesNext(TokenType.PLUS, TokenType.MINUS))
            {
                Token oper = Previous();
                Expression right = MulDiv();
                expr = new BinaryExpression(expr, oper, right);
            }

            return expr;
        }

        private Expression MulDiv()
        {
            Expression expr = Unary();

            while (MatchesNext(TokenType.ASTERISK, TokenType.SLASH))
            {
                Token oper = Previous();
                Expression right = Unary();
                expr = new BinaryExpression(expr, oper, right);
            }

            return expr;
        }

        private Expression Unary()
        {
            if (MatchesNext(TokenType.BANG, TokenType.MINUS))
            {
                Token oper = Previous();
                Expression right = Unary();
                return new UnaryExpression(oper, right);
            }

            return Exponent();
        }

        private Expression Exponent()
        {
            Expression expr = Primary();

            while (MatchesNext(TokenType.CARET))
            {
                Token oper = Previous();
                Expression right = Unary();
                expr = new BinaryExpression(expr, oper, right);
            }

            return expr;
        }

        private Expression Primary()
        {
            if (MatchesNext(TokenType.TRUE))
            {
                return new LiteralExpression(true);
            }

            if (MatchesNext(TokenType.FALSE))
            {
                return new LiteralExpression(false);
            }

            if (MatchesNext(TokenType.NUMBER, TokenType.STRING))
            {
                return new LiteralExpression(Previous().literal);
            }

            if (MatchesNext(TokenType.IDENTIFIER))
            {
                return new VariableExpression(Previous());
            }

            if (MatchesNext(TokenType.LEFT_PAREN))
            {
                Expression expr = Expression();             // after opening parenthesis, expect expression
                Consume(TokenType.RIGHT_PAREN, "Expected '(' after expression.");       // parentheses must be balanced
                return new GroupingExpression(expr);
            }

            throw Error(Peek(), "Expected expression.");
        }


        // returns true if current token is the last token
        private bool IsAtEnd() => Peek().type == TokenType.EOF;

        private Token Peek() => tokens[current];

        private Token Previous() => tokens[current - 1];

        private Token Advance()
        {
            if (!IsAtEnd())
            {
                current++;
            }

            return Previous();
        }

        // returns true if next token has the same type as parameter
        private bool CheckType(TokenType type)
        {
            if (IsAtEnd())
            {
                return false;
            }

            return Peek().type == type;
        }

        // returns true if next token has the same type as one of the supplied parameters
        // if yes, moves to the next token
        private bool MatchesNext(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (CheckType(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        // if next token has the same type as supplied parameter, move to the next token
        // if not, raise an error (expected token was not present)
        private Token Consume(TokenType type, string message)
        {
            if (CheckType(type))
            {
                return Advance();
            }

            throw Error(Peek(), message);
        }

        // when ivnoked, throws ParseError
        private ParseError Error(Token token, string message)
        {
            Repl.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().type == TokenType.SEMICOLON)
                {
                    return;
                }

                switch (Peek().type)
                {
                    case TokenType.LET:
                    case TokenType.PRINT:
                    case TokenType.IF:
                    case TokenType.FOR:
                    case TokenType.WHILE:
                        return;
                }

                Advance();
            }
        }
    }
}
