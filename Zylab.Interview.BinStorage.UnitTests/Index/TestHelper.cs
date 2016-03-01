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
			var ints = Enumerable.Range(0, 10).Select(x => x.ToString()).ToArray();
			var pos = 0;
			var filePath = Path.GetTempFileName();

			using(var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.OpenOrCreate, null, 10)) {
				ints.AsParallel()
					.WithDegreeOfParallelism(4)
					.ForAll(
						x => {
							var buffer = Encoding.UTF8.GetBytes(x);
							// ReSharper disable once AccessToModifiedClosure
							var p = Interlocked.Add(ref pos, buffer.Length) - buffer.Length;
							// ReSharper disable once AccessToDisposedClosure
							using(var stream = mmf.CreateViewStream(p, buffer.Length)) {
								stream.Write(buffer, 0, buffer.Length);
							}
						});
			}

			var actual = new string[ints.Length];
			using(var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 10)) {
				Enumerable.Range(0, 10)
					.AsParallel()
					.WithDegreeOfParallelism(4)
					.ForAll(
						x => {
							var buffer = new byte[1];
							// ReSharper disable once AccessToDisposedClosure
							using(var stream = mmf.CreateViewStream(x, buffer.Length)) {
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