using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPA.DesignEntities;
using SPA.Parsing;

namespace SPA.UnitTests
{
    [TestClass]
    public class ParserTests
    {
        private Parser parser;

        [TestInitialize]
        public void Setup()
        {
            parser = new Parser();
        }

        [TestMethod]
        public void TestProcedureParsing()
        {
            string code = "procedure myProc { x = 1; while x { x = x + 1; } }";
            parser.Parse(code);
            Program program = parser.GetProgram();

            Assert.IsNotNull(program);
            Assert.IsNotNull(program.FirstProcedure);
            Assert.AreEqual("myProc", program.FirstProcedure.ProcName);
        }

        [TestMethod]
        public void CreateAssign_ShouldReturnValidAssign()
        {
            ArrayList stringsList = new ArrayList { "x", "=", "1", ";" };
            Assign assign = parser.CreateAssign(stringsList);

            Assert.IsNotNull(assign);
            Assert.AreEqual("x", assign.Var.VarName);
            Assert.IsNotNull(assign.Expr);
            Assert.IsInstanceOfType(assign.Expr, typeof(Constant));
            Assert.AreEqual(1, ((Constant)assign.Expr).Value);
        }

        [TestMethod]
        public void CreateWhile_ShouldReturnValidWhile()
        {
            ArrayList stringsList = new ArrayList { "while", "x", "{", "x", "=", "x", "+", "1", ";", "}" };
            While whileStmt = parser.CreateWhile(stringsList);

            Assert.IsNotNull(whileStmt);
            Assert.AreEqual("x", whileStmt.Var.VarName);
            Assert.IsNotNull(whileStmt.StatementList);
            Assert.IsNotNull(whileStmt.StatementList.FirstStatement);
            Assert.IsInstanceOfType(whileStmt.StatementList.FirstStatement, typeof(Assign));
        }

        [TestMethod]
        public void CreateExpr_ShouldReturnValidExpression()
        {
            ArrayList stringsList = new ArrayList { "x", "+", "1", ";" };
            Expr expr = parser.CreateExpr(stringsList);

            Assert.IsNotNull(expr);
            Assert.IsInstanceOfType(expr, typeof(ExprPlus));
            Assert.IsInstanceOfType(((ExprPlus)expr).LeftExpr, typeof(Variable));
            Assert.IsInstanceOfType(((ExprPlus)expr).RightExpr, typeof(Constant));
        }

        [TestMethod]
        public void Parse_ShouldParseCodeCorrectly()
        {
            string code = "procedure myProc { x = 1; while x { x = x + 1; } }";
            int linesParsed = parser.Parse(code);

            Assert.AreEqual(3, linesParsed);  // 3 lines: procedure, assignment, while
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Nieprawidłowo zdefiniowana pętla while!")]
        public void CreateWhile_ShouldThrowExceptionForInvalidWhile()
        {
            ArrayList stringsList = new ArrayList { "while", "x", "{", "x", "=", "x", "+", "1" };  // Missing closing brace
            parser.CreateWhile(stringsList);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Nieprawidłowo zdefiniowane przypisanie!")]
        public void CreateAssign_ShouldThrowExceptionForInvalidAssign()
        {
            ArrayList stringsList = new ArrayList { "x", "1", ";" };  // Missing '='
            parser.CreateAssign(stringsList);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Nieprawidłowa nazwa zmiennej! [0]")]
        public void CreateExpr_ShouldThrowExceptionForInvalidVariableName()
        {
            ArrayList stringsList = new ArrayList { "1x", "+", "1", ";" };  // Invalid variable name
            parser.CreateExpr(stringsList);
        }

        [TestMethod]
        public void CreateProcedure_ShouldReturnValidProcedure()
        {
            ArrayList stringsList = new ArrayList { "procedure", "myProc", "{", "x", "=", "1", ";", "}" };
            Procedure procedure = parser.CreateProcedure(stringsList);

            Assert.IsNotNull(procedure);
            Assert.AreEqual("myProc", procedure.ProcName);
            Assert.IsNotNull(procedure.StatementList);
            Assert.IsNotNull(procedure.StatementList.FirstStatement);
            Assert.IsInstanceOfType(procedure.StatementList.FirstStatement, typeof(Assign));
        }
    }
}
