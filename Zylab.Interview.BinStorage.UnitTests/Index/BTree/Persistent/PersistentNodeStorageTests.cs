using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index.BTree.Persistent;

namespace Zylab.Interview.BinStorage.UnitTests.Index.BTree.Persistent {

	[TestClass]
	public class PersistentNodeStorageTests {
		private const int TestDegree = 3;
		private string _indexFilePath;

		[TestInitialize]
		public void TestInitialize() {
			_indexFilePath = Path.GetTempFileName();
			File.Delete(_indexFilePath);
		}

		private void Test(Action<PersistentNodeStorage> action) {
			using(var target = new PersistentNodeStorage(_indexFilePath, 0x400000, TestDegree)) {
				action(target);
			}
		}

		private T Test<T>(Func<PersistentNodeStorage, T> action) {
			using(var target = new PersistentNodeStorage(_indexFilePath, 0x400000, TestDegree)) {
				return action(target);
			}
		}

		[TestCleanup]
		public void TestCleanup() {
			File.Delete(_indexFilePath);
		}

		[TestMethod]
		public void Commit_Test() {
			var node = new PersistentNode(100, TestDegree) {
				KeysCount = 1,
				ChildrensCount = 2,
				Keys = { [1] = new KeyData { Offset = 3, Size = 4 } },
				Childrens = {
					[0] = 5,
					[1] = 6
				}
			};

			Test(storage => storage.Commit(node));

			using(var file = File.Open(_indexFilePath, FileMode.Open)) {
				var bytes = new byte[1000];
				file.Read(bytes, 100, bytes.Length);
				var ms = new MemoryStream(bytes);
				var formatter = new BinaryFormatter();
				var actual = (PersistentNode)formatter.Deserialize(ms);

				Assert.AreEqual(node.KeysCount, actual.KeysCount);
				Assert.AreEqual(node.ChildrensCount, actual.ChildrensCount);
				for(var i = 0; i < actual.KeysCount; i++) {
					Assert.AreEqual(node.Keys[i].Offset, actual.Keys[i].Offset);
					Assert.AreEqual(node.Keys[i].Size, actual.Keys[i].Size);
				}
				Assert.IsTrue(node.Childrens.SequenceEqual(actual.Childrens));
			}
		}

		[TestMethod]
		public void Compare_Test() {
			Assert.Fail();
		}

		[TestMethod]
		public void GetChildren_Test() {
			Assert.Fail();
		}

		[TestMethod]
		public void InsertChildren_Test() {
			var node = new PersistentNode(100, TestDegree) {
				KeysCount = 1,
				ChildrensCount = 2,
				Keys = { [1] = new KeyData { Offset = 3, Size = 4 } },
				Childrens = {
					[0] = 5,
					[1] = 6
				}
			};

			Test(
				storage => {
					storage.InsertChildren(node, 1, new PersistentNode(7, TestDegree));
					storage.InsertChildren(node, 3, new PersistentNode(8, TestDegree));
				});

			Assert.AreEqual(4, node.ChildrensCount);
			Assert.AreEqual(7, node.Childrens[1]);
			Assert.AreEqual(8, node.Childrens[3]);
		}

		[TestMethod]
		public void InsertKey_Test() {
			var node = new PersistentNode(100, TestDegree) {
				KeysCount = 1,
				ChildrensCount = 2,
				Keys = { [1] = new KeyData { Offset = 3, Size = 4 } },
				Childrens = {
					[0] = 5,
					[1] = 6
				}
			};

			Test(
				storage => {
					storage.InsertKey(node, 0, new KeyData { Size = 7 });
					storage.InsertKey(node, 2, new KeyData { Size = 8 });
				});

			Assert.AreEqual(3, node.Keys);
			Assert.AreEqual(7, node.Keys[1].Offset);
			Assert.AreEqual(8, node.Keys[3].Offset);
		}

		[TestMethod]
		public void MoveRightHalfChildrens_Test() {
			Assert.Fail();
		}

		[TestMethod]
		public void MoveRightHalfKeys_Test() {
			Assert.Fail();
		}

		[TestMethod]
		public void NewKey_Test() {
			Assert.Fail();
		}

		[TestMethod]
		public void SearchPosition_Test() {
			Assert.Fail();
		}
	}

}