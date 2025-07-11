using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using KCore;

public class Test_Logger
{
    // Basic
    [Test]
    public void Test_LoggerDefaultsPasses()
    {
        KCore.Logger.SetLogLevel(KCore.Logger.LogType.All);

        LogAssert.Expect(LogType.Log, "LogMessage");
        KCore.Logger.LogMessage("LogMessage");

        LogAssert.Expect(LogType.Log, "LogMessage 0,1");
        KCore.Logger.LogMessage("LogMessage {0},{1}", 0, 1);

        LogAssert.Expect(LogType.Warning, "LogWarning");
        KCore.Logger.LogWarning("LogWarning");
        LogAssert.Expect(LogType.Warning, "LogWarning 0,1");
        KCore.Logger.LogWarning("LogWarning {0},{1}", 0, 1);

        LogAssert.Expect(LogType.Error, "LogError");
        KCore.Logger.LogError("LogError");
        LogAssert.Expect(LogType.Error, "LogError 0,1");
        KCore.Logger.LogError("LogError {0},{1}", 0, 1);
    }

    // Log levels
    [Test]
    public void Test_LoggerLevelAllPasses()
    {
        KCore.Logger.SetLogLevel(KCore.Logger.LogType.All);

        LogAssert.Expect(LogType.Log, "LogMessage");
        KCore.Logger.LogMessage("LogMessage");
    }

    [Test]
    public void Test_LoggerLevelNonePasses()
    {
        KCore.Logger.SetLogLevel(KCore.Logger.LogType.None);
        LogAssert.NoUnexpectedReceived();
        KCore.Logger.LogMessage("LogMessage");
    }


    [Test]
    public void Test_LoggerLevelArbitraryPasses()
    {
        KCore.Logger.SetLogLevel(KCore.Logger.LogType.Warning);
        LogAssert.NoUnexpectedReceived();
        KCore.Logger.LogError("LogMessage");
    }

    [Test]
    public void Test_LoggerCustomLevelPasses()
    {
        KCore.Logger.SetLogLevel(KCore.Logger.LogType.All);

        string customLevelName = "TEST_LEVEL";
        uint customLevel = (uint)KCore.Logger.LogType.All + 100;
        KCore.Logger.AddCustomLogLevel(customLevelName, customLevel);

        LogAssert.Expect(LogType.Log, "LogMessage");
        KCore.Logger.LogMessage("LogMessage");

        KCore.Logger.SetLogLevel(KCore.Logger.LogType.All);
        LogAssert.Expect(LogType.Log, "LogMessage");
        KCore.Logger.LogMessage("LogMessage");

        KCore.Logger.SetCustomLogLevel(customLevelName);
        LogAssert.Expect(LogType.Log, "LogMessage");
        KCore.Logger.DebugLog(customLevel, "LogMessage");

        KCore.Logger.SetLogLevel(KCore.Logger.LogType.Message);
        LogAssert.NoUnexpectedReceived();
        KCore.Logger.DebugLog(customLevel, "LogMessage");

        KCore.Logger.SetLogLevel(KCore.Logger.LogType.None);
        LogAssert.NoUnexpectedReceived();
        KCore.Logger.DebugLog(customLevel, "LogMessage");

        KCore.Logger.SetLogLevel(KCore.Logger.LogType.All);
        LogAssert.Expect(LogType.Log, "LogMessage");
        KCore.Logger.DebugLog(customLevel, "LogMessage");
    }

}
