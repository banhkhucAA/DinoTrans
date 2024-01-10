using DinoTrans.Shared.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Data
{
    public class DinoTransDbContext: IdentityDbContext <ApplicationUser, ApplicationRole, int>
    {
        public DinoTransDbContext(DbContextOptions<DinoTransDbContext> options) : base(options)
        {

        }
        public DbSet<Company>Companies { get; set; }
        public DbSet<Location>Locations { get; set; }  
    }
}
