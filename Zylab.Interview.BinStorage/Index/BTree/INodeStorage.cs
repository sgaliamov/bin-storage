using System;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public interface INodeStorage : IDisposable {
		IndexDataKey NewKey(string key, IndexData data);
		Node NewNode(int t);
		Node GetNode(string key);
	}

}