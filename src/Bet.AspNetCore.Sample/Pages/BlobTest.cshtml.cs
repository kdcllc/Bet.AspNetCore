using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bet.AspNetCore.Sample.Options;
using Bet.Extensions.AzureStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bet.AspNetCore.Sample.Pages
{
    public class BlobTestModel : PageModel
    {
        private readonly IStorageBlob<UploadsBlobOptions> _storageBlob;

        public BlobTestModel(IStorageBlob<UploadsBlobOptions> storageBlob)
        {
            _storageBlob = storageBlob;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // TO TEST BLOB
            await _storageBlob.AddAsync(new { content = "This is added to uploads" }, $"{Guid.NewGuid()}.json");

            return Page();
        }
    }
}
