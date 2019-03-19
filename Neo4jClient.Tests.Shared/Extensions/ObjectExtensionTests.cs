using System.Collections.Generic;
using FluentAssertions;
using Neo4jClient.Extensions;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test.Extensions
{
    public class ObjectExtensionTests
    {
        public class InMethod : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void ReturnsTrue_WhenItemIsInList()
            {
                var list = new List<int> {1, 2, 3, 4};
                2.In(list).Should().BeTrue();
            }

            [Fact]
            public void ReturnsFalse_WhenItemNotInList()
            {
                var list = new List<int> { 1, 2, 3, 4 };
                5.In(list).Should().BeFalse();
            }
        }

        public class NotInMethod : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void ReturnsTrue_WhenItemIsNotInList()
            {
                var list = new List<int> { 1, 2, 3, 4 };
                5.NotIn(list).Should().BeTrue();
            }

            [Fact]
            public void ReturnsTrue_WhenItemIsNotInList2()
            {
                5.NotIn(new [] {1,2,3,4}).Should().BeTrue();
            }

            [Fact]
            public void ReturnsFalse_WhenItemInList()
            {
                var list = new List<int> { 1, 2, 3, 4 };
                4.NotIn(list).Should().BeFalse();
            }
        }
    }
}