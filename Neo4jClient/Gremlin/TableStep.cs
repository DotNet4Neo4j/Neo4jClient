using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Neo4jClient.Gremlin
{
    public static class TableStep
    {
        public static IEnumerable<TResult> Table<TResult>(
            this IGremlinQuery query) where TResult : new()
        {
            var newQuery = query.AddBlock(".table(new Table()).cap");
            return new GremlinProjectionEnumerable<TResult>(newQuery);
        }

        public static IEnumerable<TResult> Table<TResult, TClosure1>(
            this IGremlinQuery query,
            Expression<Func<TClosure1, object>> closure1) where TResult : new()
        {
            var newQuery = query.AddBlock(".table(new Table())");
            newQuery = AddClosure(newQuery, closure1);
            newQuery = newQuery.AddBlock(".cap");
            return new GremlinProjectionEnumerable<TResult>(newQuery);
        }

        public static IEnumerable<TResult> Table<TResult, TClosure1, TClosure2>(
            this IGremlinQuery query,
            Expression<Func<TClosure1, object>> closure1,
            Expression<Func<TClosure2, object>> closure2) where TResult : new()
        {
            var newQuery = query.AddBlock(".table(new Table())");
            newQuery = AddClosure(newQuery, closure1);
            newQuery = AddClosure(newQuery, closure2);
            newQuery = newQuery.AddBlock(".cap");
            return new GremlinProjectionEnumerable<TResult>(newQuery);
        }

        public static IEnumerable<TResult> Table<TResult, TClosure1, TClosure2, TClosure3>(
            this IGremlinQuery query,
            Expression<Func<TClosure1, object>> closure1,
            Expression<Func<TClosure2, object>> closure2,
            Expression<Func<TClosure3, object>> closure3) where TResult : new()
        {
            var newQuery = query.AddBlock(".table(new Table())");
            newQuery = AddClosure(newQuery, closure1);
            newQuery = AddClosure(newQuery, closure2);
            newQuery = AddClosure(newQuery, closure3);
            newQuery = newQuery.AddBlock(".cap");
            return new GremlinProjectionEnumerable<TResult>(newQuery);
        }

        public static IEnumerable<TResult> Table<TResult, TClosure1, TClosure2, TClosure3, TClosure4>(
            this IGremlinQuery query,
            Expression<Func<TClosure1, object>> closure1,
            Expression<Func<TClosure2, object>> closure2,
            Expression<Func<TClosure3, object>> closure3,
            Expression<Func<TClosure4, object>> closure4) where TResult : new()
        {
            var newQuery = query.AddBlock(".table(new Table())");
            newQuery = AddClosure(newQuery, closure1);
            newQuery = AddClosure(newQuery, closure2);
            newQuery = AddClosure(newQuery, closure3);
            newQuery = AddClosure(newQuery, closure4);
            newQuery = newQuery.AddBlock(".cap");
            return new GremlinProjectionEnumerable<TResult>(newQuery);
        }

        public static IEnumerable<TResult> Table<TResult, TClosure1, TClosure2, TClosure3, TClosure4, TClosure5>(
            this IGremlinQuery query,
            Expression<Func<TClosure1, object>> closure1,
            Expression<Func<TClosure2, object>> closure2,
            Expression<Func<TClosure3, object>> closure3,
            Expression<Func<TClosure4, object>> closure4,
            Expression<Func<TClosure5, object>> closure5) where TResult : new()
        {
            var newQuery = query.AddBlock(".table(new Table())");
            newQuery = AddClosure(newQuery, closure1);
            newQuery = AddClosure(newQuery, closure2);
            newQuery = AddClosure(newQuery, closure3);
            newQuery = AddClosure(newQuery, closure4);
            newQuery = AddClosure(newQuery, closure5);
            newQuery = newQuery.AddBlock(".cap");
            return new GremlinProjectionEnumerable<TResult>(newQuery);
        }

        public static IEnumerable<TResult> Table<TResult, TClosure1, TClosure2, TClosure3, TClosure4, TClosure5, TClosure6>(
            this IGremlinQuery query,
            Expression<Func<TClosure1, object>> closure1,
            Expression<Func<TClosure2, object>> closure2,
            Expression<Func<TClosure3, object>> closure3,
            Expression<Func<TClosure4, object>> closure4,
            Expression<Func<TClosure5, object>> closure5,
            Expression<Func<TClosure6, object>> closure6) where TResult : new()
        {
            var newQuery = query.AddBlock(".table(new Table())");
            newQuery = AddClosure(newQuery, closure1);
            newQuery = AddClosure(newQuery, closure2);
            newQuery = AddClosure(newQuery, closure3);
            newQuery = AddClosure(newQuery, closure4);
            newQuery = AddClosure(newQuery, closure5);
            newQuery = AddClosure(newQuery, closure6);
            newQuery = newQuery.AddBlock(".cap");
            return new GremlinProjectionEnumerable<TResult>(newQuery);
        }

        public static IEnumerable<TResult> Table<TResult, TClosure1, TClosure2, TClosure3, TClosure4, TClosure5, TClosure6, TClosure7>(
            this IGremlinQuery query,
            Expression<Func<TClosure1, object>> closure1,
            Expression<Func<TClosure2, object>> closure2,
            Expression<Func<TClosure3, object>> closure3,
            Expression<Func<TClosure4, object>> closure4,
            Expression<Func<TClosure5, object>> closure5,
            Expression<Func<TClosure6, object>> closure6,
            Expression<Func<TClosure7, object>> closure7) where TResult : new()
        {
            var newQuery = query.AddBlock(".table(new Table())");
            newQuery = AddClosure(newQuery, closure1);
            newQuery = AddClosure(newQuery, closure2);
            newQuery = AddClosure(newQuery, closure3);
            newQuery = AddClosure(newQuery, closure4);
            newQuery = AddClosure(newQuery, closure5);
            newQuery = AddClosure(newQuery, closure6);
            newQuery = AddClosure(newQuery, closure7);
            newQuery = newQuery.AddBlock(".cap");
            return new GremlinProjectionEnumerable<TResult>(newQuery);
        }

        public static IEnumerable<TResult> Table<TResult, TClosure1, TClosure2, TClosure3, TClosure4, TClosure5, TClosure6, TClosure7, TClosure8>(
            this IGremlinQuery query,
            Expression<Func<TClosure1, object>> closure1,
            Expression<Func<TClosure2, object>> closure2,
            Expression<Func<TClosure3, object>> closure3,
            Expression<Func<TClosure4, object>> closure4,
            Expression<Func<TClosure5, object>> closure5,
            Expression<Func<TClosure6, object>> closure6,
            Expression<Func<TClosure7, object>> closure7,
            Expression<Func<TClosure8, object>> closure8) where TResult : new()
        {
            var newQuery = query.AddBlock(".table(new Table())");
            newQuery = AddClosure(newQuery, closure1);
            newQuery = AddClosure(newQuery, closure2);
            newQuery = AddClosure(newQuery, closure3);
            newQuery = AddClosure(newQuery, closure4);
            newQuery = AddClosure(newQuery, closure5);
            newQuery = AddClosure(newQuery, closure6);
            newQuery = AddClosure(newQuery, closure7);
            newQuery = AddClosure(newQuery, closure8);
            newQuery = newQuery.AddBlock(".cap");
            return new GremlinProjectionEnumerable<TResult>(newQuery);
        }

        public static IEnumerable<TResult> Table<TResult, TClosure1, TClosure2, TClosure3, TClosure4, TClosure5, TClosure6, TClosure7, TClosure8, TClosure9>(
            this IGremlinQuery query,
            Expression<Func<TClosure1, object>> closure1,
            Expression<Func<TClosure2, object>> closure2,
            Expression<Func<TClosure3, object>> closure3,
            Expression<Func<TClosure4, object>> closure4,
            Expression<Func<TClosure5, object>> closure5,
            Expression<Func<TClosure6, object>> closure6,
            Expression<Func<TClosure7, object>> closure7,
            Expression<Func<TClosure8, object>> closure8,
            Expression<Func<TClosure9, object>> closure9) where TResult : new()
        {
            var newQuery = query.AddBlock(".table(new Table())");
            newQuery = AddClosure(newQuery, closure1);
            newQuery = AddClosure(newQuery, closure2);
            newQuery = AddClosure(newQuery, closure3);
            newQuery = AddClosure(newQuery, closure4);
            newQuery = AddClosure(newQuery, closure5);
            newQuery = AddClosure(newQuery, closure6);
            newQuery = AddClosure(newQuery, closure7);
            newQuery = AddClosure(newQuery, closure8);
            newQuery = AddClosure(newQuery, closure9);
            newQuery = newQuery.AddBlock(".cap");
            return new GremlinProjectionEnumerable<TResult>(newQuery);
        }

        public static IEnumerable<TResult> Table<TResult, TClosure1, TClosure2, TClosure3, TClosure4, TClosure5, TClosure6, TClosure7, TClosure8, TClosure9, TClosure10>(
            this IGremlinQuery query,
            Expression<Func<TClosure1, object>> closure1,
            Expression<Func<TClosure2, object>> closure2,
            Expression<Func<TClosure3, object>> closure3,
            Expression<Func<TClosure4, object>> closure4,
            Expression<Func<TClosure5, object>> closure5,
            Expression<Func<TClosure6, object>> closure6,
            Expression<Func<TClosure7, object>> closure7,
            Expression<Func<TClosure8, object>> closure8,
            Expression<Func<TClosure9, object>> closure9,
            Expression<Func<TClosure10, object>> closure10) where TResult : new()
        {
            var newQuery = query.AddBlock(".table(new Table())");
            newQuery = AddClosure(newQuery, closure1);
            newQuery = AddClosure(newQuery, closure2);
            newQuery = AddClosure(newQuery, closure3);
            newQuery = AddClosure(newQuery, closure4);
            newQuery = AddClosure(newQuery, closure5);
            newQuery = AddClosure(newQuery, closure6);
            newQuery = AddClosure(newQuery, closure7);
            newQuery = AddClosure(newQuery, closure8);
            newQuery = AddClosure(newQuery, closure9);
            newQuery = AddClosure(newQuery, closure10);
            newQuery = newQuery.AddBlock(".cap");
            return new GremlinProjectionEnumerable<TResult>(newQuery);
        }

        static IGremlinQuery AddClosure<TIn>(IGremlinQuery newQuery, Expression<Func<TIn, object>> closure)
        {
            var expressionKey = FilterFormatters.ParseKeyFromExpression(closure.Body);
            return newQuery.AddBlock("{{it[{0}]}}", expressionKey.Name);
        }
    }
}
