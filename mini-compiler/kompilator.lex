%using Parser;           //include the namespace of the generated Parser-class
%namespace Scanner       //names the Namespace of the generated Scanner-class
%visibility public       //visibility of the types "Tokens","ScanBase","Scanner"
%scannertype Scanner     //names the Scannerclass to "Scanner"
%scanbasetype ScanBase   //names the Scanbaseclass to "ScanBase"
%tokentype Tokens        //names the Tokenenumeration to "Tokens"

%{ //user-specified code will be copied in the Output-file
%}

OR [Oo][Rr]
AND [Aa][Nn][Dd]
Identifier [A-Za-z][A-Za-z0-9_]*

%% //Rules Section
%{ //user-code that will be executed before getting the next token
%}

{OR}           {return (int)Tokens.kwAND;}
{AND}          {return (int)Tokens.kwAND;}
{Identifier}   {return (int)Tokens.ID;}

%% //User-code Section