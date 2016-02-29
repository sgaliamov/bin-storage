using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;
using Zylab.Interview.BinStorage.Index.BTree;
using Zylab.Interview.BinStorage.Index.BTree.InMemory;

namespace Zylab.Interview.BinStorage.UnitTests.Index.BTree.InMemory {

	[TestClass]
	public class BTreeIndexTests : IndexTests {
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