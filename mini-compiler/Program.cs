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

        private static int ParseFile(string filename)
        {
            // reset compiler
            Compiler.Reset(filename + ".ll");

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

            Compiler.GenerateCode();
            Compiler.PrintErrors();

            Console.WriteLine();

            return 0;
        }
    }

    public class Error
    {
        public int Line { get; set; }
        public string Text { get; set; }
    }

    public class Compiler
    {
        public static int CurrentLine { get; set; } = 1;
        public static List<Error> Errors { get; set; }
        public static List<SyntaxNode> Nodes { get; set; }

        private static StreamWriter stream = null;

        public static void Reset(string filename)
        {
            CurrentLine = 1;
            Errors = new List<Error>();
            Nodes = new List<SyntaxNode>();

            stream = new StreamWriter(filename);
        }

        public static int GenerateCode()
        {
            Write("declare i32 @printf(i8*, ...)");
            Write("define void @main()");
            Write("{");
            Write("ret void");
            Write("}");
            stream.Close();

            return Errors.Count;
        }

        public static void PrintErrors()
        {
            foreach (var error in Errors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Line {error.Line}: {error.Text}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void Write(string code)
        {
            stream.WriteLine(code);
        }
    }

    public abstract class SyntaxNode
    {
        public int Line { get; set; }
        public abstract string GenerateCode();
    }
}
