using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient.Gremlin
{
    internal class GremlinPagedEnumerator<TResult> : IEnumerator<TResult>
    {
        readonly Func<IGremlinQuery, IEnumerable<TResult>> pageLoadCallback;
        readonly IGremlinQuery query;

        const int pageSize = 100;

        int currentPageIndex = -1;
        int currentRowIndex = -1;
        TResult[] currentPageData;

        public GremlinPagedEnumerator(
            Func<IGremlinQuery, IEnumerable<TResult>> pageLoadCallback,
            IGremlinQuery query)
        {
            this.pageLoadCallback = pageLoadCallback;
            this.query = query;
        }

        public bool MoveNext()
        {
            if (currentPageIndex == -1 ||
                currentRowIndex == currentPageData.Count() - 1)
            {
                LoadNextPage();
            }
            currentRowIndex++;
            return currentRowIndex < currentPageData.Count();
        }

        void LoadNextPage()
        {
            currentPageIndex++;
            currentRowIndex = -1;
            var drop = currentPageIndex * pageSize;
            var pageQuery = query.AddBlock(".drop({0}).take({1})", drop, pageSize);
            currentPageData = pageLoadCallback(pageQuery).ToArray();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public TResult Current
        {
            get { return currentPageData[currentRowIndex]; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
        }
    }
}