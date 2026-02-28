using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Transactions;
using Microsoft.Build.Framework;

namespace CParser {
    public class CType {

        public enum TypeKind {
            Void,
            Char,
            Int,
            Float,
            Double,
            Struct,
            Union,
            Enum,
            Pointer,
            Array,
            Function,
            Typedef,
            Qualifier
        }

        public enum TypeGranularity {
            Basic,
            Composite
        }

        public TypeKind Kind => m_typekind;
        public TypeGranularity Granularity => m_granularity;

        public string? Declarator { get; set; }

        protected string m_typename;
        protected TypeKind m_typekind;
        protected TypeGranularity m_granularity;
        protected CType m_parent;
        protected List<CType> m_typeparams; // e.g., function parameter types, struct member types, etc.

        public CType M_parent => m_parent;

        // For Debugging purposes
        protected int m_typeserial;
        private static int ms_typeserialCounter;

        public CType(TypeKind mTypekind, string mTypename, TypeGranularity mTypeGranularity) {
            m_typekind = mTypekind;
            m_typename = mTypename;
            m_typeparams = new List<CType>();
            m_granularity = mTypeGranularity;
            // For debugging
            m_typeserial = ms_typeserialCounter++;
        }

        protected CType(TypeKind mTypekind, string mTypename, TypeGranularity mTypeGranularity,
            List<CType> children, CType mParent, int serialNumber)
        {
            m_typekind = mTypekind;
            m_typename = mTypename;
            m_typeparams = new List<CType>(children).ToList();
            m_granularity = mTypeGranularity;
            m_parent = mParent;
            m_typeserial = serialNumber;
        }

        public void AddTypeParameter(CType param) {
            param.m_parent = this;
            m_typeparams.Add(param);
        }

        public void RemoveParameters()
        {
            m_typeparams.Clear();
        }

        public int childCount()
        {
            return m_typeparams.Count;
        }

        public virtual CType Clone()
        {
            return new CType(m_typekind, m_typename, m_granularity, m_typeparams,
                m_parent, m_typeserial);
        }

        public virtual void TypeDebugLog(StreamWriter m_logFile=null) {
            if (m_parent == null) {
                m_logFile = new StreamWriter("type_log.dot");
                m_logFile.WriteLine("digraph G{ ");
                m_logFile.WriteLine($"\"{ToString()}_{m_typeserial}\"");
            } else {
                m_logFile.WriteLine(
                    $"\"{m_parent.ToString()}_{m_parent.m_typeserial}\"->\"{ToString()}_{m_typeserial}\"");
            }

            foreach (CType typeparam in m_typeparams) { 
                typeparam.TypeDebugLog(m_logFile);
            }

            if (m_parent == null) {
                m_logFile.WriteLine("};");
                m_logFile.Close();
                TryGenerateTypeGraphImage("type_log.dot", "type_log.gif");
            }
        }

        private static void TryGenerateTypeGraphImage(string dotFilePath, string outputImagePath) {
            try {
                var processStartInfo = new ProcessStartInfo {
                    FileName = "dot",
                    Arguments = $"-Tgif \"{dotFilePath}\" -o \"{outputImagePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                using (var process = new Process { StartInfo = processStartInfo }) { 
                    process.Start();
                    process.WaitForExit();
                }
            } catch {
                // Intentionally ignore failures in debug helper
                Console.WriteLine("Here");
            }
        }

        public override bool Equals(object? obj) {
            if (obj != null && obj is CType other) {
                return GetType() == other.GetType();
            }

            return base.Equals(obj);
        }

