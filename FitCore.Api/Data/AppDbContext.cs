using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options);
