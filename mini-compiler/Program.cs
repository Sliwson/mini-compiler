using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Scanner;
using Parser;
using QUT.Gppg;

namespace mini_compiler
{
    class Compiler
    {
        static void Main(string[] args)
        {
            FileStream file = new FileStream(args[0], FileMode.Open);
            Scanner.Scanner scanner = new Scanner.Scanner();
            scanner.SetSource(file.ToString(), 0);
            Parser.Parser parser = new Parser.Parser(scanner);

            var sw = new StreamWriter(file.Name + ".il");
            parser.Parse();

            sw.Close();
            file.Close();
        }
    }
}
