using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;
using Zylab.Interview.BinStorage.Index.BTree;
using Zylab.Interview.BinStorage.Index.BTree.Persistent;

namespace Zylab.Interview.BinStorage.UnitTests.Index.BTree.Persistent {

	[TestClass]
	public class PersistentBTreeIndexTests : IndexTests {
		private const int TestDegree = 5;
		private const int TestCapacity = Constants.Size1Kb;
		private string _indexFilePath;

		[TestInitialize]
		public void TestInitialize() {
			_indexFilePath = Path.GetTempFileName();
		}

		[TestCleanup]
		public void TestCleanup() {
			File.Delete(_indexFilePath);
		}

		protected override IIndex Create() {
			return new BTreeIndex<PersistentNode, KeyInfo>(new PersistentNodeStorage(_indexFilePath, TestCapacity, TestDegree));
		}
	}

}