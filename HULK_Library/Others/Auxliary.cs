using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hulk_Library

{
    public static class Aux
    {
        public static bool IsNumber(string s) {
            // Método para conocer si la expresión es un número
            if (s.Contains(",")) return false;

            return double.TryParse(s, out _) || s.Trim() == "PI" || s.Trim() == "E";
        }

        public static string Constants(string s) {
            // Este método ha sido implementado para sustituir los valores de PI y E 
            char[] symbols = {'*', '/', '^', '%', '+', '-', '(', ')'};

            // Se eliminan los strings que pueda tener la expresión para no modificarlos
            string s_WithoutStrings = StringOut(s);
            string s_WithoutSpaces = s_WithoutStrings.Replace(" ","");
            // Se hace 'split' por los símbolos 
            string[] operands = s_WithoutSpaces.Split(symbols, StringSplitOptions.RemoveEmptyEntries);
        
            if (operands.Contains("PI") || operands.Contains("E")) {
            
                string covert = string.Join(" ", operands);
                covert = covert.Insert(0, " ");
                covert = covert.Insert(covert.Length, " ");
                covert = covert.Replace(" PI ", " " + Math.PI + " "); 
                covert = covert.Replace(" E ", " " + Math.E + " ");
                string[] newOperands = covert.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                int position = s.Length;

                // Luego de modificarse los valores se insertan en la entrada original para devolverla
                for (int i = operands.Length - 1; i >= 0; i--)
                {
                    position = s_WithoutStrings[..position].LastIndexOf(operands[i]);
                    s = s.Remove(position, operands[i].Length);
                    s = s.Insert(position, newOperands[i]);
                } 
            }

            return s;
        }

        public static bool StringBalance(string s) {
            // Los strings estarán correctamente balanceados si tienen una cantidad par de comillas 
            int d = 0;
            int count = 0;
            int index = s.IndexOf("\"");

            while (index != -1) {

                for (int i = index; i > 0; i--) 
                {
                    if (s[i - 1] != '\\') break; 
                    count ++;
                }

                // La variable 'count' lleva la cuenta de los caracteres '\' delante de una comilla
                // dado que una cantidad par de ellos generan una comilla "válida"
                if (count % 2 == 0) d++;
                count = 0;
                index = s.IndexOf("\"", index + 1);  
            }

            return d % 2 == 0;
        }

        public static int ParenthesisBalance(string s) {
            s = StringOut(s);
            // Una expresión estará correctamente balanceada con respecto a los paréntesis si tiene
            // la misma cantidad de paréntesis abiertos que de cerrados, siempre que estén en correspondencia
            // (abierto-cerrado)

            if (!s.Contains('(') && !s.Contains(')')) return 0;

            static int ParenthesisBalance(string s, int i, int count = 0) {

                if (s[i] == '(') count++;
                else if (s[i] == ')') count--;
                
                // En el caso de que falte el paréntesis abierto la cuenta será negativa 
                if (count < 0) return 1;
                // Si se llegó al final si la cuenta es 0 estuvo correctamente balanceado, sino le faltó
                // un cerrado
                if (s.Length - 1 == i) return (count == 0)? 0 : -1;

                return ParenthesisBalance(s, i + 1, count);
            }

            return ParenthesisBalance(s, 0);
        }

        public static string Convert_Elif(string s) {
            // Este método convierte un 'else if' a un 'elif' 
            string s_WithoutStrings = StringOut(s);
            string s_WithoutSymbols = Regex.Replace(s_WithoutStrings, @"[^_""ñÑ,A-Za-z0-9]", " ");

            // Se busca el índice del else y del if siguiente a él  
            int index_1 = s_WithoutSymbols.IndexOf(" else ");
            int index_2 = s_WithoutSymbols.IndexOf(" if ", index_1 + 1);
            
            while (index_1 != -1 && index_2 != -1) {

                // Se verifica si el espacio entre las palabras es vacío, en ese caso se transforma
                //  'else if' eliminando del 'else', '-se', y los espacios de por medio ('elif')  
                if (string.IsNullOrWhiteSpace(s[(index_1 + 5)..(index_2 + 1)])) {
                    s = s.Remove(index_1 + 3, index_2 - index_1 - 2);
                }

                s_WithoutStrings = StringOut(s);
                s_WithoutSymbols = Regex.Replace(s_WithoutStrings, @"[^_""ñÑ,A-Za-z0-9]", " ");

                // Y se repite el proceso hasta que alguno de los dos dejen de existir en la expresión
                index_1 = s_WithoutSymbols.IndexOf(" else ", index_1 + 1);
                index_2 = s_WithoutSymbols.IndexOf(" if ", index_1 + 1);
            }

            return s;
        }

        public static (bool, string, string[]) FunctionInfo(string s, int index, int index2) {   
            string n = StringOut(s);

            // Se verifica si antes del paréntesis tiene caracteres válidos para ser una función
            if (index > 0 && (char.IsLetterOrDigit(n[index - 1]) || n[index - 1] == '_')) {
                int start = -1;

                for (int k = index - 1; k >= 0; k--) {
                    // Con este índice se va a iterar hasta donde la función deje de tener un nombre válido
                    if (!char.IsLetterOrDigit(n[k]) && n[k] != '_') {
                        start = k;
                        break;
                    }
                }

                // 'f' será la variable que tenga la función completa, delimitada desde start hasta el 
                // índice del paréntesis cerrado
                string f = s[(start + 1)..(index2 + 1)];
                // El argumento será lo que esté dentro de los paréntesis
                string argument = f[(f.IndexOf("(") + 1)..^1];
                string argument_WithOutStrings = StringOut(argument);
                int tempIndex = argument_WithOutStrings.IndexOf(",");
                List<string> values = new();
        
                // Se hace un 'split' manual para iterar sobre el argumento sin strings en busca de las 
                // ',', pero guardando los valores con strings  
                while (tempIndex != -1) {
                    values.Add(argument[..tempIndex]);
                    argument = argument.Remove(0, tempIndex + 1);
                    argument_WithOutStrings = argument_WithOutStrings.Remove(0, tempIndex + 1);
                    tempIndex = argument_WithOutStrings.IndexOf(",");
                }

                values.Add(argument);
                string[] arg = values.ToArray();
                string funcName = f[..(f.IndexOf("(") + 1)];

                // Se devuelve 'true' porque fue función, 'funcName' el nombre de la función y 'arg' 
                // los argumentos que recibió la función
                return (true, funcName, arg);
            }
            // Se retornará false porque no fue identificada como función
            return (false, "", new string[0]);
        }

        public static string SpacesOut(string s) {
            // Para eliminar todos los espacios en blanco del principio y el final de la expresión
            s = s.Trim();
            
            int index = s.IndexOf("(");
            
            index = (index == 0)? s.IndexOf("(", index + 1) : index;
            // Para eliminar los espacios antes de los paréntesis abiertos
            while (index != -1) {
                if (StringBalance(s[index..]) && s[index - 1] == ' ') {
                    s = s.Remove(index - 1, 1);
                    index--;
                }
                else index = s.IndexOf("(", index + 1);
            }

            index = s.IndexOf(")");
            // Para eliminar los espacios antes de los paréntesis cerrados
            while (index != -1 && index != 0) {
                if (StringBalance(s[index..]) && s[index - 1] == ' ') {
                    s = s.Remove(index - 1, 1);
                    index--;
                }
                else index = s.IndexOf(")", index + 1);
            }
            // Se retorna el string modificado
            return s;
        }

        public static string InternalSpaces(string s) {
            // Este método se encarga de eliminar los espacios entre los símbolos numéricos 
            char[] symbols = {'+', '-', '*', '/', '%', '^'};

            s = SpacesOut(s);

            int index = s.IndexOfAny(symbols);

            while (index != -1 && index < (s.Length - 1))
            {
                while (s[index + 1] == ' ') 
                s = s.Remove(index + 1, 1);

                while (index > 0 && s[index - 1] == ' ') {
                    s = s.Remove(index - 1, 1);
                    index--;
                }

                index = s.IndexOfAny(symbols, index + 1);
            }

            return s;
        }

        public static string StringOut(string s) {
            // Este es un método para 'anular' todo el contenido de un string para que no 
            // influya posteriormente en busca de índices o para que no sean modificados
            // Para ello se buscan los índices de las comillas (una abierta y la próxima a ella)
            // y con la longitud del string se crea uno con solo carcateres en blanco
            // No se remueve el contenido para poder mantener los índices de la expresión original

            int index1 = s.IndexOf("\"");
            int index2 = s.IndexOf("\"", index1 + 1);
            int count1 = 0;
            int count2 = 0;

            while (index1 != -1) {

                for (int i = index1; i > 0; i--) 
                {
                    if (s[i - 1] != '\\') break;
                    count1 ++;
                }

                for (int i = index2; i > 0; i--) 
                {
                    if (s[i - 1] != '\\') break;
                    count2 ++;
                }

                if (count1 % 2 == 0 && count2 % 2 == 0) {
                    
                    s = s[..(index1 + 1)] + s[index2..]; 
                    int dif = index2 - index1 - 1;
                    string spaces = new(' ', dif);
                    s = s.Insert(index1 + 1, spaces);

                    index1 = s.IndexOf("\"", index2 + 1);  
                    index2 = s.IndexOf("\"", index1 + 1);
                }

                else if (count1 % 2 != 0) { 
                    index1 = index2;
                    index2 = s.IndexOf("\"", index1 + 1);
                }

                else index2 = s.IndexOf("\"", index2 + 1);
                
                count1 = 0;
                count2 = 0;
            }

            return s;
        }

        public static string FunctionsOut(string s) {
            // Esto sigue la misma línea que el método de 'StringOut', pero con funciones
            // para trabajar con ',' sin afectar los argumentos de estas 

            string n = StringOut(s);
            int index = n.LastIndexOf("(");

            while (index != -1) {
                
                int index2 = n.IndexOf(")", index);

                if (index > 0 && (char.IsLetterOrDigit(n[index - 1]) || n[index - 1] == '_' || 
                    n[index - 1] == ' ')) {
                        
                    int start = -1;
                    
                    for (int k = index - 1; k >= 0; k--) 
                    {
                        if ((!char.IsLetterOrDigit(n[k]) && n[k] != '_' && n[k] != ' ') ||
                            (n[k] == ' ' && (char.IsLetterOrDigit(n[k + 1]) || n[k + 1] != '_'))) {

                            start = k;
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(n[(start + 1)..index])) {
                        n = RemoveCharacter(n, index, index2);
                    }

                    else {

                        string f = $"\"{s[(start + 1)..(index2 + 1)]}\"" ;
                        f = StringOut(f);
                        s = s.Remove(start + 1, index2 - start);
                        s = s.Insert(start + 1, f[1..(f.Length - 1)]);
                    }
                }

                else {
                    n = RemoveCharacter(n, index, index2);
                }

                index = n[..index].LastIndexOf("(");
            }

            return s;
        }

        public static string RemoveCharacter(string s, int index, int index2) {
            // Este método es para facilitar búsquedas de índices, si no es el índice que se
            // desee se remueve para que no intervenga en el 'IndexOf' 
            s = s.Remove(index, 1);
            s = s.Insert(index, " ");
            s = s.Remove(index2, 1);
            s = s.Insert(index2, " ");
            return s;
        }

        public static string ReplaceSymbol(string s) {
            // Método para reemplazar los símbolos '+-', '++', '-+', '--'
            while (s.Contains("+-") || s.Contains("-+") || s.Contains("++") || s.Contains("--")) {
                s = s.Replace("+-","-");
                s = s.Replace("-+","-");
                s = s.Replace("++","+");
                s = s.Replace("--","+");
            }  

            return s;
        }

        public static string Parenthesis(string s) {
            // En este método se evalúa lo que está dentro de los paréntesis o las funciones 

            s = SpacesOut(s);
            s = InternalSpaces(s);
            string n = StringOut(s);

            int index = n.LastIndexOf("(");
            int index2 = n.IndexOf(")", index);
            (bool, string, string[]) data = FunctionInfo(s, index, index2);

            // Se comprueba si es una función 
            if (data.Item1) {
                // Se guardan los valores devueltos por 'FunctionInfo'
                string funcName = data.Item2;
                int start = index + 1 - funcName.Length;
                string f = s[start..(index2 + 1)];
                string[] arg = data.Item3;
                
                for (int i = 0; i < data.Item3.Length; i++) arg[i] = Control.Analize(arg[i]);
        
                string argument = string.Join(", ", arg);  

                // Se comprueba que la función obtenida exista ya 
                if (Function.existFunctions.Contains(funcName)) {
                    // En ese caso se evalúa
                    string function = Function.Eval(f, arg);

                    if (function == "") return ""; // Si esto pasa es porque dio algún error interno
                    // Aquí se realiza el llenado del diccionario 'calls'
                    if (Function.calls.ContainsKey(funcName)) Function.calls[funcName]++; 
                    else Function.calls[funcName] = 1;

                    // Para hacer un 'control' y poder dar 'Stack Overflow' antes que se detenga el
                    // programa, se realiza un 'freno' cuando en una misma entrada cualquier función
                    // es llamada al menos 250 veces
                    if (Function.calls[funcName] == 250) {
                        Control.error = true;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("!!RUNTIME ERROR: Stack Overflow");
                        return "";
                    }
        
                    function = Control.Analize(function);
                    // Aquí se verifica si el valor de la función existe en el diccionario 'cache'
                    if (!Function.cache.ContainsKey($"{funcName}{argument})") && function != "") {
                        Function.cache[$"{funcName}{argument})"] =  function;    
                    }         
                    // Si ya existe como voy a usar un dato ya calculado no quiero generar una 
                    // llamada más al diccionario
                    else Function.calls[funcName]--;     
                    
                    return Control.Analize(s[..start] + $"({function})" + s[(index2 + 1)..]);
                }

                if (Control.error) return "";

                // Un caso especial es si es una instrucción 'print' 
                if (f.StartsWith("print(")) { 
                    // Se revisa si contiene solo un argumento
                    if (arg.Length > 1 && 
                    !Error.Semantic($"1 argument was expected but {arg.Length} were given in 'print' function"))
                    return "";
                    
                    // Luego se analiza lo que contiene el 'print'
                    return Control.Analize(s[..start] + Control.Analize(argument) + s[(index2 + 1)..]);
                }
                
                string mssg = double.TryParse(f[..f.IndexOf("(")], out _)? "is not a valid function name" : "is not a defined function";
                if (!Error.Syntax($"'{f[..f.IndexOf("(")]}' {mssg}")) return "";
            }
            // Esto es para resolver potencias con las bases dentro de paréntesis
            else if (s[(index2 + 1)..].Replace(" ", "").StartsWith("^")) {
                // Primero se revisa si tiene errores 
                if (Error.BodyDetails(s)) {
                    // Se reemplazan todos los símbolos '+-', '++', '-+', '--'
                    s = ReplaceSymbol(s);
                    char[] symbols = {'+', '-', '*', '/', '%', '&', '|', '!', '=', '>', '<', ')', '@', ','};

                    // Se busca el próximo signo que no sea propio del exponente
                    int stop = s.IndexOfAny(symbols, index2 + 3);
                    stop = stop == -1 ? s.Length : stop;

                    if (stop > 1) {
                        // Se revisa que el signo identificado como 'stop' no pertenezca a números de la 
                        // forma '#E+#' o '#E-#'
                        while(stop != s.Length && (s[stop] == '+' || s[stop] == '-') && s[stop - 1] == 'E' && char.IsDigit(s[stop - 2])) {
                            stop = s.IndexOfAny(symbols, stop + 1);
                            stop = stop == -1 ? s.Length : stop; 
                        }

                        // Se separan los miembros a evaluar
                        string leftSide = s[(index + 1)..index2];
                        string rightSide = s[(index2 + 2)..stop];

                        string pow = Binary.Power(Control.Analize(leftSide), Control.Analize(rightSide));
                        // y se insertan en la expresión
                        return Control.Analize(s[..index] + pow + s[stop..]);    
                    }
                }
            } 


            // De no ser así se evalúa el contenido del paréntesis y se inserta su valor en la expresión
            string parenthesis = Control.Analize(s[(index + 1)..index2]);

            // Si lo que se evalúa dentro del paréntesis es vacío se lanza un error 
            if (parenthesis == "" && !Error.Syntax($"Unexpected ')' after '('"))
            return "";
                
            return Control.Analize(s[..index] + parenthesis + s[(index2 + 1)..]);
        }

        public static string ExpressionType(string s) {
            // Este método es exclusivo para clasificar expresiones según el tipo
            
            if (String.IsString(s)) return "string";
            if (Boolean.IsBoolean(s)) return "boolean";
            if (Binary.IsBinary(s)) return "number";
            if (Function.IsFunction(s)) return Function.output[s[..(s.IndexOf("(") + 1)]];

            // De no ser reconocido por ninguno de los anteriores, no corresponde con 
            // un tipo válido del lenguaje
            return "invalid expression";
        }

        public static string FunctionOutputType(string body, List<string> vars) { 
            // Método para declarar el tipo que devuelve una función según los tipos del lenguaje
            string n = FunctionsOut(body);
            
            if (n.Contains('(')) {
                int index = n.LastIndexOf("(");
                int index2 = n.IndexOf(")", index);
                string parenthesis = body[(index + 1)..index2];
                
                return FunctionOutputType(body[..index] + parenthesis + body[(index2 + 1)..], vars);
            }

            if (vars.Any(x => x == body)) return "all";

            return ExpressionType(body);     
            
        }
    }
}