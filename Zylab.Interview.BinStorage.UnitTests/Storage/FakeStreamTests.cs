using System.IO;
using System.IO.MemoryMappedFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Storage;

namespace Zylab.Interview.BinStorage.UnitTests.Storage {

	[TestClass]
	public class FakeStreamTests {
		[TestMethod]
		public void Test() {
			var filePath = Path.GetTempFileName();

			using(var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.OpenOrCreate, null, Sizes.Size1Kb)) {
				var stream = new FakeStream(mmf, 100, 23);
				var buffer = new byte[10];

				var read = stream.Read(buffer, 0, 10);
				Assert.AreEqual(10, read);
				read = stream.Read(buffer, 0, 10);
				Assert.AreEqual(10, read);
				read = stream.Read(buffer, 0, 10);
				Assert.AreEqual(3, read);
				read = stream.Read(buffer, 0, 10);
				Assert.AreEqual(0, read);
			}

			File.Delete(filePath);
		}
	}

}