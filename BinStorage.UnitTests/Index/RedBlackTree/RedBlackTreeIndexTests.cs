using System.IO;
using BinStorage.Index;
using BinStorage.Index.RedBlackTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinStorage.UnitTests.Index.RedBlackTree {

	[TestClass]
	public class RedBlackTreeIndexTests : IndexTests {
		private string _indexFilePath;

		[TestInitialize]
		public void TestInitialize() {
			_indexFilePath = Path.GetTempFileName();
			File.Delete(_indexFilePath);
		}

		[TestCleanup]
		public void TestCleanup() {
			File.Delete(_indexFilePath);
		}

		protected override IIndex Create() {
			return new RedBlackTreeIndex(_indexFilePath);
		}
	}

}