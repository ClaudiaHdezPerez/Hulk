namespace Hulk
{
    public class Binary
    {   
        public static List<char> symbols = new() {'*', '/', '^', '%', '+', '-', '(', ')'};
        
        public static bool IsBinary(string s) {
            // Método para determinar si una expresión es binaria (numéricos)
            string n = Aux.StringOut(s);

            return Aux.IsNumber(s) || symbols.GetRange(0, 6).Any(n.Contains);
        }

        public static string Eval(string s)
        {
            // Primero se verifica que la expresión no tenga errores
            if (Error.BodyDetails(s)) 
            {
                string leftMember;
                string rightMember;

                // Se modifica la expresión completa para que no tenga strings, ni espacios 
                // internos entre los símbolos, los valores de PI y E son reemplazados, y
                // se sustituyen los símbolos '++', '+-', '-+', '--'
                s = Aux.StringOut(s);
                s = Aux.InternalSpaces(s);
                s = Aux.Constants(s);
                s = Aux.ReplaceSymbol(s); 
                
                // Se verifica si es un número, en ese caso se devuelve su valor inmediatamente
                if (Aux.IsNumber(s)) return double.Parse(s).ToString();
                
                // Luego se busca la suma o la resta, el último de estos índices
                if (s[1..].Contains('+') || s[1..].Contains('-')) {

                    int symbol = Math.Max(s.LastIndexOf('+'), s.LastIndexOf('-'));
                    
                    // Se revisa que el signo identificado como 'stop' no pertenezca a números de la 
                    // forma '#E+#' o '#E-#' 
                    while (symbol > 1 && s[symbol - 1] == 'E' && char.IsDigit(s[symbol - 2]) ||
                        (symbol > 0 && symbols.Contains(s[symbol - 1]))) {

                        symbol = Math.Max(s[..symbol].LastIndexOf('+'), s[..symbol].LastIndexOf('-'));
                    }

                    if (symbol > 0) {
                        leftMember = s[..symbol];
                        rightMember = s[symbol..];

                        return Add(Eval(leftMember), Eval(rightMember));
                    }
                }

                // Luego se busca la multiplicación, la división o el resto, el último de estos índices
                if (s.Contains('*') || s.Contains('/') || s.Contains('%')) {
                    int symbol = Math.Max(s.LastIndexOf('*'), Math.Max(s.LastIndexOf('/'),s.LastIndexOf('%')));
                    leftMember = s[..symbol];
                    rightMember = s[(symbol + 1)..];

                    if (s[symbol] == '*') {
                        return Mult(Eval(leftMember), Eval(rightMember));
                    } 

                    if (s[symbol] == '/') {
                        return Divide(Eval(leftMember), Eval(rightMember));
                    }

                    if (s[symbol] == '%') {
                        return Mod(Eval(leftMember), Eval(rightMember));
                    }
                } 
            
                // Finalmente, se busca la potencia
                if (s.Contains("^")) {
                    int symbol = s.LastIndexOf('^');
                    leftMember = s[..symbol];
                    rightMember = s[(symbol + 1)..];
                    string sign = "";

                    // Si el miembro izquierdo tiene un signo negativo lo que debe ser negativo
                    // es la evaulación de la potencia
                    if (leftMember.StartsWith('-')) {
                        sign = "-";
                        leftMember = leftMember[1..];
                    }
                   
                    return Eval(sign + Power(Eval(leftMember), Eval(rightMember)));
                }
            }
            
            return "";
        }
    
        public static string Add(string leftSide, string rightSide) {
            // Método para calcular una suma o una resta
            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(rightSide) + double.Parse(leftSide)).ToString();
        }

        public static string Mult(string leftSide, string rightSide) {
            // Método para calcular una multiplicación
            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(rightSide) * double.Parse(leftSide)).ToString();
        }

        public static string Divide(string leftSide, string rightSide) {
            // Método para calcular una división
            if (rightSide == "" || leftSide == "") return "";

            // En el caso de la división no es posible si el miembro derecho es 0
            if (double.Parse(rightSide) == 0) {
                Error.Semantic("Division by 0 is not defined");
                return "";
            }

            return (double.Parse(leftSide) / double.Parse(rightSide)).ToString();
        }

        public static string Mod(string leftSide, string rightSide) {
            // Método para calcular el resto de una división
            if(rightSide == "" || leftSide == "") return "";

            // En el resto no es posible si el miembro derecho es 0
            if (double.Parse(rightSide) == 0) {
                Error.Semantic("Division by 0 is not defined");
                return "";
            }

            return (double.Parse(leftSide) % double.Parse(rightSide)).ToString();
        }

        public static string Power(string leftSide, string rightSide) {
            // Método para calcular una potencia
            if(rightSide == "" || leftSide == "") return "";

            return Math.Pow(double.Parse(leftSide), double.Parse(rightSide)).ToString();
        }
    }
}