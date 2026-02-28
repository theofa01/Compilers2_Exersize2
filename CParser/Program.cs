using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Atn;


namespace CParser {

    public class Program {
        static void Main(string[] args) {
            // C# class to read from a text file
            StreamReader streamReader = new StreamReader(args[0]);
            // Read the entire file and place it in a string
            string input = streamReader.ReadToEnd();
            // Create an ANTLR input stream from the string
            AntlrInputStream inputStream = new AntlrInputStream(input);
            // Create a lexer that feeds from that stream
            CGrammarLexer lexer = new CGrammarLexer(inputStream);
            // Create a token stream that feeds from the lexer
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);


            /*tokenStream.Fill();
            Console.WriteLine("TOKENS:");
            foreach (var t in tokenStream.GetTokens()) {
                var name = lexer.Vocabulary.GetSymbolicName(t.Type)
                           ?? lexer.Vocabulary.GetDisplayName(t.Type);
                Console.WriteLine($"{name,-16} '{t.Text}'");
            }*/
            CGrammarParser parser = new CGrammarParser(tokenStream);


            // Ask the parser to start parsing at rule 'compilationUnit'
            parser.Profile = true;
            
            IParseTree syntaxTree = parser.translation_unit();

            


            //Console.WriteLine($"{CScopeSystem.GetInstance().ToString()}");
            // Print the tree in LISP format
            //Console.WriteLine(syntaxTree.ToStringTree());

            SyntaxTreePrinterVisitor syntaxTreePrinterVisitor =
                new SyntaxTreePrinterVisitor("test.dot");
            syntaxTreePrinterVisitor.Visit(syntaxTree);

            ANLTRST2ASTGenerationVisitor anltrst2AstGenerationVisitor = new ANLTRST2ASTGenerationVisitor();
            anltrst2AstGenerationVisitor.Visit(syntaxTree);

            ASTPrinterVisitor astPrinterVisitor = new ASTPrinterVisitor("ast.dot");
            astPrinterVisitor.Visit(anltrst2AstGenerationVisitor.Root, null);

            // Build Scope System after parsing and AST generation
            ScopeBuilderVisitor scopeBuilderVisitor = new ScopeBuilderVisitor();
            scopeBuilderVisitor.Visit(anltrst2AstGenerationVisitor.Root, null);
            Console.WriteLine($"{CScopeSystem.GetInstance().ToString()}");
        }

    }
}


