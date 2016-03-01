using System;
using System.IO;
using System.Linq;

namespace Zylab.Interview.BinStorage.DataGenerator {

	internal class Program {
		private static void Main(string[] args) {
			var path = args[0];
			var count = int.Parse(args[1]);
			var size = long.Parse(args[2]);

			const long step = 0x6400000;
			var buffer = new byte[Math.Min(size, step)];
			for(var i = 0; i < buffer.Length; i++) {
				buffer[i] = 1;
			}

			Enumerable.Range(0, count)
				.AsParallel()
				.WithDegreeOfParallelism(4)
				.ForAll(
					_ => {
						using(var stream = File.Create(path + "\\" + Guid.NewGuid().ToString())) {
							for(long i = 0; i < size; i += step) {
								stream.Write(buffer, 0, buffer.Length);
							}
						}
					});
		}
	}

}