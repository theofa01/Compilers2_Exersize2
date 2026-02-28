lexer grammar CGrammarLexer;

@lexer::members{
	public bool IsTypedef =false;

	Dictionary<string, int> typedefs = new Dictionary<string, int>();
}

tokens { TYPE_NAME }

// Lexer rules

fragment ESC : '\\"' | '\\\\' ; // 2-char sequences \" and \\
fragment DIGIT:	[0-9];
fragment LETTER: [a-zA-Z_];
fragment ALPHANUMERIC : [a-fA-F0-9];
fragment EXPONENT :	[Ee][+-]?DIGIT+;
fragment FLOATSPECIFIER :  ('f'|'F'|'l'|'L');
fragment INTEGERSPECIFIER: ('u'|'U'|'l'|'L')*;
fragment HEXDIGIT    : [0-9a-fA-F];


AUTO : 'auto';		
BREAK : 'break';
CASE : 'case';
CHAR : 'char';
CONST : 'const';
CONTINUE : 'continue';
DEFAULT : 'default';
DO :'do';
DOUBLE : 'double';
ELSE :'else';
ENUM: 'enum';
EXTERN : 'extern';
FLOAT :'float';
FOR :'for';
GOTO : 'goto';
IF :'if';
INT : 'int';
LONG :'long';
REGISTER : 'register';
RETURN : 'return';
SHORT :'short';
SIGNED: 'signed';
SIZEOF: 'sizeof';
STATIC : 'static';
STRUCT :'struct';
SWITCH :'switch';
TYPEDEF :'typedef' {IsTypedef =true; };
UNION : 'union';
UNSIGNED :'unsigned';
VOID :'void';
VOLATILE :'volatile';
WHILE :'while';

ELLIPSIS :'...';
RIGHT_ASSIGN : '>>=';
LEFT_ASSIGN : '<<=';
ADD_ASSIGN : '+=';
SUB_ASSIGN : '-=';
MUL_ASSIGN : '*=';
DIV_ASSIGN :'/=';
MOD_ASSIGN : '%=';
AND_ASSIGN : '&=';
XOR_ASSIGN : '^=';
OR_ASSIGN : '|=';
RIGHT_OP : '>>';
LEFT_OP :'<<';
INC_OP :'++';	
DEC_OP :'--';
PTR_OP : '->';		
AND_OP : '&&';
OR_OP : '||';	
LE_OP :'<=';
GE_OP : '>=';
EQ_OP : '==';
NE_OP :'!=';
SEMICOLON : ';';
LBRACE : ('{'|'<%');
RBRACE : ('}'|'%>');
COMMA : ',';
COLON :':';
ASSIGN :'=';
LPAREN : '(';
RPAREN : ')';
LBRACKET :('['|'<:');
RBRACKET : (']'|':>');	
MEMBEROP : '.';
AMBERSAND :'&';
NOT : '!';
TILDE : '~';
HYPHEN : '-';
PLUS : '+';
ASTERISK : '*';
SLASH : '/';
PERCENT : '%';
LESS :'<';
GREATER :'>';
CARET : '^';
OR : '|';
QMARK :'?';


IDENTIFIER :LETTER(LETTER|DIGIT)* {
									if (IsTypedef){
									IsTypedef = false;
										Type=TYPE_NAME;
										if (!typedefs.ContainsKey(Text)){
											typedefs.Add(Text, TYPE_NAME);
										}
									}
									else if (typedefs.ContainsKey(Text)){
										Type=TYPE_NAME;
									}
								  };

CONSTANT : '0'[xX]HEXDIGIT+INTEGERSPECIFIER? |
			'0'DIGIT+INTEGERSPECIFIER?	|
			DIGIT+ INTEGERSPECIFIER? |
			'L'?('\\'.|[^\\'])+ |
			DIGIT+ EXPONENT FLOATSPECIFIER? |
			DIGIT*'.'DIGIT+EXPONENT?FLOATSPECIFIER? |
			DIGIT+'.'DIGIT*EXPONENT?FLOATSPECIFIER?
			;

STRING_LITERAL : '"' (ESC|.)*? '"' ;


LINECOMMENT :	'//'.*?('\n'|'\r'|'\r\n') ->skip ;
MULTIPLELINECOMMENT : 	'/*' ('\n'|.)*?  '*/' -> skip ; 


WHITESPACE :[ \t\r\n\f]+ -> skip ;
