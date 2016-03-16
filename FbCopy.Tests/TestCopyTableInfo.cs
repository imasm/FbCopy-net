using NUnit.Framework;

namespace FbCopy.Tests
{
    [TestFixture]
    public class TestCopyTableInfo
    {
        [Test]
        public void TestBuildSelect()
        {
            CopyTableInfo copyTableInfo = new CopyTableInfo("table",
                new[] { "first", "second" },
                "",
                new[] { "first" });
            Assert.AreEqual("SELECT first,second FROM table", copyTableInfo.BuildSelectStatement());
        }

        [Test]
        public void TestBuildInsert()
        {
            CopyTableInfo copyTableInfo = new CopyTableInfo("table",
                new[] { "first", "second" },
                "",
                new[] { "first" });
            Assert.AreEqual("INSERT INTO table (first,second) VALUES (?,?)", copyTableInfo.BuildInsertStatement());
        }

        [Test]
        public void TestBuildUpdate()
        {
            CopyTableInfo copyTableInfo = new CopyTableInfo("table",
                new[] { "first", "second" },
                "",
                new[] { "first" });
            Assert.AreEqual("UPDATE table SET first=?,second=? WHERE (first=?)", copyTableInfo.BuildUpdateStatement());
        }

        [Test]
        public void TestBuildUpdateOrInsert()
        {
            CopyTableInfo copyTableInfo = new CopyTableInfo("table",
                new[] { "first", "second" },
                "",
                new[] { "first" });
            Assert.AreEqual("UPDATE OR INSERT INTO table (first,second) VALUES (?,?) MATCHING (first)", copyTableInfo.BuildUpdateOrInsertStatement());
        }
    }
}
