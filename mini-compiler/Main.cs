using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var llFilename = filename + ".ll"; 
            Compiler.Reset(llFilename);

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
            Console.WriteLine("Compiler output:");
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
            {
                File.Delete(llFilename);
                return errors;
            }

            Console.WriteLine("______________________________________________________________________________");
            Console.WriteLine("Lli output:");
            Console.WriteLine();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "Lli\\lli.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = llFilename;
            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }

            return errors;
        }
    }

    public class Error
    {
        public Error(int line, string text)
        {
            Line = line;
            Text = text;
        }

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
        private static int currentLabelId = 0;

        public static void Reset(string filename)
        {
            CurrentLine = 1;
            Errors = new List<Error>();
            Nodes = new List<SyntaxNode>();
            FrontNodes = new List<SyntaxNode>();

            stream = new StreamWriter(filename);
            currentId = 1;
            currentLabelId = 0;
        }

        public static int GenerateCode()
        {
            foreach (var node in FrontNodes)
                node.GenerateCode();

            Write("@.str_int = constant [3 x i8] c\"%d\\00\"");
            Write("@.str_int_hex = constant [5 x i8] c\"0X%X\\00\"");
            Write("@.str_double = constant [3 x i8] c\"%f\\00\"");
            Write("@.str_read_double = constant [4 x i8] c\"%lf\\00\"");
            Write("@.str_read_hex = constant [3 x i8] c\"%X\\00\"");
            Write("@.str_true = constant [5 x i8] c\"True\\00\"");
            Write("@.str_false = constant [6 x i8] c\"False\\00\"");

            Write("declare i32 @printf(i8*, ...)");
            Write("declare i32 @scanf(i8*, ...)");

            Write("define i32 @main()");
            Write("{");

            foreach (var node in Nodes)
                node.GenerateCode();

            Write("ret i32 0");
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

        public static string GetNextLabel()
        {
            var id = $"LABEL_{currentLabelId}";
            currentLabelId++;
            return id;
        }

        public static DeclarationNode GetDeclaration(string identifier)
        {
            return Nodes.FirstOrDefault(n => (n as DeclarationNode)?.Identifier == identifier) as DeclarationNode;
        }

        public static string WriteConversion(ExpressionType from, ExpressionType to, string et)
        {
            if (from == to)
                return et;

            var newEt = GetNextId();
            if (from == ExpressionType.Integer && to == ExpressionType.Double)
                Write($"%{newEt} = sitofp i32 {et} to double");
            if (from == ExpressionType.Double && to == ExpressionType.Integer)
                Write($"%{newEt} = fptosi double {et} to i32");

            return $"%{newEt}";
        }
    }

    public abstract class SyntaxNode
    {
        public SyntaxNode()
        {
            Line = Compiler.CurrentLine;
        }

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
                Compiler.Errors.Add(new Error(Line, $"Variable {Identifier} already declared"));
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
        public int Length { get; private set; }

        public DeclareStringNode(string text)
        {
            // escaping special characters
            var length = text.Length + 1;
            for (int i = 0; i < text.Length - 1; i++)
            {
                if (text[i] == '\\')
                {
                    var nextChar = text[i + 1];
                    if (nextChar == '\"')
                    {
                        text = text.Remove(i, 2);
                        text = text.Insert(i, "\\22");
                        length -= 1;
                        i += 2;
                    }
                    else if (nextChar == '\\')
                    {
                        text = text.Remove(i, 2);
                        text = text.Insert(i, "\\5C");
                        length -= 1;
                        i += 2;
                    }
                    else if (nextChar == 'n')
                    {
                        text = text.Remove(i, 2);
                        text = text.Insert(i, "\\0A");
                        length -= 1;
                        i += 2;
                    }
                    else
                    {
                        text = text.Remove(i, 1);
                        i -= 1;
                        length -= 1;
                    }
                }
            }

            Text = text + "\\00";

            Guid = ".str." + Compiler.FrontNodes.Count;
            Length = length;
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
                rhs = Compiler.Nodes.Pop();
                lhs = Compiler.Nodes.Pop();
            }
            else
            {
                Compiler.Errors.Add(new Error(Line, "Compiler error when parsing logical expression"));
            }
        }

        public override string GenerateCode()
        {
            if (lhs.GetExpressionType() != ExpressionType.Bool || rhs.GetExpressionType() != ExpressionType.Bool)
            {
                Compiler.Errors.Add(new Error(Line, "Logical expression arguments have to be of bool type"));
                return "";
            }

            var etl = lhs.GenerateCode();

            var startLabel = Compiler.GetNextLabel();
            var calculateLabel = Compiler.GetNextLabel();
            var endLabel = Compiler.GetNextLabel();

            Compiler.Write($"br label %{startLabel}");
            Compiler.Write($"{startLabel}:");

            if (type == Type.And)
                Compiler.Write($"br i1 {etl}, label %{calculateLabel}, label %{endLabel}");
            else
                Compiler.Write($"br i1 {etl}, label %{endLabel}, label %{calculateLabel}");

            Compiler.Write($"{calculateLabel}:");
            var etr = rhs.GenerateCode();
            Compiler.Write($"br label %{endLabel}");

            var et = Compiler.GetNextId();

            Compiler.Write($"{endLabel}:");
            if (type == Type.And)
            {
                Compiler.Write($"%{et} = phi i1 [0, %{startLabel}], [{etr}, %{calculateLabel}]");
                var newEt = Compiler.GetNextId();
                Compiler.Write($"%{newEt} = and i1 {etl}, %{et}");
                et = newEt;
            }
            else
            {
                Compiler.Write($"%{et} = phi i1 [1, %{startLabel}], [{etr}, %{calculateLabel}]");
                var newEt = Compiler.GetNextId();
                Compiler.Write($"%{newEt} = or i1 {etl}, %{et}");
                et = newEt;
            }

            return $"%{et}";
        }

        public override ExpressionType GetExpressionType()
        {
            return ExpressionType.Bool;
        }
    }

    public class RelationExpressionNode : SyntaxNode
    {
        public enum Type
        {
            Equals,
            NotEquals,
            GreaterThan,
            GreaterOrEqual,
            LessThan,
            LessOrEqual
        }

        private readonly Type type;
        private readonly SyntaxNode lhs;
        private readonly SyntaxNode rhs;

        public RelationExpressionNode(Type type)
        {
            this.type = type;
            if (Compiler.Nodes.Count > 1)
            {
                rhs = Compiler.Nodes.Pop();
                lhs = Compiler.Nodes.Pop();
            }
            else
            {
                Compiler.Errors.Add(new Error(Line, "Compiler error when parsing relation expression"));
            }
        }

        public override string GenerateCode()
        {
            var lhsType = lhs.GetExpressionType();
            var rhsType = rhs.GetExpressionType();

            if (lhsType != rhsType)
            {
                if (lhsType == ExpressionType.Bool || rhsType == ExpressionType.Bool)
                    Compiler.Errors.Add(new Error(Line, "One of relation expression argument is bool (can be none or both for == and != operations)"));
            }
            else
            {
                if (lhsType == ExpressionType.Bool)
                {
                    if (type != Type.Equals && type != Type.NotEquals)
                        Compiler.Errors.Add(new Error(Line, "Relation expression arguments can only be bool for == and != operations"));
                }
            }

            var etl = lhs.GenerateCode();
            var etr = rhs.GenerateCode();

            var operand = GetOperand();
            var expType = GetCompType(lhsType, rhsType);

            etl = Compiler.WriteConversion(lhsType, expType, etl);
            etr = Compiler.WriteConversion(rhsType, expType, etr);

            var et = Compiler.GetNextId();
            Compiler.Write($"%{et} = icmp {operand} {expType.ToLLVM()} {etl}, {etr}");

            return $"%{et}";
        }

        public override ExpressionType GetExpressionType()
        {
            return ExpressionType.Bool;
        }

        private ExpressionType GetCompType(ExpressionType lType, ExpressionType rType)
        {
            if (lType == ExpressionType.Bool || rType == ExpressionType.Bool)
                return ExpressionType.Bool;
            else if (lType == ExpressionType.Double || rType == ExpressionType.Double)
                return ExpressionType.Double;

            return ExpressionType.Integer;
        }

        private string GetOperand()
        {
            switch (type)
            {
                case Type.Equals:
                    return "eq";
                case Type.NotEquals:
                    return "ne";
                case Type.GreaterThan:
                    return "sgt";
                case Type.GreaterOrEqual:
                    return "sge";
                case Type.LessThan:
                    return "slt";
                case Type.LessOrEqual:
                    return "sle";
                default:
                    throw new ArgumentException();
            }
        }
    }

    public abstract class MulAddExpressionNodeBase : SyntaxNode
    {
        protected SyntaxNode lhs;
        protected SyntaxNode rhs;
        protected bool writeNsw;

        public override string GenerateCode()
        {
            var etl = lhs.GenerateCode();
            var etr = rhs.GenerateCode();
            var operand = GetOperand();

            var lhsType = lhs.GetExpressionType();
            var rhsType = lhs.GetExpressionType();

            if (lhsType == ExpressionType.Bool || rhsType == ExpressionType.Bool)
            {
                Compiler.Errors.Add(new Error(Line, "Add/mul expression arguments have to be of integer of double type"));
                return "";
            }

            var expType = GetCompType(lhsType, rhsType);
            etl = Compiler.WriteConversion(lhsType, expType, etl);
            etr = Compiler.WriteConversion(rhsType, expType, etr);

            var et = Compiler.GetNextId();
            var nsw = writeNsw ? "nsw" : "";
            Compiler.Write($"%{et} = {operand} {nsw} {expType.ToLLVM()} {etl}, {etr}");

            return $"%{et}";
        }

        protected ExpressionType GetCompType(ExpressionType lType, ExpressionType rType)
        {
            if (lType == ExpressionType.Double || rType == ExpressionType.Double)
                return ExpressionType.Double;

            return ExpressionType.Integer;
        }

        public override ExpressionType GetExpressionType()
        {
            var lhsType = lhs.GetExpressionType();
            var rhsType = rhs.GetExpressionType();
            return GetCompType(lhsType, rhsType);
        }

        protected abstract string GetOperand();
    }

    public class AddExpressionNode : MulAddExpressionNodeBase
    {
        public enum Type
        {
            Plus,
            Minus
        }

        private readonly Type type;

        public AddExpressionNode(Type type)
        {
            this.type = type;
            this.writeNsw = true;

            if (Compiler.Nodes.Count > 1)
            {
                rhs = Compiler.Nodes.Pop();
                lhs = Compiler.Nodes.Pop();
            }
            else
            {
                Compiler.Errors.Add(new Error(Line, "Compiler error when parsing add expression"));
            }
        }

        protected override string GetOperand()
        {
            switch (type)
            {
                case Type.Plus:
                    return "add";
                case Type.Minus:
                    return "sub";
                default:
                    throw new ArgumentException();
            }
        }
    }

    public class MulExpressionNode : MulAddExpressionNodeBase
    {
        public enum Type
        {
            Multiply,
            Divide
        }

        private readonly Type type;

        public MulExpressionNode(Type type)
        {
            this.type = type;
            if (Compiler.Nodes.Count > 1)
            {
                rhs = Compiler.Nodes.Pop();
                lhs = Compiler.Nodes.Pop();
            }
            else
            {
                Compiler.Errors.Add(new Error(Line, "Compiler error when parsing mul expression"));
            }
        }

        protected override string GetOperand()
        {
            switch (type)
            {
                case Type.Multiply:
                    return "mul";
                case Type.Divide:
                    return "sdiv";
                default:
                    throw new ArgumentException();
            }
        }
    }

    public class BitExpressionNode : SyntaxNode
    {
        public enum Type
        {
            Or,
            And
        }

        private readonly Type type;
        private readonly SyntaxNode lhs;
        private readonly SyntaxNode rhs;

        public BitExpressionNode(Type type)
        {
            this.type = type;
            if (Compiler.Nodes.Count > 1)
            {
                rhs = Compiler.Nodes.Pop();
                lhs = Compiler.Nodes.Pop();
            }
            else
            {
                Compiler.Errors.Add(new Error(Line, "Compiler error when parsing bit expression"));
            }
        }

        public override string GenerateCode()
        {
            var etl = lhs.GenerateCode();
            var etr = rhs.GenerateCode();
            var et = Compiler.GetNextId();

            var operand = GetOperand();
            var lhsType = lhs.GetExpressionType();
            var rhsType = lhs.GetExpressionType();

            if (lhsType != ExpressionType.Integer || rhsType != ExpressionType.Integer)
                Compiler.Errors.Add(new Error(Line, "Bit expression arguments have to be of integer type"));

            Compiler.Write($"%{et} = {operand} i32 {etl}, {etr}");

            return $"%{et}";
        }

        public override ExpressionType GetExpressionType()
        {
            return ExpressionType.Integer;
        }

        private string GetOperand()
        {
            switch (type)
            {
                case Type.Or:
                    return "or";
                case Type.And:
                    return "and";
                default:
                    throw new ArgumentException();
            }
        }
    }

    public class UnaryExpressionNode : SyntaxNode
    {
        public enum Type
        {
            Minus,
            BitwiseNegate,
            Negate,
            IntConversion,
            DoubleConversion
        }

        private readonly Type type;
        private readonly SyntaxNode rhs;

        public UnaryExpressionNode(Type type)
        {
            this.type = type;
            if (Compiler.Nodes.Count > 0)
            {
                rhs = Compiler.Nodes.Pop();
            }
            else
            {
                Compiler.Errors.Add(new Error(Line, "Compiler error when parsing unary expression"));
            }
        }

        public override string GenerateCode()
        {
            // TODO: check types
            var etr = rhs.GenerateCode();
            var et = Compiler.GetNextId();

            var rhsType = rhs.GetExpressionType();
            switch (type)
            {
                case Type.Minus:
                    Compiler.Write($"%{et} = sub {rhsType.ToLLVM()} 0, {etr}");
                    break;
                case Type.BitwiseNegate:
                    Compiler.Write($"%{et} = xor {rhsType.ToLLVM()} {etr}, -1");
                    break;
                case Type.Negate:
                    Compiler.Write($"%{et} = icmp ne {rhsType.ToLLVM()} {etr}, 0");
                    var oldEt = et;
                    et = Compiler.GetNextId();
                    Compiler.Write($"%{et} = xor i1 {oldEt}, 1");
                    oldEt = et;
                    et = Compiler.GetNextId();
                    Compiler.Write($"%{et} = zext i1 {oldEt} to {rhsType.ToLLVM()}");
                    break;
                case Type.IntConversion:
                    // TODO: implement
                    break;
                case Type.DoubleConversion:
                    // TODO: implement
                    break;
            }

            return $"%{et}";
        }

        public override ExpressionType GetExpressionType()
        {
            // TODO: return correct
            switch (type)
            {
                case Type.IntConversion:
                    return ExpressionType.Integer;
                case Type.DoubleConversion:
                    return ExpressionType.Double;
            }

            return rhs.GetExpressionType();
        }
    }

    public class WriteStringNode : SyntaxNode
    {
        private readonly string guid;
        private readonly int length;

        public static void CreateWriteStringNodes(string literal)
        {
            var declaration = new DeclareStringNode(literal);
            Compiler.PushNodeFront(declaration);
            Compiler.PushNode(new WriteStringNode(declaration.Guid, declaration.Length));
        }

        public WriteStringNode(string guid, int length)
        {
            this.guid = guid;
            this.length = length;
        }

        public override string GenerateCode()
        {
            var returnEt = Compiler.GetNextId();
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

            if (Compiler.Nodes.Count > 0)
            {
                exp = Compiler.Nodes.Pop();
            }
            else
            {
                Compiler.Errors.Add(new Error(Line, "Compiler error when parsing write expression"));
            }

        }

        public override string GenerateCode()
        {
            if (exp == null)
                return "";

            var et = exp.GenerateCode();
            var returnEt = Compiler.GetNextId();

            if (hex && exp.GetExpressionType() != ExpressionType.Integer)
            {
                Compiler.Errors.Add(new Error(Line, "Hex modifier can be used only with expression of int type"));
                return "";
            }

            switch (exp.GetExpressionType())
            {
                case ExpressionType.Integer:
                    if (hex)
                        Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @printf(i8* bitcast ([5 x i8]* @.str_int_hex to i8*), i32 {et})");
                    else
                        Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @printf(i8* bitcast ([3 x i8]* @.str_int to i8*), i32 {et})");
                    break;
                case ExpressionType.Double:
                    Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @printf(i8* bitcast ([3 x i8]* @.str_double to i8*), double {et})");
                    break;
                case ExpressionType.Bool:
                    var ifLabel = Compiler.GetNextLabel();
                    var elseLabel = Compiler.GetNextLabel();
                    var endLabel = Compiler.GetNextLabel();

                    Compiler.Write($"br i1 {et}, label %{ifLabel}, label %{elseLabel}");

                    Compiler.Write($"{ifLabel}:");
                    Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @printf(i8* bitcast ([{5} x i8]* @.str_true to i8*))");
                    Compiler.Write($"br label %{endLabel}");

                    Compiler.Write($"{elseLabel}:");
                    returnEt = Compiler.GetNextId();
                    Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @printf(i8* bitcast ([{6} x i8]* @.str_false to i8*))");
                    Compiler.Write($"br label %{endLabel}");
                    Compiler.Write($"{endLabel}:");
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
            var declaration = Compiler.GetDeclaration(identifier);
            if (declaration == null)
            {
                Compiler.Errors.Add(new Error(Line, $"Variable {identifier} not declared"));
                return "";
            }

            if (hex && declaration.GetExpressionType() != ExpressionType.Integer)
            {
                Compiler.Errors.Add(new Error(Line, "Hex modifier can be used only with variable of int type"));
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
                if (hex)
                    Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @scanf(i8* bitcast ([3 x i8]* @.str_read_hex to i8*), i32* %{identifier})");
                else
                    Compiler.Write($"%{returnEt} = call i32 (i8*, ...) @scanf(i8* bitcast ([3 x i8]* @.str_int to i8*), i32* %{identifier})");
            }
            else if (type == ExpressionType.Bool)
            {
                Compiler.Errors.Add(new Error(Line, "Cannot read to variable of bool type"));
            }

            return "";
        }
    }

    public class ReturnNode : SyntaxNode
    {
        public override string GenerateCode()
        {
            Compiler.GetNextId();
            Compiler.Write("ret i32 0");
            return "";
        }
    }

    public class IdentifierNode : SyntaxNode
    {
        private readonly string identifier;

        public IdentifierNode(string identifier)
        {
            this.identifier = identifier;
        }

        public override string GenerateCode()
        {
            var node = Compiler.GetDeclaration(identifier);
            if (node == null)
            {
                Compiler.Errors.Add(new Error(Line, $"Variable {identifier} not declared"));
                return "";
            }

            var id = Compiler.GetNextId();
            var llvmType = node.GetExpressionType().ToLLVM();
            Compiler.Write($"%{id} = load {llvmType}, {llvmType}* %{identifier}");
            return $"%{id}";
        }

        public override ExpressionType GetExpressionType()
        {
            var node = Compiler.GetDeclaration(identifier);
            if (node == null)
                return ExpressionType.None; // TODO: check this
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
            this.identifier = identifier;

            if (Compiler.Nodes.Count > 0)
            {
                rhs = Compiler.Nodes.Pop();
            }
            else
            {
                Compiler.Errors.Add(new Error(Line, "Compiler error when parsing assign expression"));
            }
        }

        public override string GenerateCode()
        {
            if (rhs == null)
                return "";

            var lhs = new IdentifierNode(identifier);
            var lhsType = lhs.GetExpressionType();
            var rhsType = rhs.GetExpressionType();
            if (lhsType == rhsType && lhsType != ExpressionType.None) // TODO: check this
            {
                var et = rhs.GenerateCode();
                string llvmType = rhsType.ToLLVM();
                Compiler.Write($"store {llvmType} {et}, {llvmType}* %{identifier}");
            }
            else
            {
                // TODO: conversions
                // TODO: error
            }

            return "";
        }

        public override ExpressionType GetExpressionType()
        {
            // TODO: correct type
            return ExpressionType.None;
        }
    }

    public class BoolFactorNode : SyntaxNode
    {
        public bool Value { get; private set; }

        public BoolFactorNode(bool value)
        {
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

    public class IfNode : SyntaxNode
    {
        SyntaxNode condition;
        SyntaxNode ifBlock;
        SyntaxNode elseBlock = null;

        public IfNode(bool withElse)
        {
            var expectedNodes = withElse ? 3 : 2;

            if (Compiler.Nodes.Count < expectedNodes)
            {
                Compiler.Errors.Add(new Error(Line, "Compiler error when parsing if expression"));
                return;
            }

            if (withElse)
            {
                elseBlock = Compiler.Nodes.Pop();
            }

            ifBlock = Compiler.Nodes.Pop();
            condition = Compiler.Nodes.Pop();
        }

        public override string GenerateCode()
        {
            var condEt = condition.GenerateCode();
            var condType = condition.GetExpressionType();

            var ifLabel = Compiler.GetNextLabel();
            var endLabel = Compiler.GetNextLabel();
            var elseLabel = Compiler.GetNextLabel();

            if (condType != ExpressionType.Bool)
            {
                Compiler.Errors.Add(new Error(Line, "If condition has to be of bool type"));
                return "";
            }
                
            if (elseBlock != null)
            {
                Compiler.Write($"br {condType.ToLLVM()} {condEt}, label %{ifLabel}, label %{elseLabel}");
            }
            else
            {
                Compiler.Write($"br {condType.ToLLVM()} {condEt}, label %{ifLabel}, label %{endLabel}");
            }

            // if body
            Compiler.Write($"{ifLabel}:");
            ifBlock.GenerateCode();
            Compiler.Write($"br label %{endLabel}");

            if (elseBlock != null)
            {
                Compiler.Write($"{elseLabel}:");
                elseBlock.GenerateCode();
                Compiler.Write($"br label %{endLabel}");
            }

            Compiler.Write($"{endLabel}:");
            return "";
        }
    }

    public class WhileNode : SyntaxNode
    {

        SyntaxNode condition;
        SyntaxNode instruction;

        public WhileNode()
        {
            if (Compiler.Nodes.Count < 2)
            {
                Compiler.Errors.Add(new Error(Line, "Compiler error when parsing while expression"));
                return;
            }

            instruction = Compiler.Nodes.Pop();
            condition = Compiler.Nodes.Pop();
        }

        public override string GenerateCode()
        {
            var conditionLabel = Compiler.GetNextLabel();
            Compiler.Write($"br label %{conditionLabel}");
            Compiler.Write($"{conditionLabel}:");

            var condEt = condition.GenerateCode();
            var condType = condition.GetExpressionType();

            if (condType != ExpressionType.Bool)
            {
                Compiler.Errors.Add(new Error(Line, "While condition has to be of bool type"));
                return "";
            }

            var beginLabel = Compiler.GetNextLabel();
            var endLabel = Compiler.GetNextLabel();
            
            Compiler.Write($"br {condType.ToLLVM()} {condEt}, label %{beginLabel}, label %{endLabel}");
            Compiler.Write($"{beginLabel}:");
            instruction.GenerateCode();
            Compiler.Write($"br label %{conditionLabel}");
            Compiler.Write($"{endLabel}:");
            return "";
        }
    }

    public class BlockInstructionNode : SyntaxNode
    {
        List<SyntaxNode> instructions = new List<SyntaxNode>();

        public void PushNode(SyntaxNode node)
        {
            instructions.Add(node);
        }

        public static void InsertInstructionToTopBlock()
        {
            var instruction = Compiler.Nodes.Pop();
            var block = Compiler.Nodes.Last() as BlockInstructionNode;
            block.instructions.Add(instruction);
        }

        public override string GenerateCode()
        {
            foreach (var node in instructions)
                node.GenerateCode();

            return "";
        }
    }
}
