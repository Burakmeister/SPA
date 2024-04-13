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
    public class ParserTests
    {
        private QueryPreprocessor preprocessor;
        private Parser parser;

        string validQuery = "Select ";
        Query tquery = new Query();
        private int position = 0;

        [TestInitialize]
        public void Setup()
        {
            parser = new Parser();
            preprocessor = new QueryPreprocessor(validQuery,tquery);
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

        [TestMethod]
        public string PeekTest()
        {
            int numTokens = 2;

            string result = preprocessor.Peek(numTokens);

            Assert.AreEqual("a", result);
            
            return result;
        }

        [TestMethod]
        public void AdvanceTest()
        {
            position = 0;
            int numTokens = 1;

            preprocessor.Advance(numTokens);
            Assert.AreEqual(validQuery, tquery);
        }

        [TestMethod]
        //public Relation ValidRelTest()
        //{
        //    try 
        //    {
        //        Relation result = preprocessor.ValidateRelation();
        //        Assert.AreEqual(expectedRelation, result);
        //    }
        //    catch (Exception e)
        //    {
        //        Assert.AreEqual("Invalid relationship", ex.Message);
        //    }

        //}

    }



}
