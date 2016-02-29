using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;
using Zylab.Interview.BinStorage.Index.BTree;

namespace Zylab.Interview.BinStorage.UnitTests.Index.BTree {

	[TestClass]
	public class BTreeIndexTests : IndexTests {
		private const int TestDegree = 5;

		protected override IIndex Create() {
			return new BTreeIndex(new MemoryNodeStorage(), TestDegree);
		}
	}

}