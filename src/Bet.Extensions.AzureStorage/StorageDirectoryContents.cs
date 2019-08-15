using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.FileProviders;

namespace Bet.Extensions.AzureStorage
{
    public class StorageDirectoryContents : IDirectoryContents
    {
        private readonly List<IListBlobItem> _blobs = new List<IListBlobItem>();

        public StorageDirectoryContents(CloudBlobDirectory blob)
        {
            BlobContinuationToken continuationToken = null;

            do
            {
                var response = blob.ListBlobsSegmented(continuationToken);
                continuationToken = response.ContinuationToken;
                _blobs.AddRange(response.Results);
            }
            while (continuationToken != null);
            Exists = _blobs.Count > 0;
        }

        public bool Exists { get; set; }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return _blobs.Select(blob => new StorageFileInfo(blob)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
