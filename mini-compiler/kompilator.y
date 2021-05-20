%output=Parser.cs
%namespace mini_compiler

%union {
	public int Integer;
	public string String;
	public double Double;
	public bool Bool;
}

// Beginning of the program
%token Program

// Types
%token <String> Identifier
%token <Integer> IntegerLiteral
%token <Double> DoubleLiteral
%token <Bool> BoolLiteral
%token <String> StringLiteral

%token Integer
%token Double 
%token Bool

%token Hex

// Operators
%token Assign 
%token Or
%token And 
%token BitwiseOr
%token BitwiseAnd
%token Equals
%token NotEquals
%token GreaterThan
%token GreaterOrEqual
%token LessThan
%token LessOrEqual
%token Plus 
%token Minus
%token Multiply
%token Divide
%token Negate
%token BitwiseNegate
%token IntConversion
%token DoubleConversion

// If instruction
%token If 
%token Else 

// While loop
%token While 

// IO
%token Read
%token Write

// Blocks
%token Return    
%token OpenBracket
%token CloseBracket
%token OpenCurl
%token CloseCurl
%token Semicolon
%token Comma

// non-terminals

%%
start : Program OpenCurl declarations CloseCurl EOF { }
	  ;

declarations : instructions { }
			 | declaration declarations { }
			 ;

declaration : Integer declarationInt { }
			| Double declarationDouble { }
			| Bool declarationBool { }
			;

declarationInt : Identifier Semicolon { Console.WriteLine("Line {0}: Integer {1}", Compiler.CurrentLine, $1); }
			   | Identifier Comma declarationInt { Console.WriteLine("Line {0}: Integer {1}", Compiler.CurrentLine, $1); }
			   ;

declarationDouble : Identifier Semicolon { Console.WriteLine("Line {0}: Double {1}", Compiler.CurrentLine, $1); }
				  | Identifier Comma declarationDouble { Console.WriteLine("Line {0}: Double {1}", Compiler.CurrentLine, $1); }
				  ;

declarationBool : Identifier Semicolon { Console.WriteLine("Line {0}: Bool {1}", Compiler.CurrentLine, $1); }
				| Identifier Comma declarationBool { Console.WriteLine("Line {0}: Bool {1}", Compiler.CurrentLine, $1); }
				;

instructions : instructions instruction { }
			 | 
			 ;

instruction : blockInstruction { }
			| expression Semicolon { }
			| ifInstruction { }
			| whileInstruction { }
			| inputInstruction { }
			| outputInstruction { }
			;

blockInstruction : OpenCurl instructions CloseCurl { }
				 ;

expression : Identifier GreaterThan Identifier { } // TODO: extend
		   | Identifier Assign Identifier { }
		   ; 

ifInstruction : If OpenBracket expression CloseBracket instruction { Console.WriteLine("Line {0}: If", Compiler.CurrentLine); }
			  | If OpenBracket expression CloseBracket instruction Else instruction { Console.WriteLine("Line {0}: If Else", Compiler.CurrentLine); }
			  ;

whileInstruction : While OpenBracket expression CloseBracket instruction {Console.WriteLine("Line {0}: While", Compiler.CurrentLine); }
				 ;

inputInstruction : Read Identifier Semicolon { Console.WriteLine("Line {0}: Read {1}", Compiler.CurrentLine, $2); }
				 | Read Identifier Hex Semicolon { Console.WriteLine("Line {0}: Read hex {1}", Compiler.CurrentLine, $2); }
				 ;

outputInstruction : Write expression Semicolon { Console.WriteLine("Line {0}: Write expression: \"{1}\"", Compiler.CurrentLine, $2); }
				  | Write expression Hex Semicolon { Console.WriteLine("Line {0}: Write hex: \"{1}\"", Compiler.CurrentLine, $2); }
				  | Write StringLiteral Semicolon { Console.WriteLine("Line {0}: Write string: \"{1}\"", Compiler.CurrentLine, $2); }
				  ;


%% 

public Parser(Scanner scnr) : base(scnr) { }