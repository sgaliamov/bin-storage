using System;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public interface INodeStorage<TNode, TKey> : IDisposable {
		int Degree { get; }
		TNode NewNode();
		TNode GetRoot();
		void SetRoot(TNode node);
		bool IsFull(TNode node);
		bool IsLeaf(TNode node);
		bool SearchPosition(TNode node, string key, out IndexData found, out int position);
		int Compare(TNode node, int keyIndex, string key);

		TKey NewKey(string key, IndexData data);
		void InsertKey(TNode node, int position, TKey key);
		TKey GetKey(TNode node, int position);
		void MoveRightHalfKeys(TNode newNode, TNode fullNode);

		void AddChildren(TNode node, TNode children);
		void InsertChildren(TNode node, int position, TNode children);
		TNode GetChildren(TNode node, int position);
		void MoveRightHalfChildrens(TNode newNode, TNode fullNode);

		void Commit(TNode node);
	}

}