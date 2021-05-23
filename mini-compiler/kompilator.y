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

declarationInt : Identifier Semicolon { Compiler.PushNode(new DeclarationNode(ExpressionType.Integer, $1)); }
			   | Identifier Comma declarationInt { Compiler.PushNode(new DeclarationNode(ExpressionType.Integer, $1)); }
			   ;

declarationDouble : Identifier Semicolon { Compiler.PushNode(new DeclarationNode(ExpressionType.Double, $1)); }
				  | Identifier Comma declarationDouble { Compiler.PushNode(new DeclarationNode(ExpressionType.Double, $1)); }
				  ;

declarationBool : Identifier Semicolon { Compiler.PushNode(new DeclarationNode(ExpressionType.Bool, $1)); }
				| Identifier Comma declarationBool { Compiler.PushNode(new DeclarationNode(ExpressionType.Bool, $1)); }
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

assignExpression : Identifier Assign assignExpression { Compiler.PushNode(new AssignNode($1)); }
				 | factorExpression { }
				 ;

factorExpression : OpenBracket expression CloseBracket { }
			     | IntegerLiteral { Compiler.PushNode(new IntegerFactorNode($1)); }
			     | BoolLiteral { Compiler.PushNode(new BoolFactorNode($1)); }
			     | DoubleLiteral { Compiler.PushNode(new DoubleFactorNode($1)); }
				 | Identifier { Compiler.PushNode(new IdentifierNode($1)); }
			     ;

ifInstruction : If OpenBracket expression CloseBracket instruction { Console.WriteLine("Line {0}: If", Compiler.CurrentLine); }
			  | If OpenBracket expression CloseBracket instruction Else instruction { Console.WriteLine("Line {0}: If Else", Compiler.CurrentLine); }
			  ;

whileInstruction : While OpenBracket expression CloseBracket instruction {Console.WriteLine("Line {0}: While", Compiler.CurrentLine); }
				 ;

inputInstruction : Read Identifier Semicolon { Console.WriteLine("Line {0}: Read {1}", Compiler.CurrentLine, $2); }
				 | Read Identifier Comma Hex Semicolon { Console.WriteLine("Line {0}: Read hex {1}", Compiler.CurrentLine, $2); }
				 ;

outputInstruction : Write expression Semicolon 
				  { 
				      Compiler.PushNode(new WriteExpressionNode());
				  }
				  | Write expression Comma Hex Semicolon { }
				  | Write StringLiteral Semicolon 
				  {
					  var declaration = new DeclareStringNode($2);
					  Compiler.PushNodeFront(declaration);
					  Compiler.PushNode(new WriteStringNode(declaration.Guid, declaration.Length, declaration.NewLine));
				  }
				  ;

returnInstruction : Return Semicolon { Compiler.PushNode(new ReturnNode()); }
				  ;


%% 

public Parser(Scanner scnr) : base(scnr) { }