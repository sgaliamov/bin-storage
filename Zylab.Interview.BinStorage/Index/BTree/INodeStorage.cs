using System;
using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public interface INodeStorage<TNode, TKey> : IDisposable {
		int Degree { get; }
		TNode NewNode();
		TNode GetRoot();
		void SetRoot(TNode node);
		bool IsFull(TNode node);
		bool IsLeaf(TNode node);
		bool SearchPosition(TNode node, string key, out IndexData found, out int position);
		int Compare(TNode parent, int keyIndex, string key);

		TKey NewKey(string key, IndexData data);
		void InsertKey(TNode node, int position, TKey key);
		TKey GetKey(TNode node, int position);
		void AddRangeKeys(TNode node, IEnumerable<TKey> keys);
		IEnumerable<TKey> GetRangeKeys(TNode node, int index, int count);
		void RemoveRangeKeys(TNode node, int index, int count);

		void AddChildren(TNode node, TNode children);
		void InsertChildren(TNode node, int position, TNode children);
		TNode GetChildren(TNode node, int position);
		void AddRangeChildrens(TNode node, IEnumerable<TNode> nodes);
		IEnumerable<TNode> GetRangeChildrens(TNode node, int index, int count);
		void RemoveRangeChildrens(TNode node, int index, int count);

		void Commit(TNode node);
	}

}