using System;
using System.Collections.Generic;
using Consonya.MSTestTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GdbControl.Tests
{
    [TestClass]
    public class ResultRecordParserTests
    {
        [TestMethod]
        public void EmptyRecordString()
        {
            const string testRecordString = "";
            var targetParser = new ResultRecordParser();

            var resultDict = targetParser.Parse(testRecordString);

            var testSequence = new Action[]
            {
                () => Assert.AreEqual(0,resultDict.Count),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        public void SingleEntryRecordString()
        {
            const string testRecordString = "addr=\"0x00402000\"";
            var targetParser = new ResultRecordParser();

            var resultDict = targetParser.Parse(testRecordString);

            var testSequence = new Action[]
            {
                () => Assert.AreEqual("0x00402000", resultDict["addr"]),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        public void UndelimitedFlatRecordString()
        {
            const string testRecordString = "addr=\"0x00402000\",nr-bytes=\"8\",total-bytes=\"8\"";
            var targetParser = new ResultRecordParser();

            var resultDict = targetParser.Parse(testRecordString);

            var testSequence = new Action[]
            {
                () => Assert.AreEqual("0x00402000", resultDict["addr"]),
                () => Assert.AreEqual("8", resultDict["nr-bytes"]),
                () => Assert.AreEqual("8", resultDict["total-bytes"]),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        public void BraceDelimitedFlatRecordString()
        {
            const string testRecordString = "{addr=\"0x00402000\",nr-bytes=\"8\",total-bytes=\"8\"}";
            var targetParser = new ResultRecordParser();

            var resultDict = targetParser.Parse(testRecordString);

            var testSequence = new Action[]
            {
                () => Assert.AreEqual("0x00402000", resultDict["addr"]),
                () => Assert.AreEqual("8", resultDict["nr-bytes"]),
                () => Assert.AreEqual("8", resultDict["total-bytes"]),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        public void SingleEntrySingleNestedRecordString()
        {
            const string testRecordString = "entry={addr=\"0x00402000\",nr-bytes=\"8\",total-bytes=\"18\"}";
            var targetParser = new ResultRecordParser();

            var resultDict = targetParser.Parse(testRecordString);

            var testSequence = new Action[]
            {
                () => Assert.IsTrue(resultDict.ContainsKey("entry")),
                () => Assert.AreEqual("0x00402000", ((dynamic)resultDict["entry"])["addr"]),
                () => Assert.AreEqual("8", ((dynamic)resultDict["entry"])["nr-bytes"]),
                () => Assert.AreEqual("18", ((dynamic)resultDict["entry"])["total-bytes"]),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }
    
        [TestMethod]
        public void MultiEntryMultipleNestedRecordString()
        {
            const string testRecordString = 
                "entry1={addr=\"0x00402000\",nr-bytes=\"8\",total-bytes=\"18\"}," +
                "entry2={addr=\"0x00402001\",nr-bytes=\"9\",total-bytes=\"19\"}," +
                "entry3={entry31={addr=\"0x00402002\",nr-bytes=\"10\"},entry32={entry321=\"otto\",entry322=\"hugo\"}}";
            var targetParser = new ResultRecordParser();

            var resultDict = targetParser.Parse(testRecordString);

            var testSequence = new Action[]
            {
                () => Assert.IsTrue(resultDict.ContainsKey("entry1")),
                () => Assert.IsTrue(resultDict.ContainsKey("entry2")),
                () => Assert.IsTrue(resultDict.ContainsKey("entry3")),
                () => Assert.IsTrue(((dynamic)resultDict["entry3"]).ContainsKey("entry31")),
                () => Assert.IsTrue(((dynamic)resultDict["entry3"]).ContainsKey("entry32")),

                () => Assert.AreEqual("0x00402000", ((dynamic)resultDict["entry1"])["addr"]),
                () => Assert.AreEqual("8", ((dynamic)resultDict["entry1"])["nr-bytes"]),
                () => Assert.AreEqual("18", ((dynamic)resultDict["entry1"])["total-bytes"]),

                () => Assert.AreEqual("0x00402001", ((dynamic)resultDict["entry2"])["addr"]),
                () => Assert.AreEqual("9", ((dynamic)resultDict["entry2"])["nr-bytes"]),
                () => Assert.AreEqual("19", ((dynamic)resultDict["entry2"])["total-bytes"]),

                () => Assert.AreEqual("0x00402002", (((dynamic)resultDict["entry3"])["entry31"])["addr"]),
                () => Assert.AreEqual("10", (((dynamic)resultDict["entry3"])["entry31"])["nr-bytes"]),
                () => Assert.AreEqual("otto", (((dynamic)resultDict["entry3"])["entry32"])["entry321"]),
                () => Assert.AreEqual("hugo", (((dynamic)resultDict["entry3"])["entry32"])["entry322"]),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        public void RecordStringWithEmptyArray()
        {
            const string testRecordString = "entry=[]";
            var targetParser = new ResultRecordParser();

            var resultDict = targetParser.Parse(testRecordString);

            var testSequence = new Action[]
            {
                () => Assert.IsTrue(resultDict["entry"] is List<object>),
                () => Assert.AreEqual(0, ((dynamic)resultDict["entry"]).Count),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        public void RecordStringWithFlatArray()
        {
            const string testRecordString = "entry=[\"1\",{inner=\"2\"},\"3\"]";
            var targetParser = new ResultRecordParser();

            var resultDict = targetParser.Parse(testRecordString);

            var testSequence = new Action[]
            {
                () => Assert.IsTrue(resultDict["entry"] is List<object>),
                () => Assert.AreEqual(3, ((dynamic)resultDict["entry"]).Count),
                () => Assert.AreEqual("1", ((dynamic)resultDict["entry"])[0]),
                () => Assert.AreEqual("2", ((dynamic)resultDict["entry"])[1]["inner"]),
                () => Assert.AreEqual("3", ((dynamic)resultDict["entry"])[2]),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }

        [TestMethod]
        public void RecordStringWithNestedArray()
        {
            const string testRecordString = "entry=[\"1\",{inner=\"2\"},[\"11\",\"22\",[\"333\",\"444\",]]]";
            var targetParser = new ResultRecordParser();

            var resultDict = targetParser.Parse(testRecordString);

            var testSequence = new Action[]
            {
                () => Assert.IsTrue(resultDict["entry"] is List<object>),
                () => Assert.AreEqual(3, ((dynamic)resultDict["entry"]).Count),
                () => Assert.AreEqual("1", ((dynamic)resultDict["entry"])[0]),
                () => Assert.AreEqual("2", ((dynamic)resultDict["entry"])[1]["inner"]),
                () => Assert.AreEqual("11", ((dynamic)resultDict["entry"])[2][0]),
                () => Assert.AreEqual("22", ((dynamic)resultDict["entry"])[2][1]),
                () => Assert.AreEqual("333", ((dynamic)resultDict["entry"])[2][2][0]),
                () => Assert.AreEqual("444", ((dynamic)resultDict["entry"])[2][2][1]),
            };
            AssertHelpers.ExecuteTestSequence(testSequence);
        }
    }

}
