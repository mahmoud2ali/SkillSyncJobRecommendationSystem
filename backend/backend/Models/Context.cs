using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Models
{
    public class Context : IdentityDbContext<ApplicationUser>
    {
            
        public DbSet<UserDocument> UserDocuments { get; set; }

        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserDocument>(entity =>
            {

                // Relationship with Identity User
                entity.HasOne(d => d.User)
                      .WithMany(u => u.Documents)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(d => d.RawData)
                    .HasColumnType("varbinary(max)");
            });
                
        }

    }
}
