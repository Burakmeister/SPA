using SPA.PKB;
using System.Windows.Documents;

namespace SPA.QueryProcessor
{
    public class QueryProcessorExec
    {
        private readonly QueryPreprocessor _preprocessor;
        private readonly QueryEvaluator _evaluator;
        public Query Query { get; }

        // String do testu parsowania w preprocessorze
        private readonly string _query;

        public QueryProcessorExec(string query, IPkb pkb)
        {
            this._query = query;

            Query = new Query();
            _preprocessor = new QueryPreprocessor(_query, Query);
            _evaluator = new QueryEvaluator(Query, pkb);
        }
    }
}
