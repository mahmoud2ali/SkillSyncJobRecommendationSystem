using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;


namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentsController : ControllerBase
    {

        private readonly Context _context;
        private readonly IWebHostEnvironment _env;

        public DocumentsController(Context context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile()
        {
            IFormFile file = Request.Form.Files.FirstOrDefault();
            
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (file.ContentType != "application/pdf")
            {
                return BadRequest("Only PDF files are allowed.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            //Read PDF bytes into memory
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            string extractedText = ExtractTextFromPdf(fileBytes);

            //var payload = new { cvData = extractedText };
            //string jsonString = JsonSerializer.Serialize(payload);

            byte[] payload = Compress(extractedText);

            var document = new UserDocument
            {
                FileName = file.FileName,
                FilePath = "filepath",
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow,
                RawData = payload,
                UserId = userId
            };

            _context.UserDocuments.Add(document);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "PDF uploaded successfully",
                documentId = document.Id
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var document = await _context.UserDocuments
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (document == null)
                return NotFound();

 
            _context.UserDocuments.Remove(document);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Document deleted" });

        }

        

        #region helper methods

        private string ExtractTextFromPdf(byte[] fileBytes)
        {
            var sb = new StringBuilder();

            using var pdfDocument = PdfDocument.Open(fileBytes);

            foreach (Page page in pdfDocument.GetPages())
            {
                //sb.AppendLine($"--- Page {page.Number} ---");
                sb.AppendLine(page.Text);
            }

            return sb.ToString();
        }

        public static byte[] Compress(string cvText)
        {
            if (string.IsNullOrEmpty(cvText))
                return Array.Empty<byte>();

            var bytes = Encoding.UTF8.GetBytes(cvText);

            using var mso = new MemoryStream();
            using (var gs = new GZipStream(mso, CompressionMode.Compress))
            {
                gs.Write(bytes, 0, bytes.Length);
            }
            return mso.ToArray();
        }

        #endregion
    }

}

