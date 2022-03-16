using MSAuth.Models;
using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MSAuth.DAL
{
    public class MSAuthContext : IdentityDbContext<CustomUserModel>
    {
        public DbSet<RefreshTokenModel> RefreshTokens { get; set; }

        public MSAuthContext(DbContextOptions<MSAuthContext> options): base(options) { }
    }
}
