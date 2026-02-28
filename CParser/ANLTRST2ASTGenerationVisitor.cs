using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace CParser {

    public class ASTGenerationBuildParameters {
        private ASTComposite m_parent;
        private uint? m_context;
        public uint? Context {
            get => m_context;
            set => m_context = value;
        }
        public ASTComposite Parent {
            get => m_parent;
            set => m_parent = value;
        }
    }

    public class ANLTRST2ASTGenerationVisitor : CGrammarParserBaseVisitor<int> {

        ASTComposite m_root;
        Stack<ASTGenerationBuildParameters> m_contexts = new Stack<ASTGenerationBuildParameters>();
        private ASTGenerationBuildParameters m_lastCreatedNode = null;


        public ASTComposite Root {
            get => m_root;
        }

        public ANLTRST2ASTGenerationVisitor() {
            m_root = null;
        }

        public void VisitChildInContext(IParseTree child, ASTGenerationBuildParameters p) {
            if (child != null) {
                m_contexts.Push(p);
                Visit(child);
                m_contexts.Pop();
            }
        }

        public void VisitChildrenInContext(IParseTree[] children,
            ASTGenerationBuildParameters p) {
            if (children != null) {
                m_contexts.Push(p);
                foreach (var child in children) {
                    Visit(child);
                }
                m_contexts.Pop();
            }
        }

        public override int VisitTranslation_unit(CGrammarParser.Translation_unitContext context) {

            // 1. Create TranslationUnitAST node
            TranslationUnitAST tuNode = new TranslationUnitAST();
            m_root = tuNode;

            //2. Visit children and populate the AST node
            ASTGenerationBuildParameters tuContext = new ASTGenerationBuildParameters() {
                Parent = tuNode,
                Context = null
            };
            VisitChildrenInContext(context.external_declaration(), tuContext);

            return 0;
        }

        public override int VisitExternal_declaration(CGrammarParser.External_declarationContext context) {


            // 1. Visit Declarations
            ASTGenerationBuildParameters parentContextParameters = new ASTGenerationBuildParameters() {
                Parent = m_contexts.Peek().Parent,
                Context = TranslationUnitAST.DECLARATIONS
            };
            VisitChildInContext(context.declaration(), parentContextParameters);

            // 2. Visit Function Definitions
            parentContextParameters = new ASTGenerationBuildParameters() {
                Parent = m_contexts.Peek().Parent,
                Context = TranslationUnitAST.FUNCTION_DEFINITION
            };
            VisitChildInContext(context.function_definition(), parentContextParameters);

            return 0;
        }

        public override int VisitFunction_definition(CGrammarParser.Function_definitionContext context) {

            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create FunctionDefinitionAST node
            FunctionDefinitionAST funcDefNode = new FunctionDefinitionAST();
            parent.AddChild(funcDefNode, currentContext.Context);

            // 3. Visit Declaration Specifiers
            ASTGenerationBuildParameters p = new ASTGenerationBuildParameters() {
                Parent = funcDefNode,
                Context = FunctionDefinitionAST.DECLARATION_SPECIFIERS
            };
            VisitChildInContext(context.declaration_specifiers(), p);

            // 4. Visit Declarator
            p = new ASTGenerationBuildParameters() {
                Parent = funcDefNode,
                Context = FunctionDefinitionAST.DECLARATOR
            };
            VisitChildInContext(context.declarator(), p);

            // 5. Visit Compound Statement (Function Body)
            p = new ASTGenerationBuildParameters() {
                Parent = funcDefNode,
                Context = FunctionDefinitionAST.FUNCTION_BODY
            };
            VisitChildInContext(context.compound_statement(), p);

            return 0;
        }

        public override int VisitDeclaration(CGrammarParser.DeclarationContext context) {

            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create DeclarationAST node
            DeclarationAST declNode = new DeclarationAST();

            // 3. Add DeclarationAST node to parent
            parent.AddChild(declNode, currentContext.Context);

            ASTGenerationBuildParameters p = new ASTGenerationBuildParameters() {
                Parent = declNode,
                Context = DeclarationAST.TYPE_SPECIFIER
            };
            VisitChildInContext(context.declaration_specifiers(), p);

            p = new ASTGenerationBuildParameters() {
                Parent = declNode,
                Context = DeclarationAST.DECLARATORS
            };
            VisitChildInContext(context.init_declarator_list(), p);

            return 0;
        }

        public override int VisitParameter_declaration(CGrammarParser.Parameter_declarationContext context) {

            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;


            ParameterDeclarationAST pardecl = new ParameterDeclarationAST();
            parent.AddChild(pardecl, currentContext.Context); // assuming context PARAMETER_DECLARATION for simplicity


            ASTGenerationBuildParameters p = new ASTGenerationBuildParameters() {
                Parent = pardecl,
                Context = ParameterDeclarationAST.TYPE_SPECIFIER
            };
            VisitChildInContext(context.declaration_specifiers(), p);

            p = new ASTGenerationBuildParameters() {
                Parent = pardecl,
                Context = ParameterDeclarationAST.DECLARATOR
            };
            VisitChildInContext(context.declarator(), p);

            return 0;
        }

        public override int VisitPointer(CGrammarParser.PointerContext context) {

            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            PointerTypeAST pointerNode = new PointerTypeAST();
            parent.AddChild(pointerNode, currentContext.Context); // assuming context POINTER_TARGER for simplicity

            ASTGenerationBuildParameters p = new ASTGenerationBuildParameters() {
                Parent = pointerNode,
                Context = PointerTypeAST.POINTER_TARGET
            };

            if (context.pointer() == null) {
                m_lastCreatedNode = p;
            }

            if (context.pointer() != null) {
                VisitChildInContext(context.pointer(), p);
            }

            if (context.type_qualifier_list() != null) {
                VisitChildInContext(context.type_qualifier_list(), p);
            }

            return 0;
        }


        public override int VisitFunctionWithArguments(CGrammarParser.FunctionWithArgumentsContext context) {

            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            ASTGenerationBuildParameters paramContext;
            if (parent.MType == (uint)TranslationUnitAST.NodeTypes.FUNCTION_DEFINITION) {
                paramContext = new ASTGenerationBuildParameters() {
                    Parent = parent,
                    Context = FunctionDefinitionAST.DECLARATOR
                };
                VisitChildInContext(context.direct_declarator(), paramContext);

                paramContext = new ASTGenerationBuildParameters() {
                    Parent = parent,
                    Context = FunctionDefinitionAST.PARAMETER_DECLARATIONS
                };
                VisitChildInContext(context.parameter_type_list(), paramContext);
            } else {
                FunctionTypeAST funcTypeNode = new FunctionTypeAST();
                parent.AddChild(funcTypeNode, currentContext.Context); // assuming context FUNCTION_TYPE for simplicity

                paramContext = new ASTGenerationBuildParameters() {
                    Parent = parent,
                    Context = FunctionTypeAST.FUNCTION_NAME
                };
                VisitChildInContext(context.direct_declarator(), paramContext);

                paramContext = new ASTGenerationBuildParameters() {
                    Parent = funcTypeNode,
                    Context = FunctionTypeAST.FUNCTION_PARAMETERS
                };
                VisitChildInContext(context.parameter_type_list(), paramContext);

            }

            return 0;
        }

        public override int VisitDeclaration_specifiers(CGrammarParser.Declaration_specifiersContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Declaration_Specifiers node
            Declaration_Specifiers declSpecNode = new Declaration_Specifiers();

            // 3. Add Declaration_Specifiers node to parent
            parent.AddChild(declSpecNode, currentContext.Context); // assuming context DECLARATION_SPECIFIERS for simplicity

            // 4. Make the Declaration_Specifiers the current parent for child visits
            m_contexts.Push(new ASTGenerationBuildParameters() {
                Parent = declSpecNode,
                Context = Declaration_Specifiers.SPECIFIERS
            });

            VisitChildren(context);
            
            return 0;

        }

        public override int VisitType_specifier(CGrammarParser.Type_specifierContext context) {
            return base.VisitType_specifier(context);
        }

        public override int VisitTerminal(ITerminalNode node) {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;
            switch (node.Symbol.Type) {
                case CGrammarParser.IDENTIFIER:
                    IDENTIFIER idNode = new IDENTIFIER(node.GetText());

                    if (m_lastCreatedNode != null) {
                        m_lastCreatedNode.Parent.AddChild(idNode, m_lastCreatedNode.Context); // assuming context IDENTIFIER for simplicity
                        m_lastCreatedNode = null;
                    } else {
                        parent.AddChild(idNode, currentContext.Context); // assuming context IDENTIFIER for simplicity
                    }
                    break;
                case CGrammarParser.INT:
                    IntegerTypeAST intNode = new IntegerTypeAST(node.GetText());
                    parent.AddChild(intNode, currentContext.Context); // assuming context INT for simplicity

                    break;
                case CGrammarParser.FLOAT:
                    FloatTypeAST floatNode = new FloatTypeAST(node.GetText());
                    parent.AddChild(floatNode, currentContext.Context); // assuming context FLOAT for simplicity
                    
                    break;
                case CGrammarParser.DOUBLE:
                    DoubleTypeAST doubleNode = new DoubleTypeAST(node.GetText());
                    parent.AddChild(doubleNode, currentContext.Context); // assuming context DOUBLE for simplicity

                    break;
                case CGrammarParser.CHAR:
                    CharTypeAST charNode = new CharTypeAST(node.GetText());
                    parent.AddChild(charNode, currentContext.Context); // assuming context INT for simplicity
                    
                    break;
                case CGrammarParser.SHORT:
                    ShortTypeAST shortNode = new ShortTypeAST(node.GetText());
                    parent.AddChild(shortNode, currentContext.Context); // assuming context SHORT for simplicity
                    
                    break;
                case CGrammarParser.LONG:
                    LongTypeAST longNode = new LongTypeAST(node.GetText());
                    parent.AddChild(longNode, currentContext.Context); // assuming context LONG for simplicity
                    
                    break;
                case CGrammarParser.SIGNED:
                    SignedTypeAST signedNode = new SignedTypeAST(node.GetText());
                    parent.AddChild(signedNode, currentContext.Context); // assuming context LONG for simplicity

                    break;
                case CGrammarParser.UNSIGNED:
                    UnsignedTypeAST unsignedNode = new UnsignedTypeAST(node.GetText());
                    parent.AddChild(unsignedNode, currentContext.Context); // assuming context LONG for simplicity
                    
                    break;
                case CGrammarParser.STRUCT:
                    StructTypeAST structNode = new StructTypeAST(node.GetText());
                    parent.AddChild(structNode, currentContext.Context); // assuming context STRUCT for simplicity
                    
                    break;
                case CGrammarParser.UNION:
                    UnionTypeAST unionNode = new UnionTypeAST(node.GetText());
                    parent.AddChild(unionNode, currentContext.Context);

                    break;
                case CGrammarParser.VOID:
                    VoidTypeAST voidNode = new VoidTypeAST(node.GetText());
                    parent.AddChild(voidNode, currentContext.Context);
                    
                    break;
                case CGrammarParser.CONSTANT: 
                    INTEGER conNode = new INTEGER(node.GetText());
                    parent.AddChild(conNode, currentContext.Context); // assuming context INTEGER for simplicity
                    
                    break;
                default:
                    break;
            }

            return 0;
        }
        
        public override int VisitCompound_statement(CGrammarParser.Compound_statementContext context) {

            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create FunctionDefinitionAST node
            CompoundStatement compStmtNode = new CompoundStatement();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(compStmtNode, currentContext.Context); // assuming context


            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = compStmtNode,
                Context = CompoundStatement.DECLARATIONS
            };
            VisitChildrenInContext(context.declaration(), paramContext);

            paramContext = new ASTGenerationBuildParameters() {
                Parent = compStmtNode,
                Context = CompoundStatement.STATEMENTS
            };
            VisitChildrenInContext(context.statement(), paramContext);


            return 0;
        }

        public override int VisitAssignment_expression_Assignment(
            CGrammarParser.Assignment_expression_AssignmentContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create FunctionDefinitionAST node
            var aoperator = context.assignment_operator();
            ASTComposite aOperatorNode = null;

            var text = aoperator.op.Text;

            switch (aoperator.op.Type) {
                case CGrammarLexer.ASSIGN:
                    aOperatorNode =
                        new Expression_Assignment();
                    break;
                case CGrammarLexer.MUL_ASSIGN:
                    aOperatorNode =
                        new ExpressionAssignmentMultiplication();
                    break;
                case CGrammarLexer.DIV_ASSIGN:
                    aOperatorNode =
                        new ExpressionAssignmentDivision();
                    break;
                case CGrammarLexer.ADD_ASSIGN:
                    aOperatorNode =
                        new ExpressionAssignmentAddition();
                    break;
                case CGrammarLexer.MOD_ASSIGN:
                    aOperatorNode =
                        new ExpressionAssignmentModulo();
                    break;
                case CGrammarLexer.LEFT_ASSIGN:
                    aOperatorNode =
                        new Expression_AssignmentLeft();
                    break;
                case CGrammarLexer.RIGHT_ASSIGN:
                    aOperatorNode =
                        new Expression_AssignmentRight();
                    break;
                case CGrammarLexer.OR_ASSIGN:
                    aOperatorNode =
                        new Expression_AssignmentOr();
                    break;
                case CGrammarLexer.AND_ASSIGN:
                    aOperatorNode =
                        new Expression_AssignmentAnd();
                    break;
                case CGrammarLexer.XOR_ASSIGN:
                    aOperatorNode =
                        new Expression_AssignmentXor();
                    break;
                case CGrammarLexer.SUB_ASSIGN:
                    aOperatorNode = 
                        new ExpressionAssignmentSubtraction();
                    break;
                default:
                    throw new NotImplementedException("Unhandled unary operator type");

            }
            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(aOperatorNode, currentContext.Context); // assuming context

            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = aOperatorNode,
                Context = Expression_Assignment.LEFT
            };
            VisitChildInContext(context.unary_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters() {
                Parent = aOperatorNode,
                Context = Expression_Assignment.RIGHT
            };
            VisitChildInContext(context.assignment_expression(), paramContext);

            return 0;
        }

        public override int VisitAdditive_expression_Addition(CGrammarParser.Additive_expression_AdditionContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Addition node
            Expression_Addition addition = new Expression_Addition();

            // 3. Add Addition node to parent
            parent.AddChild(addition, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = addition,
                Context = Expression_Addition.LEFT
            };
            VisitChildInContext(context.additive_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters() {
                Parent = addition,
                Context = Expression_Addition.RIGHT
            };
            VisitChildInContext(context.multiplicative_expression(), paramContext);


            return 0;
        }

        public override int VisitAdditive_expression_Subtraction(CGrammarParser.Additive_expression_SubtractionContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Addition node
            Expression_Subtraction subtraction = new Expression_Subtraction();

            // 3. Add Addition node to parent
            parent.AddChild(subtraction, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = subtraction,
                Context = Expression_Subtraction.LEFT
            };
            VisitChildInContext(context.additive_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters() {
                Parent = subtraction,
                Context = Expression_Subtraction.RIGHT
            };
            VisitChildInContext(context.multiplicative_expression(), paramContext);
            return 0;
        }

        public override int VisitMultiplicative_expression_Division(CGrammarParser.Multiplicative_expression_DivisionContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Addition node
            Expression_Division division = new Expression_Division();

            // 3. Add Addition node to parent
            parent.AddChild(division, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = division,
                Context = Expression_Division.LEFT
            };
            VisitChildInContext(context.multiplicative_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters() {
                Parent = division,
                Context = Expression_Division.RIGHT
            };
            VisitChildInContext(context.cast_expression(), paramContext);
            return 0;
        }

        public override int VisitMultiplicative_expression_Multiplication(CGrammarParser.Multiplicative_expression_MultiplicationContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Addition node
            Expression_Multiplication multiplications = new Expression_Multiplication();

            // 3. Add Addition node to parent
            parent.AddChild(multiplications, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = multiplications,
                Context = Expression_Multiplication.LEFT
            };
            VisitChildInContext(context.multiplicative_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters() {
                Parent = multiplications,
                Context = Expression_Multiplication.RIGHT
            };
            VisitChildInContext(context.cast_expression(), paramContext);
            return 0;
        }

        public override int VisitMultiplicative_expression_Modulus(CGrammarParser.Multiplicative_expression_ModulusContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Addition node
            Expression_Modulo modulo = new Expression_Modulo();

            // 3. Add Addition node to parent
            parent.AddChild(modulo, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = modulo,
                Context = Expression_Modulo.LEFT
            };
            VisitChildInContext(context.multiplicative_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters() {
                Parent = modulo,
                Context = Expression_Modulo.RIGHT
            };
            VisitChildInContext(context.cast_expression(), paramContext);
            return 0;
        }
        public override int VisitShift_expression_LeftShift(CGrammarParser.Shift_expression_LeftShiftContext context)
        {

            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;
            // 2. Create LeftShift node
            ExpressionShiftLeft leftShift = new ExpressionShiftLeft();
            // 3. Add LeftShift node to parent
            parent.AddChild(leftShift, currentContext.Context);
            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = leftShift,
                Context = ExpressionShiftLeft.LEFT
            };
            VisitChildInContext(context.shift_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = leftShift,
                Context = ExpressionShiftLeft.RIGHT
            };
            VisitChildInContext(context.additive_expression(), paramContext);

            return 0;

        }

        public override int VisitShift_expression_RightShift(CGrammarParser.Shift_expression_RightShiftContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;
            // 2. Create RightShift node
            ExpressionShiftRight rightShift = new ExpressionShiftRight();
            // 3. Add RightShift node to parent
            parent.AddChild(rightShift, currentContext.Context);
            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = rightShift,
                Context = ExpressionShiftRight.LEFT
            };
            VisitChildInContext(context.shift_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = rightShift,
                Context = ExpressionShiftRight.RIGHT
            };
            VisitChildInContext(context.additive_expression(), paramContext);
            return 0;
        }

        public override int VisitUnary_expression_Increment(CGrammarParser.Unary_expression_IncrementContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create FunctionDefinitionAST node
            UnaryExpressionIncrement unaryIncrement = new UnaryExpressionIncrement();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(unaryIncrement, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = unaryIncrement,
                Context = UnaryExpressionIncrement.OPERAND
            };
            VisitChildInContext(context.unary_expression(), paramContext);
            return 0;
        }

        public override int VisitUnary_expression_Decrement(CGrammarParser.Unary_expression_DecrementContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create FunctionDefinitionAST node
            UnaryExpressionDecrement unaryDecrement = new UnaryExpressionDecrement();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(unaryDecrement, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = unaryDecrement,
                Context = UnaryExpressionDecrement.OPERAND
            };
            VisitChildInContext(context.unary_expression(), paramContext);
            return 0;
        }

        public override int VisitPostfix_expression_ArraySubscript(CGrammarParser.Postfix_expression_ArraySubscriptContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Postfixexpression_ArraySubscript arraySubscript = new Postfixexpression_ArraySubscript();

            parent.AddChild(arraySubscript, currentContext.Context);
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = arraySubscript,
                Context = Postfixexpression_ArraySubscript.ARRAY
            };
            VisitChildInContext(context.postfix_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = arraySubscript,
                Context = Postfixexpression_ArraySubscript.INDEX
            };
            VisitChildInContext(context.expression(), paramContext);

            return 0;
        }

        public override int VisitPostfix_expression_FunctionCallNoArgs(CGrammarParser.Postfix_expression_FunctionCallNoArgsContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Postfixexpression_FunctionCallNoArgs noArgsCall = new Postfixexpression_FunctionCallNoArgs();

            parent.AddChild(noArgsCall, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = noArgsCall,
                Context = Postfixexpression_FunctionCallNoArgs.FUNCTION
            };
            VisitChildInContext(context.postfix_expression(), paramContext);

            return 0;
        }

        public override int VisitPostfix_expression_FunctionCallWithArgs(CGrammarParser.Postfix_expression_FunctionCallWithArgsContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Postfixexpression_FunctionCallWithArgs WithArgsCall = new Postfixexpression_FunctionCallWithArgs();

            parent.AddChild(WithArgsCall, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = WithArgsCall,
                Context = Postfixexpression_FunctionCallWithArgs.FUNCTION
            };
            VisitChildInContext(context.postfix_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = WithArgsCall,
                Context = Postfixexpression_FunctionCallWithArgs.ARGUMENTS
            };
            VisitChildInContext(context.argument_expression_list(), paramContext);

            return 0;
        }

        public override int VisitPostfix_expression_PointerMemberAccess(CGrammarParser.Postfix_expression_PointerMemberAccessContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Postfixexpression_PointerMemberAccess pointerMemberAccess = new Postfixexpression_PointerMemberAccess();

            parent.AddChild(pointerMemberAccess, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = pointerMemberAccess,
                Context = Postfixexpression_PointerMemberAccess.ACCESS
            };
            VisitChildInContext(context.postfix_expression(), paramContext);
            
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = pointerMemberAccess,
                Context = Postfixexpression_PointerMemberAccess.MEMBER
            };
            VisitChildInContext(context.IDENTIFIER(), paramContext);

            return 0;
        }

        public override int VisitPostfix_expression_MemberAccess(CGrammarParser.Postfix_expression_MemberAccessContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Postfixexpression_MemberAccess memberAccess = new Postfixexpression_MemberAccess();

            parent.AddChild(memberAccess, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = memberAccess,
                Context = Postfixexpression_MemberAccess.ACCESS
            };
            VisitChildInContext(context.postfix_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = memberAccess,
                Context = Postfixexpression_MemberAccess.MEMBER
            };
            VisitChildInContext(context.IDENTIFIER(), paramContext);

            return 0;
        }

        public override int VisitPostfix_expression_Decrement(CGrammarParser.Postfix_expression_DecrementContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Postfixexpression_Decrement dec = new Postfixexpression_Decrement();

            parent.AddChild(dec, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = dec,
                Context = Postfixexpression_Decrement.ACCESS
            };
            VisitChildInContext(context.postfix_expression(), paramContext);

            return 0;
        }

        public override int VisitPostfix_expression_Increment(CGrammarParser.Postfix_expression_IncrementContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Postfixexpression_Increment inc = new Postfixexpression_Increment();

            parent.AddChild(inc, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = inc,
                Context = Postfixexpression_Increment.ACCESS
            };
            VisitChildInContext(context.postfix_expression(), paramContext);


            return 0;
        }

        public override int VisitLogical_and_expression_LogicalAND(CGrammarParser.Logical_and_expression_LogicalANDContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            ExpressionLogicalAnd andExpr = new ExpressionLogicalAnd();

            parent.AddChild(andExpr, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = andExpr,
                Context = ExpressionLogicalAnd.LEFT
            };
            VisitChildInContext(context.logical_and_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = andExpr,
                Context = ExpressionLogicalAnd.RIGHT
            };
            VisitChildInContext(context.inclusive_or_expression(), paramContext);

            return 0;
        }

        public override int VisitLogical_or_expression_LogicalOR(CGrammarParser.Logical_or_expression_LogicalORContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            ExpressionLogicalOr orExpr = new ExpressionLogicalOr();

            parent.AddChild(orExpr, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = orExpr,
                Context = ExpressionLogicalOr.LEFT
            };
            VisitChildInContext(context.logical_or_expression(), paramContext);
            
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = orExpr,
                Context = ExpressionLogicalOr.RIGHT
            };
            VisitChildInContext(context.logical_and_expression(), paramContext);

            return 0;
        }

        public override int VisitAnd_expression_BitwiseAND(CGrammarParser.And_expression_BitwiseANDContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Expression_BitwiseAND bitwiseAnd = new Expression_BitwiseAND();

            parent.AddChild(bitwiseAnd, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = bitwiseAnd,
                Context = Expression_BitwiseAND.LEFT
            };
            VisitChildInContext(context.and_expression(), paramContext);
            
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = bitwiseAnd,
                Context = Expression_BitwiseAND.RIGHT
            };
            VisitChildInContext(context.equality_expression(), paramContext);

            return 0;
        }

        public override int VisitInclusive_or_expression_BitwiseOR(CGrammarParser.Inclusive_or_expression_BitwiseORContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Expression_BitwiseOR bitwiseOr = new Expression_BitwiseOR();

            parent.AddChild(bitwiseOr, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = bitwiseOr,
                Context = Expression_BitwiseOR.LEFT
            };
            VisitChildInContext(context.inclusive_or_expression(), paramContext);


            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = bitwiseOr,
                Context = Expression_BitwiseOR.RIGHT
            };
            VisitChildInContext(context.exclusive_or_expression(), paramContext);

            return 0;
        }

        public override int VisitExclusive_or_expression_BitwiseXOR(CGrammarParser.Exclusive_or_expression_BitwiseXORContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Expression_BitwiseXOR bitwiseXor = new Expression_BitwiseXOR();

            parent.AddChild(bitwiseXor, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = bitwiseXor,
                Context = Expression_BitwiseXOR.LEFT
            };
            VisitChildInContext(context.exclusive_or_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = bitwiseXor,
                Context = Expression_BitwiseXOR.RIGHT
            };
            VisitChildInContext(context.and_expression(), paramContext);

            return 0;
        }

        public override int VisitUnary_expression_UnaryOperator(CGrammarParser.Unary_expression_UnaryOperatorContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            CExpression expressionNode = null;
            int exprContext = -1;

            int opType = context.unary_operator().op.Type;

            switch (opType)
            {
                case CGrammarLexer.AMBERSAND:
                    expressionNode = new UnaryExpressionUnaryOperatorAmbersand();
                    exprContext = UnaryExpressionUnaryOperatorAmbersand.EXPRESSION;
                    break;
                case CGrammarLexer.ASTERISK:
                    expressionNode = new UnaryExpressionUnaryOperatorAsterisk();
                    exprContext = UnaryExpressionUnaryOperatorAsterisk.EXPRESSION;
                    break;
                case CGrammarLexer.PLUS:
                    expressionNode = new UnaryExpressionUnaryOperatorPLUS();
                    exprContext = UnaryExpressionUnaryOperatorPLUS.EXPRESSION;
                    break;
                case CGrammarLexer.HYPHEN:
                    expressionNode = new UnaryExpressionUnaryOperatorMINUS();
                    exprContext = UnaryExpressionUnaryOperatorMINUS.EXPRESSION;
                    break;
                case CGrammarLexer.TILDE:
                    expressionNode = new UnaryExpressionUnaryOperatorTilde();
                    exprContext = UnaryExpressionUnaryOperatorTilde.EXPRESSION;
                    break;
                case CGrammarLexer.NOT:
                    expressionNode = new UnaryExpressionUnaryOperatorNOT();
                    exprContext = UnaryExpressionUnaryOperatorNOT.EXPRESSION;
                    break;
                default:
                    throw new ArgumentException("Invalid Operand");
            }
            parent.AddChild(expressionNode, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = expressionNode,
                Context = (uint)exprContext
            };
            VisitChildInContext(context.cast_expression(), paramContext);

            return 0;
        }

        public override int VisitCast_expression_Cast(CGrammarParser.Cast_expression_CastContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Expression_Cast exprCast = new Expression_Cast();

            parent.AddChild(exprCast, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprCast,
                Context = Expression_Cast.TYPE
            };
            VisitChildInContext(context.type_name(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprCast,
                Context = Expression_Cast.EXPRESSION
            };
            VisitChildInContext(context.cast_expression(), paramContext);

            return 0;
        }

        public override int VisitRelational_expression_LessThan(CGrammarParser.Relational_expression_LessThanContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            ExpressionRelationalLess exprLess = new ExpressionRelationalLess();

            parent.AddChild(exprLess, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprLess,
                Context = ExpressionRelationalLess.LEFT
            };
            VisitChildInContext(context.relational_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprLess,
                Context = ExpressionRelationalLess.RIGHT
            };
            VisitChildInContext(context.shift_expression(), paramContext);

            return 0;
        }

        public override int VisitRelational_expression_GreaterThan(CGrammarParser.Relational_expression_GreaterThanContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            ExpressionRelationalGreater exprGreat = new ExpressionRelationalGreater();

            parent.AddChild(exprGreat, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprGreat,
                Context = ExpressionRelationalGreater.LEFT
            };
            VisitChildInContext(context.relational_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprGreat,
                Context = ExpressionRelationalGreater.RIGHT
            };
            VisitChildInContext(context.shift_expression(), paramContext);

            return 0;
        }

        public override int VisitEquality_expression_Equal(CGrammarParser.Equality_expression_EqualContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Expression_EqualityEqual exprEQ = new Expression_EqualityEqual();

            parent.AddChild(exprEQ, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprEQ,
                Context = Expression_EqualityEqual.LEFT
            };
            VisitChildInContext(context.equality_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprEQ,
                Context = Expression_EqualityEqual.RIGHT
            };
            VisitChildInContext(context.relational_expression(), paramContext);

            return 0;
        }

        public override int VisitEquality_expression_NotEqual(CGrammarParser.Equality_expression_NotEqualContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            Expression_EqualityNotEqual exprNEQ = new Expression_EqualityNotEqual();

            parent.AddChild(exprNEQ, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprNEQ,
                Context = Expression_EqualityNotEqual.LEFT
            };
            VisitChildInContext(context.equality_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprNEQ,
                Context = Expression_EqualityNotEqual.RIGHT
            };
            VisitChildInContext(context.relational_expression(), paramContext);

            return 0;
        }

        public override int VisitRelational_expression_GreaterThanOrEqual(CGrammarParser.Relational_expression_GreaterThanOrEqualContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            ExpressionRelationalGreaterOrEqual exprGTE = new ExpressionRelationalGreaterOrEqual();

            parent.AddChild(exprGTE, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprGTE,
                Context = ExpressionRelationalGreaterOrEqual.LEFT
            };
            VisitChildInContext(context.relational_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprGTE,
                Context = ExpressionRelationalGreaterOrEqual.RIGHT
            };
            VisitChildInContext(context.shift_expression(), paramContext);

            return 0;
        }

        public override int VisitRelational_expression_LessThanOrEqual(CGrammarParser.Relational_expression_LessThanOrEqualContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            ExpressionRelationalLessOrEqual exprLTE = new ExpressionRelationalLessOrEqual();

            parent.AddChild(exprLTE, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprLTE,
                Context = ExpressionRelationalLessOrEqual.LEFT
            };
            VisitChildInContext(context.relational_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = exprLTE,
                Context = ExpressionRelationalLessOrEqual.RIGHT
            };
            VisitChildInContext(context.shift_expression(), paramContext);

            return 0;
        }

        public override int VisitConditional_expression_Conditional(CGrammarParser.Conditional_expression_ConditionalContext context)
        {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            ConditionalExpression condExpr = new ConditionalExpression();

            parent.AddChild(condExpr, currentContext.Context);
            ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
            {
                Parent = condExpr,
                Context = ConditionalExpression.CONDITION
            };
            VisitChildInContext(context.logical_or_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = condExpr,
                Context = ConditionalExpression.TRUE_EXPRESSION
            };
            VisitChildInContext(context.expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = condExpr,
                Context = ConditionalExpression.FALSE_EXPRESSION
            };
            VisitChildInContext(context.conditional_expression(), paramContext);

            return 0;
        }
    }
}
