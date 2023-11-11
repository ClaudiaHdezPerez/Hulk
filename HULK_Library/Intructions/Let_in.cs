using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hulk_Library
{
    public class Let_in 
    {
        private static readonly string[] symbols = {
            "*", "/", "^", "%", "+", "-", "(", ")", ">", "<", "&","|","!", ",", "=", " "
        };
        
        public static bool IsLet_in(string s) {
            // Para ser una declaración de variable bastará con que contanga 'let'
            s = Aux.StringOut(s);
            string[] token = s.Split(symbols, StringSplitOptions.RemoveEmptyEntries);
            return token.Contains("let");
        }

        public static string Eval(string s, bool function = false) {  
            // Primero se revisa si la sintaxis de la expresión es válida
            (bool, string, int, int, string, List<string>, List<string>) data = Error.Correct_Let(s, function);

            // Si el 'item1' es true es porque pasó todas las revisiones y es correcto,
            // de forma contraria se devolverá ""
            if (!data.Item1) return "";

            // Si llega hasta aquí es porque no tuvo errores y se guardarán los valores para operar con ellos después

            // Esto devuelve la expresión, dado que internamente se alteran los índices
            s = data.Item2;

            // Este índice marca el inicio de la declaración de variable en la expresión 
            int start = data.Item3;
            // Mientras este marca el final
            int stop = data.Item4;

            // Este es el 'body' que debe ser sustituido
            string body = data.Item5;
            // teniendo en cuenta estas variables locales
            List<string> vars = data.Item6;
            // y los valores asignados a ellas 
            List<string> values = data.Item7;

            // En este punto se sustituye la condicional por el 'body' que se debe devolver 
            s = s.Remove(start + 1, stop - start - 1);
            s = s.Insert(start + 1, $"({Function.Sustitution(body, vars, values)})");

            // Luego se analiza la expresión general en caso de que no esté dentro de una 
            // definición de función           
            return  function? s : Control.Analize(s);
        }
    }
}