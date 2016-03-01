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

		private void Process(Action<PersistentNodeStorage> action) {
			using(var target = new PersistentNodeStorage(_indexFilePath, 0x400000, TestDegree)) {
				action(target);
			}
		}

		private T Process<T>(Func<PersistentNodeStorage, T> action) {
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
			var expected = Process(
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

			var actual = Process(
				storage => {
					var root = storage.GetRoot();
					return storage.GetChildren(root, 1);
				});

			Check(expected, actual);
		}

		[TestMethod]
		public void InsertChildren_Test() {
			Process(
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

			var actual = Process(
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
			Process(
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

			var actual = Process(
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
			Process(
				storage => {
					var newNode = storage.NewNode();
					var fullNode = storage.NewNode();
					for(var i = 0; i < fullNode.Childrens.Length; i++) {
						fullNode.Childrens[i] = i + 1;
					}
					storage.MoveRightHalfChildrens(newNode, fullNode);

					var root = storage.GetRoot();
					storage.AddChildren(root, newNode);
					storage.AddChildren(root, fullNode);

					storage.Commit(root);
					storage.Commit(newNode);
					storage.Commit(fullNode);
				});

			Process(
				storage => {
					var root = storage.GetRoot();

					var prevFullNode = storage.GetChildren(root, 1);
					Assert.AreEqual(TestDegree, prevFullNode.ChildrensCount);
					for(var i = 0; i < TestDegree; i++) {
						Assert.AreEqual(i + 1, prevFullNode.Childrens[i]);
						Assert.AreEqual(0, prevFullNode.Childrens[2 * TestDegree - i - 1]);
					}

					var prevNewNode = storage.GetChildren(root, 0);
					Assert.AreEqual(TestDegree, prevNewNode.ChildrensCount);
					for(var i = 0; i < TestDegree; i++) {
						Assert.AreEqual(i + TestDegree + 1, prevNewNode.Childrens[i]);
						Assert.AreEqual(0, prevNewNode.Childrens[2 * TestDegree - i - 1]);
					}
				});
		}

		[TestMethod]
		public void MoveRightHalfKeys_Test() {
			Process(
				storage => {
					var newNode = storage.NewNode();
					var fullNode = storage.NewNode();
					for(var i = 0; i < fullNode.Keys.Length; i++) {
						fullNode.Keys[i].Size = i + 1;
						fullNode.Keys[i].Offset = i + 1;
					}
					storage.MoveRightHalfKeys(newNode, fullNode);

					var root = storage.GetRoot();
					storage.AddChildren(root, newNode);
					storage.AddChildren(root, fullNode);

					storage.Commit(root);
					storage.Commit(newNode);
					storage.Commit(fullNode);
				});

			Process(
				storage => {
					var root = storage.GetRoot();

					var prevFullNode = storage.GetChildren(root, 1);
					Assert.AreEqual(TestDegree - 1, prevFullNode.KeysCount);
					for(var i = 0; i < TestDegree - 1; i++) {
						Assert.AreEqual(i + 1, prevFullNode.Keys[i].Offset);
						Assert.AreEqual(i + 1, prevFullNode.Keys[i].Size);
						Assert.AreEqual(0, prevFullNode.Keys[2 * TestDegree - i - 2].Offset);
						Assert.AreEqual(0, prevFullNode.Keys[2 * TestDegree - i - 2].Size);
					}
					Assert.AreEqual(0, prevFullNode.Keys[TestDegree].Offset);
					Assert.AreEqual(0, prevFullNode.Keys[TestDegree].Size);

					var prevNewNode = storage.GetChildren(root, 0);
					Assert.AreEqual(TestDegree - 1, prevNewNode.KeysCount);
					for(var i = 0; i < TestDegree - 1; i++) {
						Assert.AreEqual(i + TestDegree + 1, prevNewNode.Keys[i].Offset);
						Assert.AreEqual(i + TestDegree + 1, prevNewNode.Keys[i].Size);
						Assert.AreEqual(0, prevNewNode.Keys[2 * TestDegree - i - 2].Offset);
						Assert.AreEqual(0, prevNewNode.Keys[2 * TestDegree - i - 2].Size);
					}
				});
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