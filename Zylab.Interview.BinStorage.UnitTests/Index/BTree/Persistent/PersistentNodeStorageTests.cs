using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index.BTree.Persistent;

namespace Zylab.Interview.BinStorage.UnitTests.Index.BTree.Persistent {

	[TestClass]
	public class PersistentNodeStorageTests {
		private const int TestDegree = 3;
		private string _indexFilePath;
		private int _nodeSize;

		[TestInitialize]
		public void TestInitialize() {
			_indexFilePath = Path.GetTempFileName();
			File.Delete(_indexFilePath);
			_nodeSize = TestDegree * 2 * sizeof(long) + (TestDegree * 2 - 1) * (sizeof(long) + sizeof(long));
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
		public void Compare_Test() {
			Assert.Fail();
		}

		[TestMethod]
		public void GetChildren_Test() {
			Test(
				storage => {
					var root = storage.GetRoot();
					var child1 = storage.NewNode();
					var child2 = storage.NewNode();
					var child21 = storage.NewNode();
					var child22 = storage.NewNode();
					var child23 = storage.NewNode();
					storage.AddChildren(child2, child21);
					storage.AddChildren(child2, child22);
					storage.AddChildren(child2, child23);
					storage.Commit(child2);

					storage.AddChildren(root, child1);
					storage.AddChildren(root, child2);
				});

			var actual = Test(
				storage => {
					var root = storage.GetRoot();
					return storage.GetChildren(root, 1);
				});
		}

		[TestMethod]
		public void InsertChildren_Test() {
			var node = new PersistentNode(100, TestDegree) {
				KeysCount = 1,
				ChildrensCount = 2,
				Keys = { [1] = new KeyInfo { Offset = 3, Size = 4 } },
				Childrens = {
					[0] = 5,
					[1] = 6
				}
			};

			Test(
				storage => {
					storage.NewNode();
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
				Keys = { [1] = new KeyInfo { Offset = 3, Size = 4 } },
				Childrens = {
					[0] = 5,
					[1] = 6
				}
			};

			Test(
				storage => {
					storage.InsertKey(node, 0, new KeyInfo { Size = 7 });
					storage.InsertKey(node, 2, new KeyInfo { Size = 8 });
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