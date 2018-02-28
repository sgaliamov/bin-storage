using System.IO;
using System.IO.MemoryMappedFiles;
using BinStorage.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinStorage.UnitTests.Storage {

	[TestClass]
	public class FakeStreamTests {
		[TestMethod]
		public void Test() {
			var filePath = Path.GetTempFileName();

			var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.OpenOrCreate, null, Sizes.Size1Kb);
			var stream = new FakeStream(
				(dataOffset, dataSize, buffer, offset, count) => {
					// ReSharper disable once AccessToDisposedClosure
					using(var reader = mmf.CreateViewStream(dataOffset, dataSize, MemoryMappedFileAccess.Read)) {
						return reader.Read(buffer, offset, count);
					}
				},
				100,
				23);

			var bytes = new byte[10];
			var read = stream.Read(bytes, 0, 10);
			Assert.AreEqual(10, read);
			read = stream.Read(bytes, 0, 10);
			Assert.AreEqual(10, read);
			read = stream.Read(bytes, 0, 10);
			Assert.AreEqual(3, read);
			read = stream.Read(bytes, 0, 10);
			Assert.AreEqual(0, read);

			mmf.Dispose();
			File.Delete(filePath);
		}
	}

}