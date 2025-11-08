using KingsleyStephens.Core;
using KingsleyStephens.CoreInternal;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using KCore = KingsleyStephens.Core;

public class Test_Updateable
{
    // Basic
    [Test]
    public void Test_LogDefaultsPasses()
    {
        LogAssert.Expect(LogType.Log, "Init()");
        MockUpdateable mock = ScriptableObject.CreateInstance<MockUpdateable>();
        mock.UpdateInterval = 0.5f;

        LogAssert.Expect(LogType.Log, "Shutdown()");
        mock.OnDisable();

        LogAssert.Expect(LogType.Log, "Init()");
        mock.OnEnable();


        TimeSnapshot t = new TimeSnapshot(0.0f, 0.0f);

        mock.Enabled = false;
        LogAssert.NoUnexpectedReceived();
        mock.RunUpdate(t);

        t.time = 1.0f;
        LogAssert.NoUnexpectedReceived();
        mock.RunUpdate(t);

        mock.Enabled = true;
        t.time = 0.25f;
        LogAssert.NoUnexpectedReceived();
        mock.RunUpdate(t);

        t.time = 1.0f;
        LogAssert.Expect(LogType.Log, "Update()");
        mock.RunUpdate(t);
    }

}
