using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CParser.Symbol;

namespace CParser
{
    public class ParentInfo(ASTComposite context)
    {
    }

    public class DeclarationContext : ParentInfo
    {
        public CType? MTypeSpecifier { get; set; }
        public CType? MTypeRoot { get; set; }
        public CType? MParent { get; set; }
        public CType? MFunctionType { get; set; }
        public CType? MArrayType { get; set; }
        public int DimensionCounter { get; set; }
        public string MDeclarator { get; set; }

        public DeclarationContext(ASTComposite context)
            : base(context)
        {
        }

        public void Reset()
        {
            MTypeRoot = null;
            MParent = null;
            MFunctionType = null;
            MArrayType = null;
            DimensionCounter = 1;
            MDeclarator = string.Empty;
        }
    }

    public class ScopeBuilderVisitor : BaseASTVisitor<int, ParentInfo>
    {
        public ScopeBuilderVisitor() { }

        public override int VisitTranslationUnit(TranslationUnitAST node, ParentInfo info)
        {
            CScopeSystem.GetInstance().EnterScope(ScopeType.File);
            base.VisitTranslationUnit(node, info);
            CScopeSystem.GetInstance().ExitScope();
            return 0;
        }

        public override int VisitDeclaration(DeclarationAST node, ParentInfo info)
        {
            DeclarationContext declContext = new DeclarationContext(node);

            if (info != null)
            {
                declContext = info as DeclarationContext;
            }

            // 1. Visit Declaration Specifiers
            VisitContext(node, DeclarationAST.TYPE_SPECIFIER, declContext);

            List<uint> m_types =  node.MChildren[DeclarationAST.DECLARATORS]
                .OfType<ASTElement>()
                .Select(child => child.MType)
                .ToList();
            int dimensionCount = m_types.Count(t => t == (uint)TranslationUnitAST.NodeTypes.INTEGER);

            declContext.Reset();

            // 2. Visit Declarators
            foreach (ASTElement astElement in node.MChildren[DeclarationAST.DECLARATORS])
            {
                if (astElement is not FunctionTypeAST && astElement is not INTEGER)
                {
                    declContext.Reset();
                }

                Visit(astElement, declContext);

                if (declContext.MTypeSpecifier == null)
                {
                    continue;
                }

                if (declContext.MParent == null)
                {
                    if (declContext.MTypeSpecifier is VoidType)
                    {
                        throw new InvalidOperationException("Void type cannot be assigned to a variable");
                    }

                    declContext.MTypeRoot = declContext.MTypeSpecifier;
                    declContext.MParent = declContext.MTypeSpecifier;
                }
                else
                {
                    if (dimensionCount == 0 || declContext.DimensionCounter == dimensionCount)
                        declContext.MParent.AddTypeParameter(declContext.MTypeSpecifier);
                }

                declContext.MTypeRoot.TypeDebugLog();
            }
            declContext.MTypeSpecifier = null;

            return 0;
        }

        private void SpecifierOrParamater(CType type, DeclarationContext context)
        {
            type.Declarator = context.MDeclarator;

            if (context.MFunctionType == null)
            {
                context.MTypeSpecifier = type;
                return;
            }

            if (context.MParent == null)
            {
                if (!string.IsNullOrEmpty(type.Declarator) && type is VoidType)
                {
                   throw new InvalidOperationException("Cannot pass a parameter of void type");
                }

                if (type is VoidType && context.MFunctionType.childCount() > 1)
                {
                    throw new InvalidOperationException("This function cannot be void");
                }
                
                context.MFunctionType.AddTypeParameter(type.Clone());
            }
            else
            {
                context.MParent.AddTypeParameter(type.Clone());
                context.MFunctionType.AddTypeParameter(context.MTypeRoot.Clone());
            }

            FunctionType funcType = context.MFunctionType as FunctionType;
            if (funcType.VoidParameterBefore())
            {
                throw new InvalidOperationException("Cannot assign a parameter to this function");
            }

            context.MParent = null;
            context.MDeclarator = String.Empty;
        }
        
