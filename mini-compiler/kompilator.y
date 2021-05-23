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

declarationInt : Identifier Semicolon { Compiler.PushNode(new DeclarationNode(DeclarationNode.Type.Integer, $1)); }
			   | Identifier Comma declarationInt { Compiler.PushNode(new DeclarationNode(DeclarationNode.Type.Integer, $1)); }
			   ;

declarationDouble : Identifier Semicolon { Compiler.PushNode(new DeclarationNode(DeclarationNode.Type.Double, $1)); }
				  | Identifier Comma declarationDouble { Compiler.PushNode(new DeclarationNode(DeclarationNode.Type.Double, $1)); }
				  ;

declarationBool : Identifier Semicolon { Compiler.PushNode(new DeclarationNode(DeclarationNode.Type.Bool, $1)); }
				| Identifier Comma declarationBool { Compiler.PushNode(new DeclarationNode(DeclarationNode.Type.Bool, $1)); }
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
			| returnInstruction { }
			;

blockInstruction : OpenCurl instructions CloseCurl { }
				 ;

expression : unaryExpression { }
		   ; 

unaryExpression : Minus unaryExpression { }
				| BitwiseNegate unaryExpression { }
				| Negate unaryExpression { }
				| IntConversion unaryExpression { }
				| DoubleConversion unaryExpression { }
				| bitExpression { }
				;

bitExpression : bitExpression BitwiseOr mulExpression { }
			  | bitExpression BitwiseAnd mulExpression { }
			  | mulExpression { }
			  ;

mulExpression : mulExpression Multiply addExpression { }
			  | mulExpression Divide addExpression { }
			  | addExpression { }
			  ;

addExpression : addExpression Plus relationExpression { }
			  | addExpression Minus relationExpression { }
			  | relationExpression { }
			  ;

relationExpression : relationExpression Equals logicalExpression { }
				   | relationExpression NotEquals logicalExpression { }
				   | relationExpression GreaterThan logicalExpression { }
				   | relationExpression GreaterOrEqual logicalExpression { }
				   | relationExpression LessThan logicalExpression { }
				   | relationExpression LessOrEqual logicalExpression { }
				   | logicalExpression { }
				   ;

logicalExpression : logicalExpression Or assignExpression { }
				  | logicalExpression And assignExpression { }
				  | assignExpression { }
				  ;

assignExpression : Identifier Assign assignExpression { Console.WriteLine("Line {0}: Assign", Compiler.CurrentLine); }
				 | factorExpression { }
				 ;

factorExpression : OpenBracket expression CloseBracket { }
			     | IntegerLiteral { Console.WriteLine("Line {0}: Factor integer {1}", Compiler.CurrentLine, $1); }
			     | BoolLiteral { Console.WriteLine("Line {0}: Factor bool {1}", Compiler.CurrentLine, $1); }
			     | DoubleLiteral { Console.WriteLine("Line {0}: Factor double {1}", Compiler.CurrentLine, $1); }
				 | Identifier { Console.WriteLine("Line {0}: Factor identifier {1}", Compiler.CurrentLine, $1); }
			     ;

ifInstruction : If OpenBracket expression CloseBracket instruction { Console.WriteLine("Line {0}: If", Compiler.CurrentLine); }
			  | If OpenBracket expression CloseBracket instruction Else instruction { Console.WriteLine("Line {0}: If Else", Compiler.CurrentLine); }
			  ;

whileInstruction : While OpenBracket expression CloseBracket instruction {Console.WriteLine("Line {0}: While", Compiler.CurrentLine); }
				 ;

inputInstruction : Read Identifier Semicolon { Console.WriteLine("Line {0}: Read {1}", Compiler.CurrentLine, $2); }
				 | Read Identifier Comma Hex Semicolon { Console.WriteLine("Line {0}: Read hex {1}", Compiler.CurrentLine, $2); }
				 ;

outputInstruction : Write expression Semicolon { Console.WriteLine("Line {0}: Write expression: \"{1}\"", Compiler.CurrentLine, $2); }
				  | Write expression Comma Hex Semicolon { Console.WriteLine("Line {0}: Write hex: \"{1}\"", Compiler.CurrentLine, $2); }
				  | Write StringLiteral Semicolon 
				  {
					  var declaration = new DeclareStringNode($2);
					  Compiler.PushNodeFront(declaration);
					  Compiler.PushNode(new WriteStringNode(declaration.Guid, declaration.Length, declaration.NewLine));
				  }
				  ;

returnInstruction : Return Semicolon { Console.WriteLine("Line {0}: Return", Compiler.CurrentLine); }
				  ;


%% 

public Parser(Scanner scnr) : base(scnr) { }