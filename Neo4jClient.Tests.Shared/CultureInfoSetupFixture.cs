using System.Globalization;
using System.Threading;

namespace Neo4jClient.Test.Fixtures
{
    public class CultureInfoSetupFixture
    {
        //SetCultureToSomethingNonLatinToEnsureCodeUnderTestDoesntAssumeEnAu()
        public CultureInfoSetupFixture()
        {
            // Issue https://bitbucket.org/Readify/neo4jclient/issue/15/take-cultureinfo-into-account-for-proper

            // The idea is to minimize developer mistake by surprising culture-info assumptions. This may not be the best setup for culture-dependent
            // tests. The alternative of introducing test base class is deliberately not taken because deriving from it is another assumption by itself.
            var thread = Thread.CurrentThread;
            thread.CurrentCulture = thread.CurrentUICulture = new CultureInfo("zh-CN");
        }
    }
}
