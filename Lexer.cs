using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Repl
{
    class Lexer
    {
        private string input;
        private int start = 0;
        private int current = 0;
        private int line = 1;

        public List<Token> tokens = new List<Token>();

        private static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            {       "let",      TokenType.LET           },
            {       "if",       TokenType.IF            },
            {       "then",     TokenType.THEN          },
            {       "else",     TokenType.ELSE          },
            {       "true",     TokenType.TRUE          },
            {       "false",    TokenType.FALSE         },
            {       "and",      TokenType.AND           },
            {       "or",       TokenType.OR            },
            {       "for",      TokenType.FOR           },
            {       "in",       TokenType.IN            },
            {       "begin",    TokenType.BEGIN         },
            {       "end",      TokenType.END           },
            {       "while",    TokenType.WHILE         },
            {       "print",    TokenType.PRINT         },
        };

        public Lexer(string input)
        {
            this.input = input;
        }

        // produces tokens from the whole input
        public void Tokenize()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            // EOF token is always added at the end; needed by parser
            AddToken(TokenType.EOF, "", null);
        }

        // produces next token
        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                case '(':
                    AddToken(TokenType.LEFT_PAREN);
                    break;
                case ')':
                    AddToken(TokenType.RIGHT_PAREN);
                    break;
                case '.':
                    AddToken(MatchNext('.') ? TokenType.DOUBLE_DOT : TokenType.DOT);
                    break;
                case ',':
                    AddToken(TokenType.COMMA);
                    break;
                case ':':
                    AddToken(TokenType.COLON);
                    break;
                case ';':
                    AddToken(TokenType.SEMICOLON);
                    break;
                case '+':
                    AddToken(TokenType.PLUS);
                    break;
                case '-':
                    AddToken(TokenType.MINUS);
                    break;
                case '*':
                    AddToken(MatchNext('*') ? TokenType.CARET : TokenType.ASTERISK);        // "**" is only syntactic sugar for "^"
                    break;
                case '/':
                    if (MatchNext('/'))
                    {
                        SkipComment();
                        break;
                    }
                    AddToken(TokenType.SLASH);
                    break;
                case '^':
                    AddToken(TokenType.CARET);
                    break;
                case '=':
                    AddToken(MatchNext('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '?':
                    AddToken(TokenType.QUESTION_MARK);
                    break;
                case '!':
                    AddToken(MatchNext('=') ? TokenType.NOT_EQUAL : TokenType.BANG);
                    break;
                case '<':
                    AddToken(MatchNext('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(MatchNext('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '"':
                    ReadString();
                    break;
                case ' ':
                case '\t':
                case '\r':
                    break;
                case '\n':
                    line++;
                    break;
                default:
                    if (IsDigit(c))
                    {
                        ReadNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        ReadIdentifier();
                    }
                    else
                    {
                        Repl.Error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        // adds new token found between start and current positions
        // this is the case of symbols — they do not have value (which is therefore null)
        private void AddToken(TokenType type)
        {
            tokens.Add(new Token(type, input[start..current], null, line));
        }

        private void AddToken(TokenType type, string lexeme, object literal)
        {
            tokens.Add(new Token(type, lexeme, literal, line));
        }

        // returns current character and moves position to the right
        private char Advance() => input[current++];

        // returns current character without moving current position
        private char PeekChar() => input[current];

        // returns true if the current position is outside the length of input string
        private bool IsAtEnd() => current >= input.Length;

        private static bool IsDigit(char c) => ('0' <= c && c <= '9') || (c == '_');

        private static bool IsAlpha(char c) => ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || (c == '_');

        private static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

        // check if next character matches parameter
        // if yes, move to the next character
        private bool MatchNext(char c)
        {
            if (IsAtEnd())
            {
                return false;
            }

            if (input[current] == c)
            {
                current++;
                return true;
            }

            return false;
        }

        // one-line comments are supported
        private void SkipComment()
        {
            while (input[current] != '\n')
            {
                current++;
            }
        }

        private void ReadNumber()
        {
            while (IsDigit(input[current]))
            {
                current++;
            }

            string number = input[start..current];
            AddToken(TokenType.NUMBER, number, int.Parse(number));
        }

        // reads keyword or variable name
        private void ReadIdentifier()
        {
            while (IsAlphaNumeric(input[current]))
            {
                current++;
            }

            string word = input[start..current];
            TokenType type = keywords.ContainsKey(word) ? keywords[word] : TokenType.IDENTIFIER;
            AddToken(type, word, word);
        }

        // string literals are enclosed in double quotes
        private void ReadString()
        {
            while (!IsAtEnd() && PeekChar() != '"')
            {
                if (PeekChar() == '\n')
                {
                    line++;
                }

                Advance();
            }

            if (IsAtEnd())
            {
                Repl.Error(line, "Unterminated string.");
                return;
            }

            Advance();
            string value = input[start..current].Trim('"');
            AddToken(TokenType.STRING, value, value);
        }
    }
}
