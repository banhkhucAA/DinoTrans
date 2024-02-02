using DinoTrans.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        public FileController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> UploadFile(List<IFormFile> files)
        {
            List<UploadResult> uploadResults = new List<UploadResult>();
            foreach (var file in files) 
            {
                var uploadResult = new UploadResult();
                string trustedFileNameForFileStorage;
                var unstrustedFileName = file.FileName;
                uploadResult.FileName = unstrustedFileName;
                var trustedFileNameForDisplay = WebUtility.HtmlDecode(unstrustedFileName);
                var abc = Path.GetExtension(file.FileName);
                trustedFileNameForFileStorage = Path.ChangeExtension(Path.GetRandomFileName(),Path.GetExtension(file.FileName));
                var path = Path.Combine(_env.ContentRootPath,"Uploads", trustedFileNameForFileStorage);

                await using FileStream fs = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(fs);

                uploadResult.StoredFileName = trustedFileNameForFileStorage;
                uploadResults.Add(uploadResult);
            }
            return uploadResults;
        }
    }
}
