using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    internal class QueryProcessor
    {
        private readonly QueryPreprocessor _preprocessor;
        private readonly QueryEvaluator _evaluator;
        private readonly Query _query;

        // String do testu parsowania w preprocessorze
        private readonly string query;

        public QueryProcessor(string query)
        {
            this.query = query;

            _query = new Query();
            _preprocessor = new QueryPreprocessor(query, _query);
            _evaluator = new QueryEvaluator();
        

        }

    }
}
