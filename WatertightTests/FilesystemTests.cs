using NUnit.Framework;
using System;
using System.Collections.Generic;
using Watertight.Filesystem;
using Watertight.ResourceLoaders;

namespace WatertightTests
{
    [TestFixture]
    class FilesystemTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(FileSystem).TypeHandle);

        }

        [OneTimeTearDown]
        public void TearDown()
        {

        }

     
        [Test]
        public void TestFindingScripts()
        {
            List<ResourcePtr> Ptrs = FileSystem.ScanForAssetType("ascript");

            ObjectScriptFactory osf = new ObjectScriptFactory();

            Has.One.Member(new ResourcePtr("ascript:FileSystemTests/Player.ascript"));
        }

    }
}
