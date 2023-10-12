using System.Data;
using System.Text.RegularExpressions;
namespace Hulk
{
    public class String
    {
        public static string ?result;
        static readonly char[] scapes = {'n', 'r', 't', 'a', 'f', 'b', 'v', '"', '\'', '\\'};
        static readonly char[] scapeSlash = {'\n', '\r', '\t', '\a', '\f', '\b', '\v'};

        public static bool IsString(string s) {
            s = Aux.StringOut(s);
            s = s.Replace(" ","");
        
            return s == "\"\"" || s.Contains("@");
        }

        public static string Eval(string s)
        {
            string n = Aux.StringOut(s);
            
            if (n.Contains("@")) {
                int index = n.LastIndexOf("@");

                if (!Error.Operations(s[..index], s[(index + 1)..], "@")) return "";
                
                return Concatenate(Control.Analize(s[..index]), Control.Analize(s[(index + 1)..]));
            }
            
            return s;
        }

        public static string TraduceString(string s) {

            if (Error.BodyDetails(s)) 
            {
                int index = s.IndexOf("\\"); 

                while (index != -1) {

                    int count = 0;

                    for (int i = index; i < s.Length; i++) 
                    {
                        if (s[i] != '\\') break;
                        count ++;
                    }

                    s = s.Remove(index, count / 2);
                    if (count % 2 != 0) {
                        int scapeIndex  = Array.IndexOf(scapes, s[index + count - count / 2]);
                        if (scapeIndex != -1)  {
                            s = s.Remove(index, 1);

                            if (!(scapes[scapeIndex] == '"' || scapes[scapeIndex] == '\'' || scapes[scapeIndex] == '\\')) {
                                s = s.Remove(index + count - count / 2 - 1, 1);
                                s = s.Insert(index + count - count / 2 - 1, scapeSlash[scapeIndex].ToString());
                            }
                        }

                        else {
                            string mssg = string.Join(", ", scapes);
                            if (!Error.Syntax($"Scape sequency not allowed \nTry using those: {mssg}")) return "";
                        }
                    }
                    
                    index = s.IndexOf("\\", index + count / 2);
                }
                
                return s;
                }
            return "";
        }
    
        public static string Concatenate(string leftSide, string rightSide) {
            
            if (rightSide == "" || leftSide == "") return "";
            
            leftSide = leftSide.EndsWith("\"")? leftSide.Remove(leftSide.Length - 1) : leftSide.Insert(0, "\"");
            rightSide = rightSide.StartsWith("\"")? rightSide.Remove(0, 1) : rightSide.Insert(rightSide.Length, "\"");

            return leftSide + rightSide;
        }
    }
}