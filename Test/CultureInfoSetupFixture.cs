using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace Neo4jClient.Test
{
    [SetUpFixture]
    public class CultureInfoSetupFixture
    {
        [SetUp]
        public void SetCultureToSomethingNonLatinToEnsureCodeUnderTestDoesntAssumeEnAU()
        {
            // Issue http://hg.readify.net/neo4jclient/issue/15/take-cultureinfo-into-account-for-proper
            var thread = Thread.CurrentThread;
            thread.CurrentCulture = thread.CurrentUICulture = new CultureInfo("zh-CN");
        }
    }
}
