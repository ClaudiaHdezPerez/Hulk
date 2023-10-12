namespace Hulk
{
    public class Function 
    {
        public static Dictionary<string, string> cache = new(); 
        public static Dictionary<string, int> calls = new();
        public string arg1 = "";
        public string arg2 = Math.E.ToString();
        public Dictionary<string, string> predFunctions = new();
        public static Dictionary<string, string> functions = new() {{"f(", ""}}; 
        public static Dictionary<string, List<string>> variables = new() {{"f(", new() {"n"}}};  
        public static Dictionary<string, string> output = new() {
            {"print(", "all"}, {"cos(", "number"}, {"sin(", "number"}, {"tan(", "number"}, {"sqrt(", "number"}, 
            {"log(", "number"}, {"rand(", "number"}, {"exp(", "number"}, {"f(", "number"}
        };
        public static Dictionary<string, List<string>> input = new() {
            {"print(", new() {"all"}}, {"cos(", new() {"number"}}, {"sin(", new() {"number"}}, {"tan(", new() {"number"}}, 
            {"sqrt(", new() {"number"}}, {"log(", new() {"number", "number"}}, {"rand(", new(){""}}, 
            {"exp(", new() {"number"}}, {"f(", new() {"number"}}
        };
        public static List<string> keyWords = new() {
            "True", "False", "true", "false", "function", "if", "elif", "else", "string", "number",
            "boolean", "let", "in", "PI", "E", "print"
        };
        public static List<string> existFunctions = new() {
            "cos(", "sin(", "tan(", "sqrt(", "log(", "rand(", "exp(", "f("
        };
        public Function(string[] s) {

            if (s.Length == 1) arg1 = s[0];

            else {
                arg1 = s[1];
                arg2 = s[0];
            }

            if (!Error.error) {
                predFunctions["cos("]  = Math.Cos(double.Parse(arg1)).ToString();
                predFunctions["sin("]  = Math.Sin(double.Parse(arg1)).ToString();
                predFunctions["tan("]  = Math.Tan(double.Parse(arg1)).ToString();
                predFunctions["sqrt("] = Math.Sqrt(double.Parse(arg1)).ToString();
                predFunctions["log("]  = Math.Log(double.Parse(arg1), double.Parse(arg2)).ToString();
                predFunctions["exp("]  = Math.Pow(Math.E, double.Parse(arg1)).ToString();
            } 
        } 

        public static bool IsFunction(string s) {
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = Aux.FunctionsOut(s);
            s = s.Trim();

            return s == "";
        }

        public static bool IsInstruction(string s) {
            s = s.Trim();
            while (s.StartsWith("(") || s.StartsWith(" ")) s = s.Remove(0, 1); 
            return s.StartsWith("function ");
        }

        public static string Create(string s) {
            string n = s;
            int count = 0;

            while (s.StartsWith("(")) {
                s = s.Remove(0, 1); 
                count ++;
            }
            
            s = s.Remove(s.Length - count);
            s = Aux.SpacesOut(s)[8..];

            if (Aux.ParenthesisBalance(s) != 0) {
                if (!Error.Syntax($"'{n}' is not a valid 'function' instruction")) return "";
            }

            if (!Error.FunctionGeneral(s)) return "";
  
            string funcName = Aux.SpacesOut(s[..(s.IndexOf("(") + 1)]);
            string argument = s.Substring(s.IndexOf("(") + 1, s.IndexOf(")") - s.IndexOf("(") - 1);
            string body = Aux.SpacesOut(s[(s.IndexOf("=>") + 2)..]);
            List<string> vars = argument.Split(",").ToList();

            for (int i = 0; i < vars.Count; i++) vars[i] = vars[i].Trim();
            
            functions[funcName] = body;
            variables[funcName] = vars;
            existFunctions.Add(funcName);
            if (output[funcName] == "all") output[funcName] = Aux.FunctionOutputType(body, vars);

            return "";     
        }

        public static string Eval(string s, string[] args) {
            string n = Aux.StringOut(s);

            int index1 = n.IndexOf('(');
            string argument = string.Join(", ", args);
            string f = s[..(index1 + 1)];

            if (functions.ContainsKey(f)) {
                if(cache.ContainsKey($"{f}{argument})")) return cache[$"{f}{argument})"];
                return Sustitution(functions[f], variables[f], args.ToList(), true, f);
            }
            
            if(!Error.Restrictions(f, argument, args)) return "";

            if (f[..^1] == "rand") {
                Random r = new();
                return r.NextDouble().ToString();         
            }
            
            Function result = new(args);

            if (Error.error) return "";
            if (result.predFunctions.ContainsKey(f)) return result.predFunctions[f];
            
            Error.Semantic($"'{f[..^1]}' is not defined");
            return "";
        }
        
        public static string Sustitution(string body, List<string> vars, List<string> values, bool function, string name = "") {

            if(!Error.ArgumentCount(vars, values, name)) return "";
            if(function && !Error.Restrictions(name, string.Join(", ", values), values.ToArray())) return "";
        
            string s = Aux.StringOut(body);
            string[] symbols = {
                "*", "/", "^", "%", "+", "-", "(", ")", ">", "<", "&","|","!", ",", "@", "=", "\"", " "
            };
            
            for (int i = 0; i < vars.Count; i++)
            {
                if (s.Contains(vars[i])) {
                    List<string> tokens = s.Split(symbols, StringSplitOptions.RemoveEmptyEntries).ToList();
                    tokens.RemoveAll(string.IsNullOrWhiteSpace);
                    string[] newTokens = new string[tokens.Count];

                    for (int j = 0; j < tokens.Count; j++)
                    {
                        newTokens[j] = (Aux.SpacesOut(tokens[j]) == vars[i])? $"({values[i]})" : tokens[j];
                    }
                    
                    int position = s.Length;

                    for (int j = tokens.Count - 1; j >= 0; j--)
                    {
                        position = s[..position].LastIndexOf(tokens[j]);
                        body = body.Remove(position,tokens[j].Length);
                        body = body.Insert(position, newTokens[j]);
                    } 
                    
                    s = Aux.StringOut(body);
                }
            }

            return body;
        }
    }
}