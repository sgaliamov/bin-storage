using System;
using System.IO;
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
		public void AddChildren_Test() {
			var expected = Test(
				storage => {
					var root = storage.GetRoot();
					var child1 = storage.NewNode();
					storage.Commit(child1);
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

					return child2;
				});

			var actual = Test(
				storage => {
					var root = storage.GetRoot();
					return storage.GetChildren(root, 1);
				});

			Check(expected, actual);
		}

		[TestMethod]
		public void InsertChildren_Test() {
			Test(
				storage => {
					var node = storage.NewNode();
					node.ChildrensCount = 2;
					node.Childrens[0] = 5;
					node.Childrens[1] = 6;
					storage.InsertChildren(node, 0, new PersistentNode(7, TestDegree));
					storage.InsertChildren(node, 1, new PersistentNode(8, TestDegree));
					storage.InsertChildren(node, 3, new PersistentNode(9, TestDegree));
					storage.Commit(node);

					storage.AddChildren(storage.GetRoot(), node);
				});

			var actual = Test(
				storage => {
					var root = storage.GetRoot();
					return storage.GetChildren(root, 0);
				});

			Assert.AreEqual(5, actual.ChildrensCount);
			Assert.AreEqual(7, actual.Childrens[0]);
			Assert.AreEqual(8, actual.Childrens[1]);
			Assert.AreEqual(5, actual.Childrens[2]);
			Assert.AreEqual(9, actual.Childrens[3]);
			Assert.AreEqual(6, actual.Childrens[4]);
		}

		[TestMethod]
		public void InsertKey_Test() {
			Test(
				storage => {
					var node = storage.NewNode();
					node.KeysCount = 1;
					node.Keys[0] = new KeyInfo { Offset = 3 };
					storage.InsertKey(node, 0, new KeyInfo { Offset = 7 });
					storage.InsertKey(node, 1, new KeyInfo { Offset = 9 });
					storage.InsertKey(node, 3, new KeyInfo { Offset = 8 });
					storage.Commit(node);

					storage.AddChildren(storage.GetRoot(), node);
				});

			var actual = Test(
				storage => {
					var root = storage.GetRoot();
					return storage.GetChildren(root, 0);
				});

			Assert.AreEqual(4, actual.KeysCount);
			Assert.AreEqual(7, actual.Keys[0].Offset);
			Assert.AreEqual(9, actual.Keys[1].Offset);
			Assert.AreEqual(3, actual.Keys[2].Offset);
			Assert.AreEqual(8, actual.Keys[3].Offset);
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

		private static void Check(PersistentNode expected, PersistentNode actual) {
			Assert.AreEqual(expected.ChildrensCount, actual.ChildrensCount);
			Assert.AreEqual(expected.KeysCount, actual.KeysCount);
			Assert.AreEqual(expected.Offset, actual.Offset);
			for(var i = 0; i < TestDegree * 2; i++) {
				Assert.AreEqual(expected.Childrens[i], actual.Childrens[i]);
			}
			for(var i = 0; i < TestDegree * 2 - 1; i++) {
				Assert.AreEqual(expected.Keys[i].Offset, actual.Keys[i].Offset);
				Assert.AreEqual(expected.Keys[i].Size, actual.Keys[i].Size);
			}
		}
	}

}