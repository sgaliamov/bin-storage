using System;
using System.IO;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.Storage {

	public interface IStorage : IDisposable {
		IndexData Append(Stream input);
		Stream Get(IndexData indexData);
	}

}