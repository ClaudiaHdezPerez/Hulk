namespace Hulk
{
    public class MainClass
    {
        private static void Main()
        {
            Aspect(); // Este método solo se encarga del aspecto de la consola

            while(true) {
                Error.Reset(); // Se restauran los valores de cache y errores por entradas
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write(">> ");
                Console.ForegroundColor = ConsoleColor.Gray;
        
                string expression = Console.ReadLine()!;
                // string expression = "sqrt(-2-2-2-2-2);";

                if (expression.ToUpper() == "EXIT") break; // Otra opción para cerrar la aplicación de consola

                else {
                    // Revisa la sintaxis básica que debe tener cada expresión para ser válida
                    string resultado = Control.BasicSyntax(expression); 
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                    // Si la expresión final es un string entonces se 'traduce' para que C# 
                    // lo imprima correctamente
                    if (String.IsString(resultado)) resultado = String.TraduceString(resultado);

                    if (resultado != "") {
                        if (resultado.StartsWith("\"")) {
                            // Si no hubo ningún error y es un string entonces se le quitan las comillas del 
                            // inicio y del final de la expresión
                            resultado = resultado[(resultado.IndexOf("\"") + 1)..resultado.LastIndexOf("\"")];
                        }
                        
                        Console.WriteLine(resultado);
                    }
                }                    
            }
        }

        public static void Aspect() {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("                       H U L K");
            Console.WriteLine();
            Console.Write(" ~~~~");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" Havana  University  Language  for  Kompilers ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("~~~~ ");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;        
            Console.Write("   Press CRTL + C or type \"EXIT\" to close the console ");
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}