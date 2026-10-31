using Microsoft.EntityFrameworkCore;

namespace PriceBridge;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) { }
