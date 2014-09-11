using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Neo4jClient.Cypher.EntityExtension.Attributes;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Cypher.EntityExtension
{
    public static class CypherExtension
    {
        private static readonly CypherTypeItemHelper CypherTypeItemHelper = new CypherTypeItemHelper();
        public static CypherExtensionContext DefaultExtensionContext = new CypherExtensionContext();

        public static string EntityLabel<T>(this T entity)
        {
            var label = typeof(T).GetCustomAttributes(typeof(CypherLabelAttribute),true).FirstOrDefault() as CypherLabelAttribute;
            return label == null ? typeof(T).Name : label.Name;
        }

        public static string ToCypherString<TEntity>(this TEntity entity, ICypherExtensionContext context, List<CypherProperty> useProperties, string paramKey)
            where TEntity : class
        {
            //with the list of properties construct the string
            var label = entity.EntityLabel();
            paramKey = paramKey ?? label.ToLowerInvariant();
            //Iterate properties, insert paramKey
            var properties = new Func<string>(() => string.Format("{{{0}}}",
                        string.Join(",",useProperties.Select(x => string.Format("{0}:{{{1}}}.{0}", x.JsonName, paramKey)))));

            return string.Format("{0}:{1} {2}", paramKey, label, properties());
        }

        public static string ToCypherString<TEntity, TAttr>(this TEntity entity, ICypherExtensionContext context, string paramKey = null, List<CypherProperty> useProperties = null)
            where TAttr : CypherExtensionAttribute
            where TEntity : class
        {
            return entity.ToCypherString(context, useProperties?? CypherTypeItemHelper.PropertiesForPurpose<TEntity,TAttr>(), paramKey);
        }
        
        public static Dictionary<string, object> CreateDynamic<TEntity>(this TEntity entity, List<CypherProperty> properties) where TEntity : class
        {
            var type = typeof(TEntity);
            return properties.Select(prop => new { Key = prop.JsonName, Value = type.GetProperty(prop.TypeName).GetValue(entity, null) }).ToDictionary(x => x.Key, x => x.Value);
        } 

        public static ICypherFluentQuery MatchEntity<T>(this ICypherFluentQuery query, T entity, string paramKey = null, string preCql = "", string postCql = "", List<CypherProperty> propertyOverride = null) where T : class
        {
            paramKey = paramKey ?? entity.EntityLabel().ToLowerInvariant();
            var cql = string.Format("{0}({1}){2}", preCql, entity.ToCypherString<T, CypherMatchAttribute>(CypherExtensionContext.Create(query), paramKey, propertyOverride), postCql);
            //create a dynamic object for the type
            dynamic cutdown = entity.CreateDynamic(propertyOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMatchAttribute>());
            return query.Match(cql).WithParam(paramKey, cutdown);
        }

        public static ICypherFluentQuery MergeEntity<T>(this ICypherFluentQuery query, T entity, string paramKey = null, List<CypherProperty> mergeOverride = null, List<CypherProperty> onMatchOverride = null, List<CypherProperty> onCreateOverride = null,string preCql = "", string postCql = "") where T : class
        {
            paramKey = paramKey ?? entity.EntityLabel().ToLowerInvariant();
            var cql = string.Format("{0}({1}){2}", preCql, entity.ToCypherString<T, CypherMergeAttribute>(CypherExtensionContext.Create(query), paramKey,mergeOverride), postCql);
            return query.CommonMerge(entity, paramKey, cql, mergeOverride, onMatchOverride, onCreateOverride);
        }

        public static ICypherFluentQuery MergeRelationship<T>(this ICypherFluentQuery query, T entity, List<CypherProperty> mergeOverride = null, List<CypherProperty> onMatchOverride = null, List<CypherProperty> onCreateOverride = null) where T : BaseRelationship
        {
            //Eaxctly the same as a merge entity except the cql is different
            var cql = string.Format("({0})-[{1}]->({2})", entity.FromKey, entity.ToCypherString<T, CypherMergeAttribute>(CypherExtensionContext.Create(query), entity.Key, mergeOverride), entity.ToKey);
            return query.CommonMerge(entity, entity.Key, cql, mergeOverride, onMatchOverride, onCreateOverride);
        }

        private static ICypherFluentQuery CommonMerge<T>(this ICypherFluentQuery query, T entity, string key, string cql, List<CypherProperty> mergeOverride = null, List<CypherProperty> onMatchOverride = null, List<CypherProperty> onCreateOverride = null) where T : class
        {
            //A merge requires the properties of both merge, create and match in the cutdown object
            var merge = mergeOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMergeAttribute>();
            var create = onCreateOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMergeOnCreateAttribute>();
            var match = onMatchOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMergeOnMatchAttribute>();
            var compare = new CypherPropertyComparer();
            var propertyOverride = create.Union(match.Union(merge.Union(create, compare), compare), compare).ToList();

            dynamic cutdown = entity.CreateDynamic(propertyOverride);
            var setOnAction = new Action<List<CypherProperty>,ICypherFluentQuery,  Action<ICypherFluentQuery, string>>((list,q, action) => {
                var set = string.Join(",", list.Select(x => string.Format("{0}.{1}={{{0}}}.{1}", key, x.JsonName)));
                if (!string.IsNullOrEmpty(set))
                {
                    action(q, set);
                }
            });

            query = query.Merge(cql);

            setOnAction(match, query, (q, s) => query = q.OnMatch().Set(s));
            setOnAction(create, query, (q, s) => query = q.OnCreate().Set(s));

            return query.WithParam(key, cutdown);
        }

        public static List<CypherProperty> UseProperties<T>(this T entity, params Expression<Func<T, object>>[] properties)
            where T : class
        {
            return entity.UseProperties(DefaultExtensionContext, properties);
        }

        public static List<CypherProperty> UseProperties<T>(this T entity, CypherExtensionContext context, params Expression<Func<T, object>>[] properties)
            where T : class
        {
            //Cache the T entity properties into a dictionary of strings
            if (properties != null)
            {
                return properties.ToList().Where(x => x != null).Select(x =>
                {
                    var memberExpression = x.Body as MemberExpression ?? ((UnaryExpression) x.Body).Operand as MemberExpression;
                    return memberExpression == null ? null : memberExpression.Member.Name;
                }).Select(x => new CypherProperty {TypeName = x, JsonName = x.ApplyCasing(context)}).ToList();
            }
            return new List<CypherProperty>();
        }

        public static string GetFormattedDebugText(this ICypherFluentQuery query)
        {
            var regex = new Regex("\\\"([^(\\\")\"]+)\\\":", RegexOptions.Multiline);
            return regex.Replace(query.Query.DebugQueryText, "$1:");
        }

        public static string ApplyCasing(this string value, ICypherExtensionContext context)
        {
            var camelCase = (context.JsonContractResolver is CamelCasePropertyNamesContractResolver);
            return camelCase ? string.Format("{0}{1}", value.Substring(0, 1).ToLowerInvariant(), value.Length > 1 ? value.Substring(1, value.Length - 1) : string.Empty)
                                : value;
        }
    }
}
