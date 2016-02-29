using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;
using Zylab.Interview.BinStorage.Index.RedBlackTree;

namespace Zylab.Interview.BinStorage.UnitTests.Index.RedBlackTree {

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