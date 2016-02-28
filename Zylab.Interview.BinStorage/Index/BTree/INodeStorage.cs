using System;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public interface INodeStorage : IDisposable {
		Node Create(long key, int degree);
		Node Get(long key);
		//void Update(long  Node node)
	}

}