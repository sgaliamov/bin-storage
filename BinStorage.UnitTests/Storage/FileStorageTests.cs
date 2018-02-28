using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BinStorage.Index;
using BinStorage.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinStorage.UnitTests.Storage {

	[TestClass]
	public class FileStorageTests {
		private const int TestCapacity = Sizes.Size1Kb;
		private const int PositionHolderSize = sizeof(long);
		private const int SizeOfGuid = 16;
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
			using(var target = new FileStorage(_storageFilePath, TestCapacity, 12)) {
				var inputStream = new MemoryStream(data);

				indexData = target.Append(inputStream);
			}

			using(var file = new FileStream(_storageFilePath, FileMode.Open)) {
				var position = ReadAndCheckPosition(file, SizeOfGuid);

				var actual = new byte[SizeOfGuid];
				file.Read(actual, 0, SizeOfGuid);

				Check(data, PositionHolderSize, actual, indexData, position);
			}
		}

		[TestMethod]
		public void Append_Test() {
			using(var target = new FileStorage(_storageFilePath, TestCapacity)) {
				var ms = new MemoryStream();
				ms.Write(Guid.NewGuid().ToByteArray(), 0, SizeOfGuid);
				ms.Write(Guid.NewGuid().ToByteArray(), 0, SizeOfGuid);
				ms.Position = 0;
				target.Append(ms);
			}

			var data = Guid.NewGuid().ToByteArray();
			IndexData indexData;
			using(var target = new FileStorage(_storageFilePath, TestCapacity)) {
				indexData = target.Append(new MemoryStream(data));
			}

			using(var file = new FileStream(_storageFilePath, FileMode.Open)) {
				var position = ReadAndCheckPosition(file, SizeOfGuid * 3);

				var actual = new byte[SizeOfGuid];
				file.Position = SizeOfGuid * 2 + PositionHolderSize;
				file.Read(actual, 0, actual.Length);

				Check(data, PositionHolderSize + SizeOfGuid * 2, actual, indexData, position);
			}
		}

		[TestMethod]
		public void Appent_OverCapacity_Test() {
			var data = new[] { Guid.NewGuid().ToByteArray(), Guid.NewGuid().ToByteArray(), Guid.NewGuid().ToByteArray() };
			const int capacity = SizeOfGuid + SizeOfGuid / 2;

			using(var target = new FileStorage(_storageFilePath, capacity)) {
				foreach(var item in data) {
					target.Append(new MemoryStream(item));
				}
			}

			using(var target = new FileStorage(_storageFilePath, capacity)) {
				var offset = PositionHolderSize;
				foreach(var item in data) {
					MemoryStream ms;
					using(var stream = target.Get(
						new IndexData {
							Offset = offset,
							Md5Hash = null,
							Size = item.Length
						})) {
						ms = new MemoryStream();
						stream.CopyTo(ms);
					}
					offset += SizeOfGuid;

					Assert.IsTrue(item.SequenceEqual(ms.ToArray()));
				}
			}
		}

		[TestMethod]
		public void Get_AfterResize_Test() {
			var data = new[] { Guid.NewGuid().ToByteArray(), Guid.NewGuid().ToByteArray() };
			const int capacity = SizeOfGuid + SizeOfGuid / 3;

			using(var target = new FileStorage(_storageFilePath, capacity)) {
				var indexData = target.Append(new MemoryStream(data[0]));
				using(var stream = target.Get(indexData)) {
					target.Append(new MemoryStream(data[1]));

					var ms = new MemoryStream();
					stream.CopyTo(ms);
					Assert.IsTrue(data[0].SequenceEqual(ms.ToArray()));
				}
			}
		}

		[TestMethod]
		public void Appent_BigData_Test() {
			var bigData = Guid.NewGuid().ToByteArray();
			const int capacity = SizeOfGuid / 4;

			using(var target = new FileStorage(_storageFilePath, capacity)) {
				target.Append(new MemoryStream(bigData));
			}

			using(var target = new FileStorage(_storageFilePath, 0))
			using(var stream = target.Get(
				new IndexData {
					Offset = PositionHolderSize,
					Md5Hash = null,
					Size = SizeOfGuid
				})) {
				var ms = new MemoryStream();
				stream.CopyTo(ms);

				Assert.IsTrue(bigData.SequenceEqual(ms.ToArray()));
			}

			using(var file = new FileStream(_storageFilePath, FileMode.Open)) {
				var position = ReadAndCheckPosition(file, SizeOfGuid);
				Assert.AreEqual(position, file.Length);
			}
		}

		[TestMethod]
		public void Append_Empty_Test() {
			using(var target = new FileStorage(_storageFilePath, TestCapacity)) {
				var indexData = target.Append(new MemoryStream());
				using(var stream = target.Get(indexData)) {
					Assert.AreEqual(0, stream.Length);
				}
			}
			using(var file = new FileStream(_storageFilePath, FileMode.Open)) {
				ReadAndCheckPosition(file, 0);
			}

			Assert.AreEqual(TestCapacity + PositionHolderSize, new FileInfo(_storageFilePath).Length);
		}

		[TestMethod]
		public void Get_Test() {
			using(var target = new FileStorage(_storageFilePath, TestCapacity)) {
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

		[TestMethod]
		public void Parallel_Test() {
			const int count = 10000;
			var dictionary = Enumerable.Range(0, count).ToDictionary(x => x, x => x.ToString("00000"));
			var indexes = new IndexData[dictionary.Count];
			const int degreeOfParallelism = 4;

			using(var target = new FileStorage(_storageFilePath, TestCapacity)) {
				dictionary.AsParallel()
					.WithDegreeOfParallelism(degreeOfParallelism)
					.ForAll(
						pair => {
							var bytes = Encoding.UTF8.GetBytes(pair.Value);
							// ReSharper disable once AccessToDisposedClosure
							var indexData = target.Append(new MemoryStream(bytes));
							indexes[pair.Key] = indexData;
						});
			}

			using(var target = new FileStorage(_storageFilePath, TestCapacity)) {
				Enumerable.Range(0, count)
					.AsParallel()
					.WithDegreeOfParallelism(degreeOfParallelism)
					.ForAll(
						i => {
							// ReSharper disable once AccessToDisposedClosure
							using(var stream = target.Get(indexes[i])) {
								var ms = new MemoryStream();
								stream.CopyTo(ms);
								var buffer = ms.ToArray();

								var actual = Encoding.UTF8.GetString(buffer);
								Assert.AreEqual(dictionary[i], actual, "Wrong data");
							}
						});
			}
		}

		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		private static void Check(
			byte[] expectedData,
			int expectedOffset,
			byte[] actualData,
			IndexData actualIndexData,
			long actualPosition) {
			Assert.IsTrue(expectedData.SequenceEqual(actualData));
			Assert.AreEqual(actualPosition, actualIndexData.Offset + actualIndexData.Size);

			using(var md5 = MD5.Create()) {
				var inputStream = new MemoryStream(expectedData);
				var hash = md5.ComputeHash(inputStream);
				Assert.AreEqual(expectedOffset, actualIndexData.Offset);
				Assert.AreEqual(expectedData.Length, actualIndexData.Size);
				Assert.IsTrue(hash.SequenceEqual(actualIndexData.Md5Hash));
			}
		}

		private static long ReadAndCheckPosition(Stream stream, int dataLength) {
			stream.Position = 0;
			var buffer = new byte[PositionHolderSize];
			stream.Read(buffer, 0, buffer.Length);
			var position = BitConverter.ToInt64(buffer, 0);
			Assert.AreEqual(dataLength + PositionHolderSize, position);

			return position;
		}
	}

}