using System;

namespace Hulk_Library
{
    public class Boolean 
    {
        public static bool IsBoolean(string s) {
            // Método para determinar si una expresión es booleana
            s = Aux.StringOut(s);
            s = s.Replace("(","");
            s = s.Replace(")","");

            return bool.TryParse(s, out bool _) || s.Contains("<") || s.Contains(">") ||
                    s.Contains("==") || s.Contains("!=") || s.Contains("<") || s.Contains("!") ||
                    s.Contains("&") || s.Contains("|");
        }

        public static string Eval(string s) 
        {
            // Primero se verifica que la expresión no tenga errores
            if (Error.BodyDetails(s))
            { 
                // Se guarda la expresión sin strings
                string n = Aux.StringOut(s);

                // Si la expresión es "true" o "false", se devuelve inmediatamente
                if (bool.TryParse(s, out bool val)) return val.ToString();

                // Se busca el 'y' o el 'o', el último de estos índices 
                if (n.Contains("&") || n.Contains("|")) {
                    int index = Math.Max(n.LastIndexOf("&"), n.LastIndexOf("|"));
                    char operation = s[index];

                    return (operation == '&')? And(Eval(s[..index]), Eval(s[(index + 1)..])) : Or(Eval(s[..index]), Eval(s[(index + 1)..]));
                }

                // Se busca la igualdad o la diferencia, el último de estos índices
                if (n.Contains("==") || n.Contains("!=")) {
                    int symbol = Math.Max(n.LastIndexOf("=="), n.LastIndexOf("!="));

                    if (n[symbol..(symbol + 2)] == "==") {
                        return Equal(Control.Analize(s[..symbol]), Control.Analize(s[(symbol + 2)..]));
                    }

                    if (n[symbol..(symbol + 2)] == "!=") {
                        return NotEqual(Control.Analize(s[..symbol]), Control.Analize(s[(symbol + 2)..]));
                    }

                }

                // Se busca el mayor o igual, menor o igual, el menos o el mayor, el último de estos índices
                if (n.Contains("<=") || n.Contains(">=") || n.Contains(">") || n.Contains("<")) {
                    int symbol = Math.Max(Math.Max(n.LastIndexOf("<="), n.LastIndexOf(">=")), 
                                          Math.Max(n.LastIndexOf(">"), n.LastIndexOf("<")));

                    if (n[symbol..(symbol + 2)] == "<=") {
                        return LessEqual(Control.Analize(s[..symbol]), Control.Analize(s[(symbol + 2)..]));
                    }

                    if (n[symbol..(symbol + 2)] == ">=") {
                        return GreatEqual(Control.Analize(s[..symbol]), Control.Analize(s[(symbol + 2)..]));
                    }
                
                    if (n[symbol] == '>') {
                        return GreatThan(Control.Analize(s[..symbol]), Control.Analize(s[(symbol + 1)..]));
                    }
                
                    if (n[symbol] == '<') {
                        return LessThan(Control.Analize(s[..symbol]), Control.Analize(s[(symbol + 1)..]));
                    }                
                
                }

                // Por último el signo de 'no'
                if (n.Contains("!")) {
                    int index = n.LastIndexOf("!");

                    return Not(Eval(s[(index + 1)..]));
                }

                return Control.Analize(s);
            }

            return "";
        }
    
        public static string And(string leftSide, string rightSide) {
            // Método para evaluar el 'y' lógico
            if (rightSide == "" || leftSide == "") return "";

            return (bool.Parse(leftSide) && bool.Parse(rightSide)).ToString();
        }

        public static string Or(string leftSide, string rightSide) {
            // Método para evaluar el 'o' lógico
            if (rightSide == "" || leftSide == "") return "";

            return (bool.Parse(leftSide) || bool.Parse(rightSide)).ToString();
        }

        public static string Not(string val) {
            // Método para evaluar el 'no' lógico
            if (val == "") return "";

            return (!bool.Parse(val)).ToString();
        }

        public static string Equal(string leftSide, string rightSide) {
            // Método para evaluar la igualdad
            if (rightSide == "" || leftSide == "") return "";
            
            return (leftSide == rightSide).ToString();
        }

        public static string NotEqual(string leftSide, string rightSide) {
            // Método para evaluar la desigualdad
            if (rightSide == "" || leftSide == "") return "";
            
            return (leftSide != rightSide).ToString();
        }

        public static string GreatThan(string leftSide, string rightSide) {
            // Método para evaluar el 'mayor que'
            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(leftSide) > double.Parse(rightSide)).ToString();
        }

        public static string GreatEqual(string leftSide, string rightSide) {
            // Método para evaluar el 'mayor o igual'
            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(leftSide) >= double.Parse(rightSide)).ToString();
        }

        public static string LessThan(string leftSide, string rightSide) {
            // Método para evaluar el 'menor que'
            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(leftSide) < double.Parse(rightSide)).ToString();
        }

        public static string LessEqual(string leftSide, string rightSide) {
            // Método para evaluar el 'menor o igual'
            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(leftSide) <= double.Parse(rightSide)).ToString();
        }
    }
}