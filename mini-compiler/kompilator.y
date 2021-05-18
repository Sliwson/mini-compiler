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
%type <type> declarations

%%
start : Program OpenCurl declarations CloseCurl EOF { }
	  ;

declarations : statements { }
			 | declaration declarations { }
			 ;

declaration : Integer declarationInt { }
			| Double declarationDouble { }
			| Bool declarationBool { }
			;

declarationInt : Identifier Semicolon { Console.WriteLine("Line {0}: Integer {1}", Compiler.CurrentLine, $1); }
			   | Identifier Comma declarationInt { Console.WriteLine("Line {0}: Integer {1}", Compiler.CurrentLine, $1); }
			   ;

declarationDouble : Identifier Semicolon { }
				  | Identifier Comma declarationDouble
				  ;

declarationBool : Identifier Semicolon { }
				| Identifier Comma declarationBool
				;

statements :
		;
%% 

public Parser(Scanner scnr) : base(scnr) { }