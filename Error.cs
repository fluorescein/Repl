using System;
using System.Collections.Generic;
using System.Text;

namespace Repl
{
    class RuntimeError : Exception
    {
        public Token token;

        public RuntimeError(Token token, string message) : base(message)
        {
            this.token = token;
        }

        public RuntimeError(string message)
        { 
        
        }
    }

    class ParseError : Exception
    { 
        
    }
}
