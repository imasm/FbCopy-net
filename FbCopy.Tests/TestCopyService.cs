using System;
using System.Collections.Generic;
using System.IO;
using FbCopy.Firebird;
using NUnit.Framework;

namespace FbCopy.Tests
{
    [TestFixture]
    public class TestCopyService : TestDbBase
    {
        [Test]
        public void TestCopy()
        {
            CreateSourceDb();
            CreateDestDb();

            CopyOptions copyOptions = new CopyOptions();
            copyOptions.Source = "localhost:" + SourceDbPath;
            copyOptions.Destination = "localhost:" + DestDbPath;
            copyOptions.Verbose = true;

            List<User> sourceList = new List<User>();
            sourceList.Add(new User(1, "user1"));

            UserRepository repos = new UserRepository(GetSourceConnectionString());
            repos.InsertUsers(sourceList);
            
            string inputString = "#T:USERS:ID,NAME:::" + Environment.NewLine;
            var stringReader = new StringReader(inputString);

            CopyService service = new CopyService(copyOptions);
            service.Run(stringReader);

            repos = new UserRepository(GetDestConnectionString());
            var desList = repos.GetAllUsers();

            Assert.AreEqual(sourceList.Count, desList.Count);
            for (int i = 0; i < sourceList.Count; i++)
                Assert.AreEqual(sourceList[i], desList[i]);
        }
    }
}

