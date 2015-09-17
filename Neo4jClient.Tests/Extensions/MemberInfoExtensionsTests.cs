using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Neo4jClient.Extensions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Neo4jClient.Test.Extensions
{
    [TestFixture]
    public class MemberInfoExtensionsTests
    {
        class Foo
        {
            [JsonProperty]
            public string Property1 { get; set; }

            [JsonProperty("property_2")]
            public string Property2 { get; set; }

            public string Property3 { get; set; }
        }

        [TestFixture]
        public class GetNameUsingJsonPropertyMethod
        {
            [Test, ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsArgumentNullException_WhenMemberInfoIsNull()
            {
                MemberInfoExtensions.GetNameUsingJsonProperty(null);
            }

            [Test]
            public void ReturnsCorrectName_WhenPropertyHasJsonPropertyAddedButNoNameSet()
            {
                const string expected = "Property1";

                var member = typeof (Foo).GetMember("Property1");
                var actual = member.Single().GetNameUsingJsonProperty();

                Assert.AreEqual(expected, actual);
            }

            [Test]
            public void ReturnsCorrectName_WhenPropertyHasJsonPropertyAddedWithNameSet()
            {
                const string expected = "property_2";

                var member = typeof(Foo).GetMember("Property2");
                var actual = member.Single().GetNameUsingJsonProperty();

                Assert.AreEqual(expected, actual);
            }

            [Test]
            public void ReturnsCorrectName_WhenPropertyHasNoJsonPropertyAdded()
            {
                const string expected = "Property3";

                var member = typeof(Foo).GetMember("Property3");
                var actual = member.Single().GetNameUsingJsonProperty();

                Assert.AreEqual(expected, actual);
            }

        }
    }
}