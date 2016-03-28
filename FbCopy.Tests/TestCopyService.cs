using System;
using System.Collections.Generic;
using System.IO;
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
            copyOptions.Source = "SYSDBA:masterkey@localhost:" + SourceDbPath;
            copyOptions.Destination = "SYSDBA:masterkey@localhost:" + DestDbPath;
            copyOptions.Verbose = true;

            UserRepository repos = new UserRepository(GetSourceConnectionString());

            List<User> sourceList = repos.CreateFakes(5);
            repos.InsertUsers(sourceList);
            
            string inputString = "#T:USERS:ID,NAME,RATING,WEIGHT, ACTIVE,DDATE,DDAY,LAST_RATING:::" + Environment.NewLine;
            var stringReader = new StringReader(inputString);

            CopyService service = new CopyService(copyOptions);
            service.Run(stringReader);

            repos = new UserRepository(GetDestConnectionString());
            var desList = repos.GetAllUsers();

            Assert.AreEqual(sourceList.Count, desList.Count);
            for (int i = 0; i < sourceList.Count; i++)
                Assert.AreEqual(sourceList[i], desList[i], "Users are not equal");
        }
    }
}

