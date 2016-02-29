using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;
using Zylab.Interview.BinStorage.Index.BTree;

namespace Zylab.Interview.BinStorage.UnitTests.Index.BTree {

	[TestClass]
	public class BTreeIndexMultiThreadingTests : MultiThreadingTests {
		private const int TestDegree = 5;
		private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);


		protected override IIndex Create() {
			return new ThreadSafeIndex(new BTreeIndex(new MemoryNodeStorage(), TestDegree), _timeout);
		}
	}

}