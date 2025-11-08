using System.Collections;
using KingsleyStephens.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Test_GameTimerRuntime
{
    private void OnUpdate(float t)
    {
        Debug.Log("OnUpdate");
    }


    // Basic
    [UnityTest]
    public IEnumerator Test_GameTimerDefaultsPasses()
    {
        Assert.IsNotNull(TimeKeeper.InitSingleton());

        GameTimer gameTimer = TimeKeeper.Instance.NewTimer<GameTimer>();
        Assert.IsNotNull(gameTimer);

        Assert.IsFalse(gameTimer.IsEnabled);
        Assert.IsTrue(gameTimer.IsPaused);
        Assert.AreEqual(gameTimer.Duration, -1.0f, 0.001f);
        Assert.AreEqual(gameTimer.MinTime, 0.0f, 0.001f);

        gameTimer.OnStart += (x) => Debug.Log("OnStart");
        gameTimer.OnStop += (x) => Debug.Log("OnStop");
        gameTimer.OnUpdate += OnUpdate;
        gameTimer.OnMaxTimeBound += (x) => Debug.Log("OnMaxTimeBound");

        gameTimer.Start();

        Assert.IsTrue(gameTimer.IsEnabled);
        Assert.IsFalse(gameTimer.IsPaused);
        Assert.AreEqual(gameTimer.MinTime, TimeKeeper.Instance.Time.time, 0.1f);

        LogAssert.Expect(LogType.Log, "OnStart");

        yield return null;

        Assert.AreEqual(TimeKeeper.Instance.Time.time, gameTimer.Time, 0.01f);
        Assert.AreEqual(TimeKeeper.Instance.Time.time - gameTimer.MinTime, gameTimer.TimeInterval, 0.01f);
        Assert.AreEqual(-1.0f, gameTimer.NormalisedTime, 0.1f);

        LogAssert.Expect(LogType.Log, "OnUpdate");

        gameTimer.Stop();

        LogAssert.Expect(LogType.Log, "OnStop");

        Assert.IsFalse(gameTimer.IsEnabled);
        Assert.IsTrue(gameTimer.IsPaused);

        gameTimer.Duration = 1.0f;

        Assert.AreEqual(gameTimer.Duration, 1.0f, 0.001f);

        gameTimer.Start();

        Assert.IsTrue(gameTimer.IsEnabled);
        Assert.IsFalse(gameTimer.IsPaused);
        Assert.AreEqual(gameTimer.MinTime, TimeKeeper.Instance.Time.time, 0.1f);

        LogAssert.Expect(LogType.Log, "OnStart");

        gameTimer.OnUpdate -= OnUpdate;

        yield return null;

        LogAssert.NoUnexpectedReceived();

        Assert.AreEqual(TimeKeeper.Instance.Time.time, gameTimer.Time, 0.1f);
        Assert.Greater(gameTimer.TimeInterval, 0.0f);
        Assert.Greater(gameTimer.NormalisedTime, 0.0f);

        yield return new WaitForSeconds(1.0f);

        LogAssert.Expect(LogType.Log, "OnMaxTimeBound");
        LogAssert.Expect(LogType.Log, "OnStop");

        Assert.AreEqual(TimeKeeper.Instance.Time.time, gameTimer.Time, 0.1f);
        Assert.AreEqual(1.0f, gameTimer.TimeInterval, 0.01f);
        Assert.AreEqual(1.0f, gameTimer.NormalisedTime, 0.01f);

        Assert.IsFalse(gameTimer.IsEnabled);
        Assert.IsTrue(gameTimer.IsPaused);

        // Destroy
        gameTimer.OnUpdate += OnUpdate;
        gameTimer.Start();
        LogAssert.Expect(LogType.Log, "OnStart");

        TimeKeeper.Instance.DestroyTimer(gameTimer);
        gameTimer = null;

        yield return null;

        LogAssert.NoUnexpectedReceived();
    }
}
