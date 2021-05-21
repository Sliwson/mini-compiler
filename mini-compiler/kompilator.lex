%namespace mini_compiler
%visibility public

%{
public override void yyerror(string format, params object[] args)
{
	Console.ForegroundColor = ConsoleColor.Red;
	System.Console.WriteLine("Line {0} - " + format, yyline);
	Console.ForegroundColor = ConsoleColor.White;
}
%}

// Literals
Identifier [a-zA-Z]([a-zA-Z0-9])*
IntegerLiteral	([0-9]|[1-9][0-9]*)
DoubleLiteral	[1-9][0-9]*\.[0-9]+
BoolLiteral		(true|false)
StringLiteral	\".*\"
Whitespace		\s	

%% 

"program"			{ return (int)Tokens.Program; }

{IntegerLiteral}	{ 
						Int32.TryParse (yytext, NumberStyles.Integer, CultureInfo.InvariantCulture, out yylval.Integer);
						return (int)Tokens.IntegerLiteral; 
					}

{DoubleLiteral}		{
						double.TryParse (yytext, NumberStyles.Float, CultureInfo.InvariantCulture, out yylval.Double); 
						return (int)Tokens.DoubleLiteral; 
					}

{StringLiteral}		{
						if (yytext.Length > 2)  
							yylval.String = yytext.Substring(1, yytext.Length - 2); 
						else
							yylval.String = ""; 

						return (int)Tokens.StringLiteral;
					}

{BoolLiteral}		{ 
						bool.TryParse(yytext, out yylval.Bool);
					   return (int)Tokens.BoolLiteral; 
					}

"int"				{ return (int)Tokens.Integer; }
"double"			{ return (int)Tokens.Double; }
"bool"				{ return (int)Tokens.Bool; }

"hex"				{ return (int)Tokens.Hex; }

"="					{ return (int)Tokens.Assign; }
"||"				{ return (int)Tokens.Or; }
"&&"				{ return (int)Tokens.And; }
"|"					{ return (int)Tokens.BitwiseOr; }
"&"					{ return (int)Tokens.BitwiseAnd; }
"=="				{ return (int)Tokens.Equals; }
"!="				{ return (int)Tokens.NotEquals; }
">"					{ return (int)Tokens.GreaterThan; }
">="				{ return (int)Tokens.GreaterOrEqual; }
"<"					{ return (int)Tokens.LessThan; }
"<="				{ return (int)Tokens.LessOrEqual; }
"+"					{ return (int)Tokens.Plus; }
"-"					{ return (int)Tokens.Minus; }
"*"					{ return (int)Tokens.Multiply; }
"/"					{ return (int)Tokens.Divide; }
"!"					{ return (int)Tokens.Negate; }
"~"					{ return (int)Tokens.BitwiseNegate; }
"(int)"				{ return (int)Tokens.IntConversion; }
"(double)"			{ return (int)Tokens.DoubleConversion; }


"if"				{ return (int)Tokens.If; }
"else"				{ return (int)Tokens.Else; }

"while"				{ return (int)Tokens.While; }

"read"				{ return (int)Tokens.Read; }
"write"				{ return (int)Tokens.Write; }

"return"			{ return (int)Tokens.Return; }
"("					{ return (int)Tokens.OpenBracket; }
")"					{ return (int)Tokens.CloseBracket; }
"{"					{ return (int)Tokens.OpenCurl; }
"}"					{ return (int)Tokens.CloseCurl; }
";"					{ return (int)Tokens.Semicolon; }
","					{ return (int)Tokens.Comma; }

"//.*"				{ }
"\n"				{ Compiler.CurrentLine = yyline + 1; }
{Whitespace}		{ }

{Identifier}		{ 
						yylval.String = yytext;
						return (int) Tokens.Identifier;
					}

%% 