        public override int VisitDeclarationSpecifiers(Declaration_Specifiers node, ParentInfo info)
        {
            var mtypes = node.MChildren[Declaration_Specifiers.SPECIFIERS]
                .OfType<ASTElement>()
                .Select(child => child.MType)
                .ToList();

            CType.TypeKind tp;
            int size;

            switch (true)
            {
                case true when (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.LONG_TYPE) ||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.INTEGER_TYPE) ||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.SHORT_TYPE)) &&
                               !mtypes.Contains((uint)TranslationUnitAST.NodeTypes.DOUBLE_TYPE):
                    tp = CType.TypeKind.Int;
                    IntegerType.IntegerKind sign;
                    if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.UNSIGNED_TYPE))
                    {
                        sign = IntegerType.IntegerKind.Unsigned;
                    }
                    else
                    {
                        sign = IntegerType.IntegerKind.Signed;
                    }
                    size = mtypes.Count(t => t == (uint)TranslationUnitAST.NodeTypes.LONG_TYPE) >= 2 ? 8 : 4;
                    size = mtypes.Contains((uint)TranslationUnitAST.NodeTypes.SHORT_TYPE) ? 2 : size;

                    IntegerType intType = new IntegerType(sign, size);
                    DeclarationContext declContext = info as DeclarationContext;
                    SpecifierOrParamater(intType, declContext);
                    break;
                    
                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.FLOAT_TYPE) ||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.DOUBLE_TYPE) ||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.LONG_TYPE):
                    tp = mtypes.Contains((uint)TranslationUnitAST.NodeTypes.FLOAT_TYPE) ? CType.TypeKind.Float : CType.TypeKind.Double;
                    
                    size = mtypes.Contains((uint)TranslationUnitAST.NodeTypes.FLOAT_TYPE) ? 4 : 8;
                    size = mtypes.Contains((uint)TranslationUnitAST.NodeTypes.LONG_TYPE) && 
                        tp != CType.TypeKind.Float ? 10 : size;
                    
                    string name = tp == CType.TypeKind.Float ? "float" : "double";

                    FloatingPointType floatType = new FloatingPointType(size, name, tp);
                    DeclarationContext declContext2 = info as DeclarationContext;
                    SpecifierOrParamater(floatType, declContext2);
                    break;

                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.STRUCT_TYPE):
                    IDENTIFIER struct_id = node.MChildren[Declaration_Specifiers.SPECIFIERS][1] as IDENTIFIER;
                    string struct_name = struct_id.MLexeme;

                    StructType structType = new StructType(struct_name);
                    DeclarationContext declContext3 = info as DeclarationContext;
                    SpecifierOrParamater(structType, declContext3);
                    break;

                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.UNION_TYPE):

                    IDENTIFIER union_id = node.MChildren[Declaration_Specifiers.SPECIFIERS][1] as IDENTIFIER;
                    string union_name = union_id.MLexeme;

                    UnionType unionType = new UnionType(union_name);
                    DeclarationContext declContext4 = info as DeclarationContext;
                    SpecifierOrParamater(unionType, declContext4);
                    break;

                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.VOID_TYPE):
                    VoidType voidType = new VoidType();
                    DeclarationContext declContext5 = info as DeclarationContext;
                    SpecifierOrParamater(voidType, declContext5);
                    break;

                default:
                    tp = CType.TypeKind.Int;
                    size = 4;
                    break;
            }

            return 0;
        }

        public override int VisitInteger(INTEGER node, ParentInfo info)
        {
            DeclarationContext declContext = info as DeclarationContext;

            ArrayType arrayType = new ArrayType(declContext.MTypeSpecifier);

            if (declContext.MArrayType == null)
            {
                if (declContext.MParent is PointerType)
                {
                    declContext.MParent.RemoveParameters();
                }
                else
                {
                    declContext.Reset();
                }

                declContext.MArrayType = arrayType;
            }
            else
            {
                declContext.DimensionCounter++;
                arrayType = declContext.MArrayType as ArrayType;

                if (declContext.MParent is not PointerType)
                {
                    declContext.MParent = null;
                }
            }

            arrayType.AddHigherLevelDimensionSize(int.Parse(node.MLexeme));

            declContext.MTypeSpecifier = arrayType;

            return 0;
        }

        public override int VisitIdentifier(IDENTIFIER node, ParentInfo info)
        {
            // Check if this identifier is a function parameter
            DeclarationContext declarationContext = info as DeclarationContext;
            declarationContext.MDeclarator = node.MLexeme;
            return 0;
        }

        public override int VisitFunctionType(FunctionTypeAST node, ParentInfo info)
        {
            DeclarationContext declContext = info as DeclarationContext;

            FunctionType funcType = new FunctionType();

            if (declContext.MTypeRoot == null)
            {
                funcType.AddTypeParameter(declContext.MTypeSpecifier.Clone());
            }
            else
            {
                funcType.AddTypeParameter(declContext.MTypeRoot.Clone());
                declContext.Reset();
            }

            declContext.MFunctionType = funcType;
            base.VisitFunctionType(node, info);

            declContext.MTypeSpecifier = funcType;

            return 0;
        }

        public override int VisitPointerType(PointerTypeAST node, ParentInfo info)
        {

            DeclarationContext declContext = info as DeclarationContext;

            // Preorder actions

            // Visit children
            VisitContext(node, PointerTypeAST.POINTER_TARGET, declContext);

            // Postorder actions
            PointerType pointerType = new PointerType();
            if (declContext.MParent != null)
            {
                declContext.MParent.AddTypeParameter(pointerType);
            }
            else
            {
                declContext.MTypeRoot = pointerType;
            }
            declContext.MParent = pointerType;

            return 0;
        }
    }
}