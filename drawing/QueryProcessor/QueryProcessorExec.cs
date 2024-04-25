using SPA.PKB;

namespace SPA.QueryProcessor
{
    public class QueryProcessorExec
    {
        private readonly QueryPreprocessor _preprocessor;
        private readonly QueryEvaluator _evaluator;
        private readonly Query _query;

        // String do testu parsowania w preprocessorze
        private readonly string query;

        public QueryProcessorExec(string query, IPkb pkb)
        {
            this.query = query;

            _query = new Query();
            _preprocessor = new QueryPreprocessor(query, _query);
            _evaluator = new QueryEvaluator(_query, pkb);
        }

    }
}
