using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebServerMVCv2.Services.Cache;
using WebServerMVCv2.Services.TCP;
 

namespace WebServerMVCv2.Controllers
{

    [Authorize]
    public class VideoController : Controller
    {
        // GET: /Video/
        [HttpGet]
        public IActionResult Video()
        {

            return NotFound("Invalid request");
        }
        // GET: /Video/Details?id=5&source=abc
        [HttpGet("Details")]
        public IActionResult Details(int id, string? title = null)
        {

            RemoteTcpClient allRequest = new RemoteTcpClient($"Select%all%from%movies%where%id%equals%{id}");

            allRequest.ConnectAsync().Wait();

            if (allRequest.rawStringResponse == null)
            {
                return NotFound("No Data returned");
            }

            List<Models.VideoModel>? videoData = JsonSerializer.Deserialize<List<Models.VideoModel>>(allRequest.Response);

            //error checking 
            if (videoData == null || videoData.Count == 0)
            {
                return NotFound("Video not found");
            }

            var viewModel = videoData.First();

            //error checking
            if (viewModel == null)
            {
                return NotFound("Video not found");
            }

            //error checking for description and fills to prevent null reference exceptions
            if (viewModel.Description == null || viewModel.Description == "")
            {
                viewModel.Description = "No description available.";
            }

            return View(viewModel);
        }

        [HttpGet("/Play/{id:int}")]   
        public async Task<IActionResult> Play(int id)
        {
            
            RemoteTcpClient remoteTcpClient = new RemoteTcpClient($"Select%DIRPATH%from%movies%where%id%equals%{id}");

            var rangeHeader = Request.GetTypedHeaders().Range;

            
            if (rangeHeader == null) { 
                return StatusCode(StatusCodes.Status404NotFound);
            }
 
            Dictionary<int, long> idVidLengthDict = 
                IdVidLengthDictionaryCache.GetorLoadCache();

            if(idVidLengthDict == null || !idVidLengthDict.ContainsKey(id))
            {
                return NotFound("Video length data not found.");
            }

            //ADJUST HEADERS TO ALLOW FOR PARTIAL CONTENT
            long videoLength = idVidLengthDict[id];
           
            long amountToRequest = videoLength;
            long startPosition = 0;
            if (rangeHeader == null || rangeHeader.Ranges.Count == 0)
            {
                Response.Headers["Content-Length"] = videoLength.ToString();
            }
            else
            {
                long from = rangeHeader.Ranges.First().From.GetValueOrDefault(0);
                long to = rangeHeader.Ranges.First().To.GetValueOrDefault(videoLength - 1);
                long contentLength = to - from + 1;
                amountToRequest = contentLength; //might need later
                startPosition = from;
                Response.StatusCode = StatusCodes.Status206PartialContent; // Partial Content
                Response.Headers["Content-Range"] = $"bytes {from}-{to}/{videoLength}";
                Response.Headers["Content-Length"] = contentLength.ToString(); //may or may not need 
                Response.Headers["Accept-Ranges"] = "bytes";
            }

            Console.WriteLine($"From: {startPosition}");
            Console.WriteLine($"Amount requested from browser {amountToRequest}");

            remoteTcpClient.ConnectVideoAsync(startPosition, amountToRequest).Wait();

            byte[] slice = remoteTcpClient.VideoBuffer;

            await Response.Body.WriteAsync(slice, 0, slice.Length);

            return new EmptyResult();

        }
    }
}
