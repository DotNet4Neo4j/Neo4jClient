using System;
using NUnit.Framework;
using Neo4jClient.Serializer;
using Newtonsoft.Json;

namespace Neo4jClient.Test.Serializer
{
    [TestFixture]
    public class CustomJsonSerializerTests
    {
        [Test]
        public void SerializeTimeZoneInfo()
        {
            // Arrange
            var serializer = new CustomJsonSerializer();
            const string ausEasternStandardTime = "AUS Eastern Standard Time";
            var timeZoneData = TimeZoneInfo.FindSystemTimeZoneById(ausEasternStandardTime);

            // Act
            var result = serializer.Serialize(timeZoneData);

            // Assert
            Assert.AreEqual(ausEasternStandardTime, result.Replace("\"",""));
        }

        [Test]
        public void SerializeTimeSpan()
        {
            // Arrange
            var serializer = new CustomJsonSerializer();
            var value = new TimeSpan(400, 13, 3, 2,10);
            var model = new TimeSpanModel
                {
                    Foo = value
                };

            // Act
            var result = serializer.Serialize(model.Foo);

            // Assert
            Assert.AreEqual("400.13:03:02.0100000", result.Replace("\"", ""));
        }

        [Test]
        public void ExecuteShouldJsonSerializeAllProperties()
        {
            // Arrange
            var testNode = new TestNode { Foo = "foo", Bar = "bar" };
            var serializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore };

            // Act
            var result = serializer.Serialize(testNode);
            const string expectedValue = "{\r\n  \"Foo\": \"foo\",\r\n  \"Bar\": \"bar\"\r\n}";

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [Test]
        public void ExecuteShouldNotJSonSerializeNullProperties()
        {
            // Arrange
            var testNode = new TestNode { Foo = "foo", Bar = null };
            var serializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore };

            // Act
            var result = serializer.Serialize(testNode);

            const string expectedValue = "{\r\n  \"Foo\": \"foo\"\r\n}";

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [Test]
        public void ExecuteShouldSerializeEnumTypesToString()
        {
            // Arrange
            var testNode = new TestNodeWithEnum { Status = TestEnum.Value1 };
            var serializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore };

            // Act
            var result = serializer.Serialize(testNode);

            const string expectedValue = "{\r\n  \"Status\": \"Value1\"\r\n}";

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        public class TestNode
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        public class TestNodeWithEnum
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public TestEnum Status { get; set; }
        }

        public enum TestEnum
        {
            Value1,
            Value2
        }

        public class TimeZoneModel
        {
            public TimeZoneInfo Foo { get; set; }
        }

        public class TimeSpanModel
        {
            public TimeSpan Foo { get; set; }
		}

        public enum Gender{Male, Female, Unknown}

        public class TestFoo
        {
            public Gender Gender { get; set; }
            public Gender? GenderNullable { get; set; }
        }

        [Test]
        public void SerializeEnumToStringValues()
        {
            // Arrange
            var testClass = new TestFoo
                {
                    Gender = Gender.Female,
                    GenderNullable = Gender.Male
                };

            var serializer = new CustomJsonSerializer();
            const string expected = "{\r\n  \"Gender\": \"Female\",\r\n  \"GenderNullable\": \"Male\"\r\n}";

            // Act
            var result = serializer.Serialize(testClass);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}
