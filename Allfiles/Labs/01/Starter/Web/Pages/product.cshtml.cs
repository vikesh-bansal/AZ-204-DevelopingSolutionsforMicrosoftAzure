using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic; 
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
            string imagesJson = await _httpClient.GetStringAsync(imagesUrl);
            IEnumerable<string> imagesList = JsonConvert.DeserializeObject<IEnumerable<string>>(imagesJson);

            this.ImageList = imagesList.ToList<string>();


            var thumbImageUrl = _options.ApiUrl+ "/Images2/thumbnail";
            string thumbsJson = await _httpClient.GetStringAsync(thumbImageUrl);
            this.ThumbnailList = JsonConvert.DeserializeObject<IEnumerable<string>>(thumbsJson).ToList(); 

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
                    memoryStream.Position = 0;
                    // Generate Thumbnail
                    var thumbImageUrl = _options.ApiUrl+ "/Images2/thumbnail";
                    using (Image image = Image.Load(memoryStream.ToArray()))
                    {
                        image.Mutate(x => x.Resize(200, 200));
                        using (var thumbStream = new MemoryStream())
                        {
                            image.Save(thumbStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                            thumbStream.Position = 0;
                            using (var thumbContent = new StreamContent(thumbStream))
                            {
                                thumbContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                                await _httpClient.PostAsync(thumbImageUrl, thumbContent);
                            }
                        }
                    }
                    //using (var inputImage = SKBitmap.Decode(memoryStream))
                    //{
                    //    var resized = inputImage.Resize(new SKImageInfo(200, 200), SKSamplingOptions.Default); 
                    //    using (var thumbStream = new MemoryStream())
                    //    {
                    //        resized.Encode(thumbStream, SKEncodedImageFormat.Png, 100);
                    //        thumbStream.Position = 0;

                    //        // Code to push using API will be added here
                    //        using (var thumbContent = new StreamContent(thumbStream))
                    //        {
                    //            thumbContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    //            await _httpClient.PostAsync(thumbImageUrl, thumbContent);
                    //        }
                    //    }
                    //}
                    
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
