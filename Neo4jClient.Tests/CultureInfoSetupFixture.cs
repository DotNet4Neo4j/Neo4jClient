using System.Globalization;
using System.Threading;

namespace Neo4jClient.Tests
{
    public class CultureInfoSetupFixture
    {
        //SetCultureToSomethingNonLatinToEnsureCodeUnderTestDoesntAssumeEnAu()
        public static readonly CultureInfo CultureInfo = new CultureInfo("zh-CN");

        public static void SetDeterministicCulture()
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo;
        }

        public CultureInfoSetupFixture()
        {
            // Issue https://bitbucket.org/Readify/neo4jclient/issue/15/take-cultureinfo-into-account-for-proper

            // The idea is to minimize developer mistake by surprising culture-info assumptions. This may not be the best setup for culture-dependent
            // tests. The alternative of introducing test base class is deliberately not taken because deriving from it is another assumption by itself.
            SetDeterministicCulture();
        }
    }
}
