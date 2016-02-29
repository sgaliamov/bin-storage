using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.UnitTests.Index {

	public static class TestHelper {
		public static void AreEqual(IndexData expected, IndexData actual) {
			Assert.AreEqual(expected.Size, actual.Size);
			Assert.AreEqual(expected.Offset, actual.Offset);
			Assert.IsTrue(expected.Md5Hash.SequenceEqual(actual.Md5Hash));
		}
	}

}