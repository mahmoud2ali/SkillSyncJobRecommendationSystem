using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Claims;
namespace backend.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecommendationController : ControllerBase
    {

        private readonly Context _context;
        private readonly IHttpClientFactory _httpClientFactory;
        
        public RecommendationController(Context context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }


        [HttpGet("TopJobs/{id}")]
        public async Task<IActionResult> GetTopJobs(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var document = await _context.UserDocuments
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (document == null)
                return NotFound("Document not found");

            if (document.Embeddings == null || document.Embeddings.Length == 0)
            {
                await ComputeEmbeddings(id);
            }

            float[] embedding = BytesToFloatArray(document.Embeddings);

            var requestBody = JsonSerializer.Serialize(new
            {
                embeddings = embedding
            });

            HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient("AIService");
    
            HttpResponseMessage response = await client.PostAsync("get_top_jobs", content); 


            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"AI service error: {error}");
            }

            string result = await response.Content.ReadAsStringAsync();
            var topJobs = JsonSerializer.Deserialize<object>(result);

            return Ok(topJobs);
        }

        [HttpGet("Embeddings/{id}")]
        public async Task<IActionResult> ComputeEmbeddings(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var document = await _context.UserDocuments
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            if (document == null)
                return NotFound();

            var client = _httpClientFactory.CreateClient("AIService");

            string jsonString = Decompres(document.RawData);


            var requestBody = new { cvData = jsonString };

            var payload =JsonSerializer.Serialize(requestBody);
     

            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("get_embeddings", content);


            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"AI service error: {error}");
            }

            string result = await response.Content.ReadAsStringAsync();

            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(result);


            if (embeddingResponse?.Embedding == null)
                return BadRequest("Embedding not returned from AI service.");

            float[] embeddingArray = embeddingResponse.Embedding.ToArray();

            document.Embeddings = FloatArrayToBytes(embeddingArray);

            _context.UserDocuments.Update(document);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Embeddings computed and saved to database",
            });

        }


        #region
        public static byte[] FloatArrayToBytes(float[] floats)
        {
            byte[] bytes = new byte[floats.Length * sizeof(float)];
            Buffer.BlockCopy(floats, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static float[] BytesToFloatArray(byte[] bytes)
        {
            float[] floats = new float[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
            return floats;
        }


        public static string Decompres(byte[] compressed)
        {
            if (compressed == null || compressed.Length == 0)
                return string.Empty;

            using var msi = new MemoryStream(compressed);
            using var mso = new MemoryStream();
            using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }

            return Encoding.UTF8.GetString(mso.ToArray());
        }

        #endregion
    }
       
}
