using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;
using Zylab.Interview.BinStorage.Index.RedBlackTree;

namespace Zylab.Interview.BinStorage.UnitTests.Index.RedBlackTree {

	[TestClass]
	public class RedBlackTreeIndexMultiThreadingTests : MultiThreadingTests {
		private string _indexFilePath;
		private TimeSpan _timeout;

		[TestInitialize]
		public void TestInitialize() {
			_indexFilePath = Path.GetTempFileName();
			File.Delete(_indexFilePath);
			_timeout = TimeSpan.FromSeconds(10);
		}

		[TestCleanup]
		public void TestCleanup() {
			File.Delete(_indexFilePath);
		}

		protected override IIndex Create() {
			return new RedBlackTreeIndex(_indexFilePath, _timeout);
		}
	}

}