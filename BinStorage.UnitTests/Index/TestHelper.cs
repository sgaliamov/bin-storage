using System.Collections.Concurrent;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.UnitTests.Index {

	[TestClass]
	public class TestHelper {
		public static void AreEqual(IndexData expected, IndexData actual) {
			Assert.AreEqual(expected.Size, actual.Size);
			Assert.AreEqual(expected.Offset, actual.Offset);
			Assert.IsTrue(expected.Md5Hash.SequenceEqual(actual.Md5Hash));
		}

		[TestMethod]
		public void ParallelMemoryMappedFile_Test() {
			const int count = 10000;
			const long capacity = 50000;
			const int size = 5;
			var ints = Enumerable.Range(0, count).Select(x => x.ToString("00000")).ToArray();
			var pos = 0;
			var filePath = Path.GetTempFileName();
			var bag = new ConcurrentBag<int>();

			using(var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.OpenOrCreate, null, capacity)) {
				ints.AsParallel()
					.WithDegreeOfParallelism(4)
					.ForAll(
						x => {
							var buffer = Encoding.UTF8.GetBytes(x);
							// ReSharper disable once AccessToModifiedClosure
							var offset = Interlocked.Add(ref pos, buffer.Length) - buffer.Length;
							if(bag.Contains(offset)) {
								Assert.Fail("offset used");
							}
							bag.Add(offset);

							// ReSharper disable once AccessToDisposedClosure
							using(var stream = mmf.CreateViewStream(offset, buffer.Length)) {
								stream.Write(buffer, 0, buffer.Length);
							}
						});
			}

			var actual = new string[ints.Length];
			using(var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, capacity)) {
				Enumerable.Range(0, count)
					.AsParallel()
					.WithDegreeOfParallelism(4)
					.ForAll(
						x => {
							var buffer = new byte[size];
							// ReSharper disable once AccessToDisposedClosure
							using(var stream = mmf.CreateViewStream(x * size, buffer.Length)) {
								stream.Read(buffer, 0, buffer.Length);
								actual[x] = Encoding.UTF8.GetString(buffer);
							}
						});

				Assert.IsTrue(ints.SequenceEqual(actual.OrderBy(x => x)));
			}

			File.Delete(filePath);
		}
	}

}