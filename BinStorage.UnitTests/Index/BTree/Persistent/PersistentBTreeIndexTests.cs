using System.IO;
using BinStorage.Index;
using BinStorage.Index.BTree;
using BinStorage.Index.BTree.Persistent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinStorage.UnitTests.Index.BTree.Persistent {

	[TestClass]
	public class PersistentBTreeIndexTests : IndexTests {
		private const int TestDegree = 5;
		private const int TestCapacity = Sizes.Size1Kb;
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
			return new BTreeIndex<PersistentNode, KeyInfo>(_nodeStorage);
		}
	}

}