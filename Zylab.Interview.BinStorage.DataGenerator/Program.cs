using System;
using System.IO;
using System.Linq;

namespace Zylab.Interview.BinStorage.DataGenerator {

	internal class Program {
		private static void Main(string[] args) {
			var path = args[0];
			var count = int.Parse(args[1]);
			var size = int.Parse(args[2]);

			var buffer = new byte[size];
			for(var i = 0; i < buffer.Length; i++) {
				buffer[i] = 1;
			}

			Enumerable.Range(0, count)
				.AsParallel()
				.WithDegreeOfParallelism(4)
				.ForAll(
					i => {
						using(var stream = File.Create(path + "\\" + Guid.NewGuid().ToString())) {
							stream.Write(buffer, 0, buffer.Length);
						}
					});
		}
	}

}