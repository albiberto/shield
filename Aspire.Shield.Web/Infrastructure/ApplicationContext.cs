using Aspire.Shield.Web.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;

namespace Aspire.Shield.Web.Infrastructure;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
{
    public DbSet<Sample> Samples => Set<Sample>();
}