using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SPA.DesignEntities;
using SPA.Parsing;
using SPA.QueryProcessor;

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
            int amongus = 1;

            Assert.AreEqual(1, amongus);
        }

        [TestMethod]
        public void CreateAssign_ShouldReturnValidAssign()
        {
            var assign = parser.CreateAssign();
            Assert.IsNotNull(assign);
            Assert.IsNotNull(assign.Var);
            Assert.AreEqual("xd", assign.Var.VarName);
            Assert.AreEqual(0, assign.Var.LineNumber);
        }
    }
}
