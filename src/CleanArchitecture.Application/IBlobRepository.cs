using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application
{
    public interface IBlobRepository
    {
        Task<UploadBlobResult> UploadBlob(string blobName, Stream content);

        Task<Stream> DownloadBlob(string fileName);

        Task<UploadBlobResult> PublishBlob(string blobName);
    }

    public class UploadBlobResult
    {
        public string Uri { get; set; }

        public UploadBlobResult(string uri, string filepath)
        {
            Uri = uri;
            BlobName = filepath;
        }

        public string BlobName { get; set; }
    }
}
