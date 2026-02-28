using System;
using System.Collections.Generic;
using System.Data;
using System.Formats.Asn1;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using static CParser.CScope;

namespace CParser {
    public class CScopeSystem {

        public void EnterScope(ScopeType stype, string name=null) {

            CScope newScope = stype switch {
                ScopeType.File => new CFileScope(),
                ScopeType.Function => new CFunctionScope(m_currentScope, name),
                ScopeType.Block => new CBlockScope(m_currentScope),
                ScopeType.FunctionPrototype => new CFunctionPrototypeScope(m_currentScope,name),
                ScopeType.StructUnionEnum => new CStructUnionEnumScope(m_currentScope,name),
                _ => throw new NotImplementedException($"Scope type {stype} not implemented."),
            };
            if (m_currentScope != null) {
                m_currentScope.AddChildScope(newScope);
            }

            m_Scopes.Push(newScope);
            if (stype == ScopeType.File) {
                m_globalScope = newScope;
            }
            m_currentScope = newScope;
        }

        public void ExitScope() {
            if (m_Scopes.Count > 0) {
                m_Scopes.Pop();
                if ( m_Scopes.Count == 0) {
                    m_currentScope = null;
                    return;
                }
                m_currentScope = m_Scopes.Peek();
            } else {
                throw new Exception("Cannot exit global scope.");
            }
        }

        public void AddSymbol(Namespace nspace, string key, Symbol symbol) {
            m_currentScope.AddSymbol(nspace, key, symbol);
        }

        public Symbol LookUpSymbol(Namespace nspace, string key) {
            return m_currentScope.LookupSymbol(nspace, key);
        }

        public override string ToString() {
            return m_globalScope.ToString();
        }

        private CScopeSystem() {
            m_Scopes = new Stack<CScope>();
            m_globalScope = null;
            m_currentScope = null;
        }

        // It is during syntax analysis to trace the current scope
        private Stack<CScope> m_Scopes;
        private CScope m_currentScope;
        private CScope m_globalScope;
        private static CScopeSystem? m_instance = null;

        public CScope MCurrentScope => m_currentScope;
        public CScope MGlobalScope => m_globalScope;


        public static CScopeSystem GetInstance() {
            if (m_instance == null) {
                m_instance = new CScopeSystem();
            }

            return m_instance;

        }
    }
}
