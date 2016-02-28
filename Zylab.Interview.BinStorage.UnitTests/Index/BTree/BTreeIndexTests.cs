using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;
using Zylab.Interview.BinStorage.Index.BTree;

namespace Zylab.Interview.BinStorage.UnitTests.Index.BTree {

	[TestClass]
	public class BTreeIndexTests {
		[TestMethod]
		public void Add_Get_Test() {
			var dictionary = Enumerable.Range(0, 10).ToDictionary(x => x.ToString(), x => new IndexData { Offset = x });
			using(var target = new BTreeIndex(new MemoryNodeStorage(), 2)) {
				foreach(var item in dictionary) {
					target.Add(item.Key, item.Value);
				}

				foreach(var item in dictionary) {
					var data = target.Get(item.Key);
					Assert.AreEqual(item.Value.Offset, data.Offset);
				}
			}
		}

		[TestMethod]
		public void Get_Unknown_Test() {
			var dictionary = Enumerable.Range(0, 10).ToDictionary(x => x.ToString(), x => new IndexData { Offset = x });
			using(var target = new BTreeIndex(new MemoryNodeStorage(), 2)) {
				foreach(var item in dictionary) {
					target.Add(item.Key, item.Value);
				}

				var data = target.Get(Guid.NewGuid().ToString());
				Assert.IsNull(data);
			}
		}
	}

}