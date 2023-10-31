using System.Text.RegularExpressions;

namespace Hulk
{
    public class If_else       
    {
        static readonly string[] symbols = {
            "*", "/", "^", "%", "+", "-", "(", ")", ">", "<", "&","|","!", ",", "=", " ", "@"
        };
       
        public static bool IsIf_else(string s) {
            // Se considerará una expresión condicional si contiene un 'if'
            s = Aux.StringOut(s);
            string[] token = s.Split(symbols, StringSplitOptions.RemoveEmptyEntries);
            return token.Contains("if");
        }

        public static string Eval(string s) {
            // Primero se revisa si la sintaxis de la expresión es válida
            (bool, string, string, string, int, int, string) conditionalData = Error.Correct_if(s);

            // Si el 'item1' es true es porque pasó todas las revisiones y es correcto,
            // de forma contraria se devolverá ""
            if (!conditionalData.Item1) return "";

            // Si llega hasta aquí es porque no tuvo errores y se guardarán los valores para operar con ellos después
            
            // Esta es la condición que se evaluará para saber cuál es el 'body' que hay que devolver
            bool condition = bool.Parse(Control.Analize(conditionalData.Item2));

            // Estos son los cuerpos de la condicional que estarán delimitados de la siguiente manera:
            // Después de la condición y antes del 'else'
            string body_true = conditionalData.Item3;
            // Después del 'else' hasta el final de la expresión
            string body_false = conditionalData.Item4;

            // Este índice marca el inicio de la condicional en la expresión 
            int start = conditionalData.Item5;
            // Mientras este marca el final
            int end = conditionalData.Item6;

            // Esto devuelve la expresión, dado que internamente se alteran los índices
            s = conditionalData.Item7;

            // Aquí se comprueba que lo que antecede a la condicional sea correcto
            if (s[..(start + 1)].Trim() != "" && !symbols.Contains(s[..(start + 1)][^1].ToString())) {
                if ((s[..(start + 1)].Length < 4 || (s[..(start + 1)].Trim()[^4..] != "else"))
                && !Error.Syntax("Unexpected expression before 'if-else' instruction")) return "";
            }

            // Evaulando la condición, se reemplazará el 'body' esperado
            string body = condition? body_true : body_false;

            if(string.IsNullOrEmpty(body)) return "";

            // En este punto se sustituye la condicional por el 'body' que se debe devolver 
            s = s.Remove(start + 1, end - start - 1);
            body = Control.Analize(body);
            s = s.Insert(start + 1, $"{body}");

            // Luego se analiza la expresión general
            return Control.Analize(s);
        }
    }
}