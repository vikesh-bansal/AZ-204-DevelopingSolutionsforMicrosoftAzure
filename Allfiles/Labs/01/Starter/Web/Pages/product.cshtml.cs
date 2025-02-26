using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Web.Pages
{
    public class productModel : PageModel
    {
        private HttpClient _httpClient;
        private Options _options;
        public productModel(HttpClient httpClient, Options options)
        {
            _httpClient = httpClient;
            _options = options;
        }
        [BindProperty]  
        public List<string> ImageList { get; private set; }

        [BindProperty]
        public List<string> ThumbnailList { get; private set; }

        [BindProperty]
        public IFormFile Upload { get; set; }
        public async Task OnGetAsync()
        {
            var imagesUrl = _options.ApiUrl;
            string imagesJson=await _httpClient.GetStringAsync(imagesUrl);
            IEnumerable<string> imagesList=JsonConvert.DeserializeObject<IEnumerable<string>>(imagesJson);

            this.ImageList = imagesList.ToList<string>();


            var thumbImageUrl = _options.ApiUrl+ "/thumbnail";
            string thumbsJson = await _httpClient.GetStringAsync(thumbImageUrl);
            ThumbnailList = JsonConvert.DeserializeObject<IEnumerable<string>>(thumbsJson).ToList(); 
            this.ImageList = imagesList.ToList<string>();

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Upload != null && Upload.Length > 0)
            {
                var imageUrl = _options.ApiUrl;

                // Read image into memory first to avoid stream conflicts
                using (var memoryStream = new MemoryStream())
                {
                    await Upload.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    // Upload Original Image
                    using (var imageStream = new MemoryStream(memoryStream.ToArray()))
                    using (var imageContent = new StreamContent(imageStream))
                    {
                        imageContent.Headers.ContentType = new MediaTypeHeaderValue(Upload.ContentType);
                        await _httpClient.PostAsync(imageUrl, imageContent);
                    }

                    // Generate Thumbnail
                    var thumbImageUrl = _options.ApiUrl;
                    using (var sourceImage = new Bitmap(memoryStream))
                    using (var objBitMap = new Bitmap(200, 200))
                    using (var objGraphics = Graphics.FromImage(objBitMap))
                    {
                        objGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        objGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        // Maintain Aspect Ratio
                        int newWidth, newHeight;
                        float aspectRatio = (float)sourceImage.Width / sourceImage.Height;
                        if (aspectRatio > 1)
                        {
                            newWidth = 200;
                            newHeight = (int)(200 / aspectRatio);
                        }
                        else
                        {
                            newHeight = 200;
                            newWidth = (int)(200 * aspectRatio);
                        }

                        objGraphics.DrawImage(sourceImage, (200 - newWidth) / 2, (200 - newHeight) / 2, newWidth, newHeight);

                        // Convert Thumbnail to Stream and Upload
                        using (var thumbStream = new MemoryStream())
                        {
                            objBitMap.Save(thumbStream, System.Drawing.Imaging.ImageFormat.Png);
                            thumbStream.Position = 0;

                            using (var thumbContent = new StreamContent(thumbStream))
                            {
                                thumbContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                                await _httpClient.PostAsync(thumbImageUrl, thumbContent);
                            }
                        }
                    }
                }
            }

            return RedirectToPage("/product");
        }

        public bool ThumbnailCallBack()
        {
            return false;
        }
    }
}
