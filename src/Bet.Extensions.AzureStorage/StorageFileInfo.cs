using System;
using System.IO;

using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.FileProviders;

namespace Bet.Extensions.AzureStorage
{
    public class StorageFileInfo : IFileInfo
    {
        private readonly CloudBlockBlob? _blockBlob;

        public StorageFileInfo(IListBlobItem blob)
        {
            switch (blob)
            {
                case CloudBlobDirectory directory:
                    Exists = true;
                    IsDirectory = true;
                    Name = ((CloudBlobDirectory)blob).Prefix.TrimEnd('/');
                    PhysicalPath = directory.StorageUri.PrimaryUri.ToString();
                    break;

                case CloudBlockBlob file:
                    _blockBlob = file;
                    Name = !string.IsNullOrEmpty(file.Parent.Prefix) ? file.Name.Replace(file.Parent.Prefix, string.Empty) : file.Name;
                    Exists = file.Exists();
                    if (Exists)
                    {
                        file.FetchAttributes();
                        Length = file.Properties.Length;
                        LastModified = file.Properties.LastModified ?? DateTimeOffset.MinValue;
                    }
                    else
                    {
                        Length = -1;

                        // IFileInfo.PhysicalPath docs say: Return null if the file is not directly accessible.
                        // (PhysicalPath should maybe also be null for blobs that do exist but that would be a potentially breaking change.)
                        PhysicalPath = null!;
                    }

                    break;
            }
        }

        public bool Exists { get; }

        public long Length { get; }

        public string PhysicalPath { get; } = string.Empty;

        public string Name { get; } = string.Empty;

        public DateTimeOffset LastModified { get; }

        public bool IsDirectory { get; }

        public Stream CreateReadStream()
        {
            if (_blockBlob == null)
            {
                throw new NullReferenceException($"{nameof(_blockBlob)} wasn't created");
            }

            var stream = new MemoryStream();
            _blockBlob.DownloadToStream(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
