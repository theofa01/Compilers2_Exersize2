using Antlr4.Runtime.Sharpen;
using CParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CParser.CScope;

namespace CParser {

    public class Symbol {
        public enum SymbolType {
            Variable,
            Function,
            Type
        };
        public string m_name;
        private ASTElement m_node;
        public SymbolType m_type;
        public Symbol(){}
        
        public Symbol(string name, SymbolType type, ASTElement node) {
            m_name = name;
            m_type = type;
            m_node = node;
        }

        public string MName {
            get => m_name;
            set => m_name = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ASTElement MNode {
            get => m_node;
            set => m_node = value ?? throw new ArgumentNullException(nameof(value));
        }

        public SymbolType MType {
            get => m_type;
            set => m_type = value;
        }

        public override string ToString() {
            return $"Symbol(Name: {m_name}, Type: {m_type}, Node: {m_node})";
        }
    }
    
    public class SymbolTable{
        private SymbolTable m_parent;

        Dictionary<string,Symbol> symbols = new Dictionary<string,Symbol>();

        // Lexical scoping - parent symbol table
        public Symbol LookupSymbol(string key) {
            if (symbols.ContainsKey(key)) {
                return symbols[key];
            } else if (m_parent != null) {
                return m_parent.LookupSymbol(key);
            } else {
                return null;
            }
        }
        public void AddSymbol(string key, Symbol symbol) {
            if (symbols.ContainsKey(key)) {
                throw new InvalidOperationException($"Symbol '{key}' already exists in the current scope.");
            }
            
            if (symbol == null) {
                throw new ArgumentNullException(nameof(symbol), "Cannot add null symbol to symbol table.");
            }
            
            if (string.IsNullOrEmpty(symbol.m_name)) {
                throw new ArgumentException("Symbol must have a valid name.", nameof(symbol));
            }
            symbols.Add(key, symbol);
        }

        public override string ToString() {
            StringBuilder report= new StringBuilder();
            report.AppendLine("SymbolTable:");
            foreach (KeyValuePair<string, Symbol> pair in symbols) {
                report.AppendLine($" Key: {pair.Key} , Value: {pair.Value.ToString()}");
            }
            return report.ToString();

        }
    }
}
    