        public bool Equals(CType t) {
            if (t == null) {
                return false;
            }

            if (m_typekind != t.m_typekind) {
                return false;
            }

            if (m_typeparams.Count != t.m_typeparams.Count) {
                return false;
            }

            for (int j = 0; j < m_typeparams.Count; j++) {
                if (!m_typeparams[j].Equals(t.m_typeparams[j])) {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(CType? a, CType? b) {
            if (ReferenceEquals(a, b)) {
                return true;
            }

            if (a is null || b is null) {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(CType? a, CType? b) {
            return !(a == b);
        }

        public override string ToString()
        {
            return m_typename;
        }
    }

    public class PointerType : CType {
        public PointerType()
            : base(TypeKind.Pointer, "pointer", TypeGranularity.Basic) {
        }

        private PointerType(List<CType> children, CType mParent, int serialNumber) : 
            base(TypeKind.Pointer, "pointer", TypeGranularity.Basic,
            children, mParent, serialNumber) {
        }

        public override bool Equals(object? obj) {
            return base.Equals(obj);
        }

        public override CType Clone()
        {
            return new PointerType(m_typeparams, m_parent, m_typeserial);
        }

        public bool Equals(CType t) {
            if (t is PointerType pt) {
                if (pt.m_typeparams.Count != m_typeparams.Count) {
                    return false;
                }

                for (int i = 0; i < m_typeparams.Count; i++) {
                    if (!m_typeparams[i].Equals(pt.m_typeparams[i])) {
                        return false;
                    }
                }
            }

            return false;
        }
    }

    public class IntegerType : CType {
        public enum IntegerKind {
            Signed,
            Unsigned
        }

        private IntegerKind m_integerkind;
        private int m_size; // in bytes

        public IntegerType(IntegerKind ikind, int size)
            : base(TypeKind.Int, "int", TypeGranularity.Basic) {
            m_integerkind = ikind;
            m_size = size;
        }

        private IntegerType(IntegerKind ikind, int size,
            List<CType> children, CType mParent, int serialNumber) : 
            base(TypeKind.Int, "int", TypeGranularity.Basic,
                children, mParent, serialNumber) {
            m_integerkind = ikind;
            m_size = size;
        }

        public override CType Clone()
        {
           return new IntegerType(m_integerkind, m_size, m_typeparams, m_parent, m_typeserial);
        }

        public override bool Equals(object? obj) {
            return base.Equals(obj);
        }

        public bool Equals(CType t) {
            if (t is IntegerType it) {
                return m_integerkind == it.m_integerkind &&
                       m_size == it.m_size;
            }

            return false;
        }

        public override string ToString() {
            string sign = m_integerkind == IntegerKind.Unsigned ? "unsigned " : "signed ";
            return $"{sign}{m_typename}{m_size * 8}";
        }
    }

    public class FloatingPointType : CType {
        private int m_size; // in bytes

        public FloatingPointType(int size, string name, TypeKind kind)
            : base(kind, name, TypeGranularity.Basic) {
            m_size = size;
        }

        private  FloatingPointType(int size, string name, TypeKind kind,
            List<CType> children, CType mParent, int serialNumber) : 
            base(kind, name, TypeGranularity.Basic, 
                children, mParent, serialNumber) {
            m_size = size;
        }

        public override CType Clone()
        {
           return new FloatingPointType(m_size, m_typename, m_typekind, m_typeparams, m_parent, m_typeserial);
        }

        public override bool Equals(object? obj) {
            return base.Equals(obj);
        }

        public bool Equals(CType t) {
            if (t is FloatingPointType ft) {
                return m_size == ft.m_size;
            }

            return false;
        }

        public override string ToString() {
            if (!string.IsNullOrEmpty(m_typename)) {
                return $"{m_typename}{m_size * 8}";
            }

            return m_size switch {
                4 => "float32",
                8 => "double64",
                _ => $"double{m_size * 8}"
            };
        }
    }

    public class StructType : CType
    {
        public StructType(string name)
            : base(TypeKind.Struct, name, TypeGranularity.Composite) {
        }

        private StructType(string name, List<CType> children, CType mParent,
            int serialNumber) : 
            base(TypeKind.Struct, name, TypeGranularity.Composite,
                children, mParent, serialNumber) {
        }

        public override CType Clone()
        {
            return new StructType(m_typename, m_typeparams, m_parent, m_typeserial);
        }

        public bool Equals(CType t) {
            if (t is StructType st) {
                if (st.m_typeparams.Count != m_typeparams.Count) {
                    return false;
                }

                for (int i = 0; i < m_typeparams.Count; i++) {
                    if (!m_typeparams[i].Equals(st.m_typeparams[i])) {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override string ToString() {
            if (!string.IsNullOrEmpty(m_typename)) {
                return $"struct {m_typename}";
            }

            return "struct";
        }
    }

    public class UnionType : CType
    {
        public UnionType(string name)
            : base(TypeKind.Union, name, TypeGranularity.Composite) {
        }

        private UnionType(string name, List<CType> children, CType mParent,
                int serialNumber) : 
                base(TypeKind.Union, name, TypeGranularity.Composite,
                    children, mParent, serialNumber) {
        }

        public override CType Clone()
        {
            return new UnionType(m_typename, m_typeparams, m_parent, m_typeserial);
        }

        public bool Equals(CType t) {
            if (t is UnionType ut) {
                if (ut.m_typeparams.Count != m_typeparams.Count) {
                    return false;
                }

                for (int i = 0; i < m_typeparams.Count; i++) {
                    if (!m_typeparams[i].Equals(ut.m_typeparams[i])) {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override string ToString() {
            if (!string.IsNullOrEmpty(m_typename)) {
                return $"union {m_typename}";
            }

            return $"union";
        }
    }

    public class VoidType : CType {

        public VoidType()
            : base(TypeKind.Void, "void", TypeGranularity.Basic) {
        }

        private VoidType(List<CType> children, CType mParent, int serialNumber) : 
            base(TypeKind.Void, "void", TypeGranularity.Basic, 
                children, mParent, serialNumber) {
        }

        public override CType Clone()
        {
            return new VoidType(m_typeparams, m_parent, m_typeserial);
        }

        public bool Equals(CType t) {
            return t is VoidType;
        }
    }

    public class EnumType : CType {

        public EnumType(string name)
            : base(TypeKind.Enum, name, TypeGranularity.Basic) {
        }

        public bool Equals(CType t) {
            if (t is EnumType et) {
                if (et.m_typeparams.Count != m_typeparams.Count) {
                    return false;
                }

                for (int i = 0; i < m_typeparams.Count; i++) {
                    if (!m_typeparams[i].Equals(et.m_typeparams[i])) {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override string ToString() {
            if (!string.IsNullOrEmpty(m_typename)) {
                return $"enum {m_typename}";
            }

            return "enum";
        }
    }

    public class FunctionType : CType {
        public FunctionType()
            : base(TypeKind.Function, "function", TypeGranularity.Basic) {
        }

        private FunctionType(List<CType> children, CType mParent, int serialNumber) : 
            base(TypeKind.Function, "function", TypeGranularity.Basic,
                children, mParent, serialNumber) {
        }

        public override CType Clone()
        {
            return new FunctionType(m_typeparams, m_parent, m_typeserial);
        }

        public bool Equals(CType t) {
            if (t is FunctionType ft) {
                if (m_typeparams.Count != ft.m_typeparams.Count) {
                    return false;
                }

                for (int i = 0; i < m_typeparams.Count; i++) {
                    if (!m_typeparams[i].Equals(ft.m_typeparams[i])) {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public bool VoidParameterBefore()
        {
            if (childCount() <= 2)
            {
                return false;
            }

            int current = childCount() - 1;

            CType type = m_typeparams[current - 1];

            return type.Kind == TypeKind.Void && string.IsNullOrEmpty(type.Declarator);
        }

        public override string ToString() {
            if (m_typeparams.Count == 0) {
                return "function()";
            }

            CType returnType = m_typeparams[0];
            List<string> paramStrings = new List<string>();

            for (int i = 1; i < m_typeparams.Count; i++) {
                paramStrings.Add(m_typeparams[i].ToString());
            }

            string parameters = string.Join(", ", paramStrings);
            return $"{returnType} ({parameters})";
        }
    }

    public class ArrayType : CType {
        private CType m_elementType;

        // Dimension sizes for each dimension. Low-level to high-level meaning
        // first element is size of first dimension, second element is size of second dimension, etc.
        private List<int> m_dimensionSize;

        public CType MElementType {
            get => m_elementType;
            set => m_elementType = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ArrayType(CType elementType)
            : base(TypeKind.Array, null, TypeGranularity.Basic) {
            m_elementType = elementType;
            m_dimensionSize = new List<int>();
        }

        public void AddHigherLevelDimensionSize(int size) {
            // place at the end
            m_dimensionSize.Add(size);
        }

        public void AddLowerLevelDimensionSize(int size) {
            // place at the beginning
            m_dimensionSize.Insert(0, size);
        }

        public bool Equals(CType t) {
            if (t is ArrayType at) {
                if (!m_elementType.Equals(at.m_elementType)) {
                    return false;
                }

                if (m_dimensionSize.Count != at.m_dimensionSize.Count) {
                    return false;
                }

                for (int i = 0; i < m_dimensionSize.Count; i++) {
                    if (m_dimensionSize[i] != at.m_dimensionSize[i]) {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override string ToString() {
            string result = m_elementType.ToString();

            foreach (int size in m_dimensionSize) {
                result += size > 0 ? $"[{size}]" : "[]";
            }

            return result;
        }
    }
}
