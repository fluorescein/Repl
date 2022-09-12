using System;
using System.Collections.Generic;
using System.Text;

namespace Repl
{
    public enum TokenType
    { 
        LEFT_PAREN, RIGHT_PAREN,
        COMMA, DOT, DOUBLE_DOT,
        COLON, SEMICOLON,
        PLUS, MINUS, ASTERISK, SLASH, CARET,    // arithmetic operators

        BANG, QUESTION_MARK,
        EQUAL, EQUAL_EQUAL, NOT_EQUAL,          // comparison operators
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,

        LET,                                    // variable declaration
        IF, THEN, ELSE,
        TRUE, FALSE,
        AND, OR,
        FOR, IN, BEGIN, END,
        WHILE,

        IDENTIFIER,                             // variable names
        STRING, NUMBER,                         // data types

        PRINT, NIL,
        EOF
    }

    class Token
    {
        public TokenType type;
        public string lexeme;           // string representation of token
        public object literal;          // value of token if it represents literal (int, bool, string)
        public int line;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public override string ToString()
        {
            return String.Format("{0, -15} {1, -15} line: {2, -15}", type, lexeme, literal, line.ToString());
        }
    }
}
