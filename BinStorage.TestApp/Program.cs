using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace BinStorage.TestApp {

	internal class Program {
		private static void Main(string[] args) {
			if(args.Length < 2
			   || !Directory.Exists(args[0])
			   || !Directory.Exists(args[1])) {
				Console.WriteLine("Usage: Zylab.Interview.BinStorage.TestApp.exe InputFolder StorageFolder");
				return;
			}

			// Create storage and add data
			Console.WriteLine("Creating storage from " + args[0]);
			var sw = Stopwatch.StartNew();
			using(var storage = new BinaryStorage(new StorageConfiguration { WorkingFolder = args[1] })) {
				Directory.EnumerateFiles(args[0], "*", SearchOption.AllDirectories)
					.AsParallel().WithDegreeOfParallelism(4).ForAll(
						s => {
							// ReSharper disable once AccessToDisposedClosure
							AddFile(storage, s);
						});
			}
			Console.WriteLine("Time to create: " + sw.Elapsed);

			// Open storage and read data
			Console.WriteLine("Verifying data");
			sw = Stopwatch.StartNew();
			using(var storage = new BinaryStorage(new StorageConfiguration { WorkingFolder = args[1] })) {
				Directory.EnumerateFiles(args[0], "*", SearchOption.AllDirectories)
					.AsParallel().WithDegreeOfParallelism(4).ForAll(
						s => {
							// ReSharper disable once AccessToDisposedClosure
							using(var resultStream = storage.Get(s)) {
								using(var sourceStream = new FileStream(s, FileMode.Open, FileAccess.Read)) {
									if(sourceStream.Length != resultStream.Length) {
										throw new Exception(
											$"Length did not match: Source - '{sourceStream.Length}', Result - {resultStream.Length}");
									}

									byte[] hash1, hash2;
									using(var md5 = MD5.Create()) {
										hash1 = md5.ComputeHash(sourceStream);

										md5.Initialize();
										hash2 = md5.ComputeHash(resultStream);
									}

									if(!hash1.SequenceEqual(hash2)) {
										throw new Exception($"Hashes do not match for file - '{s}'  ");
									}
								}
							}
						});
			}
			Console.WriteLine("Time to verify: " + sw.Elapsed);
		}

		private static void AddFile(IBinaryStorage storage, string fileName) {
			using(var file = new FileStream(fileName, FileMode.Open)) {
				storage.Add(fileName, file);
			}
		}
	}

}