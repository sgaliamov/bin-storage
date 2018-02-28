using System;
using BinStorage.Index;
using BinStorage.Index.BTree;
using BinStorage.Index.BTree.InMemory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinStorage.UnitTests.Index.BTree.InMemory {

	[TestClass]
	public class InMemoryBTreeIndexMultiThreadingTests : MultiThreadingTests {
		private const int TestDegree = 6;
		private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);
		private InMemoryNodeStorage _nodeStorage;

		[TestInitialize]
		public void TestInitialize() {
			_nodeStorage = new InMemoryNodeStorage(TestDegree);
		}

		[TestCleanup]
		public void TestCleanup() {
			_nodeStorage.Dispose();
		}

		protected override IIndex Create() {
			return new ThreadSafeIndex(new BTreeIndex<InMemoryNode, IndexDataKey>(_nodeStorage), _timeout);
		}
	}

}