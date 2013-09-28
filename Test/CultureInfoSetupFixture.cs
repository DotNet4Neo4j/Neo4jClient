using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace Neo4jClient.Test
{
    [SetUpFixture]
    public class CultureInfoSetupFixture
    {
        [SetUp]
        public void SetCultureToSomethingNonLatinToEnsureCodeUnderTestDoesntAssumeEnAu()
        {
            // Issue https://bitbucket.org/Readify/neo4jclient/issue/15/take-cultureinfo-into-account-for-proper
            var thread = Thread.CurrentThread;
            thread.CurrentCulture = thread.CurrentUICulture = new CultureInfo("zh-CN");
        }
    }
}
