using System;
using System.Runtime.Serialization;

namespace Zylab.Interview.BinStorage.Errors {

	public class NotEnoughDiskSpaceException : Exception {
		public NotEnoughDiskSpaceException() {
		}

		public NotEnoughDiskSpaceException(string message) : base(message) {
		}

		public NotEnoughDiskSpaceException(string message, Exception innerException) : base(message, innerException) {
		}

		protected NotEnoughDiskSpaceException(SerializationInfo info, StreamingContext context) : base(info, context) {
		}
	}

}