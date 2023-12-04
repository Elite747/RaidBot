using Microsoft.EntityFrameworkCore;

namespace RaidBot.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Expansion> Expansions { get; set; }
    public DbSet<GuildExpansionConfiguration> GuildExpansionConfigurations { get; set; }
    public DbSet<PlayerClass> Classes { get; set; }
    public DbSet<ExpansionClass> ExpansionClasses { get; set; }
    public DbSet<Raid> Raids { get; set; }
    public DbSet<RaidMember> RaidMembers { get; set; }
    public DbSet<PlayerRole> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expansion>()
            .HasMany(e => e.Classes)
            .WithMany(e => e.Expansions)
            .UsingEntity<ExpansionClass>();

        new ExpansionAndClassBuilder().Build(out var expansions, out var classes, out var expansionClasses);

        modelBuilder.Entity<Expansion>().HasData(expansions);
        modelBuilder.Entity<PlayerClass>().HasData(classes);
        modelBuilder.Entity<ExpansionClass>().HasData(expansionClasses);

        modelBuilder.Entity<PlayerRole>().HasData([
            new PlayerRole { Id = 1, Name = "Tanks", Icon = "<:tank:945067186421116958>", SearchTerms = "tanks;tank;t" },
            new PlayerRole { Id = 2, Name = "Healers", Icon = "<:healer:945067186542772244>", SearchTerms = "healers;healer;heals;heal;h" },
            new PlayerRole { Id = 3, Name = "Ranged", Icon = "<:rdps:1179280825196478555>", SearchTerms = "ranged;caster;rdps;r" },
            new PlayerRole { Id = 4, Name = "Melee", Icon = "<:mdps:945067186421104670>", SearchTerms = "melee;mdps;m" },
        ]);
    }
}
