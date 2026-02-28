parser grammar CGrammarParser;

options { tokenVocab = CGrammarLexer; }

@parser::header{
	using CParser;
}

@parser::members {
	
}

// Parser rules

primary_expression
	: IDENTIFIER				#primary_expression_Identifier
	| CONSTANT					#primary_expression_Constant
	| STRING_LITERAL			#primary_expression_StringLiteral
	| LPAREN expression RPAREN  #primary_expression_ParenthesizedExpression
	;

postfix_expression
	: primary_expression										#postfix_expression_PrimaryExpression
	| postfix_expression LBRACKET expression RBRACKET			#postfix_expression_ArraySubscript
	| postfix_expression LPAREN RPAREN							#postfix_expression_FunctionCallNoArgs
	| postfix_expression LPAREN argument_expression_list RPAREN #postfix_expression_FunctionCallWithArgs
	| postfix_expression MEMBEROP IDENTIFIER					#postfix_expression_MemberAccess
	| postfix_expression PTR_OP IDENTIFIER						#postfix_expression_PointerMemberAccess
	| postfix_expression INC_OP									#postfix_expression_Increment
	| postfix_expression DEC_OP									#postfix_expression_Decrement
	;

argument_expression_list : assignment_expression (COMMA assignment_expression)*
	;

unary_expression
	: postfix_expression				#unary_expression_PostfixExpression
	| INC_OP unary_expression			#unary_expression_Increment
	| DEC_OP unary_expression			#unary_expression_Decrement
	| unary_operator cast_expression	#unary_expression_UnaryOperator
	| SIZEOF unary_expression			#unary_expression_SizeofExpression
	| SIZEOF LPAREN type_name RPAREN	#unary_expression_SizeofTypeName
	;

unary_operator
	: op= (AMBERSAND | ASTERISK	| PLUS 	| HYPHEN | TILDE| NOT)
	;

cast_expression
	: unary_expression							#cast_expression_UnaryExpression
	| LPAREN type_name RPAREN cast_expression	#cast_expression_Cast
	;

multiplicative_expression
	: cast_expression										#multiplicative_expression_CastExpression
	| multiplicative_expression ASTERISK cast_expression	#multiplicative_expression_Multiplication
	| multiplicative_expression SLASH cast_expression		#multiplicative_expression_Division
	| multiplicative_expression PERCENT cast_expression		#multiplicative_expression_Modulus
	;

additive_expression
	: multiplicative_expression								#additive_expression_MultiplicativeExpression
	| additive_expression PLUS multiplicative_expression	#additive_expression_Addition
	| additive_expression HYPHEN multiplicative_expression	#additive_expression_Subtraction
	;

shift_expression
	: additive_expression									#shift_expression_AdditiveExpression
	| shift_expression LEFT_OP additive_expression			#shift_expression_LeftShift
	| shift_expression RIGHT_OP additive_expression			#shift_expression_RightShift
	;

relational_expression
	: shift_expression										#relational_expression_ShiftExpression
	| relational_expression LESS shift_expression			#relational_expression_LessThan
	| relational_expression GREATER shift_expression		#relational_expression_GreaterThan
	| relational_expression LE_OP shift_expression			#relational_expression_LessThanOrEqual
	| relational_expression GE_OP shift_expression			#relational_expression_GreaterThanOrEqual
	;

equality_expression
	: relational_expression									#equality_expression_RelationalExpression
	| equality_expression EQ_OP relational_expression		#equality_expression_Equal
	| equality_expression NE_OP relational_expression		#equality_expression_NotEqual
	;

and_expression
	: equality_expression									#and_expression_EqualityExpression
	| and_expression AMBERSAND equality_expression			#and_expression_BitwiseAND
	;

exclusive_or_expression
	: and_expression										#exclusive_or_expression_AndExpression
	| exclusive_or_expression CARET and_expression			#exclusive_or_expression_BitwiseXOR
	;

inclusive_or_expression
	: exclusive_or_expression								#inclusive_or_expression_ExclusiveOrExpression
	| inclusive_or_expression OR exclusive_or_expression	#inclusive_or_expression_BitwiseOR
	;

logical_and_expression
	: inclusive_or_expression								#logical_and_expression_InclusiveOrExpression
	| logical_and_expression AND_OP inclusive_or_expression	#logical_and_expression_LogicalAND
	;

logical_or_expression
	: logical_and_expression								#logical_or_expression_InclusiveOrExpression
	| logical_or_expression OR_OP logical_and_expression	#logical_or_expression_LogicalOR
	;

conditional_expression
	: logical_or_expression													#conditional_expression_LogicalOrExpression
	| logical_or_expression QMARK expression COLON conditional_expression	#conditional_expression_Conditional
	;

assignment_expression
	: conditional_expression										#assignment_expression_ConditionalExpression
	| unary_expression assignment_operator assignment_expression	#assignment_expression_Assignment
	;

assignment_operator
	: op=(ASSIGN 
	| MUL_ASSIGN
	| DIV_ASSIGN
	| MOD_ASSIGN
	| ADD_ASSIGN
	| SUB_ASSIGN
	| LEFT_ASSIGN
	| RIGHT_ASSIGN
	| AND_ASSIGN
	| XOR_ASSIGN
	| OR_ASSIGN)
	;

