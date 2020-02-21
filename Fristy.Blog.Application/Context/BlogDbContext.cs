using Microsoft.EntityFrameworkCore;

namespace Fristy.Blog.Application
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }
    }
}
