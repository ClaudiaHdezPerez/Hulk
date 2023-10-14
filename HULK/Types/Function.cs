namespace Hulk
{
    public class Function 
    {
        // Diccionario para guardar los valores de las funciones ya calculadas hasta el momento
        public static Dictionary<string, string> cache = new(); 

        // Diccionario para guardar las llamadas a las funciones de cada entrada, para controlar recursividad
        public static Dictionary<string, int> calls = new();
        
        // Valores de los argumentos por defecto
        public string arg1 = "";
        public string arg2 = Math.E.ToString();

        // Diccionario con las funciones predefinidas por el lenguaje
        public Dictionary<string, string> predFunctions = new();
        // Diccionario con las funciones que se van creando como llave y como valor su 'cuerpo'
        public static Dictionary<string, string> bodyFunction = new() {{"fib(", "if (n > 1) fib(n-1) + fib(n-2) else 1"}}; 
        // Diccionario con las funciones que se van creando como llave y como valor sus variables
        public static Dictionary<string, List<string>> variables = new() {{"fib(", new() {"n"}}};
        // Diccionario que contiene el tipo que devuelve cada función creada
        public static Dictionary<string, string> output = new() {
            {"print(", "all"}, {"cos(", "number"}, {"sin(", "number"}, {"tg(", "number"}, {"sqrt(", "number"}, 
            {"log(", "number"}, {"rand(", "number"}, {"exp(", "number"}, {"fib(", "number"}
        };
        // Diccionario que contiene el tipo que recibe cada variable de las funciones creadas
        public static Dictionary<string, List<string>> input = new() {
            {"print(", new() {"all"}}, {"cos(", new() {"number"}}, {"sin(", new() {"number"}}, {"tg(", new() {"number"}}, 
            {"sqrt(", new() {"number"}}, {"log(", new() {"number", "number"}}, {"rand(", new(){""}}, 
            {"exp(", new() {"number"}}, {"fib(", new() {"number"}}
        };
        // Lista que contiene las palabras reservadas del lenguaje
        public static List<string> keyWords = new() {
            "True", "False", "true", "false", "function", "if", "elif", "else", "string", "number",
            "boolean", "let", "in", "PI", "E", "print"
        };
        // Lista que contiene los nombres de todas las funciones que existen 
        public static List<string> existFunctions = new() {
            "cos(", "sin(", "tg(", "sqrt(", "log(", "rand(", "exp(", "fib("
        };
        public Function(string[] s) {

            if (s.Length == 1) arg1 = s[0];

            else {
                arg1 = s[1];
                arg2 = s[0];
            }

            // Aquí se precalculan los valores de las funciones predefinidas
            if (!Error.error) {
                predFunctions["cos("]  = Math.Cos(double.Parse(arg1)).ToString();
                predFunctions["sin("]  = Math.Sin(double.Parse(arg1)).ToString();
                predFunctions["tg("]  = Math.Tan(double.Parse(arg1)).ToString();
                predFunctions["sqrt("] = Math.Sqrt(double.Parse(arg1)).ToString();
                predFunctions["log("]  = Math.Log(double.Parse(arg1), double.Parse(arg2)).ToString();
                predFunctions["exp("]  = Math.Pow(Math.E, double.Parse(arg1)).ToString();
            } 
        } 

        public static bool IsFunction(string s) {
            // Método para verificar si una expresión es una función
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = Aux.FunctionsOut(s);
            s = s.Trim();

            return s == "";
        }

        public static bool Declaration(string s) {
            // Método para saber si una expresión es una declaración de una función
            s = s.Trim();
            while (s.StartsWith("(") || s.StartsWith(" ")) s = s.Remove(0, 1); 
            return s.StartsWith("function ");
        }

        public static string Create(string s) {
            // Método para crear la función
            int count = 0;

            // Se verifica si la instrucción empieza con '(', en ese caso se eliminan todos y se 
            // cuentan cuantos tenía para eliminarlos al final
            while (s.StartsWith("(")) {
                s = s.Remove(0, 1); 
                s = s.TrimStart();
                count ++;
            }

            s = Aux.SpacesOut(s)[8..];
            // Aquí se quitan los ')'
            s = s.Remove(s.Length - count);

            // Si después de quitar los caracteres donde debían estar los paréntesis del final el
            // 'cuerpo' de la función queda 'desbalanceado' es porque desde un inicio se declaró mal
            if (Aux.ParenthesisBalance(s) != 0) {
                if (!Error.Syntax($"Invalid 'function' instruction")) return "";
            }

            // Se revisa que la declaración no tenga errores
            if (!Error.FunctionGeneral(s)) return "";
  
            // Llegado a este punto representa una función válida y entonces será creada

            // Se guarda el nombre de la función de la forma "name("
            string funcName = Aux.SpacesOut(s[..(s.IndexOf("(") + 1)]);
            // Se delimitan las variables y son separadas por ','
            string argument = s.Substring(s.IndexOf("(") + 1, s.IndexOf(")") - s.IndexOf("(") - 1);
            List<string> vars = argument.Split(",").ToList();
            // Finalmente, se guarda el 'cuerpo' de la función
            string body = Aux.SpacesOut(s[(s.IndexOf("=>") + 2)..]);

            for (int i = 0; i < vars.Count; i++) vars[i] = vars[i].Trim();

            // Aquí se guardan todos los registros necesarios 
            bodyFunction[funcName] = body;
            variables[funcName] = vars;
            existFunctions.Add(funcName);
            if (output[funcName] == "all") output[funcName] = Aux.FunctionOutputType(body, vars);

            return "";     
        }

        public static string Eval(string s, string[] args) {
            // Se guarda la expresión sin strings para la búsqueda de índices
            string s_WithOutStrings = Aux.StringOut(s);

            int index1 = s_WithOutStrings.IndexOf('(');
            string argument = string.Join(", ", args);
            string f = s[..(index1 + 1)];

            // Se verifica que la función sea creada
            if (bodyFunction.ContainsKey(f)) {
                // En ese caso se verifica si el valor está contenido en 'cache' para en 
                // caso positivo se obtenga el valor de la evaluación de forma inmediata 
                if(cache.ContainsKey($"{f}{argument})")) 
                return cache[$"{f}{argument})"];

                // Si no fue así, entonces se calculará su valor 
                return Sustitution(bodyFunction[f], variables[f], args.ToList(), f);
            }

            // Se verifica si no tiene errores con respecto a los argumentos, que se haya recibido
            // el tipo que se esperaba y más especificaciones con respecto a las predefinidas
            if(!Error.Restrictions(f, argument, args)) return "";

            if (f == "log(") {
                // El argumento tiene que ser mayor que 0
                if (args.Length == 1 && double.Parse(args[0]) <= 0) {
                    Error.Semantic($"Argument must be greater than '0' in 'log' function");
                    return "";
                }

                if (args.Length > 1) {
                    if (double.Parse(args[1]) <= 0) {
                        Error.Semantic($"Argument must be greater than '0' in 'log' function");
                        return "";
                    }
                    
                    // y la base mayor que cero y distinta de 1
                    if (double.Parse(args[0]) <= 0 || double.Parse(args[0]) == 1) {
                        Error.Semantic($"Base must be greater than '0' and diferent of '1' in 'log' function");
                        return "";
                    }
                }
            }

            if (f == "sqrt(") {
                // El argumento de la función no puede ser negativo
                if (double.IsNegative(double.Parse(argument))) {
                    Error.Semantic($"Argument must be greater than or equal to '0' in 'sqrt' function");
                    return "";
                }
            }
            
            // Si la función es 'rand' se crea el objeto 'random' de tipo Random y se usa el
            // método 'NextDouble()' que devuelve un número random entre 0 y 1
            if (f[..^1] == "rand") {
                Random random = new();
                return random.NextDouble().ToString();         
            }

            // Si llegó a este punto entonces se crea el objeto 'pred' para calcular los valores
            // en las funciones predefinidas
            Function pred = new(args);
            // y luego devolverlo
            return pred.predFunctions[f];            
        }
        
        public static string Sustitution(string body, List<string> vars, List<string> values, string funcName = "") {
            
            if (funcName != "" && !Error.ArgumentCount(vars, values, funcName)) return "";
            if (funcName != "" && !Error.Restrictions(funcName, string.Join(", ", values), values.ToArray())) return "";

            // Se guarda la expresión sin strings para la búsqueda de índices
            string body_WithOutStrings = Aux.StringOut(body);
            string[] symbols = {
                "*", "/", "^", "%", "+", "-", "(", ")", ">", "<", "&","|","!", ",", "@", "=", "\"", " "
            };
            
            for (int i = 0; i < vars.Count; i++)
            {
                // Si el 'cuerpo' contiene a la variable 
                if (body_WithOutStrings.Contains(vars[i])) {
                    List<string> tokens = body_WithOutStrings.Split(symbols, StringSplitOptions.RemoveEmptyEntries).ToList();
                    tokens.RemoveAll(string.IsNullOrWhiteSpace);
                    // se guarda cada elemento del 'cuerpo', a excepción de los símbolos
                    string[] newTokens = new string[tokens.Count];

                    for (int j = 0; j < tokens.Count; j++)
                    {
                        // si lo que se guardó en el array es la variable entonces se guarda su valor
                        newTokens[j] = (Aux.SpacesOut(tokens[j]) == vars[i])? $"({values[i]})" : tokens[j];
                    }
                    
                    int position = body_WithOutStrings.Length;

                    for (int j = tokens.Count - 1; j >= 0; j--)
                    {
                        // aquí se busca la posición, de atrás para delante, de los elementos del 
                        // array para q sean sustituidos por sí mismos o por su valor, en caso de 
                        // las variables
                        position = body_WithOutStrings[..position].LastIndexOf(tokens[j]);
                        body = body.Remove(position,tokens[j].Length);
                        body = body.Insert(position, newTokens[j]);
                    } 
                    
                    body_WithOutStrings = Aux.StringOut(body);
                }
            }

            // Finalmente se devuelve el 'cuerpo' modificado
            return body;
        }
    }
}