using System;
using System.Reflection;
using Xunit.Sdk;

namespace fAI.Tests
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TestBeforeAfter : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            System.Threading.Thread.Sleep(1000*2);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            //Debug.WriteLine(methodUnderTest.Name);
        }
    }
}