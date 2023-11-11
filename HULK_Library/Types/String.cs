using System;

namespace Hulk_Library
{
    public class String
    {
        static readonly char[] scapes = {'n', 'r', 't', 'a', 'f', 'b', 'v', '"', '\'', '\\'};
        static readonly char[] scapeSlash = {'\n', '\r', '\t', '\a', '\f', '\b', '\v'};

        public static bool IsString(string s) {
            // Método para saber si una expresión es un string
            s = Aux.StringOut(s);
            s = s.Replace(" ","");
        
            return s == "\"\"" || s.Contains("@");
        }

        public static string Eval(string s)
        {
            // Se busca el índice del operador '@' sin los strings
            string n = Aux.StringOut(s);
            
            if (n.Contains("@")) {
                // Se busca el último índice 
                int index = n.LastIndexOf("@");
                // y se verifica que no tenga errores
                if (!Error.Operations(s[..index], s[(index + 1)..], "@")) return "";
                
                return Concatenate(Control.Analize(s[..index]), Control.Analize(s[(index + 1)..]));
            }
            // Si no tiene ese operdor, es un string 'puro' y se devuelve de inmediato
            return s;
        }

        public static string TraduceString(string s) {
            // Este método traduce la entrada para que C# imprima los caracteres deseados, 
            // incluyendo los códigos '\n', '\r', '\t', '\a', '\f', '\b', '\v', '"', ''', '\'
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
                        // En caso de que no sea ninguno de los códigos 'scapes' es un error
                        string mssg = string.Join(", ", scapes);
                        if (!Error.Syntax($"Scape sequency not allowed \nTry using those: {mssg}")) return "";
                    }
                }
                
                index = s.IndexOf("\\", index + count / 2);
            }
            
            return s;
        }
    
        public static string Concatenate(string leftSide, string rightSide) {
            // Método para concatenar strings con cualquier otro tipo del lenguaje 
            if (rightSide == "" || leftSide == "") return "";
            
            leftSide = leftSide.EndsWith("\"")? leftSide.Remove(leftSide.Length - 1) : leftSide.Insert(0, "\"");
            rightSide = rightSide.StartsWith("\"")? rightSide.Remove(0, 1) : rightSide.Insert(rightSide.Length, "\"");

            return leftSide + rightSide;
        }
    }
}