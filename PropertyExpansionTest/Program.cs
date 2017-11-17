using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyExpansionTest
{
    class Program
    {
        static void Main(string[] args)
        {
            const string pathToSolution = @"..\..\..\TestPropertyExpansionProject\TestPropertyExpansionProject.sln";
            const string projectName = "TestPropertyExpansionProject";

            // start Roslyn workspace
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();

            // open solution we want to analyze
            Solution solutionToAnalyze =
                workspace.OpenSolutionAsync(pathToSolution).Result;

            Project project = solutionToAnalyze.Projects.Where((pr) => pr.Name == projectName).FirstOrDefault();

            Compilation compilation = project.GetCompilationAsync().Result;

            DocumentId docId = 
                solutionToAnalyze.GetDocumentIdsWithFilePath(@"C:\IDEAS\GITHUB\VSIXRoslynTESTS\TestPropertyExpansionProject\MyTestPropExpansionsClass.cs").FirstOrDefault();


            Document doc = solutionToAnalyze.GetDocument(docId);

            SyntaxTree syntaxTree = doc.GetSyntaxTreeAsync().Result;

            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            //INamedTypeSymbol clsToChangeSymbolsIn =
            //    compilation.GetTypeByMetadataName("TestPropertyExpansionProject.MyTestPropExpansionsClass");

           IEnumerable<SyntaxNode> classNodes =
                syntaxTree
                .GetRoot()
                .DescendantNodes()
                .Where
                (
                    node =>
                        (node.IsKind(SyntaxKind.ClassDeclaration))
                );

            SyntaxNode clsNodeToChangeSymbolsIn = classNodes.FirstOrDefault();

            INamedTypeSymbol clsToChangeSymbolsIn = 
                semanticModel.GetDeclaredSymbol(clsNodeToChangeSymbolsIn) as INamedTypeSymbol;

            IEnumerable<IPropertySymbol> thePublicProps =
                clsToChangeSymbolsIn
                    .GetMembers()
                    .Where(symb =>
                                (symb.Kind == SymbolKind.Property) &&
                                (symb.IsStatic == false) && 
                                (symb.DeclaredAccessibility == Accessibility.Public) && 
                                (symb.GetAttributes()
                                     .FirstOrDefault
                                     (
                                        attr => attr.AttributeClass.Name == nameof(NP.Paradigms.Attrs.PostNotifiablePropertyAttribute)
                                     ) != null) ).Cast<IPropertySymbol>();

            IPropertySymbol firstPropSymbol = thePublicProps.FirstOrDefault();

            PropertyDeclarationSyntax firstPropSymbolSyntaxNode =
                firstPropSymbol.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax() as PropertyDeclarationSyntax;

            AccessorDeclarationSyntax getSyntaxNode =
                firstPropSymbol.GetMethod.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax()
                as AccessorDeclarationSyntax;

            AccessorDeclarationSyntax setSyntaxNode =
                firstPropSymbol.SetMethod.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax()
                as AccessorDeclarationSyntax;

            if ( (getSyntaxNode.Body != null) || 
                 (getSyntaxNode.ExpressionBody != null) || 
                 (setSyntaxNode.Body != null) || 
                 (setSyntaxNode.ExpressionBody != null) || 
                 (firstPropSymbolSyntaxNode.ExpressionBody != null) )
            {
                return;
            }

            // otherwise - auto property. 
        }
    }
}
