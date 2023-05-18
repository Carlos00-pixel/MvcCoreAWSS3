using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using MvcCoreAWSS3.Services;

namespace MvcCoreAWSS3.Controllers
{
    public class AWSFilesController : Controller
    {
        private ServiceStorageS3 service;
        private IAmazonS3 clientS3;

        //COMO NUESTRO BUCKET ES PUBLICO, VAMOS A MOSTRAR
        //LAS IMAGENES EN LA PAGINA INDEX
        private string BucketUrl;
        public AWSFilesController
            (ServiceStorageS3 service, IConfiguration configuration,
            IAmazonS3 clientS3)
        {
            this.BucketUrl = configuration.GetValue<string>("AWS:BucketUrl");
            this.service = service;
            this.clientS3 = clientS3;
        }

        public async Task<IActionResult> Index()
        {
            List<string> filesS3 = await this.service.GetVersionsFileAsync();
            ViewData["BUCKETURL"] = this.BucketUrl;
            return View(filesS3);
        }

        public IActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            using (Stream stream = file.OpenReadStream())
            {
                await this.service.UploadFileAsync(file.FileName, stream);
            }
            return RedirectToAction("Index");
        }

        //SI DESEAMOS VISUALIZAR EL FICHERO POR SI FUERA PRIVADO
        public async Task<IActionResult> FileAWS(string fileName)
        {
            Stream stream = await this.service.GetFileAsync(fileName);
            return File(stream, "image/png");
        }

        public async Task<IActionResult> DeleteFile(string fileName)
        {
            await this.service.DeleteFileAsync(fileName);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Versiones(string nombreArchivo)
        {
            var request = new ListVersionsRequest
            {
                BucketName = "bucket-juernes-cma",
                Prefix = nombreArchivo
            };

            var response = await this.clientS3.ListVersionsAsync(request);

            return View(response.Versions);
        }

    }
}
