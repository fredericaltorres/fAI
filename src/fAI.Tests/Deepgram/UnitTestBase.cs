namespace fAI.Tests
{
    public class UnitTestBase
    {
        protected string FlexStrCompare(string s)
        {
            return s.ToLowerInvariant().Replace(",", ".");
        }
    }
}