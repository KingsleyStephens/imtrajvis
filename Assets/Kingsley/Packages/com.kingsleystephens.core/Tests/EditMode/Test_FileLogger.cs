using KingsleyStephens.Core;
using KingsleyStephens.CoreInternal;
using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TestTools;
using KCore = KingsleyStephens.Core;

public class Test_FileLogger
{
    // Basic
    [Test]
    public void Test_FileLoggerDefaultsPasses()
    {
        StreamWriterFactory.Mock = true;

        FileLogger logger = new FileLogger();

        LogAssert.Expect(LogType.Warning, "Log file not open. Call Open() first.");
        logger.Write("FAIL");
        LogAssert.Expect(LogType.Warning, "Log file not open. Call Open() first.");
        logger.WriteRaw("FAIL");
        LogAssert.Expect(LogType.Warning, "Log file not open. Call Open() first.");
        logger.WriteWarning("FAIL");
        LogAssert.Expect(LogType.Warning, "Log file not open. Call Open() first.");
        logger.WriteError("FAIL");

        LogAssert.Expect(LogType.Error, "Failed to open log file EXCEPTION: MOCK");
        logger.Open("EXCEPTION");

        LogAssert.Expect(LogType.Log, "MockStreamWriter(TESTFILEPATH, True)");
        LogAssert.Expect(LogType.Log, "AutoFlush = True");
        logger.Open("TESTFILEPATH");

        LogAssert.Expect(LogType.Log, "WriteLine(TEST)");
        logger.WriteRaw("TEST");

        LogAssert.Expect(LogType.Log, "Close()");
        LogAssert.Expect(LogType.Log, "Dispose()");
        logger.Close();
    }

}
