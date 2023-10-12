using System.Text.RegularExpressions;

namespace Hulk
{
    public class Error 
    {
        public static bool error = false;
        public static List<string> funcVars = new();
        public static List<string> vars = new();
        private const List<string> value = null!;
        public static string funcName = "";

        #region Errors Messages
        public static bool MissingMember(string symbol, string leftMember, string rightMember) {
            bool empty_L = string.IsNullOrWhiteSpace(leftMember);
            bool empty_R = string.IsNullOrWhiteSpace(rightMember);

            if (symbol == "!") {
                if (!empty_L || empty_R) {
                    string message = (!empty_L)? "can not have a left" : "needs a right";
                    Console.WriteLine($"!!SEMANTIC ERROR: ");
                    return Semantic($"Operator '!' {message} member");
                }

                return true;
            }

            if (!string.IsNullOrWhiteSpace(leftMember) & !string.IsNullOrWhiteSpace(rightMember)) return true;

            string member = string.IsNullOrWhiteSpace(leftMember)? "left" : "right"; 
            return Semantic($"Operator '{symbol}' needs a '{member}' member");
        }
   
        public static bool Syntax(string mssg) {
            if (error) return false;
            error = true;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"!!SYNTAX ERROR: {mssg}");
            return false;
        }
        
        public static bool Semantic(string mssg) {
            if (error) return false;
            error = true;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"!!SEMANTIC ERROR: {mssg}");
            return false;
        }

        public static bool Lexical(string mssg) {
            if (error) return false;
            error = true;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"!!LEXICAL ERROR: {mssg}");
            return false;
        }
        #endregion

        #region Genral y operations checker

        public static bool General(string s) {
            
            s = Aux.SpacesOut(s);

            if (If_else.IsIf_else(s) || Let_in.IsLet_in(s)) return true;

            if (error) return false;

            string temp = Aux.StringOut(s);

            if (Aux.FunctionsOut(temp).Contains(",")) {
                return Syntax("Unexpected ','");
            }

            int index = temp.IndexOf("."); 
            while (index != -1) { 
                if ((index > 0 && (!Aux.IsNumber(temp[index - 1].ToString()) || temp[index - 1] == ' ')) || !Aux.IsNumber(s[index + 1].ToString())) {
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
                if (n[i] != '∞' && !char.IsAscii(n[i]) || char.IsPunctuation(n[i])) {
                    if (simpleSymbols.Contains(n[i].ToString())) continue;
                    return Syntax($"Unexpected '{n[i]}'");
                }
            }
            
            foreach (var item in doubleSymbols) n = n.Replace(item, $" {item} ");
            foreach (var item in simpleSymbols) n = n.Replace(item, $" {item} ");
            
            n = n.Insert(n.Length, " ");
            index = n.IndexOf(" ");

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
                if (tokens[i] == "function" || tokens[i + 1] == "function") {
                    return Syntax("Invalid 'function' instruction");
                }

                if (tokens[i] == ")" && (tokens[i + 1].Any(char.IsLetterOrDigit) || 
                    tokens[i + 1].Any(char.IsPunctuation))) {

                    if (simpleSymbols.Contains(tokens[i + 1]) && tokens[i + 1] != "(") continue;
                    if (tokens[i + 1] == "else") continue;

                    if(tokens[i + 1].Contains("\"")) tokens[i + 1] = "string";
                    return Syntax($"Unexpected '{tokens[i + 1]}' after ')'");
                }

                if (i > 0 && tokens[i] == "(" && tokens[i - 1].Any(char.IsPunctuation)) {

                    if (simpleSymbols.Contains(tokens[i - 1])) continue;

                    if(tokens[i + 1].Contains("\"")) tokens[i - 1] = "string";
                    return Syntax($"Unexpected '(' after '{tokens[i - 1]}'");
                }

                else if (tokens[i].Any(char.IsLetterOrDigit) && tokens[i + 1].Any(char.IsLetterOrDigit) ) { 
            
                    if (simpleSymbols.Contains(tokens[i]) || simpleSymbols.Contains(tokens[i + 1])) continue;
                    if (tokens[i] == "else" || tokens[i + 1] == "else") continue;

                    return Syntax($"Unexpected '{tokens[i + 1]}' after '{tokens[i]}'");
                }

                else if (tokens[i].Any(char.IsPunctuation) && tokens[i + 1].Any(char.IsPunctuation)) { 
            
                    if (simpleSymbols.Contains(tokens[i]) || simpleSymbols.Contains(tokens[i + 1])) continue;

                    if(tokens[i + 1].Contains("\"")) tokens[i + 1] = "string";
                    if(tokens[i].Contains("\"")) tokens[i + 1] = "string";

                    return Syntax($"Unexpected '{tokens[i + 1]}' after '{tokens[i]}'");
                }
            }
            
            return true;
        }
        public static bool Operations(string leftSide, string rightSide, string symbol) {
            string[] binaries = {"+", "-", "*", "/", "%", "^"};
            string[] booleans1 = {">", "<", "<=", ">="};
            string[] booleans2 = {"&", "|"};
            string[] booleans3 = {"!=", "=="};

            bool passBodySyntax_L = BodyDetails(leftSide);
            bool passBodySyntax_R = BodyDetails(rightSide);
            string leftType = Aux.ExpressionType(leftSide);
            string rightType = Aux.ExpressionType(rightSide);

            if (symbol == "!") {
                if (!MissingMember(symbol, leftSide, rightSide)) 
                return false;
                
                if(passBodySyntax_R) {
                    if (rightType == "boolean") 
                    return true;

                    return Semantic($"Operator '!' cannot used by {rightType}");
                }

                return false;
            }

            if ((symbol == "+" || symbol == "-") && string.IsNullOrWhiteSpace(leftSide) && rightType == "number")
            return true;

            else if (!MissingMember(symbol, leftSide, rightSide))
            return false;

            if (symbol == "@") {
                if (passBodySyntax_L && passBodySyntax_R) {
                    if (leftType != "string" && rightType != "string") {
                        return Semantic($"Operator {symbol} cannot be used between {leftType} and {rightType}");
                    } 

                    return true;
                }

                return false;
            }

            if (binaries.Contains(symbol)) {
                if (symbol == "+" || symbol == "-") {
                    if (passBodySyntax_L && passBodySyntax_R) {
                        string right = (rightSide.StartsWith('+') || rightSide.StartsWith('-'))
                                        ? rightSide[1..] : rightSide;
                        string left = (leftSide.StartsWith('+') || leftSide.StartsWith('-'))
                                        ? leftSide[1..] : leftSide;
                        leftType = Aux.ExpressionType(left);
                        rightType = Aux.ExpressionType(right);

                        if (leftType == "number" && rightType == "number") {
                            return true;
                        }
                            
                        return Semantic($"Operator {symbol} cannot be used between {leftType} and {rightType}");
                    }

                    return false;
                }

                if (passBodySyntax_L && passBodySyntax_R) {
                    if (leftType == "number" && rightType == "number") {

                        if ((symbol == "/" || symbol == "%") && double.Parse(rightSide) == 0) {
                            if (Error.error) return false;
                            Error.error = true;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"!!SEMANTIC ERROR: Division by 0 is not defined");
                            return false;
                        }

                        return true;
                    }

                    return Semantic($"Operator {symbol} cannot be used between {leftType} and {rightType}");
                }

                return false;
            }

            if (booleans1.Contains(symbol)) {
                if (passBodySyntax_L && passBodySyntax_R) {
                    if (leftType == "number" && rightType == "number") {
                        return true;
                    }   

                    return Semantic($"Operator {symbol} cannot be used between {leftType} and {rightType}");
                }

                return false;   
            }

            if (booleans2.Contains(symbol)) {
                if (passBodySyntax_L && passBodySyntax_R) {
                    if (leftType == "boolean" && rightType == "boolean") {
                        return true;
                    }    

                    return Semantic($"Operator {symbol} cannot be used between {leftType} and {rightType}");
                }

                return false;
            }

            if (booleans3.Contains(symbol)) {
                if(passBodySyntax_L && passBodySyntax_R) {
                    if (leftType == rightType) {
                        return true;
                    }

                    return Semantic($"Operator {symbol} cannot be used between {leftType} and {rightType}");
                }

                return false;
            }

            return true;
        }        
        #endregion

        #region Function Checker
        public static bool FunctionGeneral(string s) {

            string n = Aux.StringOut(s);

            if (!n.Contains("(")) {
                return Syntax("'(' was expected after the 'function name'");
            }

            string funcName = Aux.SpacesOut(s[..s.IndexOf("(")]);

            if (funcName == "") {
                return Syntax("Missing name before '('");
            }

            s = Aux.SpacesOut(s[(n.IndexOf("(") + 1)..]);
            n = Aux.SpacesOut(n[(n.IndexOf("(") + 1)..]);
           
            string argument = s[..n.IndexOf(")")];
            n = n[..n.IndexOf(")")];
            string body = Aux.SpacesOut(s[(argument.Length + 1)..]);

            if (n.Contains("(")) {
                return Syntax("Unexpected '()' in the argument of the function");
            }
            
            if (!body.StartsWith("=>")) {
                return Syntax($"'=>' was expected after '{funcName}({argument})'");
            }

            List<string> vars = new();
            string temp = argument;
            int tempIndex = n.IndexOf(",");

            while (tempIndex != -1) {
                vars.Add(temp[..tempIndex]);
                temp = temp.Remove(0, tempIndex + 1);
                n = n.Remove(0, tempIndex + 1);
                tempIndex = n.IndexOf(",");
            }

            vars.Add(temp);


            if (!Function.input.ContainsKey(funcName + "(")) {
                Function.input[funcName + "("] = new();
                Function.output[funcName + "("] = "all";

                for (int i = 0; i < vars.Count; i++) {
                    Function.input[funcName + "("].Add("all");
                }
            } 

            for (int i = 0; i < vars.Count; i++) vars[i] = vars[i].Trim();

            if (!ValidVariable(funcName)) {
                if (!Function.existFunctions.Contains(funcName + "(")) {
                    Function.input.Remove(funcName + "(");
                    Function.output.Remove(funcName + "(");
                }

                return false;
            }

            if (vars.Distinct().Count() != vars.Count) {
                if (!Function.existFunctions.Contains(funcName + "(")) {
                    Function.input.Remove(funcName + "(");
                    Function.output.Remove(funcName + "(");
                }

                return Semantic("One argument is used more than once");
            }
            
            if (vars.Contains(funcName)) {
                if (!Function.existFunctions.Contains(funcName + "(")) {
                    Function.input.Remove(funcName + "(");
                    Function.output.Remove(funcName + "(");
                }

                return Semantic($"'{funcName}' is the function name. It can not be a variable");
            }

            if (argument != "" && vars.Any(string.IsNullOrWhiteSpace)) {
                if (!Function.existFunctions.Contains(funcName + "(")) {
                    Function.input.Remove(funcName + "(");
                    Function.output.Remove(funcName + "(");
                }

                return Semantic($"One argument is missing in '{funcName}' function");
            }

            if (argument != "" && !vars.All(ValidVariable)) {
                if (!Function.existFunctions.Contains(funcName + "(")) {
                    Function.input.Remove(funcName + "(");
                    Function.output.Remove(funcName + "(");
                }

                return false;
            }
            if (body == "=>") {
                if (!Function.existFunctions.Contains(funcName + "(")) {
                    Function.input.Remove(funcName + "(");
                    Function.output.Remove(funcName + "(");
                }

                return Syntax("The body was not defined");
            }

            if (Let_in.IsLet_in(body)) {
                funcVars = vars;
                Function.keyWords.AddRange(vars);
                body = Let_in.Eval(body, true);
                Function.keyWords.RemoveRange(Function.keyWords.Count - vars.Count, vars.Count);
                funcVars = new();
                if (body == "") {
                    if (!Function.existFunctions.Contains(funcName + "(")) {
                        Function.input.Remove(funcName + "(");
                        Function.output.Remove(funcName + "(");
                    }
                    return false;
                }
            }

            if (String.IsString(body[2..])) {
                temp = String.TraduceString(body[2..]);
                if (temp == "") {
                    if (!Function.existFunctions.Contains(funcName + "(")) {
                        Function.input.Remove(funcName + "(");
                        Function.output.Remove(funcName + "(");
                    }
 
                    return false;
                }
            }

            body = Aux.StringOut(body);
            Error.vars = new();
            Error.funcName = "";

            if (!BodyGeneral(body[2..], vars, funcName))
            return false;

            if (!BodyDetails(body[2..], funcName, vars)) {
                if (!Function.existFunctions.Contains(funcName + "(")) {
                    Function.input.Remove(funcName + "(");
                    Function.output.Remove(funcName + "(");
                }

                return false;
            }

            return true;
        }

        public static bool BodyGeneral(string body, List<string> vars, string funcName = "") {
            
            if (!General(body)) return false;

            if (string.IsNullOrWhiteSpace(body)) {
                return Syntax("Missing body after 'in' in 'let-in' expression");
            }

            body = $" {Aux.StringOut(body)} "; 
            
            if (funcName != "" && body.Contains($" {funcName} ")) {
                return Syntax($"Invalid token '{funcName}'");
            }

            if (Function.keyWords.Any(x => Function.keyWords.IndexOf(x) <= 15 && body.Trim() == x) && 
                body.Replace(" ", "").ToLower() != "true" &&  body.Replace(" ", "").ToLower() != "false" &&
                body.Trim() != "PI" &&  body.Trim() != "E") {
                    return Syntax($"Invalid token '{body.Trim()}'");
            }

            body = Regex.Replace(body, @"[^_ñÑA-Za-z0-9]", " ");
            string[] words = body.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            List<string> wrongVars = new();
            foreach (string word in words)
            {
                if (!vars.Contains(word) && word != funcName && !Hulk.Function.existFunctions.Contains(word + "(") 
                    && !Hulk.Function.keyWords.Contains(word) && !Aux.IsNumber(word) && !String.IsString(word)) {
                    
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
            body = Aux.StringOut(body);
            
            variables ??= new();
            if (vars.Count == 0) vars.AddRange(variables);
            if (funcName == "") funcName = name; 

            if (If_else.IsIf_else(body)) {
                (bool, string, string, string, int, int, string) conditionalData = Correct_if(body, vars);
                
                if (!conditionalData.Item1) return false;
                
                if (vars.Contains(conditionalData.Item2)) {
                    Function.input[funcName + "("][vars.IndexOf(conditionalData.Item2)] = "boolean";
                }

                string body_true = conditionalData.Item3;
                string body_false = conditionalData.Item4;
                int index_1 = conditionalData.Item5;
                int stop = conditionalData.Item6;
                body = conditionalData.Item7;
                string body1 = body;
                string body2 = body;

                body1 = body1.Remove(index_1 + 1, stop - index_1 - 1);
                body1 = body1.Insert(index_1 + 1, $"{body_true}");
                body2 = body2.Remove(index_1 + 1, stop - index_1 - 1);
                body2 = body2.Insert(index_1 + 1, $"{body_false}");

                bool check1 = BodyDetails(body1) && BodyDetails(body2);
                if (!check1) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("!!!! Possible invalid type in conditional !!!!");
                }

                return check1;
            } 
            
            Dictionary<string, string> defaultValues = new() {
                {"string", "\" \""}, {"number", "2"}, {"boolean", "true"}, 
                {"undefined expression", ""}, {"all", ")("}
            };

            while (body.Replace(")(", "  ").Contains('(')) {
                
                int index1 = body.Replace(")(", "  ").LastIndexOf("(");
                int index2 = body.Replace(")(", "  ").IndexOf(")", index1);
                string parenthesis = body[(index1 + 1)..index2];

                if (parenthesis.Contains(",") || BodyDetails(parenthesis, funcName, vars)) {
                    
                    (bool, string, string[]) data = Aux.FunctionInfo(body, index1, index2);

                    if (data.Item1) {

                        if (data.Item2 != funcName && !Function.input.ContainsKey(data.Item2)) {
                            return Semantic($"'{data.Item2[..^1]}' is not a valid function");
                        }

                        if (!ArgumentCount(Hulk.Function.input[data.Item2], data.Item3.ToList(), data.Item2)) return false;

                        if (funcName + "(" == data.Item2) {
                            body = body.Remove(index1, parenthesis.Length + 2);
                            Array.Resize(ref data.Item3, data.Item3.Length + 1);
                            data.Item3[^1] = funcName;
                            vars.Add(funcName);
                            Function.input[funcName + "("].Add("all");
                        }
                        
                        for (int i = 0; i < data.Item3.Length; i++) {
                            
                            if (data.Item3[i] == "") break;
                            if (!BodyDetails(data.Item3[i], funcName, vars)) return false;

                            if (data.Item2 == "print(") {
                                body = body.Remove(index2 , 1);
                                body = body.Remove(index1 - 5, 6);
                                break;
                            }

                            if (vars.Contains(data.Item3[i].Trim()) && Function.input[data.Item2][i] != "all") {
                                Function.input[funcName + "("][i] = Function.input[data.Item2][i];
                                data.Item3[i] = defaultValues[Function.input[data.Item2][i]];
                            }

                            else if (Function.input[data.Item2][i] == "all") {
                                data.Item3[i] = "2";
                            } 

                            // else if (!Aux.IsNumber(data.Item3[i])) {
                            //     data.Item3[i] = defaultValues[Aux.ExpressionType(data.Item3[i])];
                            // }
                        }

                        if (!Hulk.Function.existFunctions.Contains(data.Item2)) Hulk.Function.functions[data.Item2] = "";

                        if (funcName + "(" == data.Item2) {
                            Array.Resize(ref data.Item3, data.Item3.Length - 1);
                            Hulk.Function.input[funcName + "("].RemoveAt(Hulk.Function.input[funcName + "("].Count - 1); 
                        }

                        if (Restrictions(data.Item2, parenthesis, data.Item3)) {
                            if (!Function.existFunctions.Contains(data.Item2)) Hulk.Function.functions.Remove(data.Item2);
                            if (data.Item2 != funcName + "(" && data.Item2 != "print(") {
                                int start = index1  + 1 - data.Item2.Length;
                                body = body.Remove(start, data.Item2.Length + parenthesis.Length + 1);

                                if (Function.existFunctions.Contains(data.Item2)) {
                                    body = body.Insert(start, defaultValues[Hulk.Function.output[data.Item2]]);
                                }

                                else body = body.Insert(start, funcName);
                            }
                        }

                        else {
                            return false;
                        }
                    }

                    else {
                        string val = defaultValues[Aux.ExpressionType(parenthesis)];
                        if (val == "") val = parenthesis;
                        body = body[..index1] + val + body[(index2 + 1)..];
                    }
                }

                else return false;
            }

            if (body.Contains('@')) {
                int index = body.LastIndexOf('@');
                string left = Aux.SpacesOut(body[..index]);
                string right = Aux.SpacesOut(body[(index + 1)..]);
                left = (left == ")(")? "\" \"" : left;
                right = (right == ")(")? "\" \"" : right;

                if (vars.Contains(left)) {

                    if (left == funcName) { 
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = "string";
                        }

                        else if (Hulk.Function.output[funcName + "("] != "string") {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return string and {type}");
                        } 
                    }   

                    left = defaultValues["string"]; 
                } 

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = "string";
                        }

                        else if (Hulk.Function.output[funcName + "("] != "string") {
                            string type = Hulk.Function.output[funcName + "("];
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

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(left)] = "boolean";
                        left = defaultValues["boolean"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "boolean") {
                        left = defaultValues["boolean"];    
                    }

                    else {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(left)];
                        return Semantic($"Varibale '{left}' can not be boolean and {type}");
                    }      
                }

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = "boolean";
                        }

                        else if (Hulk.Function.output[funcName + "("] != "boolean") {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return boolean and {type}");
                        }

                        right = defaultValues["boolean"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(right)] = "boolean";
                        right = defaultValues["boolean"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "boolean") {
                        right = defaultValues["boolean"];    
                    }

                    else {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($" Varibale '{right}' can not be boolean and {type}");
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
                string typeR = vars.Contains(right) ? Hulk.Function.input[funcName + "("][vars.IndexOf(right)] : Aux.ExpressionType(right);
                string typeL = vars.Contains(left) ? Hulk.Function.input[funcName + "("][vars.IndexOf(left)] : Aux.ExpressionType(left);

                if (vars.Contains(left)) {
                    if (left == funcName) { 
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = typeR;
                            typeL = typeR;
                        }

                        else if (Hulk.Function.output[funcName + "("] != typeR) {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return {typeR} and {type}");
                        }

                        left = defaultValues[typeR]; 
                    }

                    if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(left)] = typeR;
                        typeL = typeR;
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] != typeR) {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(left)];
                        return Semantic($"Varibale '{left}' can not be {typeR} and {type}");
                    }

                    left = defaultValues[typeR];   
                }

                if (vars.Contains(right)) {
                    if (right == funcName) { 
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = typeL;
                            typeR = typeL;
                        }

                        else if (Hulk.Function.output[funcName + "("] != typeL) {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return {typeL} and {type}");
                        }

                        right = defaultValues[typeL]; 
                    }

                    if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(right)] = typeL;
                        typeR = typeL;
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] != typeL) {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($"Varibale '{right}' can not be {typeL} and {type}");
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
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = "number";
                        }

                        else if (Hulk.Function.output[funcName + "("] != "number") {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return number and {type}");
                        }

                        left = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(left)] = "number";
                        left = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "number") {
                        left = defaultValues["number"];    
                    }

                    else {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(left)];
                        return Semantic($"Varibale '{left}' can not be number and {type}");
                    }      
                }

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = "number";
                        }

                        else if (Hulk.Function.output[funcName + "("] != "number") {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return number and {type}");
                        }

                        right = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(right)] = "number";
                        right = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "number") {
                        right = defaultValues["number"];    
                    }

                    else {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($"Varibale '{right}' can not be number and {type}");
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
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = "number";
                        }

                        else if (Hulk.Function.output[funcName + "("] != "number") {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return number and {type}");
                        }

                        left = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(left)] = "number";
                        left = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "number") {
                        left = defaultValues["number"];    
                    }

                    else {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(left)];
                        return Semantic($"Varibale '{left}' can not be number and {type}");
                    }      
                }

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = "number";
                        }

                        else if (Hulk.Function.output[funcName + "("] != "number") {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return number and {type}");
                        }

                        right = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(right)] = "number";
                        right = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "number") {
                        right = defaultValues["number"];    
                    }

                    else {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($"Varibale '{right}' can not be number and {type}");
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
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = "boolean";
                        }

                        else if (Hulk.Function.output[funcName + "("] != "boolean") {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return boolean and {type}");
                        }

                        right = defaultValues["boolean"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(right)] = "boolean";
                        right = defaultValues["boolean"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "boolean") {
                        right = defaultValues["boolean"];    
                    }

                    else {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($"Varibale '{right}' can not be boolean and {type}");
                    }
                }

                return Operations(left, right, "!");
            }

            body = Aux.SpacesOut(body);
            body = Aux.InternalSpaces(body);

            if (body.Contains('+') || body.Contains('-')) {
                
                while (body.Contains("+-") || body.Contains("-+") || body.Contains("++") || body.Contains("--")) {
                    body = body.Replace("+-","-");
                    body = body.Replace("-+","-");
                    body = body.Replace("++","+");
                    body = body.Replace("--","+");
                }

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
                            if (Hulk.Function.output[funcName + "("] == "all") {
                                Hulk.Function.output[funcName + "("] = "number";
                            }

                            else if (Hulk.Function.output[funcName + "("] != "number") {
                                string type = Hulk.Function.output[funcName + "("];
                                return Semantic($"'{left}' can not return number and {type}");
                            }

                            left = defaultValues["number"]; 
                        }

                        else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                            Hulk.Function.input[funcName + "("][vars.IndexOf(left)] = "number";
                            left = defaultValues["number"]; 
                        }

                        else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "number") {
                            left = defaultValues["number"];    
                        }

                        else {
                            string type = Hulk.Function.input[funcName + "("][vars.IndexOf(left)];
                            return Semantic($"Variable '{left}' can not be number and {type}");
                        }      
                    }

                    if (vars.Contains(right)) {

                        if (right == funcName) { 
                            if (Hulk.Function.output[funcName + "("] == "all") {
                                Hulk.Function.output[funcName + "("] = "number";
                            }

                            else if (Hulk.Function.output[funcName + "("] != "number") {
                                string type = Hulk.Function.output[funcName + "("];
                                return Semantic($"'{right}' can not return number and {type}");
                            }

                            right = defaultValues["number"]; 
                        }

                        else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                            Hulk.Function.input[funcName + "("][vars.IndexOf(right)] = "number";
                            right = defaultValues["number"]; 
                        }

                        else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "number") {
                            right = defaultValues["number"];    
                        }

                        else {
                            string type = Function.input[funcName + "("][vars.IndexOf(right)];
                            return Semantic($"Varibale '{right}' can not be number and {type}");
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
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = "number";
                        }

                        else if (Hulk.Function.output[funcName + "("] != "number") {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return number and {type}");
                        }

                        left = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(left)] = "number";
                        left = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "number") {
                        left = defaultValues["number"];    
                    }

                    else {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(left)];
                        return Semantic($"Varibale '{left}' can not be number and {type}");
                    }      
                }

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = "number";
                        }

                        else if (Hulk.Function.output[funcName + "("] != "number") {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return number and {type}");
                        }

                        right = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(right)] = "number";
                        right = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "number") {
                        right = defaultValues["number"];    
                    }

                    else {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($"Varibale '{right}' can not be number and {type}");
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
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = "number";
                        }

                        else if (Hulk.Function.output[funcName + "("] != "number") {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{left}' can not return number and {type}");
                        }

                        left = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(left)] = "number";
                        left = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(left)] == "number") {
                        left = defaultValues["number"];    
                    }

                    else {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(left)];
                        return Semantic($"Varibale '{left}' can not be number and {type}");
                    }      
                }

                if (vars.Contains(right)) {

                    if (right == funcName) { 
                        if (Hulk.Function.output[funcName + "("] == "all") {
                            Hulk.Function.output[funcName + "("] = "number";
                        }

                        else if (Hulk.Function.output[funcName + "("] != "number") {
                            string type = Hulk.Function.output[funcName + "("];
                            return Semantic($"'{right}' can not return number and {type}");
                        }

                        right = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "all") {

                        Hulk.Function.input[funcName + "("][vars.IndexOf(right)] = "number";
                        right = defaultValues["number"]; 
                    }

                    else if (Hulk.Function.input[funcName + "("][vars.IndexOf(right)] == "number") {
                        right = defaultValues["number"];    
                    }

                    else {
                        string type = Hulk.Function.input[funcName + "("][vars.IndexOf(right)];
                        return Semantic($"Varibale '{right}' can not be number and {type}");
                    }
                }

                return Operations(left, right, "^");
            }

            return true;
        }

        public static bool Restrictions(string f, string argument, string[] args) {

            if (!Function.functions.ContainsKey(f)) {

                if (f == "rand(" && argument != "") {
                    return Semantic("Any argument was expected in 'rand' function");
                }

                if (f == "rand(") return true;

                if (argument == "") {
                    return Semantic($"Any argument was given in '{f[..^1]}' function");
                }

                if (args.Any(string.IsNullOrWhiteSpace)) {
                    return Semantic($"Invalid empty argument in '{f[..^1]}' function");
                }

                if (args.Length > 1 && f != "log(") {
                    return Semantic($"1 argument was expected but {args.Length} were given in '{f[..^1]}' function");
                }
            }

            if (args.Any(x => Aux.ExpressionType(x) != Function.input[f][Array.IndexOf(args, x)] &&
                Function.input[f][Array.IndexOf(args, x)] != "all")) {

                string not = args.First(x => Aux.ExpressionType(x) != Function.input[f][Array.IndexOf(args, x)]);
                string type = Function.input[f][Array.IndexOf(args, not)];
                not = Aux.ExpressionType(not);
                return Semantic($"Function '{f[..^1]}' recieves {type}, not {not}");
            }

            if (f == "log(") {

                if (args.Length > 2) {
                    return Semantic($"2 arguments were expected at most but {args.Length} were given in 'log' function");
                }

                if (args.Any(string.IsNullOrWhiteSpace)) {
                    return Semantic($"2 arguments were expected but 1 was given in 'log' function");
                }
                
                if (args.Length == 1 && double.Parse(args[0]) <= 0) {
                    return Semantic($"Argument must be greater than '0' in 'log' function");
                }

                if (args.Length > 1) {
                    if (double.Parse(args[1]) <= 0) {
                        return Semantic($"Argument must be greater than '0' in 'log' function");
                    }

                    if (double.Parse(args[0]) <= 0 || double.Parse(args[0]) == 1) {
                        return Semantic($"Base must be greater than '0' and diferent of '1' in 'log' function");
                    }
                }
            }

            return true;
        }

        public static bool ArgumentCount(List<string> vars, List<string> values, string funcName = "") {
            
            string mssg = (funcName != "")? "argument" : "value";
            string argument = (vars.Count == 1)? $"{mssg} was" : $"{mssg}s were";
            string was = (values.Count == 1)? "was" : "were";
            string f = (funcName == "")? "" : $"in '{funcName[..^1]}' function";

            if (vars.Contains("") && !values.Contains("")) {
                return Semantic($"Any {mssg} was expected but {values.Count} {was} given {f}");
            }

            if (values.Contains("") && !vars.Contains("")) {
                return Semantic($"Invalid empty {mssg} given {f}");
            }

            if (vars.Count != values.Count) {
                return Semantic($"{vars.Count} {argument} expected but {values.Count} {was} given {f}");
            }

            return true;
        }

        #endregion

        #region Variable Checker

        public static bool ValidVariable(string var) {
            if (error) return false;

            if (funcVars.Contains(var)) {
                return Syntax($"'{var}' is already defined as a variable of the function");
            }

            if (Hulk.Function.keyWords.Contains(var)) {
                return Syntax($"'{var}' is a keyword");
            }

            if (Hulk.Function.existFunctions.Contains(var + "(")) {
                return Syntax($"'{var}' is already defined");
            }

            if (char.IsDigit(var[0]) || var.ToLower() != Regex.Replace(var.ToLower(), @"[^_ña-z0-9]", "")) {
                return Lexical($"'{var}' is wrong. Variable and function names only can be written with letters, \nnumbers (not as first character) and '_'");
            }

            return true;
        }

        #endregion

        #region Let-in Checker

        public static (bool, string, int, int, string, List<string>, List<string>) Correct_Let(string s, bool function = false) {
            string[] symbols = {"*", "/", "^", "%", "+", "-", "(", ")", ">", "<", "&","|","!", ",", "=", " "};
            (bool, string, int, int, string, List<string>, List<string>) defaultValue = (false, "", 0, 0, "", null, null)!;
            
            s = s.Insert(0, " ");
            s = s.Insert(s.Length, " ");

            List<string> allVars = new();
            List<string> allValues = new();
            
            string m = Aux.StringOut(s);
            string n = Regex.Replace(m, @"[^_""ñÑA-Za-z0-9]", " ");
            
            int start = n.LastIndexOf(" let ");

            // Aquí se comprueba que lo que antecede a la condicional sea correcto
            if (s[..(start + 1)].Trim() != "" && s[..(start + 1)].Trim()[^2..] != "in" && !symbols.Contains(s[..(start + 1)].Trim()[^1].ToString())) {
                if (!Syntax("Unexpected expression before 'let-in' instruction")) return defaultValue;
            }
            
            int index_2 = n.IndexOf(" in ", start) + 1;
        
            int stop = s.Length;

            if (m[start] == '(') {
                int indexParenthesis_1 = m.LastIndexOf("(");
                string temp = m;

                while (start != indexParenthesis_1) {
                    int indexParenthesis_2 = temp.IndexOf(")", indexParenthesis_1);
                    temp = Aux.RemoveCharacter(temp, indexParenthesis_1, indexParenthesis_2);
                    indexParenthesis_1 = temp.LastIndexOf("(");
                }

                stop = temp.IndexOf(")", indexParenthesis_1);
            }

            if (index_2 == 0 || stop < index_2) {
                if (!Error.Syntax("Missing 'in' in 'let-in' expression")) return defaultValue;
            }

            string variables = m[(start + 4)..index_2];

            if (Aux.ParenthesisBalance(variables) == -1) {
                if (!Error.Syntax("Invalid token ')' after 'in' in 'let-in' expression")) return defaultValue;
            }

            int tempIndex = Aux.FunctionsOut(variables).IndexOf(",");
            List<string> argument = new(); 

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
                if (string.IsNullOrWhiteSpace(argument[i])) {
                    if (!Error.Semantic("Missing variable declaration")) return defaultValue;
                }

                int equal = argument[i].IndexOf("=");

                if (equal == -1) {
                    if (!Error.Syntax($"Missing '=' in '{argument[i].Trim()}' declaration")) return defaultValue;
                }

                if (argument[i][..equal].Contains("(")) {
                    if (!Error.Syntax("Invalid token '(' in variable in 'let-in' expression")) return defaultValue;
                }

                int lenghtVar = argument[i][..equal].Length;
                int index_1 = m.IndexOf(argument[i]) + start + 4;
                string var = s.Substring(index_1, lenghtVar);   
                
                if (!Error.General(var)) return defaultValue;

                if (string.IsNullOrWhiteSpace(var)) {
                    if (!Error.Semantic("Missing variable in 'let-in' expression")) return defaultValue;
                }

                vars.Add(var.Trim());
                allVars.Add(var.Trim());

                int lengthValue = argument[i][(equal + 1)..].Length;
                index_1 = m.IndexOf(argument[i]) + start + lenghtVar + 5;
                string val = s.Substring(index_1, lengthValue);

                if (!Error.General($"({val})")) return defaultValue;
                
                if (val.Replace(" ","") != "()") {
                    values.Add(val.Trim());
                    allValues.Add(val.Trim());
                }
            }

            string body = s[(index_2 + 2)..stop];
            m = Aux.FunctionsOut(body);
            
            if (m.Contains(",")) {
                stop = index_2 + 2 + Aux.StringOut(m).IndexOf(",");
                body = s[(index_2 + 2)..stop];
            }
            
            m = Aux.StringOut(body);
            m = Regex.Replace(m, @"[^_""ñÑ,A-Za-z0-9]", " ");

            if (m.Contains(" in ")) {
                stop = index_2 + 2 + m.IndexOf(" in ");
                body = s[(index_2 + 2)..stop];
            }

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

            if (!vars.All(Error.ValidVariable)) {
                return defaultValue;
            }

            if (n[..start].LastIndexOf(" let ") == -1) {

                if(!Error.ArgumentCount(vars, values)) {
                    return defaultValue;
                }

                if (!Error.BodyLet_in(allVars, allValues, function)) {
                    return defaultValue;
                }

                if (!Error.BodyGeneral(body, vars)) {
                    return defaultValue;
                }  
            }

            return (true, s, start, stop, body, vars, values);
        }
    
        public static bool BodyLet_in(List<string> vars, List<string> values, bool function = false) {
            List<string> newVars = new();
            List<string> newValues = new();

            for (int i = vars.Count - 1; i >= 0 ; i--)
            {
                if (!BodyGeneral(values[i], newVars)) return false;

                string evaluation = Function.Sustitution(values[i], newVars, newValues, false);
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

            (bool, string, string, string, int, int, string) valDefault = (false, "", "", "", 0, 0, "");
            s = s.Insert(0, " ");
            s = s.Insert(s.Length, " ");
            s = Aux.Convert_Elif(s);
            string m = Aux.StringOut(s);
            string n = Regex.Replace(m, @"[^_""ñÑA-Za-z0-9]", " ");

            int index_1 = n.LastIndexOf(" if ");

            string temporary = Aux.SpacesOut(m[(index_1 + 1)..]);

            if (temporary.IndexOf('(') != 2) {
                if (!Syntax("Missing '(' after 'if' in 'if-else' expression")) return valDefault;
            }
        
            int stop = s.Length;
            
            while (m[index_1] == ' ' && index_1 > 0) index_1 --;
            string temp = m;

            if (m[index_1] == '(') {
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

            while (index1 != indexParenthesis1) {
                int indexParenthesis2 = temp.IndexOf(")", indexParenthesis1);
                temp = Aux.RemoveCharacter(temp, indexParenthesis1, indexParenthesis2);
                indexParenthesis1 = temp.LastIndexOf("(");
            }

            string condition = s[(index1 + 1)..temp.IndexOf(')', index1)];

            if (vars is null || !vars.Contains(condition)) {
                string type = Aux.ExpressionType(condition);
                if (type != "boolean" || !BodyDetails(condition)) {
                    if(!Semantic($"It is not possible to convert from {type} to boolean")) return valDefault;
                }
            }

            int elif = n.IndexOf(" elif ", temp.IndexOf(')', index1));
            int _else = n.IndexOf(" else ", temp.IndexOf(')', index1));
            int index_2 = (elif != -1)? Math.Min(elif, _else) + 1 : _else + 1;


            if (index_2 == 0 || stop < index_2) {
                if (!Syntax("Missing 'else' in 'if-else' expression")) return valDefault;
            }
            
            int start_body = index1 + condition.Length + 2;
            string body_true = s[start_body..index_2];

            if (Aux.ParenthesisBalance(body_true) == -1) {
                if (!Syntax($"Missing ')' in '{body_true.Trim()}'")) return valDefault;
            }

            int start = (s[index_2..(index_2 + 4)] == "elif") ? (index_2 + 2) : (index_2 + 4);
            string body_false = s[start..stop];

            if (string.IsNullOrWhiteSpace(body_true) || string.IsNullOrWhiteSpace(body_false)) {
                string mssg = string.IsNullOrWhiteSpace(body_true)? $"if({condition})" : "else";
                if (!Semantic($"Missing expression after '{mssg}' in 'if-else' instruction")) return valDefault;
            }
            
            m = Aux.FunctionsOut(body_false);

            if (m.Contains(",")) {
                stop = start + Aux.StringOut(m).IndexOf(",");
                body_false = s[start..stop];
            }

            while (Aux.ParenthesisBalance(m) == 1) {
                stop = start + Aux.StringOut(m).LastIndexOf(")");
                body_false = s[start..stop];
                m = body_false;
            }

            m = Aux.StringOut(body_false);
            m = Regex.Replace(m, @"[^_""ñÑ,A-Za-z0-9]", " ");

            if (m.Contains(" else ") && (start == (index_2 + 4))) {
                stop = start + Aux.StringOut(m).LastIndexOf(" else ");
                body_false = s[start..stop];
            }

            if (m.Contains(" elif ") && (start == (index_2 + 4))) {
                stop = start + Aux.StringOut(m).LastIndexOf(" elif ");
                body_false = s[start..stop];
            }

            if (!General(body_true) || !General(body_false)) return valDefault;
            if (!BodyDetails(body_true) || !BodyDetails(body_false)) return valDefault;

            return (true, condition, body_true, body_false, index_1, stop, s);
        }
        #endregion

        #region Reset values     
        public static void Reset() {
            error = false;
            Function.calls = new();
        }

        #endregion
    }
}