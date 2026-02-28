using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CParser.Expression_Assignment;

namespace CParser {
    public class BaseASTVisitor<Result, INFO>{
        public BaseASTVisitor() {

        }

        public Result VisitContext(ASTComposite node, uint context, INFO info) {
            foreach (ASTElement astElement in node.MChildren[context]) {
                Visit(astElement, info);
            }

            return default(Result);
        }

        public Result VisitChildren(ASTComposite node, INFO info) {
            for (int context = 0; context < node.MContexts; context++) {
                foreach (ASTElement astElement in node.MChildren[context]) {
                    Visit(astElement, info);
                }
            }

            return default(Result);
        }

        public Result Visit(ASTElement astElement, INFO info) {
            return astElement.Accept<Result, INFO>(this, info);
        }


        public virtual Result VisitTranslationUnit(TranslationUnitAST node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitDeclaration(DeclarationAST node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitPointerType(PointerTypeAST node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitVoidType(VoidTypeAST node, INFO info)
        {
            return default(Result);
        }

        public virtual Result VisitIntegerType(IntegerTypeAST node, INFO info) {
            return default(Result);
        }

        public virtual Result VisitSignedType(SignedTypeAST node, INFO info)
        {
            return default(Result);
        }

        public virtual Result VisitUnsignedType(UnsignedTypeAST node, INFO info) {
            return default(Result);
        }

        public virtual Result VisitShortType(ShortTypeAST node, INFO info)
        {
            return default(Result);
        }

        public virtual Result VisitLongType(LongTypeAST node, INFO info) {
            return default(Result);
        }

        public virtual Result VisitFloatType(FloatTypeAST node, INFO info)
        {
            return default(Result);
        }

        public virtual Result VisitDoubleType(DoubleTypeAST node, INFO info)
        {
            return default(Result);
        }

        public virtual Result VisitCharType(CharTypeAST node, INFO info) {
            return default(Result);
        }

        public virtual Result VisitStructType(StructTypeAST node, INFO info)
        {
            return default(Result);
        }

        public virtual Result VisitUnionType(UnionTypeAST node, INFO info)
        {
            return default(Result);
        }

        public virtual Result VisitFunctionType(FunctionTypeAST node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitParameterDeclaration(ParameterDeclarationAST node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitFunctionDefinition(FunctionDefinitionAST node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitCompoundStatement(CompoundStatement node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionStatement(ExpressionStatement node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionIdentifier(Expression_Identifier node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAssignment(Expression_Assignment node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAddition(Expression_Addition node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionMultiplication(Expression_Multiplication node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionDivision(Expression_Division node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionModulo(Expression_Modulo node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionSubtraction(Expression_Subtraction node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionEqualityEqual(Expression_EqualityEqual node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionEqualityNotEqual(Expression_EqualityNotEqual node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryExpressionOperatorAmbersand(UnaryExpressionUnaryOperatorAmbersand node,
                                                                    INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryExpressionOperatorAsterisk(UnaryExpressionUnaryOperatorAsterisk node,
                                                                   INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryExpressionOperatorPLUS(UnaryExpressionUnaryOperatorPLUS node,
                                                               INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryExpressionOperatorMINUS(UnaryExpressionUnaryOperatorMINUS node,
                                                                INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryExpressionOperatorTilde(UnaryExpressionUnaryOperatorTilde node,
                                                                INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryExpressionOperatorNOT(UnaryExpressionUnaryOperatorNOT node,
                                                              INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryExpressionIncrement(UnaryExpressionIncrement node,
                                                            INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryExpressionDecrement(UnaryExpressionDecrement node,
                                                            INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryExpressionSizeOfExpression(UnaryExpressionSizeOfExpression node,
                                                                   INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryExpressionSizeOfTypename(UnaryExpressionSizeOfTypeName node,
                                                                 INFO info) {
            return VisitChildren(node, info);
        }


        public virtual Result VisitExpressionBitwiseAND(Expression_BitwiseAND node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionBitwiseOR(Expression_BitwiseOR node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionBitwiseXOR(Expression_BitwiseXOR node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionNumber(Expression_Number node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionStringLiteral(Expression_StringLiteral node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitIdentifier(IDENTIFIER node, INFO info) {
            return default(Result);
        }

        public virtual Result VisitInteger(INTEGER node, INFO info) {
            return default(Result);
        }

        public virtual Result VisitShiftExpression_Left(ExpressionShiftLeft node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitShiftExpression_Right(ExpressionShiftRight node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitRelationalLess(ExpressionRelationalLess node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitRelationalGreater(ExpressionRelationalGreater node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitRelationalLessEqual(ExpressionRelationalLessOrEqual node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitRelationalGreaterEqual(ExpressionRelationalGreaterOrEqual node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryIncreament(UnaryExpressionIncrement node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryDecreament(UnaryExpressionDecrement node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnarySIZEOFExpression(UnaryExpressionSizeOfExpression node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnarySIZEOFTypeName(UnaryExpressionSizeOfTypeName node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryCastExpression(UnaryExpressionUnaryOperatorAmbersand node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryCastExpression(UnaryExpressionUnaryOperatorAsterisk node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryCastExpression(UnaryExpressionUnaryOperatorPLUS node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryCastExpression(UnaryExpressionUnaryOperatorMINUS node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryCastExpression(UnaryExpressionUnaryOperatorTilde node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitUnaryCastExpression(UnaryExpressionUnaryOperatorNOT node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitPostfixExpression_ArraySubscript(Postfixexpression_ArraySubscript node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result Visitpostfix_expression_FunctionCallNoArgs(Postfixexpression_FunctionCallNoArgs node,
                                                                         INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result Visitpostfix_expression_FunctionCallWithArgs(Postfixexpression_FunctionCallWithArgs node,
                                                                           INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result Visitpostfix_expression_MemberAccess(Postfixexpression_MemberAccess node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result Visitpostfix_expression_PointerMemberAccess(Postfixexpression_PointerMemberAccess node,
                                                                          INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result Visitpostfix_expression_Increment(Postfixexpression_Increment node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result Visitpostfix_expression_Decrement(Postfixexpression_Decrement node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionCommaExpression(Expression_CommaExpression node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAssignmentLeft(Expression_AssignmentLeft node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAssignmentRight(Expression_AssignmentRight node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAssignmentAnd(Expression_AssignmentAnd node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAssignmentXor(Expression_AssignmentXor node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAssignmentOr(Expression_AssignmentOr node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAssignmentDivision(ExpressionAssignmentDivision node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAssignmentMultiplication(ExpressionAssignmentMultiplication node,
                                                                      INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAssignmentModulo(ExpressionAssignmentModulo node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAssignmentAddition(ExpressionAssignmentAddition node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAssignmentSubtraction(ExpressionAssignmentSubtraction node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitStatementExpression(Statement_Expression node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionLogicalOR(ExpressionLogicalOr node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionLogicalAND(ExpressionLogicalAnd node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitConditionalExpression(ConditionalExpression node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionCast(Expression_Cast node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitDeclarationSpecifiers(Declaration_Specifiers node, INFO info) {
            return VisitChildren(node, info);
        }
    }
}
