using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Neo4jClient.Cypher.EntityExtension.Attributes;


namespace Neo4jClient.Cypher.EntityExtension
{
    public class CypherTypeItemHelper
    {
        private readonly ConcurrentDictionary<CypherTypeItem, List<CypherProperty>> _typeProperties = new ConcurrentDictionary<CypherTypeItem, List<CypherProperty>>();

        public CypherTypeItem AddKeyAttribute<TEntity, TAttr>(ICypherExtensionContext context)
            where TAttr : CypherExtensionAttribute
            where TEntity : class
        {
            var type = typeof(TEntity);
            var key = new CypherTypeItem { Type = type, AttributeType = typeof(TAttr) };
            //check cache
            if (!_typeProperties.ContainsKey(key))
            {
                //strip off properties create map for usage
                _typeProperties.AddOrUpdate(key, type.GetProperties().Where(x => x.GetCustomAttributes(typeof(TAttr),true).Any())
                    .Select(x => new CypherProperty {TypeName = x.Name, JsonName = x.Name.ApplyCasing(context)})
                    .ToList(), (k, e) => e);
            }
            return key;
        }

        public List<CypherProperty> PropertiesForPurpose<TEntity, TAttr>(ICypherExtensionContext context)
            where TEntity : class
            where TAttr : CypherExtensionAttribute
        {
            var key = AddKeyAttribute<TEntity, TAttr>(context);
            return _typeProperties[key];
        }

        public List<CypherProperty> PropertiesForPurpose<TEntity, TAttr>()
            where TEntity : class
            where TAttr : CypherExtensionAttribute
        {
            return PropertiesForPurpose<TEntity, TAttr>(CypherExtension.DefaultExtensionContext);
        }
    }
}
