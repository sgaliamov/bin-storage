using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Zylab.Interview.BinStorage {
    public class BinaryStorage : IBinaryStorage {

        public BinaryStorage(StorageConfiguration configuration) {

        }

        public void Add(string key, Stream data) {
            throw new NotImplementedException();
        }

        public Stream Get(string key) {
            throw new NotImplementedException();
        }

        public bool Contains(string key) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}
