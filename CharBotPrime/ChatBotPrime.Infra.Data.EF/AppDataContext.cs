using ChatBotPrime.Core.Data.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace ChatBotPrime.Infra.Data.EF
{
	public class AppDataContext : IdentityDbContext
	{
		public DbSet<BasicCommand> BasicCommands { get; set; }
		public DbSet<BasicMessage> BasicMessages { get; set; }

		public AppDataContext(DbContextOptions options) : base(options)
		{
			
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<BasicCommand>()
				.HasMany(c => c.Aliases)
				.WithOne(a => a.command);

			modelBuilder.Entity<BasicMessage>()
				.HasMany(m => m.Aliases)
				.WithOne(a => a.Message);
		}
	}

	public class AppDataContextFactory : IDesignTimeDbContextFactory<AppDataContext>
	{
		public AppDataContext CreateDbContext(string[] args)
		{
			// Build config
			IConfiguration config = new ConfigurationBuilder()
				.AddUserSecrets<AppDataContext>()
				.Build();

			var optionsBuilder = new DbContextOptionsBuilder<AppDataContext>();
			var connectionString = config.GetConnectionString("DefaultConnection");
			optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("ChatBotPrime.Infra.Data.EF"));
			return new AppDataContext(optionsBuilder.Options);
		}
	}
}
