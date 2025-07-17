using NUnit.Framework;
using SpatioTemporalDatabase;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.TestTools;

public class Test_JsonSpatioTemporalDB
{
    private static string path = "c:\\temp\\testdb.json";

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

        SpatioTemporalDB.MovingObjectPositionUpdate readPre = db.ReadTable("TableTest", 1, 0.0f);
        Assert.IsNull(readPre);

        SpatioTemporalDB.MovingObjectPositionUpdate update = new SpatioTemporalDB.MovingObjectPositionUpdate(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, 0.0f, 0.0f);

        bool result1 = db.UpdateTable("TableTest1", 1, update);
        Assert.IsFalse(result1);

        bool resultDelta = db.UpdateTable("TableTest", 1, 0.5f);
        Assert.IsTrue(resultDelta);

        bool result2 = db.UpdateTable("TableTest", 1, update);
        Assert.IsTrue(result2);

        SpatioTemporalDB.MovingObjectPositionUpdate readPost = db.ReadTable("TableTest", 1, 0.0f);
        Assert.NotNull(readPost);
        Assert.AreEqual(readPost.position, update.position);
        Assert.AreEqual(readPost.orientation, update.orientation);
        Assert.AreEqual(readPost.speed, update.speed);

        db.Save();
        db.CreateOrOpen();
        SpatioTemporalDB.MovingObjectPositionUpdate readPostSave = db.ReadTable("TableTest", 1, 0.0f);
        Assert.NotNull(readPostSave);
        Assert.AreEqual(readPostSave.position, update.position);
        Assert.AreEqual(readPostSave.orientation, update.orientation);
        Assert.AreEqual(readPostSave.speed, update.speed);


        int[] movingObjectsFail = db.ListTable("TableTest1");
        Assert.AreEqual(0, movingObjectsFail.Length);

        int[] movingObjects = db.ListTable("TableTest");
        Assert.AreEqual(1, movingObjects.Length);
        Assert.AreEqual(1, movingObjects[0]);

        // Moving Object 2
        bool resultDelta2 = db.UpdateTable("TableTest", 2, 0.5f);
        Assert.IsTrue(resultDelta2);

        SpatioTemporalDB.MovingObjectPositionUpdate update2 = new SpatioTemporalDB.MovingObjectPositionUpdate(new Vector3(1.0f, 2.0f, 3.0f), Quaternion.identity, 1.0f, 1.0f );
        db.UpdateTable("TableTest", 2, update2);

        db.Save();

        int[] movingObjects2 = db.ListTable("TableTest");
        Assert.AreEqual(2, movingObjects2.Length);
        Assert.AreEqual(1, movingObjects2[0]);
        Assert.AreEqual("MO2", movingObjects2[1]);

        SpatioTemporalDB.MovingObjectPositionUpdate readPost2 = db.ReadTable("TableTest", 2, 0.0f);
        Assert.NotNull(readPost2);
        Assert.AreEqual(update2.position, readPost2.position);
        Assert.AreEqual(update2.orientation, readPost2.orientation);
        Assert.AreEqual(update2.speed, readPost2.speed);

