using System;

namespace Zylab.Interview.BinStorage.Index {

	public interface IIndex : IDisposable {
		void Add(string key, IndexData indexData);
		IndexData Get(string key);
		bool Contains(string key);
	}

}