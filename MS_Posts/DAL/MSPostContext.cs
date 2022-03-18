using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MS_Posts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MS_Posts.DAL
{
    public class MSPostContext : DbContext
    {
        public DbSet<PostModel> Posts { get; set; }

        public MSPostContext(DbContextOptions<MSPostContext> options) : base(options) { }
    }
}
