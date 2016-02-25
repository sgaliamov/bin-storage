using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.UnitTests {

	[TestClass]
	public class RedBlackTreeIndexTests {
		private string _indexFilePath;
		private TimeSpan _timeout;

		[TestInitialize]
		public void TestInitialize() {
			_indexFilePath = Path.GetTempFileName();
			File.Delete(_indexFilePath);
			_timeout = TimeSpan.FromSeconds(20);
		}

		[TestCleanup]
		public void TestCleanup() {
			File.Delete(_indexFilePath);
		}

		[TestMethod]
		public void Add_and_Contains_Test() {
			var key = Guid.NewGuid().ToString();
			var data = new IndexData {
				Start = 1,
				End = 2,
				Md5Hash = Guid.NewGuid().ToByteArray()
			};

			using(var target = new RedBlackTreeIndex(_indexFilePath, _timeout)) {
				target.Add(key, data);

				Assert.IsTrue(target.Contains(key));
				Assert.IsFalse(target.Contains(Guid.NewGuid().ToString()));
			}

			using(var target = new RedBlackTreeIndex(_indexFilePath, _timeout)) {
				Assert.IsTrue(target.Contains(key));
				Assert.IsFalse(target.Contains(Guid.NewGuid().ToString()));
			}
		}

		[TestMethod]
		public void Get_Test() {
			var key = Guid.NewGuid().ToString();
			var data = new IndexData {
				Start = 1,
				End = 2,
				Md5Hash = Guid.NewGuid().ToByteArray()
			};

			using(var target = new RedBlackTreeIndex(_indexFilePath, _timeout)) {
				target.Add(key, data);

				var actual = target.Get(key);

				AreEqual(data, actual);
			}

			using(var target = new RedBlackTreeIndex(_indexFilePath, _timeout)) {
				var actual = target.Get(key);

				AreEqual(data, actual);
			}
		}

		private static void AreEqual(IndexData expected, IndexData actual) {
			Assert.AreEqual(expected.End, actual.End);
			Assert.AreEqual(expected.Start, actual.Start);
			Assert.IsTrue(expected.Md5Hash.SequenceEqual(actual.Md5Hash));
		}

		[TestMethod]
		public void GetUnknown_Test() {
			var key = Guid.NewGuid().ToString();
			var data = new IndexData {
				Start = 1,
				End = 2,
				Md5Hash = Guid.NewGuid().ToByteArray()
			};

			using(var target = new RedBlackTreeIndex(_indexFilePath, _timeout)) {
				target.Add(key, data);

				try {
					target.Get(Guid.NewGuid().ToString());

					Assert.Fail("Expected exception");
				}
				catch(KeyNotFoundException e) {
					Console.WriteLine(e);
				}
			}

			using(var target = new RedBlackTreeIndex(_indexFilePath, _timeout)) {
				try {
					target.Get(Guid.NewGuid().ToString());

					Assert.Fail("Expected exception");
				}
				catch(KeyNotFoundException e) {
					Console.WriteLine(e);
				}
			}
		}

		[TestMethod]
		public void MultyThreading_Test() {
			var data = Enumerable.Range(0, 1000).ToDictionary(
				x => Guid.NewGuid().ToString(),
				i => new IndexData {
					Md5Hash = Guid.NewGuid().ToByteArray(),
					Start = (ulong)i,
					End = (ulong)(i + 1)
				});

			using(var target = new RedBlackTreeIndex(_indexFilePath, _timeout)) {
				Parallel.Invoke(
					() => {
						data.OrderBy(x => Guid.NewGuid())
							.AsParallel()
							.WithDegreeOfParallelism(4)
							.ForAll(
								// ReSharper disable once AccessToDisposedClosure
								item => {
									var retries = 3;
									while(retries-- > 0) {
										try {
											target.Add(item.Key, item.Value);
											break;
										}
										catch(TimeoutException) {
											if(retries == 0) {
												throw;
											}
										}
									}
								});
					},
					() => {
						data.OrderBy(x => Guid.NewGuid())
							.AsParallel()
							.WithDegreeOfParallelism(4)
							.ForAll(
								item => {
									try {
										// ReSharper disable once AccessToDisposedClosure
										var indexData = target.Get(item.Key);
										AreEqual(indexData, data[item.Key]);
									}
									catch(KeyNotFoundException) {
									}
									catch(TimeoutException) {
									}
								});
					});
			}

			using(var target = new RedBlackTreeIndex(_indexFilePath, _timeout)) {
				foreach(var item in data) {
					var actual = target.Get(item.Key);
					AreEqual(item.Value, actual);
				}
			}
		}
	}

}