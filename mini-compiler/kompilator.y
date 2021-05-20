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

declarationInt : Identifier Semicolon { }
			   | Identifier Comma declarationInt { }
			   ;

declarationDouble : Identifier Semicolon { }
				  | Identifier Comma declarationDouble
				  ;

declarationBool : Identifier Semicolon { }
				| Identifier Comma declarationBool
				;

instructions : instructions instruction { }
			 | 
			 ;

instruction : blockInstruction { }
			| expression { }
			| ifInstruction { }
			| whileInstruction { }
			| inputInstruction { }
			| outputInstruction { }
			;

blockInstruction : OpenCurl instructions CloseCurl { }
				 ;

expression : Semicolon { } // TODO: fix
		   ;

ifInstruction : If OpenBracket expression CloseBracket CloseBracket ifBody { }
			  ;

ifBody : blockInstruction { }
	   | expression { }
	   ;

whileInstruction : While OpenBracket expression CloseBracket whileBody { }
				 ;

whileBody : blockInstruction { }
		  | expression { }
		  ;

inputInstruction : Read Identifier Comma { }
				 | Read Identifier Hex Comma { }
				 ;

outputInstruction : Write expression Comma { }
				  | Write expression Hex Comma { }
				  | Write StringLiteral Comma { }
				  ;


%% 

public Parser(Scanner scnr) : base(scnr) { }