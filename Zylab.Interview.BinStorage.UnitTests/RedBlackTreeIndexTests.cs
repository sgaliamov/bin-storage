using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			_timeout = TimeSpan.FromSeconds(10);
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

				Assert.AreEqual(data.End, actual.End);
				Assert.AreEqual(data.Start, actual.Start);
				Assert.IsTrue(data.Md5Hash.SequenceEqual(actual.Md5Hash));
			}

			using(var target = new RedBlackTreeIndex(_indexFilePath, _timeout)) {
				var actual = target.Get(key);

				Assert.AreEqual(data.End, actual.End);
				Assert.AreEqual(data.Start, actual.Start);
				Assert.IsTrue(data.Md5Hash.SequenceEqual(actual.Md5Hash));
			}
		}

		[TestMethod]
		public void GetUnknown_Test()
		{
			var key = Guid.NewGuid().ToString();
			var data = new IndexData
			{
				Start = 1,
				End = 2,
				Md5Hash = Guid.NewGuid().ToByteArray()
			};

			using (var target = new RedBlackTreeIndex(_indexFilePath, _timeout)) {
				target.Add(key, data);

				try {
					target.Get(Guid.NewGuid().ToString());

					Assert.Fail("Expected exception");
				}
				catch(KeyNotFoundException e) {
					Console.WriteLine(e);
				}
			}

			using (var target = new RedBlackTreeIndex(_indexFilePath, _timeout))
			{
				try
				{
					target.Get(Guid.NewGuid().ToString());

					Assert.Fail("Expected exception");
				}
				catch (KeyNotFoundException e)
				{
					Console.WriteLine(e);
				}
			}
		}
	}

}