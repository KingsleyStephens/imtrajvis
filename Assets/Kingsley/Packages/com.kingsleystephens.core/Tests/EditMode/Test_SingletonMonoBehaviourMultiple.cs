using KingsleyStephens.Core;
using KingsleyStephens.CoreInternal;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using KCore = KingsleyStephens.Core;


public class Test_SingletonMonoBehaviourMultiple
{
    public class TestSingleton : SingletonMonoBehaviour<TestSingleton>
    {
        public bool Test()
        {
            return true;
        }
    }

    private GameObject _singleton1;
    private GameObject _singleton2;

    [SetUp]
    public void SetUp()
    {
        _singleton1 = new GameObject("Singleton1");
        _singleton1.AddComponent<TestSingleton>();
        _singleton2 = new GameObject("Singleton2");
        _singleton2.AddComponent<TestSingleton>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_singleton1);
        Object.DestroyImmediate(_singleton2);
    }

    // Basic
    [Test]
    public void Test_SingletonMonoBehaviourMultiplePasses()
    {
        Assert.IsNull(TestSingleton.Instance);
        LogAssert.Expect(LogType.Error, "Multiple singleton objects found " + typeof(TestSingleton));
    }
}
