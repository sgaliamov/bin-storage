using System;
using System.Runtime.Serialization;

namespace Zylab.Interview.BinStorage.Errors {

	/// <summary>
	///     Throw when an entry with the same key already exists
	/// </summary>
	public class DuplicateException : Exception {
		public DuplicateException() {
		}

		public DuplicateException(string message) : base(message) {
		}

		public DuplicateException(string message, Exception innerException) : base(message, innerException) {
		}

		protected DuplicateException(SerializationInfo info, StreamingContext context) : base(info, context) {
		}
	}

}