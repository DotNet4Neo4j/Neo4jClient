﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Serialization.Json
{
    public class CustomJsonDeserializer
    {
        private readonly IEnumerable<JsonConverter> jsonConverters;
        private readonly CultureInfo culture;
        private readonly DefaultContractResolver jsonResolver;

        public CustomJsonDeserializer(IEnumerable<JsonConverter> jsonConverters) : this(jsonConverters, null)
        {
        }

        public CustomJsonDeserializer(IEnumerable<JsonConverter> jsonConverters, CultureInfo cultureInfo = null,
            DefaultContractResolver resolver = null)
        {
            this.jsonConverters = jsonConverters;
            culture = cultureInfo ?? CultureInfo.InvariantCulture;
            jsonResolver = resolver ?? GraphClient.DefaultJsonContractResolver;
        }

        public T Deserialize<T>(string contents) where T : new()
        {
            var context = new DeserializationContext<JsonConverter>
            {
                Culture = culture,
                JsonContractResolver = jsonResolver,
                Converters = jsonConverters.ToArray(),
                TypeMappings = new TypeMapping[0]
            };

            contents = DateDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(contents);

            var reader = new JsonTextReader(new StringReader(contents))
            {
                DateParseHandling = DateParseHandling.None
            };
            
            var json = JToken.ReadFrom(reader);
            var root = json.Root;
            
            var deserializer = new CypherJsonDeserializer<T>(null, CypherResultMode.Set,
                CypherResultFormat.DependsOnEnvironment);

            return deserializer.DeserializeObject(context, root);
        }
    }


    public class SingleObjectJsonDeserializer<TResult> : CypherJsonDeserializer<TResult>
    {
        readonly IEnumerable<JsonConverter> jsonConverters;
        readonly CultureInfo culture;
        readonly DefaultContractResolver jsonResolver;

        public SingleObjectJsonDeserializer(IEnumerable<JsonConverter> jsonConverters) : this(jsonConverters, null)
        {
        }

        public SingleObjectJsonDeserializer(IEnumerable<JsonConverter> jsonConverters, CultureInfo cultureInfo = null, DefaultContractResolver resolver = null)
            : base(null, CypherResultMode.Set, CypherResultFormat.Transactional, false)
        {
            this.jsonConverters = jsonConverters;
            culture = cultureInfo ?? CultureInfo.InvariantCulture;
            jsonResolver = resolver ?? GraphClient.DefaultJsonContractResolver;
        }

        protected override DeserializationContext<JsonConverter> GenerateContext(JToken results, CypherResultMode resultMode)
        {
            var context = new DeserializationContext<JsonConverter>
            {
                Culture = culture,
                JsonContractResolver = jsonResolver,
                Converters = jsonConverters.ToArray(),
                TypeMappings = new TypeMapping[0]

            };

            return context;
        }

        protected override string[] GetColumnNames(JToken resultRoot)
        {
            return new string[0];
        }

        protected override IEnumerable<JToken> GetRecordsFromResults(JToken root)
        {
            return root;
        }

        protected override IEnumerable<FieldEntry> GetFieldEntries(string[] columnNames, JToken row)
        {
            yield return new FieldEntry("", row);
        }

        protected override JToken GetElementForDeserializationInSingleColumn(JToken record)
        {
            return record;
        }
    }
}
