using System;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public interface INodeStorage : IDisposable {
		IndexDataKey NewKey(string key, IndexData data);
		INode NewNode();
		INode GetRoot();
		void SetRoot(INode node);
	}

}