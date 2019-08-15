using System;
using System.IO;

using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.FileProviders;

namespace Bet.Extensions.AzureStorage
{
    public class StorageFileInfo : IFileInfo
    {
        private readonly CloudBlockBlob _blockBlob;

        public StorageFileInfo(IListBlobItem blob)
        {
            Exists = blob.Container.Exists();
            switch (blob)
            {
                case CloudBlobDirectory directory:
                    IsDirectory = true;
                    Name = ((CloudBlobDirectory)blob).Prefix.TrimEnd('/');
                    PhysicalPath = directory.StorageUri.PrimaryUri.ToString();
                    break;

                case CloudBlockBlob file:
                    file.FetchAttributes();
                    Length = file.Properties.Length;
                    PhysicalPath = file.Uri.ToString();
                    Name = !string.IsNullOrEmpty(file.Parent.Prefix) ? file.Name.Replace(file.Parent.Prefix, string.Empty) : file.Name;
                    LastModified = file.Properties.LastModified ?? DateTimeOffset.MinValue;

                    _blockBlob = file;
                    break;
            }
        }

        public bool Exists { get; }

        public long Length { get; }

        public string PhysicalPath { get; }

        public string Name { get; }

        public DateTimeOffset LastModified { get; }

        public bool IsDirectory { get; }

        public Stream CreateReadStream()
        {
            var stream = new MemoryStream();
            _blockBlob.DownloadToStream(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
