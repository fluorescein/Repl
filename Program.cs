using System;
using System.Collections.Generic;

namespace Repl
{
    class Program
    {
        static void Main(string[] args)
        {
            Repl repl = new Repl();
            repl.RunPrompt();
        }
    }

    class Repl
    {
        private static Interpreter interpreter = new Interpreter();
        private static string prompt = "[repl] ";
        private static bool errorDetected = false;

        public void RunPrompt()
        {
            while (true)
            {
                errorDetected = false;
                Console.Write(prompt);
                Run(Console.ReadLine() + "\n");         // Console.ReadLine() removes trailing newline, we have to add it back
            }
        }

        public void Run(string input)
        {
            Lexer lexer = new Lexer(input);
            lexer.Tokenize();

            Parser parser = new Parser(lexer.tokens);
            List<Statement> statements = parser.Parse();

            if (lexer.tokens.Count == 1 || errorDetected)
            {
                return;
            }

            interpreter.Interpret(statements);
        }

        public static void Error(int line, string message)
        {
            ReportError(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                ReportError(token.line, "at end", message);
            }
            else
            {
                ReportError(token.line, "at '" + token.lexeme + "'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            if (error.token == null)
            {
                Console.WriteLine($"{error.Message}");
                return;
            }

            Console.WriteLine($"line {error.token.line}: {error.Message}");
        }

        private static void ReportError(int line, string where, string message)
        {
            Console.WriteLine($"line {line}: error {where}: {message}");
            errorDetected = true;
        }
    }
}
