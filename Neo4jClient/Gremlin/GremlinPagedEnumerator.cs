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

        const int PageSize = 100;

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
            var hasAPageLoaded = currentPageIndex != -1;
            var curentPageIsPartialPage = currentPageData != null && currentPageData.Count() < PageSize;
            var currentRecordIsLastOneOnPage = currentPageData != null && currentRowIndex == currentPageData.Count() - 1;

            if (hasAPageLoaded && curentPageIsPartialPage && currentRecordIsLastOneOnPage)
                return false;

            if (!hasAPageLoaded || currentRecordIsLastOneOnPage)
                LoadNextPage();

            if (currentPageData == null)
                throw new InvalidOperationException("CurrentPageData is null even though we have a page index.");

            currentRowIndex++;
            return currentRowIndex < currentPageData.Count();
        }

        void LoadNextPage()
        {
            currentPageIndex++;
            currentRowIndex = -1;
            var drop = currentPageIndex * PageSize;
            var pageQuery = query.AddBlock(".drop({0}).take({1})._()", drop, PageSize);
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