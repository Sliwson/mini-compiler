using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using QUT.Gppg;

namespace mini_compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles("Sources", "*.mini");
            foreach (var file in files)
                ParseFile(file);
        }

        private static void ParseFile(string filename)
        {
            // reset compiler
            Compiler.Reset();

            // read and parse file
            Console.WriteLine("==============================================================================");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{filename}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();

            FileStream file = new FileStream(filename, FileMode.Open);
            var reader = new StreamReader(file);

            var content = reader.ReadToEnd();

            int lineno = 1;
            foreach (var line in content.Split('\n'))
            {
                Console.WriteLine($"{lineno, -3} {line}");
                lineno++;
            }

            Console.WriteLine();

            Console.WriteLine("______________________________________________________________________________");
            Console.WriteLine("Output:");
            Console.WriteLine();

            var scanner = new Scanner();
            scanner.SetSource(content, 0);
            var parser = new Parser(scanner);

            Console.ForegroundColor = ConsoleColor.Green;
            parser.Parse();
            Console.ForegroundColor = ConsoleColor.White;

            file.Close();
            Console.WriteLine();
        }
    }

    public class Compiler
    {
        public static int CurrentLine { get; set; } = 1;
        public static int Errors { get; set; } = 0;

        public static void Reset()
        {
            CurrentLine = 1;
            Errors = 0;
        }
    }
}
