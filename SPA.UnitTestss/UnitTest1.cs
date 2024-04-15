using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPA.Parsering;

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
            string code = "procedure P { x = 1; }";
            parser.Parse(code);
            Assert.IsNotNull(parser.program);
            Assert.IsNotNull(parser.program.FirstProcedure);
            Assert.AreEqual("P", parser.program.FirstProcedure.Name);
        }

        [TestMethod]
        public void TestAssignmentParsing()
        {
            string code = "procedure P { x = 1; }";
            parser.Parse(code);
            var proc = parser.program.FirstProcedure;
            Assert.IsNotNull(proc.StatementList.FirstStatement as Assign);
            Assert.AreEqual("x", ((Assign)proc.StatementList.FirstStatement).Variable.Name);
        }

       
    }
}
