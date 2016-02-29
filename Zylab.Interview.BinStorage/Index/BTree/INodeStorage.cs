using System;
using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public interface INodeStorage : IDisposable {
		int Degree { get; }
		IndexDataKey NewKey(string key, IndexData data);
		INode NewNode();
		INode GetRoot();
		void SetRoot(INode node);

		bool IsFull(INode node);
		bool IsLeaf(INode node);
		int SearchPosition(INode node, string key, out IndexDataKey found);

		void InsertKey(INode node, int position, IndexDataKey key);
		IndexDataKey GetKey(INode node, int position);
		void AddRangeKeys(INode node, IEnumerable<IndexDataKey> keys);
		IEnumerable<IndexDataKey> GetRangeKeys(INode node, int index, int count);
		void RemoveRangeKeys(INode node, int index, int count);

		void AddChildren(INode node, INode children);
		void InsertChildren(INode node, int position, INode children);
		INode GetChildren(INode node, int position);
		void AddRangeChildrens(INode node, IEnumerable<INode> nodes);
		IEnumerable<INode> GetRangeChildrens(INode node, int index, int count);
		void RemoveRangeChildrens(INode node, int index, int count);
	}

}