using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
 

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPA.DesignEntities;
using SPA.Parsing;
using SPA.QueryProcessor;


namespace SPA.UnitTests
{
    [TestClass]
    public class PreprocessorTests
    {
        [TestMethod]
        public void TestQuery1()
        {
            // Arrange
            string query = "stmt s; Select s such that Follows(s, v);";
            Query queryObject = new Query();
            QueryPreprocessor preprocessor = new QueryPreprocessor(query, queryObject);

            // Act
            preprocessor.ValidateQuery();

            // Assert
            Assert.IsNotNull(queryObject.Declarations);
            Assert.AreEqual(1, queryObject.Declarations.Count);
            Assert.AreEqual("s", queryObject.Declarations[0].Synonyms[0]);
            Assert.IsNotNull(queryObject.SuchThatClause);
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(query, @"\bFollows\b"));
            Assert.IsNotNull(queryObject.SuchThatClause.Relation);
        }

        [TestMethod]
        public void TestQuery2()
        {
            // Arrange
            string query = "assign a; Select a such that Modifies (1, \"x\");";
            Query queryObject = new Query();
            QueryPreprocessor preprocessor = new QueryPreprocessor(query, queryObject);

            // Act
            preprocessor.ValidateQuery();

            Assert.IsNotNull(queryObject.Declarations);
            Assert.AreEqual(1, queryObject.Declarations.Count);
            Assert.AreEqual("a", queryObject.Declarations[0].Synonyms[0]);
            Assert.IsNotNull(queryObject.SuchThatClause);
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(query, @"\bModifies\b"));
            Assert.IsNotNull(queryObject.SuchThatClause.Relation);
        }

        [TestMethod]
        public void TestQuery3()
        {
            // Arrange
            string query = "variable v; Select v such that Parent (5, 7);";
            Query queryObject = new Query();
            QueryPreprocessor preprocessor = new QueryPreprocessor(query, queryObject);

            // Act
            preprocessor.ValidateQuery();

            Assert.IsNotNull(queryObject.Declarations);
            Assert.AreEqual(1, queryObject.Declarations.Count);
            Assert.AreEqual("v", queryObject.Declarations[0].Synonyms[0]);
            Assert.IsNotNull(queryObject.SuchThatClause);
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(query, @"\bParent\b"));
            Assert.IsNotNull(queryObject.SuchThatClause.Relation);
        }

        [TestMethod]
        public void TestQuery4()
        {
            // Arrange
            string query = "assign a1, a2; while w1, w2; Select a1 pattern a1 (\"x\", _) and a2 (\"x\",_\"x\"_) such that Affects (a1, a2) and Parent* (w2, a2) and Parent* (w1, w2)";
            Query queryObject = new Query();
            QueryPreprocessor preprocessor = new QueryPreprocessor(query, queryObject);

            // Act
            preprocessor.ValidateQuery();

            Assert.IsNotNull(queryObject.Declarations);
            Assert.AreEqual(2, queryObject.Declarations.Count);
            Assert.AreEqual("a1", queryObject.Declarations[0].Synonyms[0]);
            Assert.AreEqual("a2", queryObject.Declarations[0].Synonyms[1]);
            Assert.AreEqual("w1", queryObject.Declarations[1].Synonyms[0]);
            Assert.AreEqual("w2", queryObject.Declarations[1].Synonyms[1]);
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(query, @"\bpattern\b"));
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(query, @"\bAffects\b"));
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(query, @"\bParent*\b"));
            //Assert.IsNotNull(queryObject.SuchThatClause);
        }

    }
}
