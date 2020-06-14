using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GardensPoint;
using QUT.Gppg;

namespace mini_compiler
{
    class Compiler
    {
        static void Main(string[] args)
        {
            FileStream file = new FileStream(args[0], FileMode.Open);
            var scanner = new Scanner();
            scanner.SetSource(file.ToString(), 0);
            var parser = new Parser(scanner);

            parser.Parse();
            file.Close();
        }
    }
}