        // Time read out of range
        SpatioTemporalDB.MovingObjectPositionUpdate readPost3 = db.ReadTable("TableTest", 2, 10.0f);
        Assert.IsNull(readPost3);
    }

    [Test]
    public void Test_UpdateTableInterpolation()
    {
        JsonSpatioTemporalDB db = CreateDb();
        db.CreateTable("TableTest");

        bool resultDelta = db.UpdateTable("TableTest", 1, 0.5f);
        Assert.IsTrue(resultDelta);

        // No Entry
        SpatioTemporalDB.MovingObjectPositionUpdate readPost0 = db.ReadTable("TableTest", 1, 0.0f);
        Assert.IsNull(readPost0);

        // Single entry
        SpatioTemporalDB.MovingObjectPositionUpdate update1 = new SpatioTemporalDB.MovingObjectPositionUpdate(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, 0.0f, 0.0f);

        bool resultAdd1 = db.UpdateTable("TableTest", 1, update1);
        Assert.IsTrue(resultAdd1);
        db.Save();

        SpatioTemporalDB.MovingObjectPositionUpdate readPost1_1 = db.ReadTable("TableTest", 1, 0.0f);
        Assert.NotNull(readPost1_1);
        Assert.AreEqual(update1.position, readPost1_1.position);
        Assert.AreEqual(update1.orientation, readPost1_1.orientation);
        Assert.AreEqual(update1.speed, readPost1_1.speed);

        SpatioTemporalDB.MovingObjectPositionUpdate readPost1_2 = db.ReadTable("TableTest", 1, 1.0f);
        Assert.NotNull(readPost1_2);
        Assert.AreEqual(update1.position, readPost1_2.position);
        Assert.AreEqual(update1.orientation, readPost1_2.orientation);
        Assert.AreEqual(update1.speed, readPost1_2.speed);

        // Two entries
        SpatioTemporalDB.MovingObjectPositionUpdate update2 = new SpatioTemporalDB.MovingObjectPositionUpdate(new Vector3(1.0f, 0.0f, 0.0f), Quaternion.identity, 1.0f, 0.5f);
        bool resultAdd2 = db.UpdateTable("TableTest", 1, update2);
        Assert.IsTrue(resultAdd2);
        db.Save();

        SpatioTemporalDB.MovingObjectPositionUpdate readPost2_1 = db.ReadTable("TableTest", 1, 0.0f);
        Assert.NotNull(readPost2_1);
        Assert.AreEqual(update1.position, readPost2_1.position);
        Assert.AreEqual(update1.orientation, readPost2_1.orientation);
        Assert.AreEqual(update1.speed, readPost2_1.speed);

        SpatioTemporalDB.MovingObjectPositionUpdate readPost2_2 = db.ReadTable("TableTest", 1, 0.5f);
        Assert.NotNull(readPost2_2);
        Assert.AreEqual(update2.position, readPost2_2.position);
        Assert.AreEqual(update2.orientation, readPost2_2.orientation);
        Assert.AreEqual(update2.speed, readPost2_2.speed);


        SpatioTemporalDB.MovingObjectPositionUpdate readPost2_3 = db.ReadTable("TableTest", 1, 0.25f);
        Assert.NotNull(readPost2_3);
        Assert.AreEqual(new Vector3(0.5f, 0.0f, 0.0f), readPost2_3.position);
        Assert.AreEqual(Quaternion.identity, readPost2_3.orientation);
        Assert.AreEqual(0.5f, readPost2_3.speed);

        // Three entries
        SpatioTemporalDB.MovingObjectPositionUpdate update3 = new SpatioTemporalDB.MovingObjectPositionUpdate(new Vector3(2.0f, 0.0f, 0.0f), Quaternion.identity, 2.0f, 1.0f);
        bool resultAdd3 = db.UpdateTable("TableTest", 1, update3);
        Assert.IsTrue(resultAdd3);
        db.Save();

        SpatioTemporalDB.MovingObjectPositionUpdate readPost3_1 = db.ReadTable("TableTest", 1, 0.0f);
        Assert.NotNull(readPost3_1);
        Assert.AreEqual(update1.position, readPost3_1.position);
        Assert.AreEqual(update1.orientation, readPost3_1.orientation);
        Assert.AreEqual(update1.speed, readPost3_1.speed);

        SpatioTemporalDB.MovingObjectPositionUpdate readPost3_2 = db.ReadTable("TableTest", 1, 0.5f);
        Assert.NotNull(readPost3_2);
        Assert.AreEqual(update2.position, readPost3_2.position);
        Assert.AreEqual(update2.orientation, readPost3_2.orientation);
        Assert.AreEqual(update2.speed, readPost3_2.speed);


        SpatioTemporalDB.MovingObjectPositionUpdate readPost3_3 = db.ReadTable("TableTest", 1, 0.25f);
        Assert.NotNull(readPost3_3);
        Assert.AreEqual(new Vector3(0.5f, 0.0f, 0.0f), readPost3_3.position);
        Assert.AreEqual(Quaternion.identity, readPost3_3.orientation);
        Assert.AreEqual(0.5f, readPost3_3.speed);

        SpatioTemporalDB.MovingObjectPositionUpdate readPost3_4 = db.ReadTable("TableTest", 1, 1.0f);
        Assert.NotNull(readPost3_4);
        Assert.AreEqual(update3.position, readPost3_4.position);
        Assert.AreEqual(update3.orientation, readPost3_4.orientation);
        Assert.AreEqual(update3.speed, readPost3_4.speed);

        SpatioTemporalDB.MovingObjectPositionUpdate readPost3_5 = db.ReadTable("TableTest", 1, 0.75f);
        Assert.NotNull(readPost3_5);
        Assert.AreEqual(new Vector3(1.5f, 0.0f, 0.0f), readPost3_5.position);
        Assert.AreEqual(Quaternion.identity, readPost3_5.orientation);
        Assert.AreEqual(1.5f, readPost3_5.speed);

        // Four entries
        SpatioTemporalDB.MovingObjectPositionUpdate update4 = new SpatioTemporalDB.MovingObjectPositionUpdate(new Vector3(3.0f, 0.0f, 0.0f), Quaternion.identity, 3.0f, 1.5f);
        bool resultAdd4 = db.UpdateTable("TableTest", 1, update4);
        Assert.IsTrue(resultAdd4);
        db.Save();

        SpatioTemporalDB.MovingObjectPositionUpdate readPost4_1 = db.ReadTable("TableTest", 1, 0.0f);
        Assert.NotNull(readPost4_1);
        Assert.AreEqual(update1.position, readPost4_1.position);
        Assert.AreEqual(update1.orientation, readPost4_1.orientation);
        Assert.AreEqual(update1.speed, readPost4_1.speed);

        SpatioTemporalDB.MovingObjectPositionUpdate readPost4_2 = db.ReadTable("TableTest", 1, 0.5f);
        Assert.NotNull(readPost4_2);
        Assert.AreEqual(update2.position, readPost4_2.position);
        Assert.AreEqual(update2.orientation, readPost4_2.orientation);
        Assert.AreEqual(update2.speed, readPost4_2.speed);


        SpatioTemporalDB.MovingObjectPositionUpdate readPost4_3 = db.ReadTable("TableTest", 1, 0.25f);
        Assert.NotNull(readPost4_3);
        Assert.AreEqual(new Vector3(0.5f, 0.0f, 0.0f), readPost4_3.position);
        Assert.AreEqual(Quaternion.identity, readPost4_3.orientation);
        Assert.AreEqual(0.5f, readPost4_3.speed);

        SpatioTemporalDB.MovingObjectPositionUpdate readPost4_4 = db.ReadTable("TableTest", 1, 1.0f);
        Assert.NotNull(readPost4_4);
        Assert.AreEqual(update3.position, readPost4_4.position);
        Assert.AreEqual(update3.orientation, readPost4_4.orientation);
        Assert.AreEqual(update3.speed, readPost4_4.speed);

        SpatioTemporalDB.MovingObjectPositionUpdate readPost4_5 = db.ReadTable("TableTest", 1, 0.75f);
        Assert.NotNull(readPost4_5);
        Assert.AreEqual(new Vector3(1.5f, 0.0f, 0.0f), readPost4_5.position);
        Assert.AreEqual(Quaternion.identity, readPost4_5.orientation);
        Assert.AreEqual(1.5f, readPost4_5.speed);

        SpatioTemporalDB.MovingObjectPositionUpdate readPost4_6 = db.ReadTable("TableTest", 1, 1.25f);
        Assert.NotNull(readPost4_6);
        Assert.AreEqual(new Vector3(2.5f, 0.0f, 0.0f), readPost4_6.position);
        Assert.AreEqual(Quaternion.identity, readPost4_6.orientation);
        Assert.AreEqual(2.5f, readPost4_6.speed);
    }
}
