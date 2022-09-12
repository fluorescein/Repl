using System;
using System.Collections.Generic;
using System.Text;

namespace Repl
{
    class Environment
    {
        private Dictionary<string, object> variables = new Dictionary<string, object>();

        // if the variable exists, assign it a new value
        // if not, define it
        public void DefineVariable(string name, object value)
        {
            variables[name] = value;
        }

        public object GetValue(Token name)
        {
            if (variables.ContainsKey(name.lexeme))
            {
                return variables[name.lexeme];
            }

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }

        public void Assign(Token name, object value)
        {
            if (variables.ContainsKey(name.lexeme))
            {
                variables[name.lexeme] = value;
                return;
            }

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }
    }
}
