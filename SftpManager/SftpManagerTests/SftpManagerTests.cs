using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using System.IO;

namespace SftpManager.Tests
{
    [TestClass()]
    public class SftpManagerTests
    {
        const string host = "transfer.remit.trayport.com";
        const int port = 22;
        const string user = "mar.axpoitalia";
        const string pass = "6Hh9FSaX8qxT";

        [TestMethod()]
        public void ListDirectoryTest()
        {
            string path = "/";
            var res = SftpManager.ListDirectory(host, port, user, pass, path);
            int cnt = 0;
            foreach (var tup in res)
            {
                Console.WriteLine($"{cnt++} {tup.Item1}  {tup.Item2}");
            }
            Assert.IsNotNull(res);
            Assert.IsTrue(cnt > 0);
        }

        [TestMethod()]
        public void GetAndUnzipTest()
        {
            string inPath = "/";
            string outPath = @"H:\My Documents\Downloads";
            string filename = "mar.axpoitalia_Ipts-MAR-161001.csv.zip";
            var res = SftpManager.GetAndUnzip(host, port, user, pass, filename, inPath, outPath);
            Assert.IsNotNull(res);
        }

        [TestMethod()]
        public void GetAndUnzipArchiveTimestampTest()
        {
            string inPath = "/";
            string outPath = @"H:\My Documents\Downloads";
            string filename = "mar.axpoitalia_Ipts-MAR-161001.csv.zip";
            var res = SftpManager.GetAndUnzip(host, port, user, pass, filename, inPath, outPath, true);
            Assert.IsNotNull(res);
        }

        [TestMethod()]
        public void GetTest()
        {
            string inPath = "/";
            string outPath = @"H:\My Documents\Downloads";
            string filename = "mar.axpoitalia_Ipts-MAR-161001.csv.zip";
            var res = SftpManager.Get(host, port, user, pass, filename, inPath, outPath);
            Assert.IsNotNull(res);
        }
    }
}