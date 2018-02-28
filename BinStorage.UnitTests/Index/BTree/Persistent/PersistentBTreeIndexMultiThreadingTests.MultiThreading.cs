using System;
using System.IO;
using BinStorage.Index;
using BinStorage.Index.BTree;
using BinStorage.Index.BTree.Persistent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinStorage.UnitTests.Index.BTree.Persistent {

	[TestClass]
	public class PersistentBTreeIndexMultiThreadingTests : MultiThreadingTests {
		private const int TestDegree = 4;
		private const int TestCapacity = Sizes.Size10Kb;
		private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);
		private string _indexFilePath;
		private PersistentNodeStorage _nodeStorage;

		[TestInitialize]
		public void TestInitialize() {
			_indexFilePath = Path.GetTempFileName();
			_nodeStorage = new PersistentNodeStorage(_indexFilePath, TestCapacity, TestDegree);
		}

		[TestCleanup]
		public void TestCleanup() {
			_nodeStorage.Dispose();
			File.Delete(_indexFilePath);
		}

		protected override IIndex Create() {
			return new ThreadSafeIndex(new BTreeIndex<PersistentNode, KeyInfo>(_nodeStorage), _timeout);
		}
	}

}