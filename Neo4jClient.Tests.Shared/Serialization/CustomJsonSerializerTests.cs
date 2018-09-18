﻿using System;
using System.Globalization;
using FluentAssertions;
using Neo4jClient.Serialization;
using Neo4jClient.Test.Fixtures;
using Newtonsoft.Json;
using Xunit;

namespace Neo4jClient.Test.Serialization
{
    public class CustomJsonSerializerTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Theory]
        [InlineData("2012-08-31T00:11:00.3642578")]
        [InlineData("2012-08-31T00:11:00Z")]
        public void ShouldSerializeDateTimeInCorrectStringFormat(string dateTimeStr)
        {
            //Arrange
            var serializer = new CustomJsonSerializer {JsonConverters = GraphClient.DefaultJsonConverters};
            var model = new DateModel
            {
                DateTime = DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                DateTimeNullable = DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
            };

            //Act
            var actual = serializer.Serialize(model);

            //Assert
            var expected =
                "{\r\n  \"DateTime\": \"" + dateTimeStr + "\",\r\n  \"DateTimeNullable\": \"" + dateTimeStr + "\"\r\n}";
            Assert.Equal(expected, actual);
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

        public class DateOffsetModel
        {
            public DateTimeOffset DateTime { get; set; }
            public DateTimeOffset? DateTimeNullable { get; set; }
        }

        public class DateModel
        {
            public DateTime DateTime { get; set; }
            public DateTime? DateTimeNullable { get; set; }
        }

        public enum Gender
        {
            Male,
            Female,
            Unknown
        }

        public class TestFoo
        {
            public Gender Gender { get; set; }
            public Gender? GenderNullable { get; set; }
        }

        public class NodeWithBuiltInTypes
        {
            public int? Foo { get; set; }
            public bool? Bar { get; set; }
        }

        [Fact]
        public void JsonSerializerShouldNotSerializeNullProperties()
        {
            // Arrange
            var testNode = new TestNode {Foo = "foo", Bar = null};
            var serializer = new CustomJsonSerializer
            {
                NullHandling = NullValueHandling.Ignore,
                JsonConverters = GraphClient.DefaultJsonConverters
            };

            // Act
            var result = serializer.Serialize(testNode);

            const string expectedValue = "{\r\n  \"Foo\": \"foo\"\r\n}";

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void JsonSerializerShouldSerializeAllProperties()
        {
            // Arrange
            var testNode = new TestNode {Foo = "foo", Bar = "bar"};
            var serializer = new CustomJsonSerializer
            {
                NullHandling = NullValueHandling.Ignore,
                JsonConverters = GraphClient.DefaultJsonConverters
            };

            // Act
            var result = serializer.Serialize(testNode);
            const string expectedValue = "{\r\n  \"Foo\": \"foo\",\r\n  \"Bar\": \"bar\"\r\n}";

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void JsonSerializerShouldSerializeEnumToString()
        {
            // Arrange
            var testNode = new TestNodeWithEnum {Status = TestEnum.Value1};
            var serializer = new CustomJsonSerializer
            {
                NullHandling = NullValueHandling.Ignore,
                JsonConverters = new[] {new EnumValueConverter()}
            };

            // Act
            var result = serializer.Serialize(testNode);

            const string expectedValue = "{\r\n  \"Status\": \"Value1\"\r\n}";

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void JsonSerializerShouldSerializeTimeZoneInfo()
        {
            // Arrange
            var serializer = new CustomJsonSerializer
            {
                JsonConverters = GraphClient.DefaultJsonConverters
            };

            const string ausEasternStandardTime = "AUS Eastern Standard Time";
            var timeZoneData = TimeZoneInfo.FindSystemTimeZoneById(ausEasternStandardTime);

            // Act
            var result = serializer.Serialize(timeZoneData);

            // Assert
            Assert.Equal(ausEasternStandardTime, result.Replace("\"", ""));
        }

        [Fact]
        public void JsonSerializerWithEnumConverterShouldConvertEnumToStringValues()
        {
            // Arrange
            var testClass = new TestFoo
            {
                Gender = Gender.Female,
                GenderNullable = Gender.Male
            };

            var serializer = new CustomJsonSerializer
            {
                JsonConverters = new JsonConverter[]
                {
                    new EnumValueConverter(),
                    new NullableEnumValueConverter()
                }
            };

            const string expected = "{\r\n  \"Gender\": \"Female\",\r\n  \"GenderNullable\": \"Male\"\r\n}";

            // Act
            var result = serializer.Serialize(testClass);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SerializeTimeSpan()
        {
            // Arrange
            var serializer = new CustomJsonSerializer {JsonConverters = GraphClient.DefaultJsonConverters};
            var value = new TimeSpan(400, 13, 3, 2, 10);
            var model = new TimeSpanModel
            {
                Foo = value
            };

            // Act
            var result = serializer.Serialize(model);

            const string expected = "{\r\n  \"Foo\": \"400.13:03:02.0100000\"\r\n}";

            result.Should().Be(expected);
        }

        [Fact]
        public void ShouldSerializeDateTimeOffsetInCorrectStringFormat()
        {
            //Arrange
            var serializer = new CustomJsonSerializer {JsonConverters = GraphClient.DefaultJsonConverters};
            var model = new DateOffsetModel
            {
                DateTime = DateTimeOffset.Parse("2012-08-31T00:11:00.3642578+10:00"),
                DateTimeNullable = DateTimeOffset.Parse("2012-08-31T00:11:00.3642578+10:00")
            };

            //Act
            var actual = serializer.Serialize(model);

            //Assert
            const string expected =
                "{\r\n  \"DateTime\": \"2012-08-31T00:11:00.3642578+10:00\",\r\n  \"DateTimeNullable\": \"2012-08-31T00:11:00.3642578+10:00\"\r\n}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldSerializeNullableBoolToJsonBooleanUsingDefaultJsonConverters()
        {
            // Arrange
            var testNode = new NodeWithBuiltInTypes {Bar = true};
            var serializer = new CustomJsonSerializer
            {
                NullHandling = NullValueHandling.Ignore,
                JsonConverters = GraphClient.DefaultJsonConverters
            };

            // Act
            var result = serializer.Serialize(testNode);
            const string expectedValue = "{\r\n  \"Bar\": true\r\n}";

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void ShouldSerializeNullableInt32ToJsonNumberUsingDefaultJsonConverters()
        {
            // Arrange
            var testNode = new NodeWithBuiltInTypes {Foo = 123};
            var serializer = new CustomJsonSerializer
            {
                NullHandling = NullValueHandling.Ignore,
                JsonConverters = GraphClient.DefaultJsonConverters
            };

            // Act
            var result = serializer.Serialize(testNode);
            const string expectedValue = "{\r\n  \"Foo\": 123\r\n}";

            // Assert
            Assert.Equal(expectedValue, result);
        }
    }
}