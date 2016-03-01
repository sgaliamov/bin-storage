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
		private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);
		private string _indexFilePath;
		private PersistentNodeStorage _nodeStorage;

		[TestInitialize]
		public void TestInitialize() {
			_indexFilePath = Path.GetTempFileName();
			_nodeStorage = new PersistentNodeStorage(_indexFilePath, degree: TestDegree);
		}

		[TestCleanup]
		public void TestCleanup() {
			_nodeStorage.Dispose();
			File.Delete(_indexFilePath);
		}

		protected override IIndex Create() {
			return new ThreadSafeIndex(new BTreeIndex<PersistentNode, KeyInfo>(_nodeStorage), _timeout);
		}
	}

}