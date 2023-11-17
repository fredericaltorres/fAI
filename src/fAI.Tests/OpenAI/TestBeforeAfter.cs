using System;
using System.Reflection;
using Xunit.Sdk;

namespace fAI.Tests
{
    // https://hamidmosalla.com/2018/08/30/xunit-beforeaftertestattribute-how-to-run-code-before-and-after-test/
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TestBeforeAfter : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            System.Threading.Thread.Sleep(1000*6);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            //Debug.WriteLine(methodUnderTest.Name);
        }
    }
}