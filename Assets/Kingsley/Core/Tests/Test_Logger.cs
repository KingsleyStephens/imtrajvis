using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SATestbed;

public class Test_Logger
{
    // Basic
    [Test]
    public void Test_LoggerDefaultsPasses()
    {
        SATestbed.Logger.SetLogLevel(SATestbed.Logger.LogType.All);

        LogAssert.Expect(LogType.Log, "LogMessage");
        SATestbed.Logger.LogMessage("LogMessage");

        LogAssert.Expect(LogType.Log, "LogMessage 0,1");
        SATestbed.Logger.LogMessage("LogMessage {0},{1}", 0, 1);

        LogAssert.Expect(LogType.Warning, "LogWarning");
        SATestbed.Logger.LogWarning("LogWarning");
        LogAssert.Expect(LogType.Warning, "LogWarning 0,1");
        SATestbed.Logger.LogWarning("LogWarning {0},{1}", 0, 1);

        LogAssert.Expect(LogType.Error, "LogError");
        SATestbed.Logger.LogError("LogError");
        LogAssert.Expect(LogType.Error, "LogError 0,1");
        SATestbed.Logger.LogError("LogError {0},{1}", 0, 1);
    }

    // Log levels
    [Test]
    public void Test_LoggerLevelAllPasses()
    {
        SATestbed.Logger.SetLogLevel(SATestbed.Logger.LogType.All);

        LogAssert.Expect(LogType.Log, "LogMessage");
        SATestbed.Logger.LogMessage("LogMessage");
    }

    [Test]
    public void Test_LoggerLevelNonePasses()
    {
        SATestbed.Logger.SetLogLevel(SATestbed.Logger.LogType.None);
        LogAssert.NoUnexpectedReceived();
        SATestbed.Logger.LogMessage("LogMessage");
    }


    [Test]
    public void Test_LoggerLevelArbitraryPasses()
    {
        SATestbed.Logger.SetLogLevel(SATestbed.Logger.LogType.Warning);
        LogAssert.NoUnexpectedReceived();
        SATestbed.Logger.LogError("LogMessage");
    }

    [Test]
    public void Test_LoggerCustomLevelPasses()
    {
        SATestbed.Logger.SetLogLevel(SATestbed.Logger.LogType.All);

        string customLevelName = "TEST_LEVEL";
        uint customLevel = (uint)SATestbed.Logger.LogType.All + 100;
        SATestbed.Logger.AddCustomLogLevel(customLevelName, customLevel);

        LogAssert.Expect(LogType.Log, "LogMessage");
        SATestbed.Logger.LogMessage("LogMessage");

        SATestbed.Logger.SetLogLevel(SATestbed.Logger.LogType.All);
        LogAssert.Expect(LogType.Log, "LogMessage");
        SATestbed.Logger.LogMessage("LogMessage");

        SATestbed.Logger.SetCustomLogLevel(customLevelName);
        LogAssert.Expect(LogType.Log, "LogMessage");
        SATestbed.Logger.DebugLog(customLevel, "LogMessage");

        SATestbed.Logger.SetLogLevel(SATestbed.Logger.LogType.Message);
        LogAssert.NoUnexpectedReceived();
        SATestbed.Logger.DebugLog(customLevel, "LogMessage");

        SATestbed.Logger.SetLogLevel(SATestbed.Logger.LogType.None);
        LogAssert.NoUnexpectedReceived();
        SATestbed.Logger.DebugLog(customLevel, "LogMessage");

        SATestbed.Logger.SetLogLevel(SATestbed.Logger.LogType.All);
        LogAssert.Expect(LogType.Log, "LogMessage");
        SATestbed.Logger.DebugLog(customLevel, "LogMessage");
    }

}
