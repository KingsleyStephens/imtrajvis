using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SpatioTemporalDatabase;
using System.IO;

public class Test_JsonSpatioTemporalDB
{
    private static string path = "d:\\temp\\testdb.json";

    private JsonSpatioTemporalDB CreateDb()
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return new JsonSpatioTemporalDB(path);
    }


    // Basic
    [Test]
    public void Test_CreateOrOpen()
    {
        JsonSpatioTemporalDB db = CreateDb();

        db.CreateOrOpen();
        db.Save();

        Assert.IsTrue(File.Exists(path));
    }

}
