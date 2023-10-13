namespace Hulk
{
    public class Control
    {
        public static string BasicSyntax(string s) {
            s = s.Trim();

            if (string.IsNullOrWhiteSpace(s)) return "";
        
            // Revisa si las comillas de los strings están correctamente balanceadas
            if (!Aux.StringBalance(s) && !Error.Syntax("Strings are not balanced")) {
                return "";
            }

            //  Revisa si los paréntesis están corractamente balanceados
            if (Aux.ParenthesisBalance(s) != 0)  {
                string parenthesis = (Aux.ParenthesisBalance(s) == 1)? "'('" : "')'";
                Error.Syntax($"Parenthesis are not balanced: missing {parenthesis}");
                return "";
            }

            // Revisa si la expresión termina en ';'
            if (!s.EndsWith(";") && !Error.Syntax("Expected ';'")) {
                return "";
            }

            // Se remueven los ';' del final de la expresión, que pueden ser más de uno
            while (s.EndsWith(";")) {
                s = s.Remove(s.Length - 1);
                s = s.TrimEnd();
            }    

            // Llegado a este punto la expresión ya puede ser analizada
            return Analize(s);
        }

        public static string Analize(string s) {
            s = s.Trim();

            // En caso de ser la declaración de una función, se intenta crear    
            if (Function.Declaration(s)) return Function.Create(s);
            
            // En caso de ser una declaración de una variable, se intenta resolver
            if (Let_in.IsLet_in(s)) return Let_in.Eval(s);
            
            // En caso de ser una expresión condicional, se evalúa
            if (If_else.IsIf_else(s)) return If_else.Eval(s);

            // Pasa una revisión general de la expresión que excluye a las instrucciones 
            if (!Error.General(s)) return "";

            // Se evalúan las expresiones dentro de los paréntesis antes de cualquier tipo básico
            // así como los llamados de funciones
            if (Aux.StringOut(s).Contains("(")) return Aux.Parenthesis(s);

            if (string.IsNullOrWhiteSpace(s)) return "";

            // En caso de ser un string puro, se evalúa
            if (String.IsString(s)) return String.Eval(s);
            
            // En caso de ser una expresión booleana, se evalúa
            if (Boolean.IsBoolean(s)) return Boolean.Eval(s);
            
            // En caso de ser una expresión numérica, se evalúa
            if (Binary.IsBinary(s)) return Binary.Eval(s);
            
            Error.Syntax($"'{s}' is an invalid expression");
            return "";
        }
    }
}