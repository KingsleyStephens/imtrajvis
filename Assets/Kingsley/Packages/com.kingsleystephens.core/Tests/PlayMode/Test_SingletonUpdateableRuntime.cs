using System.Collections;
using KingsleyStephens.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Test_SingletonUpdateableRuntime
{
    public class TestSingleton : SingletonUpdateable<TestSingleton>
    {
        public bool Test()
        {
            return true;
        }

        protected override void DoUpdate(TimeSnapshot time)
        {
            Debug.Log("Update");
        }
    }

    // Basic
    [UnityTest]
    public IEnumerator Test_SingletonUpdateableDefaultsPasses()
    {
        Assert.IsNotNull(TestSingleton.InitSingleton());
        Assert.IsTrue(TestSingleton.Instance.Test());
        
        yield return null;
        
        LogAssert.Expect(LogType.Log, "Update");

        TestSingleton.DestroySingleton();
        
        yield return null;

        LogAssert.NoUnexpectedReceived();


        Assert.IsNotNull(TestSingleton.Instance);

        yield return null;

        LogAssert.Expect(LogType.Log, "Update");

        TestSingleton.DestroySingleton();

        yield return null;

        LogAssert.NoUnexpectedReceived();
    }
}
