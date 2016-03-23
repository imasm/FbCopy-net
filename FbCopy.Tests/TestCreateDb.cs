using System.IO;
using FbCopy.Firebird;
using NUnit.Framework;

namespace FbCopy.Tests
{
    [TestFixture]
    public class TestCreateDb: TestDbBase
    {
        [Test]
        public void Create()
        {
            CreateSourceDb();

            var cstr = GetSourceConnectionString();
            using (var db = new DbConnection(cstr))
            {
                Assert.IsTrue(db.Open());
            }

            Assert.IsTrue(File.Exists(SourceDbPath));
        }
    }
}
