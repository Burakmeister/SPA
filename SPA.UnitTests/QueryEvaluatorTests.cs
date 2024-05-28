using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SPA.DesignEntities;
using SPA.PKB;
using SPA.QueryProcessor;
using System.Collections.Generic;

namespace SPA.UnitTests
{
    [TestClass]
    public class QueryEvaluatorTests
    {
        private Mock<IPkb> mockPkb;
        private Query query;
        private QueryEvaluator evaluator;

        [TestInitialize]
        public void Setup()
        {
            mockPkb = new Mock<IPkb>();
            query = new Query();
        }

        [TestMethod]
public void ExecuteQuery_ShouldReturnCorrectResultsForAssign()
{
    // Arrange
    mockPkb.Setup(pkb => pkb.GetAssigns()).Returns(new List<int> { 1, 2, 3 });
    query.Declarations = new List<Declaration>
    {
        new Declaration { DesignEntity = "assign", Synonyms = new List<string> { "a" } }
    };
    query.Synonyms = new List<string> { "a" };

    // Act
    evaluator = new QueryEvaluator(query, mockPkb.Object);
    var results = evaluator.ExecuteQuery();

    // Assert
    Assert.IsNotNull(results);
    Assert.IsTrue(results.ContainsKey("a"));
    CollectionAssert.AreEqual(new List<string> { "1", "2", "3" }, results["a"]);
}


        [TestMethod]
        public void ExecuteQuery_ShouldReturnCorrectResultsForVariable()
        {
            // Arrange
            mockPkb.Setup(pkb => pkb.GetVariables()).Returns(new List<string> { "x", "y", "z" });
            query.Declarations = new List<Declaration>
            {
                new Declaration { DesignEntity = "variable", Synonyms = new List<string> { "v" } }
            };
            query.Synonyms = new List<string> { "v" };

            // Act
            evaluator = new QueryEvaluator(query, mockPkb.Object);
            var results = evaluator.ExecuteQuery();

            // Assert
            Assert.IsNotNull(results);
            Assert.IsTrue(results.ContainsKey("v"));
            CollectionAssert.AreEqual(new List<string> { "x", "y", "z" }, results["v"]);
        }

        [TestMethod]
        public void ExecuteQuery_WithFollowsRelation_ShouldReturnCorrectResults()
        {
            // Arrange
            var follows = new Follows
            {
                leftStmtRef = new StmtRef { Value = "a" },
                rightStmtRef = new StmtRef { Value = "_" }
            };
            query.Declarations = new List<Declaration>
            {
                new Declaration { DesignEntity = "assign", Synonyms = new List<string> { "a" } }
            };
            query.Synonyms = new List<string> { "a" };
            query.SuchThatClause = new SuchThat { Relation = follows };

            mockPkb.Setup(pkb => pkb.GetAllFollowed()).Returns(new List<int> { 1, 2, 3 });

            // Act
            evaluator = new QueryEvaluator(query, mockPkb.Object);
            var results = evaluator.ExecuteQuery();

            // Assert
            Assert.IsNotNull(results);
            Assert.IsTrue(results.ContainsKey("a"));
            CollectionAssert.AreEqual(new List<string> { "1", "2", "3" }, results["a"]);
        }

        [TestMethod]
        public void ExecuteQuery_WithModifiesSRelation_ShouldReturnCorrectResults()
        {
            // Arrange
            var modifiesS = new ModifiesS
            {
                StmtRef = new StmtRef { Value = "a" },
                EntRef = new EntRef { Value = "v" }
            };
            query.Declarations = new List<Declaration>
            {
                new Declaration { DesignEntity = "assign", Synonyms = new List<string> { "a" } },
                new Declaration { DesignEntity = "variable", Synonyms = new List<string> { "v" } }
            };
            query.Synonyms = new List<string> { "a", "v" };
            query.SuchThatClause = new SuchThat { Relation = modifiesS };

            mockPkb.Setup(pkb => pkb.GetAllStatementsThatModifieVariables()).Returns(new List<int> { 1 });
            mockPkb.Setup(pkb => pkb.GetModified(1)).Returns(new List<string> { "x" });

            // Act
            evaluator = new QueryEvaluator(query, mockPkb.Object);
            var results = evaluator.ExecuteQuery();

            // Assert
            Assert.IsNotNull(results);
            Assert.IsTrue(results.ContainsKey("a"));
            Assert.IsTrue(results.ContainsKey("v"));
            CollectionAssert.AreEqual(new List<string> { "1" }, results["a"]);
            CollectionAssert.AreEqual(new List<string> { "x" }, results["v"]);
        }

        [TestMethod]
        public void ExecuteQuery_WithUsesSRelation_ShouldReturnCorrectResults()
        {
            // Arrange
            var usesS = new UsesS
            {
                StmtRef = new StmtRef { Value = "a" },
                EntRef = new EntRef { Value = "v" }
            };
            query.Declarations = new List<Declaration>
            {
                new Declaration { DesignEntity = "assign", Synonyms = new List<string> { "a" } },
                new Declaration { DesignEntity = "variable", Synonyms = new List<string> { "v" } }
            };
            query.Synonyms = new List<string> { "a", "v" };
            query.SuchThatClause = new SuchThat { Relation = usesS };

            mockPkb.Setup(pkb => pkb.GetAllStatementsThatUseVariables()).Returns(new List<int> { 1 });
            mockPkb.Setup(pkb => pkb.GetUsed(1)).Returns(new List<string> { "y" });

            // Act
            evaluator = new QueryEvaluator(query, mockPkb.Object);
            var results = evaluator.ExecuteQuery();

            // Assert
            Assert.IsNotNull(results);
            Assert.IsTrue(results.ContainsKey("a"));
            Assert.IsTrue(results.ContainsKey("v"));
            CollectionAssert.AreEqual(new List<string> { "1" }, results["a"]);
            CollectionAssert.AreEqual(new List<string> { "y" }, results["v"]);
        }

        [TestMethod]
        public void ExecuteQuery_WithFollowsTRelation_ShouldReturnCorrectResults()
        {
            // Arrange
            var followsT = new FollowsT
            {
                leftStmtRef = new StmtRef { Value = "a" },
                rightStmtRef = new StmtRef { Value = "b" }
            };
            query.Declarations = new List<Declaration>
            {
                new Declaration { DesignEntity = "assign", Synonyms = new List<string> { "a" } },
                new Declaration { DesignEntity = "assign", Synonyms = new List<string> { "b" } }
            };
            query.Synonyms = new List<string> { "a", "b" };
            query.SuchThatClause = new SuchThat { Relation = followsT };

            mockPkb.Setup(pkb => pkb.GetAllFollowed()).Returns(new List<int> { 1, 2 });
            mockPkb.Setup(pkb => pkb.GetFollowed(1)).Returns(2);
            mockPkb.Setup(pkb => pkb.GetFollowed(2)).Returns(3);

            // Act
            evaluator = new QueryEvaluator(query, mockPkb.Object);
            var results = evaluator.ExecuteQuery();

            // Assert
            Assert.IsNotNull(results);
            Assert.IsTrue(results.ContainsKey("a"));
            Assert.IsTrue(results.ContainsKey("b"));
            CollectionAssert.AreEqual(new List<string> { "1", "2" }, results["a"]);
            CollectionAssert.AreEqual(new List<string> { "2", "3" }, results["b"]);
        }
    }
}
