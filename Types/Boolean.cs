namespace Hulk
{
    public class Boolean 
    {
        public static string ?result;

        public static bool IsBoolean(string s) {
            s = Aux.StringOut(s);
            s = s.Replace("(","");
            s = s.Replace(")","");

            return bool.TryParse(s, out bool _) || s.Contains("<") || s.Contains(">") ||
                    s.Contains("==") || s.Contains("!=") || s.Contains("<") || s.Contains("!") ||
                    s.Contains("&") || s.Contains("|");
        }

        public static string Eval(string s) 
        {
            if (Error.BodyDetails(s))
            { 
                string n = Aux.StringOut(s);

                if (bool.TryParse(s, out bool val)) return val.ToString();

                if (n.Contains("&") || n.Contains("|")) {
                    int index = Math.Max(n.LastIndexOf("&"), n.LastIndexOf("|"));
                    char operation = s[index];

                    return (operation == '&')? And(Eval(s[..index]), Eval(s[(index + 1)..])) : Or(Eval(s[..index]), Eval(s[(index + 1)..]));
                }

                if (n.Contains("==")) {
                    int index = n.LastIndexOf("==");

                        return Equal(Control.Analize(s[..index]), Control.Analize(s[(index + 2)..]));
                }

                if (n.Contains("!=")) {
                    int index = n.LastIndexOf("!=");

                        return NotEqual(Control.Analize(s[..index]), Control.Analize(s[(index + 2)..]));
                }

                if (n.Contains("<=")) {
                    int index = n.LastIndexOf("<=");

                    return LessEqual(Eval(s[..index]), Eval(s[(index + 2)..]));
                }

                if (n.Contains(">=")) {
                    int index = n.LastIndexOf(">=");
                    
                        return GreatEqual(Eval(s[..index]), Eval(s[(index + 2)..]));
                }

                if (n.Contains(">")) {
                    int index = n.LastIndexOf(">");
                
                    return GreatThan(Eval(s[..index]), Eval(s[(index + 1)..]));
                }

                if (n.Contains("<")) {
                    int index = n.LastIndexOf("<");

                    return LessThan(Eval(s[..index]), Eval(s[(index + 1)..]));
                }

                if (n.Contains("!")) {
                    int index = n.LastIndexOf("!");

                    return Not(Eval(s[(index + 1)..]));
                }

                return Control.Analize(s);
            }

            return "";
        }
    
        public static string And(string leftSide, string rightSide) {

            if (rightSide == "" || leftSide == "") return "";

            return (bool.Parse(leftSide) && bool.Parse(rightSide)).ToString();
        }

        public static string Or(string leftSide, string rightSide) {

            if (rightSide == "" || leftSide == "") return "";

            return (bool.Parse(leftSide) || bool.Parse(rightSide)).ToString();
        }

        public static string Not(string val) {

            if (val == "") return "";

            return (!bool.Parse(val)).ToString();
        }

        public static string Equal(string leftSide, string rightSide) {

            if (rightSide == "" || leftSide == "") return "";
            
            return (leftSide == rightSide).ToString();
        }

        public static string NotEqual(string leftSide, string rightSide) {

            if (rightSide == "" || leftSide == "") return "";
            
            return (leftSide != rightSide).ToString();
        }

        public static string GreatThan(string leftSide, string rightSide) {

            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(leftSide) > double.Parse(rightSide)).ToString();
        }

        public static string GreatEqual(string leftSide, string rightSide) {

            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(leftSide) >= double.Parse(rightSide)).ToString();
        }

        public static string LessThan(string leftSide, string rightSide) {

            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(leftSide) < double.Parse(rightSide)).ToString();
        }

        public static string LessEqual(string leftSide, string rightSide) {

            if (rightSide == "" || leftSide == "") return "";

            return (double.Parse(leftSide) <= double.Parse(rightSide)).ToString();
        }
    }
}