using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Errors;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.UnitTests.Index {

	public abstract class IndexTests {
		[TestMethod]
		public void Add_and_Contains_Test() {
			var key = Guid.NewGuid().ToString();
			var data = new IndexData {
				Offset = 1,
				Size = 2,
				Md5Hash = Guid.NewGuid().ToByteArray()
			};

			using(var target = Create()) {
				target.Add(key, data);

				Assert.IsTrue(target.Contains(key));
				Assert.IsFalse(target.Contains(Guid.NewGuid().ToString()));
			}

			using(var target = Create()) {
				Assert.IsTrue(target.Contains(key));
				Assert.IsFalse(target.Contains(Guid.NewGuid().ToString()));
			}
		}

		[TestMethod]
		public void Add_Collection_Test() {
			var dictionary = Enumerable.Range(0, 100)
				.ToDictionary(
					x => Guid.NewGuid().ToString(),
					x => new IndexData {
						Offset = x,
						Md5Hash = Guid.NewGuid().ToByteArray(),
						Size = x + 1
					});

			using(var target = Create()) {
				foreach(var item in dictionary) {
					target.Add(item.Key, item.Value);
				}

				foreach(var item in dictionary) {
					var data = target.Get(item.Key);
					TestHelper.AreEqual(item.Value, data);
				}
			}
		}

		[TestMethod]
		public void Add_Duplicate_Test() {
			var key = Guid.NewGuid().ToString();
			var data = new IndexData {
				Offset = 1,
				Size = 2,
				Md5Hash = Guid.NewGuid().ToByteArray()
			};

			var newData = new IndexData {
				Offset = 3,
				Size = 4,
				Md5Hash = Guid.NewGuid().ToByteArray()
			};

			using(var target = Create()) {
				target.Add(key, data);
			}

			using(var target = Create()) {
				try {
					target.Add(key, newData);

					Assert.Fail("Exception expected");
				}
				catch(DuplicateException) {
				}
			}

			using(var target = Create()) {
				var actual = target.Get(key);
				TestHelper.AreEqual(data, actual);
			}
		}

		[TestMethod]
		public void Get_Test() {
			var key = Guid.NewGuid().ToString();
			var data = new IndexData {
				Offset = 1,
				Size = 2,
				Md5Hash = Guid.NewGuid().ToByteArray()
			};

			using(var target = Create()) {
				target.Add(key, data);

				var actual = target.Get(key);

				TestHelper.AreEqual(data, actual);
			}

			using(var target = Create()) {
				var actual = target.Get(key);

				TestHelper.AreEqual(data, actual);
			}
		}

		[TestMethod]
		public void Get_Unknown_Test() {
			var key = Guid.NewGuid().ToString();
			var data = new IndexData {
				Offset = 1,
				Size = 2,
				Md5Hash = Guid.NewGuid().ToByteArray()
			};

			using(var target = Create()) {
				target.Add(key, data);

				try {
					target.Get(Guid.NewGuid().ToString());

					Assert.Fail("Expected exception");
				}
				catch(KeyNotFoundException e) {
					Console.WriteLine(e);
				}
			}

			using(var target = Create()) {
				try {
					target.Get(Guid.NewGuid().ToString());

					Assert.Fail("Expected exception");
				}
				catch(KeyNotFoundException e) {
					Console.WriteLine(e);
				}
			}
		}

		protected abstract IIndex Create();
	}

}