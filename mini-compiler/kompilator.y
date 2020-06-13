%using Scanner      //include the Namespace of the scanner-class
%output=Parser.cs   //names the output-file
%namespace Parser  //names the namespace of the Parser-class

%parsertype Parser      //names the Parserclass to "Parser"
%scanbasetype ScanBase  //names the ScanBaseclass to "ScanBase"
%tokentype Tokens       //names the Tokensenumeration to "Tokens"

%token kwAND "AND", kwOR "OR" //the received Tokens from GPLEX
%token ID

%% //Grammar Rules Section

program  : /* nothing */
         | Statements
         ;

Statements : EXPR "AND" EXPR
           | EXPR "OR" EXPR
           ;

EXPR : ID
     ;

%% //User-code Section

// Don't forget to declare the Parser-Constructor
public Parser(Scanner.Scanner scnr) : base(scnr) { }