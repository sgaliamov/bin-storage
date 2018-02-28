using System;
using System.IO;
using System.Runtime.Serialization;

namespace BinStorage.Errors {

	/// <summary>
	///     Throw when there is not enough space on the disk
	/// </summary>
	public class NotEnoughDiskSpaceException : IOException {
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