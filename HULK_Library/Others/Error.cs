using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hulk_Library
{
    public class Error 
    {
        public static List<string> funcVars = new();
        public static List<string> vars = new();
        private const List<string> value = null!;
        public static string funcName = "";

        #region Errors Messages
        public static bool Syntax(string mssg) {
            // Método para dar errores sintácticos
            if (Control.error) return false;
            Control.error = true;
            Control.Mssg = $"!!SYNTAX ERROR: {mssg}";
            return false;
        }
        
        public static bool Semantic(string mssg) {
            // Método para dar errores semánticos
            if (Control.error) return false;
            Control.error = true;
            Control.Mssg = $"!!SEMANTIC ERROR: {mssg}";
            return false;
        }

        public static bool Lexical(string mssg) {
            // Método para dar errores léxicos
            if (Control.error) return false;
            Control.error = true;
            Control.Mssg = $"!!LEXICAL ERROR: {mssg}";
            return false;
        }

        public static bool MissingMember(string symbol, string leftMember, string rightMember) {
            // Este método verifica si falta algún miembro en las operaciones
            bool empty_L = string.IsNullOrWhiteSpace(leftMember);
            bool empty_R = string.IsNullOrWhiteSpace(rightMember);

            if (symbol == "!") {
                if (!empty_L || empty_R) {
                    string message = (!empty_L)? "can not have a left" : "needs a right";
                    return Semantic($"Operator '!' {message} member");
                }

                return true;
            }

            if (!string.IsNullOrWhiteSpace(leftMember) && !string.IsNullOrWhiteSpace(rightMember)) return true;

            string member = string.IsNullOrWhiteSpace(leftMember)? "left" : "right"; 
            return Semantic($"Operator '{symbol}' needs a '{member}' member");
        }
        
        #endregion

        #region General y operations checker

        public static bool General(string s) {
            // Se realizan revisiones un poco más especificas en cuanto a la escritura de la expresión
            
            s = Aux.SpacesOut(s);

            // Si son condicionales o declaraciones de variable se salta porque tienen su revisión específica
            if (If_else.IsIf_else(s) || Let_in.IsLet_in(s)) return true;

            if (Control.error) return false;

            string temp = Aux.StringOut(s);

            if (Aux.FunctionsOut(temp).Contains(",")) {
                // Si quitando las funciones y los strings de la exoresión y aun contiene ',' es un error
                return Syntax("Unexpected ','");
            }

            int index = temp.IndexOf(".");
            // Si quitando las funciones y los strings de la exoresión y aun contiene '.' 
            // que no pertenezca a un número es un error 
            while (index != -1) { 
                if ((index > 0 && (!Aux.IsNumber(temp[index - 1].ToString()) || temp[index - 1] == ' ')) || ( index < (s.Length - 1) && !Aux.IsNumber(s[index + 1].ToString()))) {
                    return Syntax("Unexpected '.'");
                }

                index = temp.IndexOf(".", index + 1);
            }
            
            string[] simpleSymbols = {"*", "/", "^", "%", "+", "-", "(", ")", ">", "<", "&","|","!", ",", "@", ".", "_", "\""};
            string[] doubleSymbols = {">=", "<=", "!=", "=="};
            string[] notsymbols = {"`", "$", "~"};
            List<string> tokens = new();

            string n = Aux.StringOut(s);
            int quotes = n.IndexOf("\"");

            if (quotes == 0) quotes = n.IndexOf("\"", quotes + 1);

            while (quotes != -1 && quotes < n.Length - 1) {
                // Aquí se revisa que cualquier elemento siguiente o anterior a un string si es
                // una letra o un número de forma directa es un error

                if (Aux.StringBalance(n[..(quotes + 1)]) && n[(quotes + 1)..].Replace(" ", "")[0] == '"') {
                    return Syntax("Strings can be concatenated only using operator '@'");
                }

                if (Aux.StringBalance(n[..(quotes + 1)]) && char.IsLetterOrDigit(n[(quotes + 1)..].Replace(" ", "")[0])) {
                    return Syntax("Operator '@' was expected after string");
                }
                
                if (!Aux.StringBalance(n[..(quotes + 1)]) && char.IsLetterOrDigit(n[..quotes].Replace(" ", "")[^1])) {
                    return Syntax("Operator '@' was expected before string");
                }     
                
                quotes = n.IndexOf("\"", quotes + 1);
            }

            for (int i = 0; i < n.Length; i++) {
                // Si algún caracter que no esté en el teclado aparece en la expresión es un error
                if (n[i] != '∞' && !char.IsAscii(n[i]) || char.IsPunctuation(n[i])) {
                    if (simpleSymbols.Contains(n[i].ToString())) continue;
                    return Syntax($"Unexpected '{n[i]}'");
                }
            }
            
            foreach (var item in doubleSymbols) n = n.Replace(item, $" {item} ");
            foreach (var item in simpleSymbols) n = n.Replace(item, $" {item} ");
            
            n = n.Insert(n.Length, " ");
            index = n.IndexOf(" ");

            // Se hace un 'split' manual para guardar en los valores los strings pero sin tenerlos en cuenta
            // para los índices
            while (index != -1) {
                if (Aux.StringBalance(n[(index + 1)..])) {
                    tokens.Add(n[..index]);
                    n = n[(index + 1)..];
                    index = n.IndexOf(" ");
                }
                else index = n.IndexOf(" ", index + 1);
            }
            
            tokens.RemoveAll(string.IsNullOrWhiteSpace);

            for (int i = 0; i < tokens.Count - 1; i++)
            {
                // Si llegó aquí es porque no fue reconocido como una instrucción o forma 
                // parte de una, por tanto que aparezca la palabra 'function' es un error
                if (tokens[i] == "function" || tokens[i + 1] == "function") {
                    return Syntax("Invalid 'function' instruction");
                }

                if (tokens[i] == ")" && (tokens[i + 1].Any(char.IsLetterOrDigit) || 
                    tokens[i + 1].Any(char.IsPunctuation))) {
                    // Después de un ')' solo pueden haber los 'simpleSymbols' excepto el '(' o un 'else'

                    if (simpleSymbols.Contains(tokens[i + 1]) && tokens[i + 1] != "(") continue;
                    if (tokens[i + 1] == "else") continue;

                    if(tokens[i + 1].Contains("\"")) tokens[i + 1] = "string";
                    return Syntax($"Unexpected '{tokens[i + 1]}' after ')'");
                }

                if (i > 0 && tokens[i] == "(" && tokens[i - 1].Any(char.IsPunctuation)) {
                    // Antes de un '(' solo pueden haber los 'simpleSymbols' 
                    if (simpleSymbols.Contains(tokens[i - 1])) continue;

                    if(tokens[i + 1].Contains("\"")) tokens[i - 1] = "string";
                    return Syntax($"Unexpected '(' after '{tokens[i - 1]}'");
                }

                else if (tokens[i].Any(char.IsLetterOrDigit) && tokens[i + 1].Any(char.IsLetterOrDigit) ) { 
                    // No pueden haber palabras p números consecutivos sin símbolos
                    if (simpleSymbols.Contains(tokens[i]) || simpleSymbols.Contains(tokens[i + 1])) continue;
                    if (tokens[i] == "else" || tokens[i + 1] == "else") continue;

                    return Syntax($"Unexpected '{tokens[i + 1]}' after '{tokens[i]}'");
                }

                else if (tokens[i].Any(char.IsPunctuation) && tokens[i + 1].Any(char.IsPunctuation)) { 
                    // No pueden haber signos de puntuación que no sean los símbolos
                    if (simpleSymbols.Contains(tokens[i]) || simpleSymbols.Contains(tokens[i + 1])) continue;

                    if(tokens[i + 1].Contains("\"")) tokens[i + 1] = "string";
                    if(tokens[i].Contains("\"")) tokens[i] = "string";

                    return Syntax($"Unexpected '{tokens[i + 1]}' after '{tokens[i]}'");
                }
            }
            
            return true;
        }

        public static bool Operations(string leftSide, string rightSide, string symbol) {
            string[] numerics = {"+", "-", "*", "/", "%", "^", ">", "<", "<=", ">="};

            bool passBodySyntax_L = BodyDetails(leftSide);
            bool passBodySyntax_R = BodyDetails(rightSide);
            string leftType = Aux.ExpressionType(leftSide);
            string rightType = Aux.ExpressionType(rightSide);

            if (symbol == "!") {
                // Se revisa que no le falte el miembro derecho y que no tenga el izquierdo
                if (!MissingMember(symbol, leftSide, rightSide)) 
                return false;
                
                if(passBodySyntax_R || rightType != "boolean") {
                    return Semantic($"Operator '!' cannot used by {rightType}");
                }
            }

            if ((symbol == "+" || symbol == "-") && string.IsNullOrWhiteSpace(leftSide) && rightType == "number")
            // Si le falta el miembro izquiero pero lo de la derecha es un número es correcto
            return true;

            else if (!MissingMember(symbol, leftSide, rightSide))
            // sino se revusa q tenga los dos miembros
            return false;

            if (passBodySyntax_L && passBodySyntax_R) { 
                // Si no tuvo errores se sigue la revisión 
                if (symbol == "@") {
                    // En el '@' al menos uno de los dos tiene que ser string
                    if (leftType != "string" && rightType != "string") {
                        return Semantic($"Operator {symbol} cannot be used between {leftType} and {rightType}");
                    } 
                }

                else if (numerics.Contains(symbol)) {
                    // De estos símbolos ambos miembros tienen que ser números
                    if (leftType != "number" || rightType != "number") {
                        return Semantic($"Operator {symbol} cannot be used between {leftType} and {rightType}");
                    }

                    // En el caso de la división y el resto no es posible si el miembro derecho es 0
                    // solo lo dará si el lado derecho es exactamente 0
                    if ((symbol == "/" || symbol == "%") && rightSide.Trim() == "0") {
                        return Semantic("Division by 0 is not defined");
                    }
                }

                else if ((symbol == "&" || symbol == "|") && (leftType != "boolean" || rightType != "boolean")) {
                    // Estos símbolos ambos miembros tienen que ser booleanos
                    return Semantic($"Operator {symbol} cannot be used between {leftType} and {rightType}");
                }

                else if ((symbol == "!=" || symbol == "==") && leftType != rightType) {
                    // Estos símbolos ambos miembros tienen que ser del mismo tipo
                    return Semantic($"Operator {symbol} cannot be used between {leftType} and {rightType}");
                }

                return true;
            }

            return false;
        }        
        #endregion

        #region Function Checker
        public static bool FunctionGeneral(string s) {
            // Se guarda la expresión sin strings para la búsqueda de índices
            string auxiliar = Aux.StringOut(s);

            if (!auxiliar.Contains("(")) {
                // Es condición necesaria que después del nombre de la función haya un '('
                return Syntax("'(' was expected after the 'function name'");
            }

            string name = Aux.SpacesOut(s[..s.IndexOf("(")]);

            if (name == "") {
                // Se verifica que tenga nombre la función
                return Syntax("Missing function name before '('");
            }

            s = Aux.SpacesOut(s[(auxiliar.IndexOf("(") + 1)..]);
            auxiliar = Aux.SpacesOut(auxiliar[(auxiliar.IndexOf("(") + 1)..]);

            string argument = s[..auxiliar.IndexOf(")")];
            auxiliar = auxiliar[..auxiliar.IndexOf(")")];
            string body = Aux.SpacesOut(s[(argument.Length + 1)..]);

            if (auxiliar.Contains("(")) {
                // Dentro de los argumentos de la función no pueden haber paréntesis 
                return Syntax("Unexpected parenthesis in the argument of the function");
            }
            
            if (!body.StartsWith("=>")) {
                // Después del nombre de los argumentos tiene que tener el signo de asignación '=>'
                return Syntax($"Is not a valid assignment. Missing operator: '=>'");
            }

            List<string> vars = new();
            string temp = argument;
            int tempIndex = auxiliar.IndexOf(",");
             
            // Se hace un 'split' manual para guardar en los valores los strings pero sin tenerlos en cuenta
            // para los índices
            while (tempIndex != -1) {
                vars.Add(temp[..tempIndex]);
                temp = temp.Remove(0, tempIndex + 1);
                auxiliar = auxiliar.Remove(0, tempIndex + 1);
                tempIndex = auxiliar.IndexOf(",");
            }

            vars.Add(temp);

            // Se verifica si no hay registros de la función 
            if (!Function.input.ContainsKey(name + "(")) {
                // En caso de que no, se llenan con los valores por defecto 'all'
                Function.input[name + "("] = new();
                Function.output[name + "("] = "all";

                for (int i = 0; i < vars.Count; i++) {
                    Function.input[name + "("].Add("all");
                }
            } 

            for (int i = 0; i < vars.Count; i++) vars[i] = vars[i].Trim();

            if (!ValidVariable(name)) {
                // Se revisa si el nombre de la función es correcto
                if (!Function.existFunctions.Contains(name + "(")) {
                    Function.input.Remove(name + "(");
                    Function.output.Remove(name + "(");
                }

                return false;
            }

            if (vars.Distinct().Count() != vars.Count) {
                // Se verifica que las variables no tengan el mismo nombre
                if (!Function.existFunctions.Contains(name + "(")) {
                    Function.input.Remove(name + "(");
                    Function.output.Remove(name + "(");
                }

                return Semantic("One argument is used more than once");
            }
            
            if (vars.Contains(name)) {
                // Las variables no pueden tener el nombre de la función
                if (!Function.existFunctions.Contains(name + "(")) {
                    Function.input.Remove(name + "(");
                    Function.output.Remove(name + "(");
                }

                return Semantic($"'{name}' is the function name. It can not be a variable name");
            }

            if (argument != "" && vars.Any(string.IsNullOrWhiteSpace)) {
                // Revisar si no se dio alguna variable
                if (!Function.existFunctions.Contains(name + "(")) {
                    Function.input.Remove(name + "(");
                    Function.output.Remove(name + "(");
                }

                return Semantic($"One argument is missing in '{name}' function");
            }

            if (argument != "" && !vars.All(ValidVariable)) {
                // Verifica que todas las variables sean válidas 
                if (!Function.existFunctions.Contains(name + "(")) {
                    Function.input.Remove(name + "(");
                    Function.output.Remove(name + "(");
                }

                return false;
            }

            if (body == "=>") {
                // Si el body contiene solamente el símbolo '=>' es porque no se definió
                if (!Function.existFunctions.Contains(name + "(")) {
                    Function.input.Remove(name + "(");
                    Function.output.Remove(name + "(");
                }

                return Syntax("The body was not defined");
            }

            while (Let_in.IsLet_in(body[2..])) {
                // Si tiene un let-in se maneja de otra forma, se efectúa el let-in antes
                // pero sin llegar a evaluar el 'body' del let-in solo sustituyendolo en la expresión
                funcVars = vars;
                Function.keyWords.AddRange(vars);
                body = $"=> {Let_in.Eval(body[2..], true)}";
                Function.keyWords.RemoveRange(Function.keyWords.Count - vars.Count, vars.Count);
                funcVars = new();
                if (body == "") {
                    if (!Function.existFunctions.Contains(name + "(")) {
                        Function.input.Remove(name + "(");
                        Function.output.Remove(name + "(");
                    }
                    return false;
                }
            }

            if (String.IsString(body[2..])) {
                // Si es string se verifica que no tenga los errores de string
                temp = String.TraduceString(body[2..]);
                if (temp == "") {
                    if (!Function.existFunctions.Contains(name + "(")) {
                        Function.input.Remove(name + "(");
                        Function.output.Remove(name + "(");
                    }
 
                    return false;
                }
            }

            body = Aux.StringOut(body);
            Error.vars = new();
            funcName = "";

            if (!BodyGeneral(body[2..], vars, name))
            // Se verifica que no tenga errores en el 'cuerpo'
            return false;

            if (!BodyDetails(body[2..], name, vars)) {
                // Se verifica que no tenga errores más detallados
                if (!Function.existFunctions.Contains(name + "(")) {
                    Function.input.Remove(name + "(");
                    Function.output.Remove(name + "(");
                }

                return false;
            }

            return true;
        }

        public static bool BodyGeneral(string body, List<string> vars, string funcName = "") {
            // Método para realizar el chequeo del 'cuerpo' de forma general

            if (!General(body)) return false;

            if (string.IsNullOrWhiteSpace(body)) {
                // Si el 'cuerpo' es vacío es porque faltó definirlo
                return Syntax("Missing body after 'in' in 'let-in' expression");
            }

            body = $" {Aux.StringOut(body)} "; 
            
            if (funcName != "" && body.Contains($" {funcName} ")) {
                // Si en el 'cuerpo' está el nombre de la función sin el '(' es un error
                return Syntax($"Invalid token '{funcName}'");
            }

            if (Function.keyWords.Any(x => Function.keyWords.IndexOf(x) <= 15 && body.Trim() == x) && 
                body.Replace(" ", "").ToLower() != "true" &&  body.Replace(" ", "").ToLower() != "false" &&
                body.Trim() != "PI" &&  body.Trim() != "E") {
                    // Si el 'cuerpo' de la función es una keyword que no sean las que se excluyeron, es un error
                    return Syntax($"Invalid token '{body.Trim()}'");
            }

            body = Regex.Replace(body, @"[^_ñÑA-Za-z0-9]", " ");
            string[] words = body.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            List<string> wrongVars = new();
            foreach (string word in words)
            {
                if (!vars.Contains(word) && word != funcName && !Function.existFunctions.Contains(word + "(") 
                    && !Function.keyWords.Contains(word) && !Aux.IsNumber(word) && !String.IsString(word)) {
                    // Se verifica que cada palabra del 'cuerpo' sea válida, de otra forma se imprimirán todas
                    if (!wrongVars.Contains($"'{word}'")) wrongVars.Add($"'{word}'");
                }
            }

            if (wrongVars.Count > 0) {
                string are = (wrongVars.Count > 1)? "are" : "is";
                string wrongs = string.Join(",", wrongVars);
                string mssg = (wrongVars.Count > 1)? $"{wrongs[..wrongs.LastIndexOf(",")]} and {wrongs[(wrongs.LastIndexOf(",") + 1)..]}" : wrongs;
                return Syntax($"{mssg} {are} not defined");
            }

            return true;
        }

        public static bool BodyDetails(string body, string name = "", List<string> variables = null!) {
            // Este método revisa que la expresión esté correctamente escrita, incluyendo 
            // los 'cuerpos' de las funciones que se declaran 
            
            // Si se está revisando una declaración de función interesa saber, si el 'cuerpo'
            // contiene variables, cuál sería el tipo que debería tener, para ello durante el
            // método se estará verificando si contiene alguna de las variables, para en dependencia 
            // del contexto definir cuál es el tipo que se espera

            body = Aux.StringOut(body);
            
            // Si la lista es 'null' se crea una nueva 
            variables ??= new();
            if (vars.Count == 0) vars.AddRange(variables);
            if (funcName == "") funcName = name; 

            if (If_else.IsIf_else(body)) {
                // Si contiene una condicional se revisa de forma diferentes, primero se idetntifican
                // todos los componentes de una condicional
                (bool, string, string, string, int, int, string) conditionalData = Correct_if(body, vars);
                
                // Si es primer elemento es false es porque ya identificó algún error
                if (!conditionalData.Item1) return false;
                
                if (vars.Contains(conditionalData.Item2)) {
                    Function.input[funcName + "("][vars.IndexOf(conditionalData.Item2)] = "boolean";
                }

                string body_true = conditionalData.Item3;
                string body_false = conditionalData.Item4;
                int start = conditionalData.Item5;
                int stop = conditionalData.Item6;
                body = conditionalData.Item7;
                string body1 = body;
                string body2 = body;

                // Como la condicional puede tener dos tipos distintos en sus 'cuerpos' se verifica que ambos 
                // sean válidos en el contexto de la condicional
                body1 = body1.Remove(start + 1, stop - start - 1);
                body1 = body1.Insert(start + 1, $"{body_true}");
                body2 = body2.Remove(start + 1, stop - start - 1);
                body2 = body2.Insert(start + 1, $"{body_false}");

                // Si alguno de los dos dio error se imprime y además se aclara que el error es de tipos en los
                // 'cuerpos'
                bool check1 = BodyDetails(body1) && BodyDetails(body2);
                if (!check1) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("!!!! Possible invalid type in conditional !!!!");
                }

                return check1;
            } 
            
            // Diccionario que guarda los valores por 'defecto' por tipo para sustituirlos por las variables
            // una vez identificado el tipo que tiene que ser
            // una variable será de tipo 'all' si puede ser string, boolean o number
            Dictionary<string, string> defaultValues = new() {
                {"string", "\" \""}, {"number", "2"}, {"boolean", "true"}, 
                {"invalid expression", ""}, {"all", ")("}
            };

            while (body.Replace(")(", "  ").Contains('(')) {
                
                int index1 = body.Replace(")(", "  ").LastIndexOf("(");
                int index2 = body.Replace(")(", "  ").IndexOf(")", index1);
                string parenthesis = body[(index1 + 1)..index2];

                // Se revisa si tiene ',' por si es una función, si no sería un error, en caso de que 
                // no tenga ',', se revisa si el interior del paréntesis es correcto
                if (parenthesis.Contains(",") || BodyDetails(parenthesis, funcName, vars)) {
                    
                    (bool, string, string[]) data = Aux.FunctionInfo(body, index1, index2);

                    // Este dato es true si fue identificada como función
                    if (data.Item1) {
                        string function = data.Item2;
                        string[] args = data.Item3;

                        // Se revisa que la función exista, si no es un error
                        if (function != funcName && !Function.input.ContainsKey(function)) {
                            return Semantic($"'{function[..^1]}' is not a defined function");
                        }

                        // Se revisa que haya recibido la misma cantidad de argumentos que de variables
                        if (!ArgumentCount(Function.input[function], args.ToList(), function)) return false;

                        if (funcName + "(" == function) {
                            // Esto es para saber que tipo devuelven las funciones recursivas, se les 
                            // elimina el argumento para tratarla como una variable más, en dependencia
                            // del valor que debería tomar como variable es el que debe retornar
                            body = body.Remove(index1, parenthesis.Length + 2);
                            Array.Resize(ref args, args.Length + 1);
                            args[^1] = funcName;
                            vars.Add(funcName);
                            Function.input[funcName + "("].Add("all");
                        }
                        
                        for (int i = 0; i < args.Length; i++) {
                            
                            if (args[i] == "") break;
                            // Se revisa que cada argumento de la función esté correcto
                            if (!BodyDetails(args[i], funcName, vars)) return false;

                            // Si la función es print, como devuelve lo que recibe, entonces se inserta lo
                            // que fue recibido 
                            if (function == "print(") {
                                body = body.Remove(index2 , 1);
                                body = body.Remove(index1 - 5, 6);
                                break;
                            }
                            
                            // Si es recibida una variable como argumento entonces se le asigna el tipo que
                            // debe tener según lo que espera la función donde es utilizada y se sustituye
                            // por el valor por defecto del tipo
                            if (vars.Contains(args[i].Trim()) && Function.input[function][i] != "all") {
                                int position = vars.IndexOf(args[i].Trim());
                                Function.input[funcName + "("][position] = Function.input[function][i];
                                args[i] = defaultValues[Function.input[function][i]];
                            }
                            // Si la función recibe cualquier tipo se pone un valor válido
                            else if (Function.input[function][i] == "all") {
                                args[i] = "2";
                            } 
                        }

                        if (!Function.existFunctions.Contains(function)) Function.bodyFunction[function] = "";

                        if (funcName + "(" == function) {
                            // Ya aquí habrá sido asignado el valor de retorno de la función recursiva
                            Array.Resize(ref args, args.Length - 1);
                            Function.input[funcName + "("].RemoveAt(Function.input[funcName + "("].Count - 1); 
                        }

                        // Se revisa que los argumentos de las funciones sean lo que esperan
                        if (Restrictions(function, parenthesis, args)) {
                            if (!Function.existFunctions.Contains(function)) Function.bodyFunction.Remove(function);
                            // Si la función no es print ni la recursiva entonces se sustituye en la 
                            // expresión por su valor de retorno por defecto
                            if (function != funcName + "(" && function != "print(") {
                                int start = index1  + 1 - function.Length;
                                body = body.Remove(start, function.Length + parenthesis.Length + 1);

                                if (Function.existFunctions.Contains(function)) {
                                    body = body.Insert(start, defaultValues[Function.output[function]]);
                                }

                                else body = body.Insert(start, funcName);
                            }
                        }

                        else {
                            return false;
                        }
                    }

                    else {
                        // Si no fue una función se halla el tipo de lo que está dentro de paréntesis
                        // y se sustituye por su valor por defecto
                        string val = defaultValues[Aux.ExpressionType(parenthesis)];
                        // Si es una expresión inválida entonces se dejará tal como estaba
                        if (val == "") val = parenthesis;
                        body = body[..index1] + val + body[(index2 + 1)..];
                    }
                }

                else return false;
            }

            // En las operaciones se revisa que los miembros de la expresión pasen la revisión completa
            // y además reciban los tipos que esperaban
            // Aquí es donde si existe una variable se le asigna un ripo 
            
            if (body.Contains('@')) {
                int index = body.LastIndexOf('@');
                string left = Aux.SpacesOut(body[..index]);
                string right = Aux.SpacesOut(body[(index + 1)..]);
                left = (left == ")(")? "\" \"" : left;
                right = (right == ")(")? "\" \"" : right;

                if (vars.Contains(left)) {

                    if (left == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "string";
                        }

                        else if (Function.output[funcName + "("] != "string") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return string and {type}");
                        } 
                    }   

                    left = defaultValues["string"]; 
                } 

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "string";
                        }

                        else if (Function.output[funcName + "("] != "string") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return string and {type}");
                        } 
                    }

                    right = defaultValues["string"];
                }

                if (left.Contains("==") || left.Contains("!=")) (left, right) = (right, left);

                return Operations(left, right, "@");
            }

            if (body.Contains('&') || body.Contains('|')) {
                int index = Math.Max(body.LastIndexOf("&"), body.LastIndexOf("|"));
                char operation = body[index];
                string left = Aux.SpacesOut(body[..index]);
                string right = Aux.SpacesOut(body[(index + 1)..]);
                left = (left == ")(")? "true" : left;
                right = (right == ")(")? "true" : right;

                if (vars.Contains(left)) {

                    if (left == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "boolean";
                        }

                        else if (Function.output[funcName + "("] != "boolean") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return boolean and {type}");
                        }

                        left = defaultValues["boolean"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(left)] = "boolean";
                        left = defaultValues["boolean"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(left)] == "boolean") {
                        left = defaultValues["boolean"];    
                    }

                    else {
                        string type = Function.input[funcName + "("][vars.IndexOf(left)];
                        return Semantic($"Variable '{left}' can not be boolean and {type}");
                    }      
                }

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "boolean";
                        }

                        else if (Function.output[funcName + "("] != "boolean") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return boolean and {type}");
                        }

                        right = defaultValues["boolean"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(right)] = "boolean";
                        right = defaultValues["boolean"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] == "boolean") {
                        right = defaultValues["boolean"];    
                    }

                    else {
                        string type = Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($" Variable '{right}' can not be boolean and {type}");
                    }
                }

                if (left.Contains("==") || left.Contains("!=")) (left, right) = (right, left);

                return Operations(left, right, operation.ToString()) ;
            }

            if (body.Contains("==") || body.Contains("!=")) {
                int index = Math.Max(body.LastIndexOf("=="), body.LastIndexOf("!="));
                string operation = body.Substring(index, 2);
                string left = Aux.SpacesOut(body[..index]);
                string right = Aux.SpacesOut(body[(index + 2)..]);
                string typeR = vars.Contains(right) ? Function.input[funcName + "("][vars.IndexOf(right)] : Aux.ExpressionType(right);
                string typeL = vars.Contains(left) ? Function.input[funcName + "("][vars.IndexOf(left)] : Aux.ExpressionType(left);

                if (vars.Contains(left)) {
                    if (left == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = typeR;
                            typeL = typeR;
                        }

                        else if (Function.output[funcName + "("] != typeR) {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return {typeR} and {type}");
                        }

                        left = defaultValues[typeR]; 
                    }

                    if (Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(left)] = typeR;
                        typeL = typeR;
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(left)] != typeR) {
                        string type = Function.input[funcName + "("][vars.IndexOf(left)];
                        return Semantic($"Variable '{left}' can not be {typeR} and {type}");
                    }

                    left = defaultValues[typeR];   
                }

                if (vars.Contains(right)) {
                    if (right == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = typeL;
                            typeR = typeL;
                        }

                        else if (Function.output[funcName + "("] != typeL) {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return {typeL} and {type}");
                        }

                        right = defaultValues[typeL]; 
                    }

                    if (Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(right)] = typeL;
                        typeR = typeL;
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] != typeL) {
                        string type = Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($"Variable '{right}' can not be {typeL} and {type}");
                    }

                    right = defaultValues[typeL];   
                }

                if (left == ")(") return true;

                return Operations(left, right, operation);
            }

            if (body.Contains("<=") || body.Contains(">=")) {
                int index = Math.Max(body.LastIndexOf("<="), body.LastIndexOf(">="));
                string operation = body.Substring(index, 2);
                string left = Aux.SpacesOut(body[..index]);
                string right = Aux.SpacesOut(body[(index + 2)..]);
                left = (left == ")(")? "2" : left;
                right = (right == ")(")? "2" : right;


                if (vars.Contains(left)) {

                    if (left == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "number";
                        }

                        else if (Function.output[funcName + "("] != "number") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return number and {type}");
                        }

                        left = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(left)] = "number";
                        left = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(left)] == "number") {
                        left = defaultValues["number"];    
                    }

                    else {
                        string type = Function.input[funcName + "("][vars.IndexOf(left)];
                        return Semantic($"Variable '{left}' can not be number and {type}");
                    }      
                }

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "number";
                        }

                        else if (Function.output[funcName + "("] != "number") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return number and {type}");
                        }

                        right = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(right)] = "number";
                        right = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] == "number") {
                        right = defaultValues["number"];    
                    }

                    else {
                        string type = Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($"Variable '{right}' can not be number and {type}");
                    }
                }

                return Operations(left, right, operation);
            }

            if (body.Contains('<') || body.Contains('>')) {
                int index = Math.Max(body.LastIndexOf("<"), body.LastIndexOf(">"));
                char operation = body[index];
                string left = Aux.SpacesOut(body[..index]);
                string right = Aux.SpacesOut(body[(index + 1)..]);
                left = (left == ")(")? "2" : left;
                right = (right == ")(")? "2" : right;

                if (vars.Contains(left)) {

                    if (left == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "number";
                        }

                        else if (Function.output[funcName + "("] != "number") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return number and {type}");
                        }

                        left = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(left)] = "number";
                        left = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(left)] == "number") {
                        left = defaultValues["number"];    
                    }

                    else {
                        string type = Function.input[funcName + "("][vars.IndexOf(left)];
                        return Semantic($"Variable '{left}' can not be number and {type}");
                    }      
                }

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "number";
                        }

                        else if (Function.output[funcName + "("] != "number") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return number and {type}");
                        }

                        right = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(right)] = "number";
                        right = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] == "number") {
                        right = defaultValues["number"];    
                    }

                    else {
                        string type = Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($"Variable '{right}' can not be number and {type}");
                    }
                }

                return Operations(left, right, operation.ToString());
            }

            if (body.Contains('!')) {
                int index = body.LastIndexOf("!");
                string left = Aux.SpacesOut(body[..index]);
                string right = Aux.SpacesOut(body[(index + 1)..]);
                left = (left == ")(")? "true" : left;
                right = (right == ")(")? "true" : right;

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "boolean";
                        }

                        else if (Function.output[funcName + "("] != "boolean") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return boolean and {type}");
                        }

                        right = defaultValues["boolean"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(right)] = "boolean";
                        right = defaultValues["boolean"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] == "boolean") {
                        right = defaultValues["boolean"];    
                    }

                    else {
                        string type = Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($"Variable '{right}' can not be boolean and {type}");
                    }
                }

                return Operations(left, right, "!");
            }

            body = Aux.SpacesOut(body);
            body = Aux.InternalSpaces(body);

            if (body.Contains('+') || body.Contains('-')) {
                
                body = Aux.ReplaceSymbol(body);

                int index = Math.Max(body.LastIndexOf("+"), body.LastIndexOf("-"));
                char operation = body[index];
                string[] binaries = {"+", "-", "*", "/", "%", "^"};   

                while (index > 1 && body[index - 1] == 'E' && char.IsDigit(body[index - 2]) ||
                    (index > 0 && binaries.Contains(body[index - 1].ToString()))) { 

                    index = Math.Max(body[..index].LastIndexOf('+'), body[..index].LastIndexOf('-'));
                }

                if (index >= 0) {
                    string left = Aux.SpacesOut(body[..index]);
                    string right = Aux.SpacesOut(body[(index + 1)..]);
                    left = (left == ")(")? "2" : left;
                    right = (right == ")(")? "2" : right;

                    if (vars.Contains(left)) {

                        if (left == funcName) { 
                            if (Function.output[funcName + "("] == "all") {
                                Function.output[funcName + "("] = "number";
                            }

                            else if (Function.output[funcName + "("] != "number") {
                                string type = Function.output[funcName + "("];
                                return Semantic($"'{left}' can not return number and {type}");
                            }

                            left = defaultValues["number"]; 
                        }

                        else if (Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                            Function.input[funcName + "("][vars.IndexOf(left)] = "number";
                            left = defaultValues["number"]; 
                        }

                        else if (Function.input[funcName + "("][vars.IndexOf(left)] == "number") {
                            left = defaultValues["number"];    
                        }

                        else {
                            string type = Function.input[funcName + "("][vars.IndexOf(left)];
                            return Semantic($"Variable '{left}' can not be number and {type}");
                        }      
                    }

                    if (vars.Contains(right)) {

                        if (right == funcName) { 
                            if (Function.output[funcName + "("] == "all") {
                                Function.output[funcName + "("] = "number";
                            }

                            else if (Function.output[funcName + "("] != "number") {
                                string type = Function.output[funcName + "("];
                                return Semantic($"'{right}' can not return number and {type}");
                            }

                            right = defaultValues["number"]; 
                        }

                        else if (Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                            Function.input[funcName + "("][vars.IndexOf(right)] = "number";
                            right = defaultValues["number"]; 
                        }

                        else if (Function.input[funcName + "("][vars.IndexOf(right)] == "number") {
                            right = defaultValues["number"];    
                        }

                        else {
                            string type = Function.input[funcName + "("][vars.IndexOf(right)];
                            return Semantic($"Variable '{right}' can not be number and {type}");
                        }
                    }

                    return Operations(left, right, operation.ToString());
                }
            }

            if (body.Contains('*') || body.Contains('/') || body.Contains('%')) {
                int index = Math.Max(body.LastIndexOf("*"), Math.Max(body.LastIndexOf("/"), body.LastIndexOf("%")));
                char operation = body[index];
                string left = Aux.SpacesOut(body[..index]);
                string right = Aux.SpacesOut(body[(index + 1)..]);
                left = (left == ")(")? "2" : left;
                right = (right == ")(")? "2" : right;

                if (vars.Contains(left)) {

                    if (left == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "number";
                        }

                        else if (Function.output[funcName + "("] != "number") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return number and {type}");
                        }

                        left = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(left)] = "number";
                        left = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(left)] == "number") {
                        left = defaultValues["number"];    
                    }

                    else {
                        string type = Function.input[funcName + "("][vars.IndexOf(left)];
                        return Semantic($"Variable '{left}' can not be number and {type}");
                    }      
                }

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "number";
                        }

                        else if (Function.output[funcName + "("] != "number") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return number and {type}");
                        }

                        right = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(right)] = "number";
                        right = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] == "number") {
                        right = defaultValues["number"];    
                    }

                    else {
                        string type = Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($"Variable '{right}' can not be number and {type}");
                    }
                }

                return Operations(left, right, operation.ToString()); 
            }

            if (body.Contains('^')) {
                int index = body.LastIndexOf('^');
                string left = Aux.SpacesOut(body[..index]);
                string right = Aux.SpacesOut(body[(index + 1)..]);
                left = (left == ")(")? "2" : left;
                right = (right == ")(")? "2" : right;

                if (vars.Contains(left)) {

                    if (left == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "number";
                        }

                        else if (Function.output[funcName + "("] != "number") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return number and {type}");
                        }

                        left = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(left)] = "number";
                        left = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(left)] == "number") {
                        left = defaultValues["number"];    
                    }

                    else {
                        string type = Function.input[funcName + "("][vars.IndexOf(left)];
                        return Semantic($"Variable '{left}' can not be number and {type}");
                    }      
                }

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Function.output[funcName + "("] == "all") {
                            Function.output[funcName + "("] = "number";
                        }

                        else if (Function.output[funcName + "("] != "number") {
                            string type = Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return number and {type}");
                        }

                        right = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Function.input[funcName + "("][vars.IndexOf(right)] = "number";
                        right = defaultValues["number"]; 
                    }

                    else if (Function.input[funcName + "("][vars.IndexOf(right)] == "number") {
                        right = defaultValues["number"];    
                    }

                    else {
                        string type = Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($"Variable '{right}' can not be number and {type}");
                    }
                }

                return Operations(left, right, "^");
            }

            return true;
        }

        public static bool Restrictions(string f, string argument, string[] args) {
            // Comprueba que algunas restricciones de las funciones predefinidas 
            // sean correctas

            if (!Function.bodyFunction.ContainsKey(f)) {

                if (f == "rand(" && argument != "") {
                    // Si es 'rand' no puede recibir argumentos
                    return Semantic("No argument was expected in 'rand' function");
                }

                if (f == "rand(") return true;

                if (argument == "") {
                    // Si no es 'rand' recibe al menos un argumento
                    return Semantic($"No argument was given in '{f[..^1]}' function");
                }

                if (args.Any(string.IsNullOrWhiteSpace)) {
                    // Si algún argumento fue vacío es un error
                    return Semantic($"Invalid empty argument in '{f[..^1]}' function");
                }

                if (args.Length > 1 && f != "log(") {
                    // El 'log' es el único que recibe como máximo dos argumentos, de otra forma es un error
                    return Semantic($"1 argument was expected but {args.Length} were given in '{f[..^1]}' function");
                }
            }

            if (args.Any(x => Aux.ExpressionType(x) != Function.input[f][Array.IndexOf(args, x)] &&
                Function.input[f][Array.IndexOf(args, x)] != "all")) {

                // Se verifica que se hayan recibido los argumentos del tipo que se esperaba
                string not = args.First(x => Aux.ExpressionType(x) != Function.input[f][Array.IndexOf(args, x)]);
                string type = Function.input[f][Array.IndexOf(args, not)];
                not = Aux.ExpressionType(not);
                return Semantic($"Function '{f[..^1]}' recieves {type}, not {not}");
            }

            if (f == "log(") {
                // Si es 'log':

                // Recibe como máximo dos argumentos
                if (args.Length > 2) {
                    return Semantic($"2 arguments were expected at most but {args.Length} were given in 'log' function");
                }

                // Si llegó hasta aquí sin error se puede asumir que el argumento completo es 
                // distinto de vacío, por tanto si uno es vacío es porque se esperaban dos 
                if (args.Any(string.IsNullOrWhiteSpace)) {
                    return Semantic($"2 arguments were expected but 1 was given in 'log' function");
                }
            }

            return true;
        }

        public static bool ArgumentCount(List<string> vars, List<string> values, string funcName = "") {
            // Verifica que la cantidad de variables y valores que recibe como parámetros coincide
            string mssg = (funcName != "")? "argument" : "value";
            string argument = (vars.Count == 1)? $"{mssg} was" : $"{mssg}s were";
            string was = (values.Count == 1)? "was" : "were";
            string f = (funcName == "")? "" : $"in '{funcName[..^1]}' function";

            if (vars.Contains("") && !values.Contains("")) {
                // Para el caso en que la función no reciba argumentos
                return Semantic($"Any {mssg} was expected but {values.Count} {was} given {f}");
            }

            if (values.Contains("") && !vars.Contains("")) {
                // Para el caso en que la función haya recibido argumentos vacíos no esperados
                return Semantic($"Invalid empty {mssg} given {f}");
            }

            if (vars.Count != values.Count) {
                // Para el caso en que la función reciba una cantidad distinta de argumentos
                return Semantic($"{vars.Count} {argument} expected but {values.Count} {was} given {f}");
            }

            return true;
        }

        #endregion

        #region Variable Checker

        public static bool ValidVariable(string var) {
            if (Control.error) return false;

            if (funcVars.Contains(var)) {
                // Este error es exclusivo en la revisión de las variables del let-in dentro de una 
                // declaración de función
                return Syntax($"'{var}' is already defined as a variable of the function");
            }

            if (Function.keyWords.Contains(var.Trim())) {
                // Las variables no pueden tener el mismo nombre que una 'keyword'
                return Syntax($"'{var}' is a keyword");
            }

            if (Function.existFunctions.Contains(var + "(")) {
                // Las funciones no pueden repetir los nombres de funciones que ya existen
                return Syntax($"'{var}' is already defined");
            }

            if (char.IsDigit(var[0]) || var.ToLower() != Regex.Replace(var.ToLower(), @"[^_ña-z0-9]", "")) {
                // Esta es la regla q tiene que cumplir para que el nombre de una variable 
                // o de una función sean válidos
                return Lexical($"'{var}' is wrong. Variable and function names only can be written with letters, \nnumbers (not as first character) and '_'");
            }

            return true;
        }

        #endregion

        #region Let-in Checker

        public static (bool, string, int, int, string, List<string>, List<string>) Correct_Let(string s, bool function = false) {
            string[] symbols = {"*", "/", "^", "%", "+", "-", "(", ")", ">", "<", "&","|","!", ",", "=", " "};
            
            // Este será el valor por defecto en caso de error
            (bool, string, int, int, string, List<string>, List<string>) defaultValue = (false, "", 0, 0, "", null, null)!;

            // Se insertan espacios en blanco al principio y al final para la búsqueda de índices 
            s = s.Insert(0, " ");
            s = s.Insert(s.Length, " ");

            List<string> allVars = new();
            List<string> allValues = new();
            
            string m = Aux.StringOut(s);
            string n = Regex.Replace(m, @"[^_""ñÑA-Za-z0-9]", " ");
            
            // Estos índices se buscan con los espacios alrededor para evitar que se cojan índices
            // de palabras que no sean exactamente las q estamos buscando
            int start = n.LastIndexOf(" let ");

            // Aquí se comprueba que lo que antecede a la declaración de variable sea correcto
            if (s[..(start + 1)].Trim() != "" && !symbols.Contains(s[..(start + 1)].Trim()[^1].ToString())) {
                if ((s[..(start + 1)].Length < 2 || (s[..(start + 1)].Trim()[^2..] != "in"))
                && !Syntax("Unexpected expression before 'let-in' instruction")) return defaultValue;
            }
            
            int index_2 = n.IndexOf(" in ", start) + 1;
        
            int stop = s.Length;

            if (m[start] == '(') {
                int indexParenthesis_1 = m.LastIndexOf("(");
                string temp = m;

                while (start != indexParenthesis_1) {
                    // En caso de que el let-in esté dentro de paréntesis se modifica el stop hasta la posición
                    // del paréntesis que cierra la condicional
                    int indexParenthesis_2 = temp.IndexOf(")", indexParenthesis_1);
                    temp = Aux.RemoveCharacter(temp, indexParenthesis_1, indexParenthesis_2);
                    indexParenthesis_1 = temp.LastIndexOf("(");
                }

                stop = temp.IndexOf(")", indexParenthesis_1);
            }

            if (index_2 == 0 || stop < index_2) {
                // Si en la expresión no existe un 'in' es un error, es de carácter obligatorio
                if (!Syntax("Missing 'in' in 'let-in' expression")) return defaultValue;
            }

            string variables = m[(start + 4)..index_2];

            if (Aux.ParenthesisBalance(variables) == -1) {
                if (!Syntax("Invalid token ')' after 'in' in 'let-in' expression")) return defaultValue;
            }

            int tempIndex = Aux.FunctionsOut(variables).IndexOf(",");
            List<string> argument = new(); 

            // Se hace un 'split' manual para guardar en los valores los strings pero sin tenerlos en cuenta
            // para los índices
            while (tempIndex != -1) {
                argument.Add(variables[..tempIndex]);
                variables = variables[(tempIndex + 1)..];
                tempIndex = Aux.FunctionsOut(variables).IndexOf(",");
            }

            argument.Add(variables);

            List<string> vars = new();
            List<string> values = new();
            m = m[(start + 4)..];
            
            for (int i = argument.Count - 1; i >= 0; i--)
            {
                // Si 'argument[i]' es vacío es porque no se declaró alguna variable
                if (string.IsNullOrWhiteSpace(argument[i])) {
                    if (!Semantic("Missing variable declaration")) return defaultValue;
                }

                int equal = argument[i].IndexOf("=");

                // Si no hay un signo '=' es un error, a las variables se les asigna valor con ese símbolo
                if (equal == -1) {
                    if (!Syntax($"Missing '=' in '{argument[i].Trim()}' declaration")) return defaultValue;
                }

                if (argument[i][..equal].Contains("(")) {
                    if (!Syntax("Invalid token '(' in variable in 'let-in' expression")) return defaultValue;
                }

                int lenghtVar = argument[i][..equal].Length;
                int index_1 = m.IndexOf(argument[i]) + start + 4;
                string var = s.Substring(index_1, lenghtVar);   
                
                if (!General(var)) return defaultValue;

                if (string.IsNullOrWhiteSpace(var)) {
                    // Si esto pasa es porque no se le dio un nombre a la variable
                    if (!Semantic("Missing variable in 'let-in' expression")) return defaultValue;
                }

                // Si llegó a aquí no tuvo ningún error anterior y se añaden a las listas
                vars.Add(var.Trim());
                allVars.Add(var.Trim());

                int lengthValue = argument[i][(equal + 1)..].Length;
                index_1 = m.IndexOf(argument[i]) + start + lenghtVar + 5;
                string val = s.Substring(index_1, lengthValue);

                // Luego se verifican los valores que pasen en chequeo general
                if (!General($"({val})")) return defaultValue;
                
                if (val.Replace(" ","") != "()") {
                    values.Add(val.Trim());
                    allValues.Add(val.Trim());
                }
            }

            string body = s[(index_2 + 2)..stop];
            m = Aux.FunctionsOut(body);
            
            if (m.Contains(",")) {
                // Si quitando las funciones el 'body' contiene ',' es porque es un argumento de una función
                stop = index_2 + 2 + Aux.StringOut(m).IndexOf(",");
                body = s[(index_2 + 2)..stop];
            }
            
            m = Aux.StringOut(body);
            m = Regex.Replace(m, @"[^_""ñÑ,A-Za-z0-9]", " ");

            if (m.Contains(" in ")) {
                // Es posible que dentro del body tenga otro 'in' esto marcará el stop
                stop = index_2 + 2 + m.IndexOf(" in ");
                body = s[(index_2 + 2)..stop];
            }

            // Si tiene 'else' o 'elif' tiene que verificar si forma parte del 'body'
            // si no es así entonces marcará el stop
            if (m.Contains(" else ")) {
                int else_index = m.LastIndexOf(" else ");
                int if_index = m[..else_index].LastIndexOf(" if ");
            
                while (if_index != -1) {
                    else_index = m.IndexOf(" else ", if_index);
                    m = m.Remove(if_index, else_index + 5 - if_index);
                    string spaces = new(' ', else_index + 5 - if_index);
                    m = m.Insert(if_index, spaces);
                    else_index = m.LastIndexOf(" else ");
                    if (else_index == -1) break;
                    if_index = m[..else_index].LastIndexOf(" if ");
                }
            
                if (m.Contains(" else ") && If_else.IsIf_else(s[..start])) {
                    stop = index_2 + 2 + m.IndexOf(" else ");
                }

                body = s[(index_2 + 2)..stop];
            }

            m = Aux.StringOut(body);
            m = Regex.Replace(m, @"[^_""ñÑ,A-Za-z0-9]", " ");

            if (m.Contains(" elif ")) {
                int elif_index = m.LastIndexOf(" elif ");
                int if_index = m[..elif_index].LastIndexOf(" if ");
            
                while (if_index != -1) {
                    elif_index = m.IndexOf(" elif ", if_index);
                    m = m.Remove(if_index, elif_index + 5 - if_index);
                    string spaces = new(' ', elif_index + 5 - if_index);
                    m = m.Insert(if_index, spaces);
                    elif_index = m.LastIndexOf(" elif ");
                    if (elif_index == -1) break;
                    if_index = m[..elif_index].LastIndexOf(" if ");
                }
            
                if (m.Contains(" elif ") && If_else.IsIf_else(s[..(index_2 + 2 + m.IndexOf(" elif "))])) {
                    stop = index_2 + 2 + m.IndexOf(" elif ");
                }

                body = s[(index_2 + 2)..stop];
            }

            // Esta revisión la hará por cada if
            if (!vars.All(ValidVariable)) {
                // Revisa si los nombres de las variables son válidos
                return defaultValue;
            }

            // Y esta revisión solo ocurre cuando se haya llegado al último let que será cuando tenga todos los
            // datos que necesito
            if (n[..start].LastIndexOf(" let ") == -1) {
                // Se revisa que la cantidad de variables se corresponda con la de valores dados
                if(!ArgumentCount(vars, values)) {
                    return defaultValue;
                }

                // Se verifican que los valores sean correctos
                if (!ValuesLet_in(allVars, allValues, function)) {
                    return defaultValue;
                }

                // Se verifica que el 'cuerpo' sea correcto
                if (!BodyGeneral(body, vars)) {
                    return defaultValue;
                }  
            }

            // true porque no hubo errores, la expresión que se modificó al inicio, donde inicia y termina, 
            // el cuerpo, las variables y los valores
            return (true, s, start, stop, body, vars, values);
        }
    
        public static bool ValuesLet_in(List<string> vars, List<string> values, bool function = false) {
            // Este método verifica si los argumentos dados en el let-in son correctos
            List<string> newVars = new();
            List<string> newValues = new();

            for (int i = vars.Count - 1; i >= 0 ; i--)
            {
                if (!BodyGeneral(values[i], newVars)) return false;

                // Se verifican desde la primera variable que aparece con su valor, si son 
                // correctos se añaden a las listas 'newVars' y 'newValues', a partir de la 
                // segunda variable en los como valores pueden aparecer variables antes 
                // definidas, es por eso que se van teniendo en cuenta
                string evaluation = Function.Sustitution(values[i], newVars, newValues);
                string temp = function? evaluation : Control.Analize(evaluation);

                if (temp == "") return false;

                newVars.Add(vars[i]);
                newValues.Add(temp);
            }
            
            return true;
        }       
        #endregion

        #region If-Else Checher
        public static (bool, string, string, string, int, int, string) Correct_if(string s, List<string> vars = value) {
            // Este será el valor por defecto en caso de error
            (bool, string, string, string, int, int, string) valDefault = (false, "", "", "", 0, 0, "");

            // Se insertan espacios en blanco al principio y al final para la búsqueda de índices 
            s = s.Insert(0, " ");
            s = s.Insert(s.Length, " ");
            // A conveniencia se convierten los 'else-if' a 'elif'
            s = Aux.Convert_Elif(s);

            string m = Aux.StringOut(s);
            string n = Regex.Replace(m, @"[^_""ñÑA-Za-z0-9]", " ");

            // Estos índices se buscan con los espacios alrededor para evitar que se cojan índices
            // de palabras que no sean exactamente las q estamos buscando
            int index_1 = n.LastIndexOf(" if ");

            string temporary = Aux.SpacesOut(m[(index_1 + 1)..]);

            if (temporary.IndexOf('(') != 2) {
                // Se verifica si justo después del if hay un '('
                if (!Syntax("Missing '(' after 'if' in 'if-else' expression")) return valDefault;
            }

            // Este índice marcará donde termine la condicional, que en un principio será el final
            int stop = s.Length;
            
            // Todos los cambios se irán haciendo sobre otros strings para no afectar el original
            while (m[index_1] == ' ' && index_1 > 0) index_1 --;
            string temp = m;

            if (m[index_1] == '(') {
                // En caso de que la condicional esté dentro de paréntesis se modifica el stop hasta la posición
                // del paréntesis que cierra la condicional
                int indexParenthesis_1 = m.LastIndexOf("(");

                while (index_1 != indexParenthesis_1) {
                    int indexParenthesis_2 = temp.IndexOf(")", indexParenthesis_1);
                    temp = Aux.RemoveCharacter(temp, indexParenthesis_1, indexParenthesis_2);
                    indexParenthesis_1 = temp.LastIndexOf("(");
                }

                stop = temp.IndexOf(")", indexParenthesis_1);
            }

            int indexParenthesis1 = m.LastIndexOf("(");
            int index1 = m.IndexOf('(', index_1 + 1);
            temp = m;

            // Aquí se eliminan todos los paréntesis internos en la condición para identificar el
            // paréntesis que la cierra y poder extraerla completa
            while (index1 != indexParenthesis1) {
                int indexParenthesis2 = temp.IndexOf(")", indexParenthesis1);
                temp = Aux.RemoveCharacter(temp, indexParenthesis1, indexParenthesis2);
                indexParenthesis1 = temp.LastIndexOf("(");
            }

            // La condición estará delimitada por el '(' depués del if y el ')' que lo cierra
            string condition = s[(index1 + 1)..temp.IndexOf(')', index1)];

            // Si vars es null es porque se está evaluando un let-in 'normal', entonces se verá si 
            // la condición es booleana
            if (vars is null || !vars.Contains(condition)) {
                string type = Aux.ExpressionType(condition);
                if (type != "boolean" || !BodyDetails(condition)) {
                    if(!Semantic($"It is not possible to convert from {type} to boolean")) return valDefault;
                }
            }

            int elif = n.IndexOf(" elif ", temp.IndexOf(')', index1));
            int _else = n.IndexOf(" else ", temp.IndexOf(')', index1));
            int index_2 = (elif != -1)? Math.Min(elif, _else) + 1 : _else + 1;

            // Si en la expresión no existe un 'else' es un error, es de carácter obligatorio
            if (index_2 == 0 || stop < index_2) {
                if (!Syntax("Missing 'else' in 'if-else' expression")) return valDefault;
            }
            
            int start_body = index1 + condition.Length + 2;
            // El 'body_true' corresponde al cuerpo que resulta de la evalución true de la condición,
            // en caso del 'body_false', lo contrario
            string body_true = s[start_body..index_2];

            if (Aux.ParenthesisBalance(body_true) == -1) {
                if (!Syntax($"Missing ')' in '{body_true.Trim()}'")) return valDefault;
            }

            // Si como 'index_2' se tiene un 'elif' en el body false quiero mantener 'if...' para 
            // seguir evaluando en caso de ser escogido ese 'body'
            int start = (s[index_2..(index_2 + 4)] == "elif") ? (index_2 + 2) : (index_2 + 4);
            string body_false = s[start..stop];

            // Si alguno de los dos 'cuerpos' es vacío es un error
            if (string.IsNullOrWhiteSpace(body_true) || string.IsNullOrWhiteSpace(body_false)) {
                string mssg = string.IsNullOrWhiteSpace(body_true)? $"if({condition})" : "else";
                if (!Semantic($"Missing expression after '{mssg}' in 'if-else' instruction")) return valDefault;
            }
            
            m = Aux.FunctionsOut(body_false);

            // Si quitando las funciones el 'body_false' contiene ',' es porque es un argumento de una función
            if (m.Contains(",")) {
                stop = start + Aux.StringOut(m).IndexOf(",");
                body_false = s[start..stop];
            }

            // Si en el 'body_false' hay ')' es porque se empezó con '(' por tanto debe modificarse el stop
            // por tantos ')' tenga
            while (Aux.ParenthesisBalance(m) == 1) {
                stop = start + Aux.StringOut(m).LastIndexOf(")");
                body_false = s[start..stop];
                m = body_false;
            }

            m = Aux.StringOut(body_false);
            m = Regex.Replace(m, @"[^_""ñÑ,A-Za-z0-9]", " ");

            // Es válido que pueda tener un 'else' o un 'elif' en ese caso eso marcará el stop
            if (m.Contains(" else ") && (start == (index_2 + 4))) {
                stop = start + Aux.StringOut(m).LastIndexOf(" else ");
                body_false = s[start..stop];
            }

            if (m.Contains(" elif ") && (start == (index_2 + 4))) {
                stop = start + Aux.StringOut(m).LastIndexOf(" elif ");
                body_false = s[start..stop];
            }

            // Si ambos 'cuerpos' pasan la revisión general y detallada entonces se devuelve:
            if (!General(body_true) || !General(body_false)) return valDefault;
            if (!BodyDetails(body_true) || !BodyDetails(body_false)) return valDefault;

            // true porque no hubo errores, la conndición, el 'body_true' y el 'body_false', donde inicia y termina, 
            // además de la expresión que se modificó al inicio 
            return (true, condition, body_true, body_false, index_1, stop, s);
        }
        #endregion

        #region Reset values     
        public static void Reset() {
            // Este método restaura datos por cada entrada
            Control.error = false;
            Control.Mssg = "";
            Function.calls = new();
            funcVars = new();
            vars = new();
            funcName = "";
        }

        #endregion
    }
}