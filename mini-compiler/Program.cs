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
        public static List<SyntaxNode> FrontNodes { get; set; }
        public static List<SyntaxNode> Nodes { get; set; }

        private static StreamWriter stream = null;
        private static int currentId = 0; 

        public static void Reset(string filename)
        {
            CurrentLine = 1;
            Errors = new List<Error>();
            Nodes = new List<SyntaxNode>();
            FrontNodes = new List<SyntaxNode>();

            stream = new StreamWriter(filename);
            currentId = 1;
        }

        public static int GenerateCode()
        {
            foreach (var node in FrontNodes)
                node.GenerateCode();

            Write("@.str_int = constant [3 x i8] c\"%d\\00\"");
            Write("@.str_double = constant [3 x i8] c\"%f\\00\"");
            Write("@.str_read_double = constant [4 x i8] c\"%lf\\00\"");
            Write("@.str_true = constant [5 x i8] c\"true\\00\"");
            Write("@.str_false = constant [6 x i8] c\"false\\00\"");

            Write("declare i32 @printf(i8*, ...)");
            Write("declare i32 @scanf(i8*, ...)");
            Write("declare i32 @puts(i8*)");

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

        public static void PushNodeFront(SyntaxNode node)
        {
            FrontNodes.Add(node);
        }

        public static string GetNextId()
        {
            var id = $"{currentId}";
            currentId++;
            return id;
        }

        public static DeclarationNode GetDeclaration(string identifier)
        {
            return Nodes.FirstOrDefault(n => (n as DeclarationNode)?.Identifier == identifier) as DeclarationNode;
        }
    }

    public abstract class SyntaxNode
    {
        public int Line { get; set; }
        public abstract string GenerateCode();
        public virtual ExpressionType GetExpressionType()
        {
            return ExpressionType.None;
        }
    }

    public enum ExpressionType
    {
        None,
        Integer, 
        Double,
        Bool 
    }

    static class Extensions
    {
        public static string ToLLVM(this ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Bool:
                    return "i1";
                case ExpressionType.Integer:
                    return "i32";
                case ExpressionType.Double:
                    return "double";
                default:
                    throw new ArgumentException("Exprsesion type None cannot be converted to llvm type");
            }
        }

        public static T Pop<T>(this List<T> list)
        {
            if (list.Count <= 0)
                return default(T);

            var last = list.Last();
            list.RemoveAt(list.Count - 1);
            return last;
        }
    }

    public class DeclarationNode : SyntaxNode
    {
        public string Identifier { get; private set; }
        private ExpressionType type;

        public DeclarationNode(ExpressionType type, string identifier)
        {
            Line = Compiler.CurrentLine;
            Identifier = identifier;

            this.type = type;
        }

        public override string GenerateCode()
        {
            var previousDeclarations = Compiler.Nodes
                .TakeWhile(n => n != this)
                .Where(n => n is DeclarationNode)
                .Select(n => n as DeclarationNode);

            if (previousDeclarations.FirstOrDefault(n => n.Identifier == Identifier) != null)
            {
                Compiler.Errors.Add(new Error { Line = Line, Text = $"Variable {Identifier} already declared" });
                return "";
            }

            switch (type)
            {
                case ExpressionType.Integer:
                    Compiler.Write($"%{Identifier} = alloca i32");
                    break;
                case ExpressionType.Double:
                    Compiler.Write($"%{Identifier} = alloca double");
                    break;
                case ExpressionType.Bool:
                    Compiler.Write($"%{Identifier} = alloca i1");
                    break;
            }

            return "";
        }

        public override ExpressionType GetExpressionType()
        {
            return type;
        }
    }

    public class DeclareStringNode : SyntaxNode
    {
        public string Guid { get; private set; }
        public string Text { get; private set; }
        public bool NewLine { get; private set; }
        public int Length => Text.Length - 2;

        public DeclareStringNode(string text, bool newline)
        {
            Line = Compiler.CurrentLine;
            NewLine = newline;

            Text = text + "\\00";
            var guid = System.Guid.NewGuid().ToString().Replace("-", "");
            Guid = "a" + guid.ToString();
        }

        public override string GenerateCode()
        {
            Compiler.Write($"@{Guid} = constant [{Length} x i8] c\"{Text}\"");
            return "";
        }
    }

    public class LogicalExpressionNode : SyntaxNode
    {
        public enum Type
        {
            Or,
            And
        }

        private readonly Type type;
        private readonly SyntaxNode lhs;
        private readonly SyntaxNode rhs;

        public LogicalExpressionNode(Type type)
        {
            this.type = type;
            if (Compiler.Nodes.Count > 1)
            {
                lhs = Compiler.Nodes.Pop();
                rhs = Compiler.Nodes.Pop();
            }
            else
            {
                // TODO: error
            }
        }

        public override string GenerateCode()
        {
            // TODO: check types
            var etl = lhs.GenerateCode();
            var etr = rhs.GenerateCode();
            var et = Compiler.GetNextId();

            Compiler.Write($"%{et} = and i1 {etl}, {etr}");
            return $"%{et}";
        }

        public override ExpressionType GetExpressionType()
        {
            return ExpressionType.Bool;
        }
    }

    public class WriteStringNode : SyntaxNode
    {
        private readonly string guid;
        private readonly int length;
        private readonly bool newline;

        public static void CreateWriteStringNodes(string literal)
        {
            // split by newlines
            var pos = 0;
            while (pos >= 0)
            {
                pos = literal.IndexOf("\\n");
                var newline = pos >= 0;

                if (pos < 0 && literal.Length == 0)
                    break;

                if (newline)
                {
                    var line = literal.Substring(0, pos);
                    literal = literal.Substring(pos + 2);

                    var declaration = new DeclareStringNode(line, newline);
                    Compiler.PushNodeFront(declaration);
                    Compiler.PushNode(new WriteStringNode(declaration.Guid, declaration.Length, declaration.NewLine));
                }
                else
                { 
                    // write everything
                    var declaration = new DeclareStringNode(literal, newline);
                    Compiler.PushNodeFront(declaration);
                    Compiler.PushNode(new WriteStringNode(declaration.Guid, declaration.Length, declaration.NewLine));
                }
            }
        }
        public WriteStringNode(string guid, int length, bool newline)
        {
            Line = Compiler.CurrentLine;
            this.guid = guid;
            this.length = length;
            this.newline = newline;
        }

        public override string GenerateCode()
        {
            var returnEt = Compiler.GetNextId();
            if (newline)
                Compiler.Write($"%{returnEt} = call i32 (i8*) @puts(i8* bitcast ([{length} x i8]* @{guid} to i8*))");
            else
                Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @printf(i8* bitcast ([{length} x i8]* @{guid} to i8*))");

            return "";
        }
    }

    public class WriteExpressionNode : SyntaxNode
    {
        private readonly SyntaxNode exp;
        private readonly bool hex;

        public WriteExpressionNode(bool hex = false)
        {
            this.hex = hex;
            Line = Compiler.CurrentLine;

            if (Compiler.Nodes.Count > 0)
            {
                exp = Compiler.Nodes.Pop();
            }
            else
            {
                // TODO: error
            }

        }

        public override string GenerateCode()
        {
            // TODO: implement hex
            if (exp == null)
                return "";

            var et = exp.GenerateCode();
            var returnEt = Compiler.GetNextId();
            switch (exp.GetExpressionType())
            {
                case ExpressionType.Integer:
                    Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @printf(i8* bitcast ([3 x i8]* @.str_int to i8*), i32 %{et})");
                    break;
                case ExpressionType.Double:
                    Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @printf(i8* bitcast ([3 x i8]* @.str_double to i8*), double %{et})");
                    break;
                case ExpressionType.Bool:
                    Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @printf(i8* bitcast ([3 x i8]* @.str_int to i8*), i1 %{et})");
                    break;
            }

            return "";
        }
    }

    public class ReadNode : SyntaxNode
    {
        private readonly string identifier;
        private readonly bool hex;

        public ReadNode(string identifier, bool hex)
        {
            this.identifier = identifier;
            this.hex = hex;
        }

        public override string GenerateCode()
        {
            // TODO: implement hex

            var declaration = Compiler.GetDeclaration(identifier);
            if (declaration == null)
            {
                // TODO: error
                return "";
            }

            var type = declaration.GetExpressionType();
            if (type == ExpressionType.Double)
            {
                var returnEt = Compiler.GetNextId();
                Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @scanf(i8* bitcast ([4 x i8]* @.str_read_double to i8*), double* %{identifier})");
            }
            else if (type == ExpressionType.Integer)
            {
                var returnEt = Compiler.GetNextId();
                Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @scanf(i8* bitcast ([3 x i8]* @.str_int to i8*), i32* %{identifier})");
            }
            else
            {
                // TODO: error
            }

            return "";
        }
    }

    public class ReturnNode : SyntaxNode
    {
        public ReturnNode()
        {
            Line = Compiler.CurrentLine;
        }

        public override string GenerateCode()
        {
            Compiler.GetNextId();
            Compiler.Write("ret void");
            return "";
        }
    }

    public class IdentifierNode : SyntaxNode
    {
        private readonly string identifier;

        public IdentifierNode(string identifier)
        {
            Line = Compiler.CurrentLine;
            this.identifier = identifier;
        }

        public override string GenerateCode()
        {
            var node = Compiler.GetDeclaration(identifier);
            if (node == null)
            {
                // TODO: error
                return "";
            }

            var id = Compiler.GetNextId();
            var llvmType = node.GetExpressionType().ToLLVM();
            Compiler.Write($"%{id} = load {llvmType}, {llvmType}* %{identifier}");
            return id;
        }

        public override ExpressionType GetExpressionType()
        {
            var node = Compiler.GetDeclaration(identifier);
            if (node == null)
                return ExpressionType.None;
            else
                return node.GetExpressionType();
        }
    }

    public class AssignNode : SyntaxNode
    {
        private readonly string identifier;
        private readonly SyntaxNode rhs;

        public AssignNode(string identifier)
        {
            Line = Compiler.CurrentLine;
            this.identifier = identifier;

            if (Compiler.Nodes.Count > 0)
            {
                rhs = Compiler.Nodes.Pop();
            }
            else
            {
                // TODO: error
            }
        }

        public override string GenerateCode()
        {
            if (rhs == null)
                return "";

            var lhs = new IdentifierNode(identifier);
            var lhsType = lhs.GetExpressionType();
            var rhsType = rhs.GetExpressionType();
            if (lhsType == rhsType && lhsType != ExpressionType.None)
            {
                var et = rhs.GenerateCode();
                string llvmType = rhsType.ToLLVM();
                Compiler.Write($"store {llvmType} {et}, {llvmType}* %{identifier}");
            }
            else
            {
                // TODO: error
            }

            return "";
        }

        public override ExpressionType GetExpressionType()
        {
            return ExpressionType.None;
        }
    }

    public class BoolFactorNode : SyntaxNode
    {
        public bool Value { get; private set; }

        public BoolFactorNode(bool value)
        {
            Line = Compiler.CurrentLine;
            Value = value;
        }

        public override string GenerateCode()
        {
            return Value ? "1" : "0";
        }

        public override ExpressionType GetExpressionType()
        {
            return ExpressionType.Bool;
        }
    }

    public class IntegerFactorNode : SyntaxNode
    {
        public int Value { get; private set; }

        public IntegerFactorNode(int value)
        {
            Line = Compiler.CurrentLine;
            Value = value;
        }

        public override string GenerateCode()
        {
            return Value.ToString();
        }

        public override ExpressionType GetExpressionType()
        {
            return ExpressionType.Integer;
        }
    }

    public class DoubleFactorNode : SyntaxNode
    {
        public double Value { get; private set; }

        public DoubleFactorNode(double value)
        {
            Line = Compiler.CurrentLine;
            Value = value;
        }

        public override string GenerateCode()
        {
            var str = Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (!str.Contains('.'))
                str += ".0";

            return str;
        }

        public override ExpressionType GetExpressionType()
        {
            return ExpressionType.Double;
        }
    }
}
