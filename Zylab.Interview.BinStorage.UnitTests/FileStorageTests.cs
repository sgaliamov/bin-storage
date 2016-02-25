using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;
using Zylab.Interview.BinStorage.Storage;

namespace Zylab.Interview.BinStorage.UnitTests {

	[TestClass]
	public class FileStorageTests {
		private string _storageFilePath;

		[TestInitialize]
		public void TestInitialize() {
			_storageFilePath = Path.GetTempFileName();
			File.Delete(_storageFilePath);
		}

		[TestCleanup]
		public void TestCleanup() {
			File.Delete(_storageFilePath);
		}

		[TestMethod]
		public void Create_Test() {
			var data = Guid.NewGuid().ToByteArray();

			IndexData indexData;
			using(var target = new FileStorage(_storageFilePath, 12)) {
				var inputStream = new MemoryStream(data);

				indexData = target.Append(inputStream);
			}

			using(var file = new FileStream(_storageFilePath, FileMode.Open)) {
				var tempStream = new MemoryStream();
				file.CopyTo(tempStream);
				var actual = tempStream.ToArray();

				Check(data, 0, actual, indexData, file.Length);
			}
		}

		[TestMethod]
		public void Append_Test() {
			using(var target = new FileStorage(_storageFilePath)) {
				var ms = new MemoryStream();
				ms.Write(Guid.NewGuid().ToByteArray(), 0, 16);
				ms.Write(Guid.NewGuid().ToByteArray(), 0, 16);
				ms.Position = 0;
				target.Append(ms);
			}

			var data = Guid.NewGuid().ToByteArray();
			IndexData indexData;
			using(var target = new FileStorage(_storageFilePath)) {
				var inputStream = new MemoryStream(data);
				indexData = target.Append(inputStream);
			}

			using(var file = new FileStream(_storageFilePath, FileMode.Open)) {
				var tempStream = new MemoryStream();
				file.CopyTo(tempStream);
				var actual = new byte[16];
				tempStream.Position = 32;
				tempStream.Read(actual, 0, actual.Length);

				Check(data, 32, actual, indexData, file.Length);
			}
		}

		[TestMethod]
		public void Append_Empty_Test() {
			using(var target = new FileStorage(_storageFilePath)) {
				var indexData = target.Append(new MemoryStream());
				Assert.IsNull(indexData);
			}

			Assert.AreEqual(0, new FileInfo(_storageFilePath).Length);
		}

		[TestMethod]
		public void Get_Test() {
			using(var target = new FileStorage(_storageFilePath)) {
				var list = new List<KeyValuePair<IndexData, byte[]>>();
				for(var i = 0; i < 10; i++) {
					var data = Guid.NewGuid().ToByteArray();
					var indexData = target.Append(new MemoryStream(data));
					list.Add(new KeyValuePair<IndexData, byte[]>(indexData, data));
				}

				foreach(var item in list.OrderBy(x => Guid.NewGuid())) {
					using(var stream = target.Get(item.Key)) {
						var ms = new MemoryStream();
						stream.CopyTo(ms);
						var buffer = ms.ToArray();

						Assert.IsTrue(item.Value.SequenceEqual(buffer));
					}
				}
			}
		}

		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		private static void Check(
			byte[] inputData,
			int expectedOffset,
			byte[] actualData,
			IndexData indexData,
			long fileLength) {
			Assert.IsTrue(inputData.SequenceEqual(actualData));
			Assert.AreEqual(fileLength, indexData.Offset + indexData.Size);

			using(var md5 = MD5.Create()) {
				var inputStream = new MemoryStream(inputData);
				var hash = md5.ComputeHash(inputStream);
				Assert.AreEqual(expectedOffset, indexData.Offset);
				Assert.AreEqual(inputData.Length, indexData.Size);
				Assert.IsTrue(hash.SequenceEqual(indexData.Md5Hash));
			}
		}
	}

}