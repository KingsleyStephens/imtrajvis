using System.Collections;
using KingsleyStephens.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Test_VariableTimerRuntime
{
    private void OnUpdate(float t)
    {
        Debug.Log("OnUpdate");
    }


    // Basic
    [UnityTest]
    public IEnumerator Test_VariableTimerDefaultsPasses()
    {
        Assert.IsNotNull(TimeKeeper.InitSingleton());

        VariableTimer timer = TimeKeeper.Instance.NewTimer<VariableTimer>();
        Assert.IsNotNull(timer);

        Assert.IsFalse(timer.IsEnabled);
        Assert.IsTrue(timer.IsPaused);
        Assert.AreEqual(timer.Duration, -1.0f, 0.001f);
        Assert.AreEqual(timer.MinTime, 0.0f, 0.001f);

        timer.OnStart += (x) => Debug.Log("OnStart");
        timer.OnStop += (x) => Debug.Log("OnStop");
        timer.OnUpdate += OnUpdate;
        timer.OnMinTimeBound += (x) => Debug.Log("OnMinTimeBound");
        timer.OnMaxTimeBound += (x) => Debug.Log("OnMaxTimeBound");
        timer.OnPause += (x) => Debug.Log("OnPause");
        timer.OnUnPause += (x) => Debug.Log("OnUnPause");

        timer.Start();

        Assert.IsTrue(timer.IsEnabled);
        Assert.IsFalse(timer.IsPaused);
        Assert.AreEqual(0.0f, timer.MinTime, 0.01f);

        LogAssert.Expect(LogType.Log, "OnStart");

        yield return null;

        Assert.Greater(timer.Time, 0.0f);
        Assert.Greater(timer.TimeInterval, 0.0f);
        Assert.AreEqual(-1.0f, timer.NormalisedTime, 0.01f);

        LogAssert.Expect(LogType.Log, "OnUpdate");

        timer.Stop();

        LogAssert.Expect(LogType.Log, "OnStop");

        Assert.IsFalse(timer.IsEnabled);
        Assert.IsTrue(timer.IsPaused);

        timer.Duration = 1.0f;

        Assert.AreEqual(1.0f, timer.Duration, 0.001f);

        timer.Start();

        Assert.IsTrue(timer.IsEnabled);
        Assert.IsFalse(timer.IsPaused);
        Assert.AreEqual(0.0f, timer.MinTime, 0.01f);

        LogAssert.Expect(LogType.Log, "OnStart");

        timer.OnUpdate -= OnUpdate;

        yield return null;

        LogAssert.NoUnexpectedReceived();

        Assert.Greater(timer.Time, 0.0f);
        Assert.Greater(timer.TimeInterval, 0.0f);
        Assert.Greater(timer.NormalisedTime, 0.0f);

        yield return new WaitForSeconds(1.0f);

        LogAssert.Expect(LogType.Log, "OnMaxTimeBound");
        LogAssert.Expect(LogType.Log, "OnPause");

        Assert.AreEqual(1.0f, timer.Time, 0.01f);
        Assert.AreEqual(1.0f, timer.TimeInterval, 0.01f);
        Assert.AreEqual(1.0f, timer.NormalisedTime, 0.01f);

        Assert.IsTrue(timer.IsPaused);
        Assert.IsTrue(timer.IsEnabled);

        timer.Time = 0.5f;
        timer.Speed = -1.0f;

        Assert.AreEqual(0.5f, timer.Time, 0.01f);
        Assert.AreEqual(0.5f, timer.TimeInterval, 0.01f);
        Assert.AreEqual(0.5f, timer.NormalisedTime, 0.01f);

        timer.Pause(false);

        LogAssert.Expect(LogType.Log, "OnUnPause");

        yield return null;

        Assert.Less(timer.Time, 0.5f);
        Assert.Less(timer.TimeInterval, 0.5f);
        Assert.Less(timer.NormalisedTime, 0.5f);

        yield return new WaitForSeconds(0.5f);

        LogAssert.Expect(LogType.Log, "OnMinTimeBound");
        LogAssert.Expect(LogType.Log, "OnPause");

        // Test destroy
        timer.Speed = 1.0f;

        timer.Pause(false);
        LogAssert.Expect(LogType.Log, "OnUnPause");

        timer.OnUpdate += OnUpdate;

        TimeKeeper.Instance.DestroyTimer(timer);
        timer = null;
        yield return null;

        LogAssert.NoUnexpectedReceived();
    }
}
