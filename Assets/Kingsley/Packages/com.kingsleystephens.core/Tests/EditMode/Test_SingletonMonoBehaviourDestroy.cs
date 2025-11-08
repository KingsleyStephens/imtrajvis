using KingsleyStephens.Core;
using KingsleyStephens.CoreInternal;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using KCore = KingsleyStephens.Core;


public class Test_SingletonMonoBehaviourDestroy
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
        if (_singleton != null)
        {
            Object.DestroyImmediate(_singleton);
        }
    }

    // Basic
    [Test]
    public void Test_SingletonMonoBehaviourDestroyPasses()
    {
        Assert.IsNotNull(TestSingleton.Instance);

        TestSingleton.DestroySingleton();
        _singleton = null;
    }

}
