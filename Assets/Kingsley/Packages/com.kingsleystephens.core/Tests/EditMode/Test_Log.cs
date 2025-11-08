using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using KCore = KingsleyStephens.Core;

public class Test_Log
{
    // Basic
    [Test]
    public void Test_LogDefaultsPasses()
    {
        KCore.Log.SetLogLevel(KCore.Log.LogType.All);

        LogAssert.Expect(LogType.Log, "LogMessage");
        KCore.Log.LogMessage("LogMessage");

        LogAssert.Expect(LogType.Log, "LogMessage 0,1");
        KCore.Log.LogMessage("LogMessage {0},{1}", 0, 1);

        LogAssert.Expect(LogType.Warning, "LogWarning");
        KCore.Log.LogWarning("LogWarning");
        LogAssert.Expect(LogType.Warning, "LogWarning 0,1");
        KCore.Log.LogWarning("LogWarning {0},{1}", 0, 1);

        LogAssert.Expect(LogType.Error, "LogError");
        KCore.Log.LogError("LogError");
        LogAssert.Expect(LogType.Error, "LogError 0,1");
        KCore.Log.LogError("LogError {0},{1}", 0, 1);
    }

    // Log levels
    [Test]
    public void Test_LogLevelAllPasses()
    {
        KCore.Log.SetLogLevel(KCore.Log.LogType.All);

        LogAssert.Expect(LogType.Log, "LogMessage");
        KCore.Log.LogMessage("LogMessage");
    }

    [Test]
    public void Test_LogLevelNonePasses()
    {
        KCore.Log.SetLogLevel(KCore.Log.LogType.None);
        LogAssert.NoUnexpectedReceived();
        KCore.Log.LogMessage("LogMessage");
    }


    [Test]
    public void Test_LogLevelArbitraryPasses()
    {
        KCore.Log.SetLogLevel(KCore.Log.LogType.Warning);
        LogAssert.NoUnexpectedReceived();
        KCore.Log.LogError("LogMessage");
    }

    [Test]
    public void Test_LogCustomLevelPasses()
    {
        KCore.Log.SetLogLevel(KCore.Log.LogType.All);

        string customLevelName = "TEST_LEVEL";
        uint customLevel = (uint)KCore.Log.LogType.All + 100;
        KCore.Log.AddCustomLogLevel(customLevelName, customLevel);

        LogAssert.Expect(LogType.Log, "LogMessage");
        KCore.Log.LogMessage("LogMessage");

        KCore.Log.SetLogLevel(KCore.Log.LogType.All);
        LogAssert.Expect(LogType.Log, "LogMessage");
        KCore.Log.LogMessage("LogMessage");

        KCore.Log.SetCustomLogLevel(customLevelName);
        LogAssert.Expect(LogType.Log, "LogMessage");
        KCore.Log.DebugLog(customLevel, "LogMessage");

        KCore.Log.SetLogLevel(KCore.Log.LogType.Message);
        LogAssert.NoUnexpectedReceived();
        KCore.Log.DebugLog(customLevel, "LogMessage");

        KCore.Log.SetLogLevel(KCore.Log.LogType.None);
        LogAssert.NoUnexpectedReceived();
        KCore.Log.DebugLog(customLevel, "LogMessage");

        KCore.Log.SetLogLevel(KCore.Log.LogType.All);
        LogAssert.Expect(LogType.Log, "LogMessage");
        KCore.Log.DebugLog(customLevel, "LogMessage");
    }


    [Test]
    public void Test_LogCustomLogs()
    {
        Assert.IsNull(KCore.Log.Get(0));

        int logId = KCore.Log.CreateCustom();

        Assert.IsNotNull(KCore.Log.Get(logId));

        KCore.Log.Get(logId).LogMessage("LogMessage");
        LogAssert.Expect(LogType.Log, "LogMessage");

        int logId2 = KCore.Log.CreateCustom();

        KCore.Log.Get(logId2).SetLogLevel(KCore.Logger.LogType.None);
        
        KCore.Log.Get(logId).LogMessage("LogMessage");
        LogAssert.Expect(LogType.Log, "LogMessage");

        KCore.Log.Get(logId2).LogMessage("LogMessage");
        LogAssert.NoUnexpectedReceived();

        KCore.Log.CloseCustom(logId);
        Assert.IsNull(KCore.Log.Get(logId));
    }
}
