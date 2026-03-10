using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class UserDocument
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string FileName { get; set; }
        [Required]
        public string FilePath { get; set; }
        [Required]

        public long FileSize { get; set; }

        public byte[] RawData { get; set; }

        public byte[]? Embeddings { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to ApplicationUser
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }

}
