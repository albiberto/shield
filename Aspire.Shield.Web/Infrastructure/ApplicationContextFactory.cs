// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Design;
//
// namespace Aspire.Shield.Web.Infrastructure;
//
// public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
// {
//     public ApplicationContext CreateDbContext(string[] args)
//     {
//         var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
//
//         // Utilizziamo una connessione locale per la generazione delle migrazioni.
//         // Non è necessario che il DB esista realmente per il comando 'add migration',
//         // serve solo per capire il provider (SQL Server) e generare il codice.
//         optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AspireShield_Dev;Trusted_Connection=True;MultipleActiveResultSets=true");
//
//         return new ApplicationContext(optionsBuilder.Options);
//     }
// }