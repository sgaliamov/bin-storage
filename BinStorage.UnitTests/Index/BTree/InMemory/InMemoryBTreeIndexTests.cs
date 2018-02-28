using BinStorage.Index;
using BinStorage.Index.BTree;
using BinStorage.Index.BTree.InMemory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinStorage.UnitTests.Index.BTree.InMemory {

	[TestClass]
	public class InMemoryBTreeIndexTests : IndexTests {
		private const int TestDegree = 5;
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
			return new BTreeIndex<InMemoryNode, IndexDataKey>(_nodeStorage);
		}
	}

}