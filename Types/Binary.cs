namespace Hulk
{
    public class Binary
    {   
        public static string ?result;
        public static char[] symbols = {'*', '/', '^', '%', '+', '-', '(', ')'};
        public static char[] symbols1 = {'*', '/', '^', '%', '+', '-'};
        
        public static bool IsBinary(string s) {
            string n = Aux.StringOut(s);

            return Aux.IsNumber(s) || symbols1.Any(n.Contains);
        }

        public static string Eval(string s)
        {
            if (Error.BodyDetails(s)) 
            {
                string leftSide;
                string rightSide;
                string operation;

                s = Aux.StringOut(s);
                s = Aux.InternalSpaces(s);
                s = Aux.Constants(s);

                while (s.Contains("+-") || s.Contains("-+") || s.Contains("++") || s.Contains("--")) {
                    s = s.Replace("+-","-");
                    s = s.Replace("-+","-");
                    s = s.Replace("++","+");
                    s = s.Replace("--","+");
                }   
                
                if (Aux.IsNumber(s)) return double.Parse(s).ToString();
                
                if (s[1..].Contains('+') || s[1..].Contains('-')) {

                    int index = Math.Max(s.LastIndexOf('+'), s.LastIndexOf('-'));
                    

                    while (index > 1 && s[index - 1] == 'E' && char.IsDigit(s[index - 2]) ||
                        (index > 0 && symbols.Contains(s[index - 1]))) {

                        index = Math.Max(s[..index].LastIndexOf('+'), s[..index].LastIndexOf('-'));
                    }

                    if (index > 0) {
                        leftSide = s[..index];
                        rightSide = s[index..];
                        operation = s[index].ToString();

                        if (Error.Operations(leftSide, rightSide[1..], operation)) {
                        
                            return Add(Eval(leftSide), Eval(rightSide));
                        }

                        return "";
                    }
                }

                if (s.Contains('*') || s.Contains('/') || s.Contains('%')) {
                    int index = Math.Max(s.LastIndexOf('*'), Math.Max(s.LastIndexOf('/'),s.LastIndexOf('%')));
                    leftSide = s[..index];
                    rightSide = s[(index + 1)..];
                    operation = s[index].ToString();

                    if (s[index] == '*') {
                        if (Error.Operations(leftSide, rightSide, operation)) {
                        
                            return Mult(Eval(leftSide), Eval(rightSide));
                        }
                    } 

                    if (s[index] == '/') {

                        if (Error.Operations(leftSide, rightSide, operation)) {
                        
                            return Divide(Eval(leftSide), Eval(rightSide));
                        }
                    }

                    if (s[index] == '%') {
                        if (Error.Operations(leftSide, rightSide, operation)) {
                        
                            return Mod(Eval(leftSide), Eval(rightSide));
                        }
                    }
                } 
            
                if (s.Contains("^")) {
                    int index = s.LastIndexOf('^');
                    leftSide = s[..index];
                    rightSide = s[(index + 1)..];
                    string sign = "";

                    if (leftSide.StartsWith('-')) {
                        sign = "-";
                        leftSide = leftSide[1..];
                    }

                    if (Error.Operations(leftSide, rightSide, "^")) {
                    
                        return Eval(sign + Power(Eval(leftSide), Eval(rightSide)));
                    }
                }
            }
            
            return "";
        }
    
        public static string Add(string leftSide, string rightSide) {

            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(rightSide) + double.Parse(leftSide)).ToString();
        }

        public static string Mult(string leftSide, string rightSide) {

            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(rightSide) * double.Parse(leftSide)).ToString();
        }

        public static string Divide(string leftSide, string rightSide) {
            
            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(leftSide) / double.Parse(rightSide)).ToString();
        }

        public static string Mod(string leftSide, string rightSide) {

            if(rightSide == "" || leftSide == "") return "";

            return (double.Parse(leftSide) % double.Parse(rightSide)).ToString();
        }

        public static string Power(string leftSide, string rightSide) {

            if(rightSide == "" || leftSide == "") return "";

            return Math.Pow(double.Parse(leftSide), double.Parse(rightSide)).ToString();
        }
    }
}