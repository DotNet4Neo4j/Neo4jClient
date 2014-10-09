using System;
using System.Collections.Generic;
using Neo4jClient.Cypher;
using Neo4jClient.Cypher.EntityExtension;
using Neo4jClient.Cypher.EntityExtension.Attributes;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test.Cypher.EntityExtension
{
    public class CypherExtensionTestHelper
    {

        public IRawGraphClient GraphClient { get; private set; }
        public CypherExtensionContext CypherExtensionContext { get; private set; }
        public CypherFluentQuery Query { get; private set; }

        public CypherExtensionTestHelper()
        {
            CypherExtensionContext = new CypherExtensionContext();
        }

        public CypherExtensionTestHelper SetupGraphClient()
        {
            GraphClient = Substitute.For<IRawGraphClient>();
            //GraphClient.Setup(x => x.JsonContractResolver).Returns(new CamelCasePropertyNamesContractResolver());
            Query = new CypherFluentQuery(GraphClient);
            return this;
        }
    }

    [TestFixture]
    public class CypherExtensionTests
    {
        [Test]
        public void ToCypherStringMergeTest()
        {
            //setup
            var model = CreateModel();
            var helper = new CypherExtensionTestHelper();

            //act
            var result = model.ToCypherString<CypherModel, CypherMergeAttribute>(helper.CypherExtensionContext);
            var result2 = model.ToCypherString<CypherModel, CypherMergeAttribute>(helper.CypherExtensionContext);

            //assert
            Assert.AreEqual("cyphermodel:CypherModel {id:{cyphermodel}.id}", result);
            Assert.AreEqual(result,result2);
        }

        [Test]
        public void MatchEntityTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = CreateModel();
            model.id = Guid.Parse("9aa1343f-18a4-41a6-a414-34b7df62c919");
            //act
            var q = helper.Query.MatchEntity(model).Return(cyphermodel => cyphermodel.As<CypherModel>());
            
            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MATCH (cyphermodel:CypherModel {id:{cyphermodel}.id})\r\nRETURN cyphermodel", q.Query.QueryText);
        }

        [Test]
        public void MatchEntityOverrideTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = CreateModel();
            
            //act
            var q = helper.Query
                            .MatchEntity(model, propertyOverride: model.UseProperties(x => x.firstName, x => x.isLegend))
                            .Return(cyphermodel => cyphermodel.As<CypherModel>());

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MATCH (cyphermodel:CypherModel {firstName:{cyphermodel}.firstName,isLegend:{cyphermodel}.isLegend})\r\nRETURN cyphermodel", q.Query.QueryText);
        }

        [Test]
        public void MatchEntityKeyTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = CreateModel();
            
            //act
            var q = helper.Query.MatchEntity(model,"key").Return(cyphermodel => cyphermodel.As<CypherModel>());

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MATCH (key:CypherModel {id:{key}.id})\r\nRETURN cyphermodel", q.Query.QueryText);
        }

        [Test]
        public void MatchEntityPreTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = CreateModel();

            //act
            var q = helper.Query.MatchEntity(model, preCql: "(a:Node)-->").Return(cyphermodel => cyphermodel.As<CypherModel>());

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MATCH (a:Node)-->(cyphermodel:CypherModel {id:{cyphermodel}.id})\r\nRETURN cyphermodel", q.Query.QueryText);
        }

        [Test]
        public void MatchEntityPostTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = CreateModel();

            //act
            var q = helper.Query.MatchEntity(model, postCql: "<--(a:Node)").Return(cyphermodel => cyphermodel.As<CypherModel>());

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MATCH (cyphermodel:CypherModel {id:{cyphermodel}.id})<--(a:Node)\r\nRETURN cyphermodel", q.Query.QueryText);
        }

        [Test]
        public void MatchEntityPrePostKeyOverrideTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = CreateModel();

            //act
            var q = helper.Query.MatchEntity(model, "key",  "(a:Node)-->", "<--(b:Node)", new List<CypherProperty>()).Return(cyphermodel => cyphermodel.As<CypherModel>());

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MATCH (a:Node)-->(key:CypherModel {})<--(b:Node)\r\nRETURN cyphermodel", q.Query.QueryText);
        }

        [Test]
        public void MatchAllTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            //act
            var result = helper.Query.MatchEntity(new CypherModel(), propertyOverride: new List<CypherProperty>());

            //assert
            Assert.AreEqual("MATCH (cyphermodel:CypherModel {})", result.GetFormattedDebugText());
        }

        [Test]
        public void MergeEntityTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();
            var model = CreateModel();

            //act
            var q = helper.Query.MergeEntity(model);

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (cyphermodel:CypherModel {id:{cyphermodel}.id})\r\nON MATCH\r\nSET cyphermodel.isLegend={cyphermodel}.isLegend,cyphermodel.answerToTheMeaningOfLifeAndEverything={cyphermodel}.answerToTheMeaningOfLifeAndEverything\r\nON CREATE\r\nSET cyphermodel.firstName={cyphermodel}.firstName,cyphermodel.dateOfBirth={cyphermodel}.dateOfBirth,cyphermodel.isLegend={cyphermodel}.isLegend,cyphermodel.answerToTheMeaningOfLifeAndEverything={cyphermodel}.answerToTheMeaningOfLifeAndEverything", q.Query.QueryText);
        }

        [Test]
        public void MergeEntityKeyTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();
            var model = CreateModel();

            //act
            var q = helper.Query.MergeEntity(model,"key");

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (key:CypherModel {id:{key}.id})\r\nON MATCH\r\nSET key.isLegend={key}.isLegend,key.answerToTheMeaningOfLifeAndEverything={key}.answerToTheMeaningOfLifeAndEverything\r\nON CREATE\r\nSET key.firstName={key}.firstName,key.dateOfBirth={key}.dateOfBirth,key.isLegend={key}.isLegend,key.answerToTheMeaningOfLifeAndEverything={key}.answerToTheMeaningOfLifeAndEverything", q.Query.QueryText);
        }

        [Test]
        public void MergeEntityOverrideMergeTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();
            var model = CreateModel();

            //act
            var q = helper.Query.MergeEntity(model, mergeOverride:model.UseProperties(x => x.firstName));

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (cyphermodel:CypherModel {firstName:{cyphermodel}.firstName})\r\nON MATCH\r\nSET cyphermodel.isLegend={cyphermodel}.isLegend,cyphermodel.answerToTheMeaningOfLifeAndEverything={cyphermodel}.answerToTheMeaningOfLifeAndEverything\r\nON CREATE\r\nSET cyphermodel.firstName={cyphermodel}.firstName,cyphermodel.dateOfBirth={cyphermodel}.dateOfBirth,cyphermodel.isLegend={cyphermodel}.isLegend,cyphermodel.answerToTheMeaningOfLifeAndEverything={cyphermodel}.answerToTheMeaningOfLifeAndEverything", q.Query.QueryText);
        }

        [Test]
        public void MergeEntityOverrideOnMatchTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();
            var model = CreateModel();

            //act
            var q = helper.Query.MergeEntity(model, onMatchOverride: model.UseProperties(x => x.firstName));

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (cyphermodel:CypherModel {id:{cyphermodel}.id})\r\nON MATCH\r\nSET cyphermodel.firstName={cyphermodel}.firstName\r\nON CREATE\r\nSET cyphermodel.firstName={cyphermodel}.firstName,cyphermodel.dateOfBirth={cyphermodel}.dateOfBirth,cyphermodel.isLegend={cyphermodel}.isLegend,cyphermodel.answerToTheMeaningOfLifeAndEverything={cyphermodel}.answerToTheMeaningOfLifeAndEverything", q.Query.QueryText);
        }

        [Test]
        public void MergeEntityOverrideOnCreateTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();
            var model = CreateModel();

            //act
            var q = helper.Query.MergeEntity(model, onCreateOverride: model.UseProperties(x => x.firstName));

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (cyphermodel:CypherModel {id:{cyphermodel}.id})\r\nON MATCH\r\nSET cyphermodel.isLegend={cyphermodel}.isLegend,cyphermodel.answerToTheMeaningOfLifeAndEverything={cyphermodel}.answerToTheMeaningOfLifeAndEverything\r\nON CREATE\r\nSET cyphermodel.firstName={cyphermodel}.firstName", q.Query.QueryText);
        }

        [Test]
        public void MergeEntityAllArgsTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();
            var model = CreateModel();

            //act
            var q = helper.Query.MergeEntity(model,"key", new List<CypherProperty>(),new List<CypherProperty>(), new List<CypherProperty>(), "(a:Node)-->","<--(b:Node)");

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (a:Node)-->(key:CypherModel {})<--(b:Node)", q.Query.QueryText);
        }


        [Test]
        public void MergeRelationshipTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = new ComponentOf("from", "to");
            
            //act
            var q = helper.Query.MergeRelationship(model);

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (from)-[fromto:COMPONENT_OF {quantity:{fromto}.quantity,unitOfMeasure:{fromto}.unitOfMeasure,factor:{fromto}.factor,instructionText:{fromto}.instructionText}]->(to)\r\nON MATCH\r\nSET fromto.quantity={fromto}.quantity,fromto.unitOfMeasure={fromto}.unitOfMeasure,fromto.factor={fromto}.factor,fromto.instructionText={fromto}.instructionText", q.Query.QueryText);
        }

        [Test]
        public void MergeRelationshipDownCastTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = (BaseRelationship) new ComponentOf("from", "to");

            //act
            var q = helper.Query.MergeRelationship(model);

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (from)-[fromto:COMPONENT_OF {quantity:{fromto}.quantity,unitOfMeasure:{fromto}.unitOfMeasure,factor:{fromto}.factor,instructionText:{fromto}.instructionText}]->(to)\r\nON MATCH\r\nSET fromto.quantity={fromto}.quantity,fromto.unitOfMeasure={fromto}.unitOfMeasure,fromto.factor={fromto}.factor,fromto.instructionText={fromto}.instructionText", q.Query.QueryText);
        }

        [Test]
        public void MergeRelationshipMergeOverrideTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = new ComponentOf("from", "to");

            //act
            var q = helper.Query.MergeRelationship(model, model.UseProperties(x => x.quantity));

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (from)-[fromto:COMPONENT_OF {quantity:{fromto}.quantity}]->(to)\r\nON MATCH\r\nSET fromto.quantity={fromto}.quantity,fromto.unitOfMeasure={fromto}.unitOfMeasure,fromto.factor={fromto}.factor,fromto.instructionText={fromto}.instructionText", q.Query.QueryText);
        }

        [Test]
        public void MergeRelationshipOnMatchOverrideTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = new ComponentOf("from", "to");

            //act
            var q = helper.Query.MergeRelationship(model,onMatchOverride:model.UseProperties(x => x.quantity));

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (from)-[fromto:COMPONENT_OF {quantity:{fromto}.quantity,unitOfMeasure:{fromto}.unitOfMeasure,factor:{fromto}.factor,instructionText:{fromto}.instructionText}]->(to)\r\nON MATCH\r\nSET fromto.quantity={fromto}.quantity", q.Query.QueryText);
        }

        [Test]
        public void MergeRelationshipOnCreateOverrideTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = new ComponentOf("from", "to");

            //act
            var q = helper.Query.MergeRelationship(model, onCreateOverride: model.UseProperties(x => x.quantity));

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (from)-[fromto:COMPONENT_OF {quantity:{fromto}.quantity,unitOfMeasure:{fromto}.unitOfMeasure,factor:{fromto}.factor,instructionText:{fromto}.instructionText}]->(to)\r\nON MATCH\r\nSET fromto.quantity={fromto}.quantity,fromto.unitOfMeasure={fromto}.unitOfMeasure,fromto.factor={fromto}.factor,fromto.instructionText={fromto}.instructionText\r\nON CREATE\r\nSET fromto.quantity={fromto}.quantity", q.Query.QueryText);
        }

        [Test]
        public void MergeRelationshipAllArgsTest()
        {
            //setup
            var helper = new CypherExtensionTestHelper().SetupGraphClient();

            var model = new ComponentOf("from", "to");

            //act
            var q = helper.Query.MergeRelationship(model, new List<CypherProperty>(), new List<CypherProperty>(), new List<CypherProperty>());

            Console.WriteLine(q.GetFormattedDebugText());

            //assert
            Assert.AreEqual("MERGE (from)-[fromto:COMPONENT_OF {}]->(to)", q.Query.QueryText);
        }

        [Test]
        public void EntityLabelWithoutAttrTest()
        {
            //setup
            var model = CreateModel();

            //act
            var result = model.EntityLabel();

            //assert
            Assert.AreEqual("CypherModel", result);
        }

        [Test]
        public void EntityLabelWithTest()
        {
            //setup
            var model = new LabelledModel();

            //act
            var result = model.EntityLabel();

            //assert
            Assert.AreEqual("MyName", result);
        }

        private CypherModel CreateModel()
        {
            return new CypherModel
            {
                dateOfBirth = new DateTime(1981, 4, 1),
                answerToTheMeaningOfLifeAndEverything = 42,
                firstName = "Foo",
                isLegend = false
            };
        }

        public enum UnitsOfMeasure
        {
            Gram,
            Millimeter,
            Cup,
            TableSpoon,
            TeaSpoon,
            Unit
        }

        [CypherLabel(Name = "COMPONENT_OF")]
        public class ComponentOf : BaseRelationship
        {
            public ComponentOf(string from = null, string to = null): base(from, to)
            {
                instructionText = string.Empty;
            }
            [CypherMerge]
            [CypherMergeOnMatch]
            public double quantity { get; set; }
            [CypherMerge]
            [CypherMergeOnMatch]
            public UnitsOfMeasure unitOfMeasure { get; set; }
            [CypherMerge]
            [CypherMergeOnMatch]
            public int factor { get; set; }
            [CypherMerge]
            [CypherMergeOnMatch]
            public string instructionText { get; set; }
        }
    }
    [CypherLabel(Name = "MyName")]
    public class LabelledModel { }

    public class CypherModel
    {
        public CypherModel()
        {
            id = Guid.NewGuid();
        }

        [CypherMatch]
        [CypherMerge]
        public Guid id { get; set; }

        [CypherMergeOnCreate]
        public string firstName { get; set; }
        
        [CypherMergeOnCreate]
        public DateTimeOffset dateOfBirth { get; set; }
        
        [CypherMergeOnCreate]
        [CypherMergeOnMatch]
        public bool isLegend { get; set; }
        
        [CypherMergeOnCreate]
        [CypherMergeOnMatch]
        public int answerToTheMeaningOfLifeAndEverything { get; set; }
    }
}
