using KingsleyStephens.Core;
using KingsleyStephens.CoreInternal;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using KCore = KingsleyStephens.Core;


public class Test_SingletonMonoBehaviourFails
{
    public class TestSingleton : SingletonMonoBehaviour<TestSingleton>
    {
        public bool Test()
        {
            return true;
        }
    }

    // Basic
    [Test]
    public void Test_SingletonMonoBehaviourDefaultsFails()
    {
        Assert.IsNotNull(TestSingleton.Instance);
    }

}
