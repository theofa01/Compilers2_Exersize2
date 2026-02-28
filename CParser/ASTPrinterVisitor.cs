using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace CParser {
    public class ASTPrinterVisitor : BaseASTVisitor<int, ASTComposite>{
        private string m_astDOTFilename;
        StreamWriter m_writer;
        private static uint ms_clusternumber = 0;

        public ASTPrinterVisitor(string dotFilename) {
            m_astDOTFilename = dotFilename;
        }

        public void CreateContext(ASTComposite node, uint context, string ContextName) {
            m_writer.WriteLine($"subgraph cluster{ms_clusternumber++} {{");
            m_writer.WriteLine($"\t node [style=filled, color=white]; ");
            m_writer.WriteLine($"\t style=filled; color=lightgrey;");
            foreach (var child in node.MChildren[context]) {
                m_writer.Write($"\"{child.MName}\";");
            }

            m_writer.WriteLine();
            m_writer.WriteLine($"\t label = \"{ContextName}\";");
            m_writer.WriteLine("}");
        }


        public override int VisitTranslationUnit(TranslationUnitAST node, ASTComposite parent) {

            // 1. Open DOT file for writing
            m_writer = new StreamWriter(m_astDOTFilename);

            // 2. Write DOT file header
            m_writer.WriteLine("digraph AST {");

            // 2.a Add context clusters
            CreateContext(node, TranslationUnitAST.DECLARATIONS, "Declarations");
            CreateContext(node, TranslationUnitAST.FUNCTION_DEFINITION, "Function Definitions");


            // 3. Visit children and print AST nodes and edges
            VisitChildren(node, node);

            // 4. Write DOT file footer
            m_writer.WriteLine("}");

            // 5. Close DOT file
            m_writer.Close();

            // 6. Call dot to generate PNG from DOT file
            // call dot to generate png
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "dot";
            startInfo.Arguments = $"-Tgif {m_astDOTFilename} -o {m_astDOTFilename}.gif";
            Process process = Process.Start(startInfo);
            process.WaitForExit();

            return 0;
        }


        public override int VisitParameterDeclaration(ParameterDeclarationAST node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ParameterDeclarationAST.DECLARATOR, "Declarators");
            CreateContext(node, ParameterDeclarationAST.TYPE_SPECIFIER, "Type Specifier");
            CreateContext(node, ParameterDeclarationAST.TYPE_QUALIFIER, "Type Qualifier");
            CreateContext(node, ParameterDeclarationAST.STORAGE_SPECIFIER, "Storage Specifier");

            // . Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");

            return base.VisitParameterDeclaration(node, node);
        }

        public override int VisitDeclaration(DeclarationAST node, ASTComposite parent) {


            // 1.Create context clusters
            CreateContext(node, DeclarationAST.DECLARATORS, "Declarators");
            CreateContext(node, DeclarationAST.TYPE_SPECIFIER, "Type Specifier");
            CreateContext(node, DeclarationAST.TYPE_QUALIFIER, "Type Qualifier");
            CreateContext(node, DeclarationAST.STORAGE_SPECIFIER, "Storage Specifier");

            // . Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");

            // 3. Visit children and print AST nodes and edges
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitFunctionDefinition(FunctionDefinitionAST node, ASTComposite parent) {

            // 1.Create context clusters
            CreateContext(node, FunctionDefinitionAST.DECLARATION_SPECIFIERS, "Declaration Specifier");
            CreateContext(node, FunctionDefinitionAST.FUNCTION_BODY, "Body");
            CreateContext(node, FunctionDefinitionAST.DECLARATOR, "Declarator");
            CreateContext(node, FunctionDefinitionAST.PARAMETER_DECLARATIONS, "Parameter Declarations");

            // 1. Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitStatementExpression(Statement_Expression node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Statement_Expression.EXPRESSION, "EXPRESSION_BODY");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAssignment(Expression_Assignment node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Assignment.LEFT, "L_VALUE");
            CreateContext(node, Expression_Assignment.RIGHT, "R_VALUE");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAddition(Expression_Addition node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Addition.LEFT, "LEFT");
            CreateContext(node, Expression_Addition.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionSubtraction(Expression_Subtraction node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Subtraction.LEFT, "LEFT");
            CreateContext(node, Expression_Subtraction.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionMultiplication(Expression_Multiplication node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Multiplication.LEFT, "LEFT");
            CreateContext(node, Expression_Multiplication.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionDivision(Expression_Division node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Division.LEFT, "LEFT");
            CreateContext(node, Expression_Division.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionModulo(Expression_Modulo node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Modulo.LEFT, "LEFT");
            CreateContext(node, Expression_Modulo.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitShiftExpression_Left(ExpressionShiftLeft node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionShiftLeft.LEFT, "LEFT");
            CreateContext(node, ExpressionShiftLeft.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;

        }

        public override int VisitShiftExpression_Right(ExpressionShiftRight node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionShiftRight.LEFT, "LEFT");
            CreateContext(node, ExpressionShiftRight.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitUnaryExpressionIncrement(UnaryExpressionIncrement node, ASTComposite info) {
            // 1.Create context clusters
            CreateContext(node, UnaryExpressionIncrement.OPERAND, "Increment");
            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;

        }

        public override int VisitUnaryExpressionDecrement(UnaryExpressionDecrement node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, UnaryExpressionDecrement.OPERAND, "Decrement");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;

        }


        public override int VisitCompoundStatement(CompoundStatement node, ASTComposite parent) {

            // 1.Create context clusters
            CreateContext(node, CompoundStatement.DECLARATIONS, "Declarations");
            CreateContext(node, CompoundStatement.STATEMENTS, "Statements");

            // 1. Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }


        public override int VisitPointerType(PointerTypeAST node, ASTComposite parent) {

            // 1.Create context clusters
            CreateContext(node, PointerTypeAST.POINTER_TARGET, "Target");


            // 2. Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitFunctionType(FunctionTypeAST node, ASTComposite parent) {

            // 1.Create context clusters
            CreateContext(node, FunctionTypeAST.FUNCTION_NAME, "Function Name");
            CreateContext(node, FunctionTypeAST.FUNCTION_TYPE, "Declarator");
            CreateContext(node, FunctionTypeAST.FUNCTION_PARAMETERS, "Function Parameters");

            // 2. Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitPostfixExpression_ArraySubscript(Postfixexpression_ArraySubscript node, ASTComposite info)
        {
            CreateContext(node, Postfixexpression_ArraySubscript.ARRAY, "Array Name");
            CreateContext(node, Postfixexpression_ArraySubscript.INDEX, "Index");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int Visitpostfix_expression_FunctionCallNoArgs(Postfixexpression_FunctionCallNoArgs node, ASTComposite info)
        {
            CreateContext(node, Postfixexpression_FunctionCallNoArgs.FUNCTION, "Function Call with no arguments");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int Visitpostfix_expression_FunctionCallWithArgs(Postfixexpression_FunctionCallWithArgs node, ASTComposite info)
        {
            CreateContext(node, Postfixexpression_FunctionCallNoArgs.FUNCTION, "Function Call");
            CreateContext(node, Postfixexpression_FunctionCallWithArgs.ARGUMENTS, "Arguments");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int Visitpostfix_expression_MemberAccess(Postfixexpression_MemberAccess node, ASTComposite info)
        {
            CreateContext(node, Postfixexpression_MemberAccess.ACCESS, "Member Access");
            CreateContext(node, Postfixexpression_MemberAccess.MEMBER, "Member");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int Visitpostfix_expression_PointerMemberAccess(Postfixexpression_PointerMemberAccess node, ASTComposite info)
        {
            CreateContext(node, Postfixexpression_PointerMemberAccess.ACCESS, "Pointer member Access");
            CreateContext(node, Postfixexpression_PointerMemberAccess.MEMBER, "Member");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int Visitpostfix_expression_Increment(Postfixexpression_Increment node, ASTComposite info)
        {
            CreateContext(node, Postfixexpression_Increment.ACCESS, "Increment");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int Visitpostfix_expression_Decrement(Postfixexpression_Decrement node, ASTComposite info)
        {
            CreateContext(node, Postfixexpression_Decrement.ACCESS, "Decrement");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionLogicalOR(ExpressionLogicalOr node, ASTComposite info)
        {
            CreateContext(node, ExpressionLogicalOr.LEFT, "LEFT"); 
            CreateContext(node, ExpressionLogicalOr.RIGHT, "RIGHT");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionLogicalAND(ExpressionLogicalAnd node, ASTComposite info)
        {
            CreateContext(node, ExpressionLogicalAnd.LEFT, "LEFT");
            CreateContext(node, ExpressionLogicalAnd.RIGHT, "RIGHT");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionBitwiseAND(Expression_BitwiseAND node, ASTComposite info)
        {
            CreateContext(node, Expression_BitwiseAND.LEFT, "LEFT");
            CreateContext(node, Expression_BitwiseAND.RIGHT, "RIGHT");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionBitwiseOR(Expression_BitwiseOR node, ASTComposite info)
        {
            CreateContext(node, Expression_BitwiseOR.LEFT, "LEFT");
            CreateContext(node, Expression_BitwiseOR.RIGHT, "RIGHT");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionBitwiseXOR(Expression_BitwiseXOR node, ASTComposite info)
        {
            CreateContext(node, Expression_BitwiseXOR.LEFT, "LEFT");
            CreateContext(node, Expression_BitwiseXOR.RIGHT, "RIGHT");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitUnaryExpressionOperatorAmbersand(UnaryExpressionUnaryOperatorAmbersand node, ASTComposite info)
        {
            CreateContext(node, UnaryExpressionUnaryOperatorAmbersand.EXPRESSION, "Expression");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitUnaryExpressionOperatorAsterisk(UnaryExpressionUnaryOperatorAsterisk node, ASTComposite info)
        {
            CreateContext(node, UnaryExpressionUnaryOperatorAsterisk.EXPRESSION, "Expression");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitUnaryExpressionOperatorPLUS(UnaryExpressionUnaryOperatorPLUS node, ASTComposite info)
        {
            CreateContext(node, UnaryExpressionUnaryOperatorPLUS.EXPRESSION, "Expression");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitUnaryExpressionOperatorMINUS(UnaryExpressionUnaryOperatorMINUS node, ASTComposite info)
        {
            CreateContext(node, UnaryExpressionUnaryOperatorMINUS.EXPRESSION, "Expression");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitUnaryExpressionOperatorTilde(UnaryExpressionUnaryOperatorTilde node, ASTComposite info)
        {
            CreateContext(node, UnaryExpressionUnaryOperatorTilde.EXPRESSION, "Expression");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitUnaryExpressionOperatorNOT(UnaryExpressionUnaryOperatorNOT node, ASTComposite info)
        {
            CreateContext(node, UnaryExpressionUnaryOperatorNOT.EXPRESSION, "Expression");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionCast(Expression_Cast node, ASTComposite info)
        {
            CreateContext(node, Expression_Cast.TYPE, "Type");
            CreateContext(node, Expression_Cast.EXPRESSION, "Expression");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitRelationalLess(ExpressionRelationalLess node, ASTComposite info)
        {
            CreateContext(node, ExpressionRelationalLess.LEFT, "Left");
            CreateContext(node, ExpressionRelationalLess.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitRelationalGreater(ExpressionRelationalGreater node, ASTComposite info)
        {
            CreateContext(node, ExpressionRelationalGreater.LEFT, "Left");
            CreateContext(node, ExpressionRelationalGreater.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitRelationalLessEqual(ExpressionRelationalLessOrEqual node, ASTComposite info)
        {
            CreateContext(node, ExpressionRelationalLessOrEqual.LEFT, "Left");
            CreateContext(node, ExpressionRelationalLessOrEqual.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitRelationalGreaterEqual(ExpressionRelationalGreaterOrEqual node, ASTComposite info)
        {
            CreateContext(node, ExpressionRelationalGreaterOrEqual.LEFT, "Left");
            CreateContext(node, ExpressionRelationalGreaterOrEqual.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionEqualityEqual(Expression_EqualityEqual node, ASTComposite info)
        {
            CreateContext(node, Expression_EqualityEqual.LEFT, "Left");
            CreateContext(node, Expression_EqualityEqual.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionEqualityNotEqual(Expression_EqualityNotEqual node, ASTComposite info)
        {
            CreateContext(node, Expression_EqualityNotEqual.LEFT, "Left");
            CreateContext(node, Expression_EqualityNotEqual.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionAssignmentAnd(Expression_AssignmentAnd node, ASTComposite info)
        {
            CreateContext(node, Expression_Assignment.LEFT, "Left");
            CreateContext(node, Expression_Assignment.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionAssignmentXor(Expression_AssignmentXor node, ASTComposite info)
        {
            CreateContext(node, Expression_Assignment.LEFT, "Left");
            CreateContext(node, Expression_Assignment.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionAssignmentOr(Expression_AssignmentOr node, ASTComposite info)
        {
            CreateContext(node, Expression_Assignment.LEFT, "Left");
            CreateContext(node, Expression_Assignment.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionAssignmentDivision(ExpressionAssignmentDivision node, ASTComposite info)
        {
            CreateContext(node, Expression_Assignment.LEFT, "Left");
            CreateContext(node, Expression_Assignment.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionAssignmentMultiplication(ExpressionAssignmentMultiplication node, ASTComposite info)
        {
            CreateContext(node, Expression_Assignment.LEFT, "Left");
            CreateContext(node, Expression_Assignment.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionAssignmentModulo(ExpressionAssignmentModulo node, ASTComposite info)
        {
            CreateContext(node, Expression_Assignment.LEFT, "Left");
            CreateContext(node, Expression_Assignment.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionAssignmentAddition(ExpressionAssignmentAddition node, ASTComposite info)
        {
            CreateContext(node, Expression_Assignment.LEFT, "Left");
            CreateContext(node, Expression_Assignment.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionAssignmentSubtraction(ExpressionAssignmentSubtraction node, ASTComposite info)
        {
            CreateContext(node, Expression_Assignment.LEFT, "Left");
            CreateContext(node, Expression_Assignment.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionAssignmentLeft(Expression_AssignmentLeft node, ASTComposite info)
        {
            CreateContext(node, Expression_Assignment.LEFT, "Left");
            CreateContext(node, Expression_Assignment.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitExpressionAssignmentRight(Expression_AssignmentRight node, ASTComposite info)
        {
            CreateContext(node, Expression_Assignment.LEFT, "Left");
            CreateContext(node, Expression_Assignment.RIGHT, "Right");

            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitConditionalExpression(ConditionalExpression node, ASTComposite info)
        {
            CreateContext(node, ConditionalExpression.CONDITION, "Condition");
            CreateContext(node, ConditionalExpression.TRUE_EXPRESSION, "True");
            CreateContext(node, ConditionalExpression.FALSE_EXPRESSION, "False");


            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitDeclarationSpecifiers(Declaration_Specifiers node, ASTComposite info) {
            CreateContext(node, ConditionalExpression.CONDITION, "Specifiers");
            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);

            return 0;
        }

        public override int VisitIdentifier(IDENTIFIER node, ASTComposite parent) {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitIdentifier(node, parent);
        }

        public override int VisitInteger(INTEGER node, ASTComposite parent) {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitInteger(node, parent);
        }

        public override int VisitIntegerType(IntegerTypeAST node, ASTComposite parent) {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitIntegerType(node, parent);
        }

        public override int VisitSignedType(SignedTypeAST node, ASTComposite info)
        {
            m_writer.WriteLine($"    \"{info.MName}\" -> \"{node.MName}\";");
            return base.VisitSignedType(node, info);
        }

        public override int VisitUnsignedType(UnsignedTypeAST node, ASTComposite parent) {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitUnsignedType(node, parent);
        }

        public override int VisitShortType(ShortTypeAST node, ASTComposite parent)
        {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitShortType(node, parent);
        }

        public override int VisitLongType(LongTypeAST node, ASTComposite parent) {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitLongType(node, parent);
        }

        public override int VisitFloatType(FloatTypeAST node, ASTComposite parent)
        {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitFloatType(node, parent);
        }

        public override int VisitDoubleType(DoubleTypeAST node, ASTComposite parent)
        {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitDoubleType(node, parent);
        }

        public override int VisitStructType(StructTypeAST node, ASTComposite parent)
        {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitStructType(node, parent);
        }

        public override int VisitUnionType(UnionTypeAST node, ASTComposite parent)
        {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitUnionType(node, parent);
        }

        public override int VisitVoidType(VoidTypeAST node, ASTComposite parent)
        {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitVoidType(node, parent);
        }

        public override int VisitCharType(CharTypeAST node, ASTComposite parent) {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitCharType(node, parent);
        }
    }
}
