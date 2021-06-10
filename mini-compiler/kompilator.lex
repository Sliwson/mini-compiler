%namespace mini_compiler
%visibility public

%{
public override void yyerror(string format, params object[] args)
{
	Compiler.Errors.Add(new Error(yyline,string.Format(format, args)));
}
%}

// Literals
Identifier [a-zA-Z]([a-zA-Z0-9])*
IntegerLiteral	([0-9]|[1-9][0-9]*)
IntegerLiteralHex (0x|0X)([0-9a-fA-F]+)
DoubleLiteral	([0-9]|[1-9][0-9]*)[.][0-9]+
BoolLiteral		(true|false)
StringLiteral	\".*\"
Whitespace		\s	
Comment			"//".*

%% 

{Comment}           { }

"program"			{ return (int)Tokens.Program; }

{DoubleLiteral}		{
						yylval.String = yytext;
						return (int)Tokens.DoubleLiteral; 
					}

{IntegerLiteral}	{ 
						Int32.TryParse (yytext, NumberStyles.Integer, CultureInfo.InvariantCulture, out yylval.Integer);
						return (int)Tokens.IntegerLiteral; 
					}

{IntegerLiteralHex}	{ 
						yylval.Integer = Convert.ToInt32(yytext, 16);
						return (int)Tokens.IntegerLiteral; 
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

"\n"				{ Compiler.CurrentLine = yyline + 1; }

{Whitespace}		{ }

{Identifier}		{ 
						yylval.String = yytext;
						return (int) Tokens.Identifier;
					}


%% 