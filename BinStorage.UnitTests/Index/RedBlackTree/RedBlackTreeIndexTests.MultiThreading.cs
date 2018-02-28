using System;
using System.IO;
using BinStorage.Index;
using BinStorage.Index.RedBlackTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinStorage.UnitTests.Index.RedBlackTree {

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
			return new ThreadSafeIndex(new RedBlackTreeIndex(_indexFilePath), _timeout);
		}
	}

}