expression
	: assignment_expression						#expression_AssignmentExpression
	| expression COMMA assignment_expression	#expression_CommaExpression
	;

constant_expression
	: conditional_expression
	;

translation_unit : external_declaration+ EOF
;

external_declaration : function_definition
					 | declaration
					 ;

declaration	: declaration_specifiers init_declarator_list? SEMICOLON
	;

init_declarator_list: init_declarator (COMMA init_declarator)*
	;

init_declarator	:  declarator (ASSIGN initializer)?
	;

initializer
	: assignment_expression
	| LBRACE initializer_list RBRACE	
	;

initializer_list: initializer (COMMA initializer)*
	;

declarator: pointer direct_declarator  
	| direct_declarator
	;

direct_declarator
	: IDENTIFIER												#IDENTIFIER
	| LPAREN declarator RPAREN									#Parenthesis
	| direct_declarator LBRACKET constant_expression RBRACKET	#ArrayDimensionWithSIZE
	| direct_declarator LBRACKET RBRACKET						#ArrayDimensionWithNOSIZE
	| direct_declarator LPAREN parameter_type_list RPAREN		#FunctionWithArguments
	| direct_declarator LPAREN RPAREN							#FunctionWithNOArguments
	;

type_name
	: specifier_qualifier_list
	| specifier_qualifier_list abstract_declarator
	;

pointer
	: ASTERISK
	| ASTERISK type_qualifier_list
	| ASTERISK pointer
	| ASTERISK type_qualifier_list pointer
	;

type_qualifier_list
	: type_qualifier
	| type_qualifier_list type_qualifier
	;

abstract_declarator
	: pointer
	| direct_abstract_declarator
	| pointer direct_abstract_declarator
	;

direct_abstract_declarator
	: LPAREN abstract_declarator RPAREN
	| LBRACKET RBRACKET
	| LBRACKET constant_expression RBRACKET
	| direct_abstract_declarator LBRACKET RBRACKET
	| direct_abstract_declarator LBRACKET constant_expression RBRACKET
	| LPAREN RPAREN
	| LPAREN parameter_type_list RPAREN
	| direct_abstract_declarator LPAREN RPAREN
	| direct_abstract_declarator LPAREN parameter_type_list RPAREN
	;


function_definition	: declaration_specifiers  declarator  compound_statement 
					;

declaration_specifiers : (storage_class_specifier|type_specifier|type_qualifier)+
	;

storage_class_specifier : TYPEDEF
	| EXTERN
	| STATIC
	| AUTO
	| REGISTER;

type_specifier :  VOID
	| CHAR
	| SHORT
	| INT
	| LONG
	| FLOAT
	| DOUBLE
	| SIGNED
	| UNSIGNED
	| struct_or_union_specifier
	| enum_specifier
	| TYPE_NAME
	;

struct_or_union_specifier
	: struct_or_union IDENTIFIER? LBRACE struct_declaration+ RBRACE
	| struct_or_union IDENTIFIER
	;

struct_or_union
	: STRUCT
	| UNION
	;

struct_declaration
	: specifier_qualifier_list struct_declarator_list SEMICOLON
	;

specifier_qualifier_list : (type_specifier | type_qualifier )+
	;

struct_declarator_list	: struct_declarator (COMMA struct_declarator)*
	;

struct_declarator
	: declarator
	| COLON constant_expression
	| declarator COLON constant_expression
	;

enum_specifier
	: ENUM LBRACE enumerator_list RBRACE
	| ENUM IDENTIFIER LBRACE enumerator_list RBRACE
	| ENUM IDENTIFIER
	;

enumerator_list : enumerator (COMMA enumerator)* ;
	

enumerator:  IDENTIFIER (ASSIGN constant_expression)?
	;


type_qualifier	: CONST
	| VOLATILE
	;


parameter_type_list : parameter_declaration ( COMMA parameter_declaration )* (COMMA ELLIPSIS)?
	;

parameter_declaration : declaration_specifiers declarator
	| declaration_specifiers abstract_declarator?
	;
	
compound_statement : LBRACE declaration* statement* RBRACE
	;

statement
	: labeled_statement			#statement_LabeledStatement
	| compound_statement		#statement_CompoundStatement
	| expression_statement		#statement_ExpressionStatement
	| selection_statement		#statement_SelectionStatement
	| iteration_statement		#statement_IterationStatement
	| jump_statement			#statement_JumpStatement
	;

labeled_statement
	: IDENTIFIER COLON statement
	| CASE constant_expression COLON statement
	| DEFAULT COLON statement
	;


expression_statement
	: SEMICOLON
	| expression SEMICOLON
	;

selection_statement
	: IF LPAREN expression RPAREN statement
	| IF LPAREN expression RPAREN statement ELSE statement
	| SWITCH LPAREN expression RPAREN statement
	;

iteration_statement
	: WHILE LPAREN expression RPAREN statement
	| DO statement WHILE LPAREN expression RPAREN SEMICOLON
	| FOR LPAREN expression_statement expression_statement RPAREN statement
	| FOR LPAREN expression_statement expression_statement expression RPAREN statement
	;

jump_statement
	: GOTO IDENTIFIER SEMICOLON
	| CONTINUE SEMICOLON
	| BREAK SEMICOLON
	| RETURN SEMICOLON
	| RETURN expression SEMICOLON
	;
