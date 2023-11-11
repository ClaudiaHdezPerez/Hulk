using Hulk_Library;
using String = Hulk_Library.String;

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

                if (expression.ToUpper() == "EXIT") break; // Otra opción para cerrar la aplicación de consola

                else {
                    // Revisa la sintaxis básica que debe tener cada expresión para ser válida
                    (string, bool) resultado = Control.BasicSyntax(expression); 
                    if(resultado.Item2)
                    Console.ForegroundColor = ConsoleColor.Red;

                    else Console.ForegroundColor = ConsoleColor.DarkGray;

                    // Si la expresión final es un string entonces se 'traduce' para que C# 
                    // lo imprima correctamente
                    if (String.IsString(resultado.Item1)) resultado.Item1 = String.TraduceString(resultado.Item1);

                    if (resultado.Item1 != "") {
                        if (resultado.Item1.StartsWith("\"")) {
                            // Si no hubo ningún error y es un string entonces se le quitan las comillas del 
                            // inicio y del final de la expresión
                            resultado.Item1 = resultado.Item1[(resultado.Item1.IndexOf("\"") + 1)..resultado.Item1.LastIndexOf("\"")];
                        }
                        
                        Console.WriteLine(resultado.Item1);
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