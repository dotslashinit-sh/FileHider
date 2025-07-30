namespace FileHider.Tests
{
    using FileHider.Lib;
    [TestClass]
    public sealed class FileHiderLibTests
    {
        [TestClass]
        public class FileListTests
        {
            DirectoryItem rootDir = DirectoryItem.RootDir;
            DirectoryItem dirInRoot;
            DirectoryItem dirInDir;

            public FileListTests()
            {
                rootDir = DirectoryItem.RootDir;
                dirInRoot = DirectoryItem.Create("dir", rootDir);
                dirInDir = DirectoryItem.Create("dir2", dirInRoot);

                var f = FileItem.Create("FileHider.sln", dirInDir);
            }

            [TestMethod]
            public void ItemDirectoryTestRoot1()
            {
                TestPath(rootDir.GetVirtualPath(), "");
            }

            [TestMethod]
            public void ItemDirectoryTest2()
            {
                TestPath(dirInRoot.GetVirtualPath(), "dir");
            }

            [TestMethod]
            public void ItemDirectoryTest3()
            {
                TestPath(dirInDir.GetVirtualPath(), "dir/dir2");
            }

            [TestMethod]
            public void DirectoryCountTest1()
            {
                Assert.IsTrue(rootDir.Children.Count == 1);
            }

            [TestMethod]
            public void DirectoryCountTest2()
            {
                Assert.IsTrue(dirInRoot.Children.Count == 1);
            }

            [TestMethod]
            public void DirectoryCountTest3()
            {
                Assert.IsTrue(dirInDir.Children.Count == 1);
            }

            //[TestMethod]
            //public void DirectoryClearTest()
            //{
            //    dirInDir.Delete();
            //    Assert.IsTrue(dirInDir.Children.Count == 0);
            //}

            void TestPath(string path1, string path2)
            {
                if (System.OperatingSystem.IsWindows())
                {
                    path1 = path1.Replace("/", "\\");
                    path2 = path2.Replace("/", "\\");
                }
                Assert.IsTrue(path1 == path2);
            }

        }

    }
}
