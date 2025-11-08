using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using KCore = KingsleyStephens.Core;

public class Test_Logger
{
    // Basic
    [Test]
    public void Test_LoggerDefaultsPasses()
    {
        KCore.Logger log = new KCore.Logger();

        log.SetLogLevel(KCore.Logger.LogType.All);

        LogAssert.Expect(LogType.Log, "LogMessage");
        log.LogMessage("LogMessage");

        LogAssert.Expect(LogType.Log, "LogMessage 0,1");
        log.LogMessage("LogMessage {0},{1}", 0, 1);

        LogAssert.Expect(LogType.Warning, "LogWarning");
        log.LogWarning("LogWarning");
        LogAssert.Expect(LogType.Warning, "LogWarning 0,1");
        log.LogWarning("LogWarning {0},{1}", 0, 1);

        LogAssert.Expect(LogType.Error, "LogError");
        log.LogError("LogError");
        LogAssert.Expect(LogType.Error, "LogError 0,1");
        log.LogError("LogError {0},{1}", 0, 1);
    }

    // Log levels
    [Test]
    public void Test_LoggerLevelAllPasses()
    {
        KCore.Logger log = new KCore.Logger();

        log.SetLogLevel(KCore.Logger.LogType.All);

        LogAssert.Expect(LogType.Log, "LogMessage");
        log.LogMessage("LogMessage");
    }

    [Test]
    public void Test_LoggerLevelNonePasses()
    {
        KCore.Logger log = new KCore.Logger();

        log.SetLogLevel(KCore.Logger.LogType.None);
        LogAssert.NoUnexpectedReceived();
        log.LogMessage("LogMessage");
    }


    [Test]
    public void Test_LoggerLevelArbitraryPasses()
    {
        KCore.Logger log = new KCore.Logger();

        log.SetLogLevel(KCore.Logger.LogType.Warning);
        LogAssert.NoUnexpectedReceived();
        log.LogError("LogMessage");
    }

    [Test]
    public void Test_LoggerCustomLevelPasses()
    {
        KCore.Logger log = new KCore.Logger();

        log.SetLogLevel(KCore.Logger.LogType.All);

        string customLevelName = "TEST_LEVEL";
        uint customLevel = (uint)KCore.Logger.LogType.All + 100;
        log.AddCustomLogLevel(customLevelName, customLevel);

        LogAssert.Expect(LogType.Log, "LogMessage");
        log.LogMessage("LogMessage");

        log.SetLogLevel(KCore.Logger.LogType.All);
        LogAssert.Expect(LogType.Log, "LogMessage");
        log.LogMessage("LogMessage");

        log.SetCustomLogLevel(customLevelName);
        LogAssert.Expect(LogType.Log, "LogMessage");
        log.DebugLog(customLevel, "LogMessage");

        log.SetLogLevel(KCore.Logger.LogType.Message);
        LogAssert.NoUnexpectedReceived();
        log.DebugLog(customLevel, "LogMessage");

        log.SetLogLevel(KCore.Logger.LogType.None);
        LogAssert.NoUnexpectedReceived();
        log.DebugLog(customLevel, "LogMessage");

        log.SetLogLevel(KCore.Logger.LogType.All);
        LogAssert.Expect(LogType.Log, "LogMessage");
        log.DebugLog(customLevel, "LogMessage");
    }

}
