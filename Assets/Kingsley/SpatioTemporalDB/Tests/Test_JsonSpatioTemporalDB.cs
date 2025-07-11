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

        JsonSpatioTemporalDB db = new JsonSpatioTemporalDB(path);
        db.CreateOrOpen();
        return db;
    }


    // Basic
    [Test]
    public void Test_CreateOrOpen()
    {
        JsonSpatioTemporalDB db = CreateDb();
        db.Save();

        Assert.IsTrue(File.Exists(path));

        string[] tables = db.ListTables();
        Assert.AreEqual(0, tables.Length);
    }

    [Test]
    public void Test_CreateTable()
    {
        JsonSpatioTemporalDB db = CreateDb();

        string[] tablesPre = db.ListTables();
        Assert.AreEqual(0, tablesPre.Length);

        db.CreateTable("TableTest");
        string[] tablesPost = db.ListTables();
        Assert.AreEqual(1, tablesPost.Length);
        Assert.AreEqual(tablesPost[0], "TableTest");

        db.Save();
        db.CreateOrOpen();

        string[] tablesPostSave = db.ListTables();
        Assert.AreEqual(1, tablesPostSave.Length);
        Assert.AreEqual(tablesPostSave[0], "TableTest");
    }

    [Test]
    public void Test_DeleteTable()
    {
        JsonSpatioTemporalDB db = CreateDb();

        string[] tablesPre = db.ListTables();
        Assert.AreEqual(0, tablesPre.Length);

        db.CreateTable("TableTest");
        string[] tablesPost = db.ListTables();
        Assert.AreEqual(1, tablesPost.Length);
        Assert.AreEqual(tablesPost[0], "TableTest");

        bool result1 = db.DeleteTable("TableTest2");
        Assert.IsFalse(result1);

        bool result2 = db.DeleteTable("TableTest");
        Assert.IsTrue(result2);
        string[] tablesPostDelete = db.ListTables();
        Assert.AreEqual(0, tablesPostDelete.Length);
    }

    [Test]
    public void Test_UpdateTable()
    {
        JsonSpatioTemporalDB db = CreateDb();
        db.CreateTable("TableTest");

        SpatioTemporalDB.MovingObjectPositionUpdate[] readPre = db.ReadTable("TableTest", "MO1", 0.0f, 1.0f);
        Assert.AreEqual(0, readPre.Length);

        SpatioTemporalDB.MovingObjectPositionUpdate update = new SpatioTemporalDB.MovingObjectPositionUpdate() { position = new Vector3(0.0f, 0.0f, 0.0f), orientation = new Vector3(0.0f, 0.0f, 0.0f), speed = 0.0f, time = 0.0f };

        bool result1 = db.UpdateTable("TableTest1", "MO1", update);
        Assert.IsFalse(result1);

        bool result2 = db.UpdateTable("TableTest", "MO1", update);
        Assert.IsTrue(result2);

        SpatioTemporalDB.MovingObjectPositionUpdate[] readPost = db.ReadTable("TableTest", "MO1", 0.0f, 1.0f);
        Assert.AreEqual(1, readPost.Length);
        Assert.AreEqual(readPost[0].position, update.position);
        Assert.AreEqual(readPost[0].orientation, update.orientation);
        Assert.AreEqual(readPost[0].speed, update.speed);
        Assert.AreEqual(readPost[0].time, update.time);

        db.Save();
        db.CreateOrOpen();
        SpatioTemporalDB.MovingObjectPositionUpdate[] readPostSave = db.ReadTable("TableTest", "MO1", 0.0f, 1.0f);
        Assert.AreEqual(1, readPostSave.Length);
        Assert.AreEqual(readPost[0].position, update.position);
        Assert.AreEqual(readPost[0].orientation, update.orientation);
        Assert.AreEqual(readPost[0].speed, update.speed);
        Assert.AreEqual(readPost[0].time, update.time);


        string[] movingObjectsFail = db.ListTable("TableTest1");
        Assert.AreEqual(0, movingObjectsFail.Length);

        string[] movingObjects = db.ListTable("TableTest");
        Assert.AreEqual(1, movingObjects.Length);
        Assert.AreEqual("MO1", movingObjects[0]);

        // Moving Object 2

        SpatioTemporalDB.MovingObjectPositionUpdate update2 = new SpatioTemporalDB.MovingObjectPositionUpdate() { position = new Vector3(1.0f, 2.0f, 3.0f), orientation = new Vector3(4.0f, 5.0f, 6.0f), speed = 1.0f, time = 1.0f };
        db.UpdateTable("TableTest", "MO2", update2);

        db.Save();

        string[] movingObjects2 = db.ListTable("TableTest");
        Assert.AreEqual(2, movingObjects2.Length);
        Assert.AreEqual("MO1", movingObjects2[0]);
        Assert.AreEqual("MO2", movingObjects2[1]);

        SpatioTemporalDB.MovingObjectPositionUpdate[] readPost2 = db.ReadTable("TableTest", "MO2", 0.0f, 1.0f);
        Assert.AreEqual(1, readPost2.Length);
        Assert.AreEqual(readPost2[0].position, update2.position);
        Assert.AreEqual(readPost2[0].orientation, update2.orientation);
        Assert.AreEqual(readPost2[0].speed, update2.speed);
        Assert.AreEqual(readPost2[0].time, update2.time);

        // Time read out of range
        SpatioTemporalDB.MovingObjectPositionUpdate[] readPost3 = db.ReadTable("TableTest", "MO2", 2.0f, 1.0f);
        Assert.AreEqual(0, readPost3.Length);
    }
}
