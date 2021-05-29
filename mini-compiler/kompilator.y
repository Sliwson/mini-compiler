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

expression : assignExpression { }
		   ; 

assignExpression : Identifier Assign assignExpression { Compiler.PushNode(new AssignNode($1)); }
				 | logicalExpression { }
				 ;

logicalExpression : logicalExpression Or relationExpression { Compiler.PushNode(new LogicalExpressionNode(LogicalExpressionNode.Type.Or)); }
				  | logicalExpression And relationExpression { Compiler.PushNode(new LogicalExpressionNode(LogicalExpressionNode.Type.And)); }
				  | relationExpression { }
				  ;

relationExpression : relationExpression Equals addExpression { Compiler.PushNode(new RelationExpressionNode(RelationExpressionNode.Type.Equals)); }
				   | relationExpression NotEquals addExpression { Compiler.PushNode(new RelationExpressionNode(RelationExpressionNode.Type.NotEquals)); }
				   | relationExpression GreaterThan addExpression { Compiler.PushNode(new RelationExpressionNode(RelationExpressionNode.Type.GreaterThan )); }
				   | relationExpression GreaterOrEqual addExpression { Compiler.PushNode(new RelationExpressionNode(RelationExpressionNode.Type.GreaterOrEqual)); }
				   | relationExpression LessThan addExpression { Compiler.PushNode(new RelationExpressionNode(RelationExpressionNode.Type.LessThan)); }
				   | relationExpression LessOrEqual addExpression { Compiler.PushNode(new RelationExpressionNode(RelationExpressionNode.Type.LessOrEqual)); }
				   | addExpression { }
				   ;

addExpression : addExpression Plus mulExpression { Compiler.PushNode(new AddExpressionNode(AddExpressionNode.Type.Plus)); }
			  | addExpression Minus mulExpression { Compiler.PushNode(new AddExpressionNode(AddExpressionNode.Type.Minus)); }
			  | mulExpression { }
			  ;


mulExpression : mulExpression Multiply bitExpression { Compiler.PushNode(new MulExpressionNode(MulExpressionNode.Type.Multiply)); }
			  | mulExpression Divide bitExpression { Compiler.PushNode(new MulExpressionNode(MulExpressionNode.Type.Divide)); }
			  | bitExpression { }
			  ;


bitExpression : bitExpression BitwiseOr unaryExpression { Compiler.PushNode(new BitExpressionNode(BitExpressionNode.Type.Or)); }
			  | bitExpression BitwiseAnd unaryExpression { Compiler.PushNode(new BitExpressionNode(BitExpressionNode.Type.And)); }
			  | unaryExpression { }
			  ;

unaryExpression : Minus unaryExpression { Compiler.PushNode(new UnaryExpressionNode(UnaryExpressionNode.Type.Minus)); }
				| BitwiseNegate unaryExpression { Compiler.PushNode(new UnaryExpressionNode(UnaryExpressionNode.Type.BitwiseNegate)); }
				| Negate unaryExpression { Compiler.PushNode(new UnaryExpressionNode(UnaryExpressionNode.Type.Negate)); }
				| IntConversion unaryExpression { Compiler.PushNode(new UnaryExpressionNode(UnaryExpressionNode.Type.IntConversion)); }
				| DoubleConversion unaryExpression { Compiler.PushNode(new UnaryExpressionNode(UnaryExpressionNode.Type.DoubleConversion)); }
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

inputInstruction : Read Identifier Semicolon { Compiler.PushNode(new ReadNode($2, false)); }
				 | Read Identifier Comma Hex Semicolon { Compiler.PushNode(new ReadNode($2, true)); }
				 ;

outputInstruction : Write expression Semicolon { Compiler.PushNode(new WriteExpressionNode(false)); }
				  | Write expression Comma Hex Semicolon { Compiler.PushNode(new WriteExpressionNode(true)); }
				  | Write StringLiteral Semicolon { WriteStringNode.CreateWriteStringNodes($2); }
				  ;

returnInstruction : Return Semicolon { Compiler.PushNode(new ReturnNode()); }
				  ;


%% 

public Parser(Scanner scnr) : base(scnr) { }