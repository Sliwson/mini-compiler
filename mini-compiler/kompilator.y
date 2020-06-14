%output=Parser.cs
%namespace GardensPoint

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

%token Eof
%token Whitespace

%%
start	:  
		;
%% 

public Parser(Scanner scnr) : base(scnr) { }