using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;
using Zylab.Interview.BinStorage.Index.BTree;
using Zylab.Interview.BinStorage.Index.BTree.Persistent;

namespace Zylab.Interview.BinStorage.UnitTests.Index.BTree.Persistent {

	[TestClass]
	public class PersistentBTreeIndexMultiThreadingTests : MultiThreadingTests {
		private const int TestDegree = 4;
		private const int TestCapacity = Constants.Size4Kb;
		private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);
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
			return
				new ThreadSafeIndex(
					new BTreeIndex<PersistentNode, KeyInfo>(new PersistentNodeStorage(_indexFilePath, TestCapacity, TestDegree)),
					_timeout);
		}
	}

}