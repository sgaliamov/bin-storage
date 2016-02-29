using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.UnitTests.Index {

	public abstract class MultiThreadingTests : IndexTests {
		[TestMethod]
		public void MultiThreading_Test() {
			var data = Enumerable.Range(0, 1000).ToDictionary(
				x => Guid.NewGuid().ToString(),
				i => new IndexData {
					Md5Hash = Guid.NewGuid().ToByteArray(),
					Offset = i,
					Size = i + 1
				});

			using(var target = Create()) {
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
										TestHelper.AreEqual(indexData, data[item.Key]);
									}
									catch(KeyNotFoundException) {
									}
									catch(TimeoutException) {
									}
								});
					});
			}

			using(var target = Create()) {
				foreach(var item in data) {
					var actual = target.Get(item.Key);
					TestHelper.AreEqual(item.Value, actual);
				}
			}
		}
	}

}