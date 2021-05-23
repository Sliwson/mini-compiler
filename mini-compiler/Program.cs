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
            Console.WriteLine(filename);
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

            var errors = Compiler.GenerateCode();
            Compiler.PrintErrors();

            Console.WriteLine();

            if (errors > 0)
                File.Delete("filename");

            return errors;
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

            foreach (var node in Nodes)
                node.GenerateCode();

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

        public static void PushNode(SyntaxNode node)
        {
            Nodes.Add(node);
        }
    }

    public abstract class SyntaxNode
    {
        public int Line { get; set; }
        public abstract string GenerateCode();
    }

    public class DeclarationNode : SyntaxNode
    {
        private Type type;
        private string identifier;

        public enum Type { Integer, Double, Bool }

        public DeclarationNode(Type type, string identifier)
        {
            Line = Compiler.CurrentLine;
            this.type = type;
            this.identifier = identifier;
        }

        public override string GenerateCode()
        {
            var previousDeclarations = Compiler.Nodes
                .TakeWhile(n => n != this)
                .Where(n => n.Line <= Line && n is DeclarationNode)
                .Select(n => n as DeclarationNode);

            if (previousDeclarations.FirstOrDefault(n => n.identifier == identifier) != null)
            {
                Compiler.Errors.Add(new Error { Line = Line, Text = $"Variable {identifier} already declared" });
                return "";
            }

            switch (type)
            {
                case Type.Integer:
                    Compiler.Write($"%{identifier} = alloca i32");
                    Compiler.Write($"store i32 0, i32* %{identifier}");
                    break;
                case Type.Double:
                    Compiler.Write($"%{identifier} = alloca double");
                    Compiler.Write($"store double 0.0, double* %{identifier}");
                    break;
                case Type.Bool:
                    Compiler.Write($"%{identifier} = alloca i1");
                    Compiler.Write($"store i1 0, i1* %{identifier}");
                    break;
            }

            return "";
        }
    }
}
