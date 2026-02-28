using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CParser {
    public class SyntaxTreePrinterVisitor : AbstractParseTreeVisitor<int> {
        private string m_dotfileName;
        StreamWriter m_writer;
        private Stack<string> m_nodeIds = new Stack<string>();
        private static int ms_nodeSerial = 0;

        public SyntaxTreePrinterVisitor(string dotFileName) {
            m_dotfileName = dotFileName;
        }

        public override int VisitChildren(IRuleNode node) {
            ParserRuleContext context;
            if (node as ParserRuleContext != null) {
                context = (ParserRuleContext)node;
                int n = context.ChildCount;
                for (int i = 0; i < n; i++) {
                    IParseTree c = context.GetChild(i);
                    Visit(c);
                }
            }


            return 0;
        }

        private string UnquoteOnce(string s) {
            if (string.IsNullOrEmpty(s)) return s;
            int start = s[0] == '"' ? 1 : 0;
            int end = s[^1] == '"' ? s.Length - 1 : s.Length;
            if (start == 1 && end == s.Length - 1 && end > start)
                return s.Substring(1, s.Length - 2);   // both ends quoted
            if (start == 1 && end == s.Length)
                return s.Substring(1);                 // only leading quote
            if (start == 0 && end == s.Length - 1)
                return s.Substring(0, s.Length - 1);   // only trailing quote
            return s;
        }


        public override int Visit(IParseTree tree) {

            ParserRuleContext context;
            ITerminalNode terminalNode;
            if (tree as ParserRuleContext != null) {
                context = (ParserRuleContext)tree;
                // 1. Generate unique node id
                string unquote_s = UnquoteOnce(CGrammarParser.ruleNames[context.RuleIndex]);

                string node_id = unquote_s + "_" + ms_nodeSerial++;

                // 2  If not root node ,print graphviz edge from parent to this node
                if (context.Parent != null) {
                    string parent_node_id = m_nodeIds.Peek();
                    // print edge from parent to this node
                    m_writer.WriteLine($"    \"{parent_node_id}\" -> \"{node_id}\";");
                } else {
                    // 2a If root node, print graphviz file header i.e digraph G {
                    m_writer = new StreamWriter(m_dotfileName);
                    m_writer.WriteLine("digraph G {");
                }

                // 3. Push this node's id onto stack
                m_nodeIds.Push(node_id);

                // 4. Visit children nodes
                VisitChildren(context);

                // 5. Pop this node's id from stack
                m_nodeIds.Pop();

                // 6. If root node, print graphviz file footer i.e }, close file,
                // call dot to generate png
                if (context.Parent == null) {
                    m_writer.WriteLine("}");
                    m_writer.Close();
                    // call dot to generate png
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "dot";
                    startInfo.Arguments = $"-Tgif {m_dotfileName} -o {m_dotfileName}.gif";
                    Process process = Process.Start(startInfo);
                    process.WaitForExit();
                }

            }
            else if ( tree as ITerminalNode != null) {
                terminalNode = tree as ITerminalNode;
                // 1.Generate unique node id
                if (terminalNode.Symbol.Type > -1) {

                    string unquote_s = UnquoteOnce(terminalNode.Symbol.Text);

                    string node_id = unquote_s + "_" + ms_nodeSerial++;

                    // 2  If not root node ,print graphviz edge from parent to this node
                    string parent_node_id = m_nodeIds.Peek();
                    // print edge from parent to this node
                    m_writer.WriteLine($"    \"{parent_node_id}\" -> \"{node_id}\";");
                }
            } 
            else {
                throw new Exception("Incompatible syntax tree node type");
            }

            
            return 0;
        }
    }
}
