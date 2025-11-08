using KingsleyStephens.Core;
using KingsleyStephens.CoreInternal;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using KCore = KingsleyStephens.Core;


public class Test_SingletonMonoBehaviourPasses
{
    public class TestSingleton : SingletonMonoBehaviour<TestSingleton>
    {
        public bool Test()
        {
            return true;
        }
    }

    private GameObject _singleton;

    [SetUp]
    public void SetUp()
    {
        _singleton = new GameObject("Singleton");
        _singleton.AddComponent<TestSingleton>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_singleton);
    }

    // Basic
    [Test]
    public void Test_SingletonMonoBehaviourDefaultsPasses()
    {
        Assert.IsNotNull(TestSingleton.Instance);

        Assert.IsTrue(TestSingleton.Instance.Test());
    }
}
