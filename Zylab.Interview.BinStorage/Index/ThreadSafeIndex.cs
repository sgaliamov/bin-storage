using System;
using System.Threading;

namespace Zylab.Interview.BinStorage.Index {

	/// <summary>
	///     Decorator for thread safe operations with index
	///     Uses ReaderWriterLockSlim in base
	/// </summary>
	public class ThreadSafeIndex : IIndex {
		private readonly IIndex _index;
		private readonly ReaderWriterLockSlim _locker;
		private readonly TimeSpan _timeout;

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="index">Wrapped index</param>
		/// <param name="timeout">Lock timeout</param>
		public ThreadSafeIndex(IIndex index, TimeSpan timeout) {
			_index = index;
			_timeout = timeout;
			_locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		}

		public void Dispose() {
			WriteLock(() => _index.Dispose(), $"Timeout {_timeout} expired while disposing index");
		}

		public void Add(string key, IndexData indexData) {
			WriteLock(() => _index.Add(key, indexData), $"Timeout {_timeout} expired to add key {key} to index");
		}

		public bool Contains(string key) {
			return ReadLock(() => _index.Contains(key), $"Timeout {_timeout} expired to read index by key {key}");
		}

		public IndexData Get(string key) {
			return ReadLock(() => _index.Get(key), $"Timeout {_timeout} expired to read index by key {key}");
		}

		private void WriteLock(Action action, string errorMessage) {
			try {
				if(!_locker.TryEnterWriteLock(_timeout))
					throw new TimeoutException(errorMessage);

				action();
			}
			finally {
				_locker.ExitWriteLock();
			}
		}

		private T ReadLock<T>(Func<T> action, string errorMessage) {
			try {
				if(!_locker.TryEnterWriteLock(_timeout))
					throw new TimeoutException(errorMessage);

				return action();
			}
			finally {
				_locker.ExitWriteLock();
			}
		}
	}

}