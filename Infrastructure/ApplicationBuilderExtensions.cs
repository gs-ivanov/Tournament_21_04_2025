namespace Tournament.Infrastructure
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Models;
    using static Tournament.WebConstants;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder PrepareDatabase(
            this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var services = serviceScope.ServiceProvider;

            var context = services.GetRequiredService<TurnirDbContext>();

            MigrateDatabase(services);
            SeedDefaultTeams(context);
            SeedTournamentsAsync(context);

            SeedRolesAndAdministrator(services);
            //SeedRolesAndAdministrator(services);

            return app;
        }

        private static void MigrateDatabase(IServiceProvider services)
        {
            var data = services.GetRequiredService<TurnirDbContext>();
            data.Database.Migrate();
        }

        private static void SeedRolesAndAdministrator(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            Task
                .Run(async () =>
                {
                    // Създаване на роли, ако не съществуват
                    string[] roleNames = { AdministratorRoleName, "Editor" };

                    foreach (var roleName in roleNames)
                    {
                        if (!await roleManager.RoleExistsAsync(roleName))
                        {
                            await roleManager.CreateAsync(new IdentityRole(roleName));
                        }
                    }

                    // Създаване на Администратор
                    const string adminEmail = "admin@tur.com";
                    const string adminPassword = "123";

                    var adminUser = await userManager.FindByEmailAsync(adminEmail);
                    if (adminUser == null)
                    {
                        var user = new User
                        {
                            Email = adminEmail,
                            UserName = adminEmail,
                            FullName = "Admin"
                        };

                        await userManager.CreateAsync(user, adminPassword);
                        await userManager.AddToRoleAsync(user, AdministratorRoleName);
                    }

                    // (Опционално) Създаване на Editor акаунт за тестове
                    const string editorEmail = "editor@tur.com";
                    const string editorPassword = "123";

                    var editorUser = await userManager.FindByEmailAsync(editorEmail);
                    if (editorUser == null)
                    {
                        var user = new User
                        {
                            Email = editorEmail,
                            UserName = editorEmail,
                            FullName = "Editor"
                        };

                        await userManager.CreateAsync(user, editorPassword);
                        await userManager.AddToRoleAsync(user, "Editor");
                    }
                })
                .GetAwaiter()
                .GetResult();
        }

        private static void SeedDefaultTeams(TurnirDbContext context)
        {
            if (context.Teams.Any())
                return;

            List<string> teamNames = new()
                {
                    "Лудогорец", "Крумовград", "Левски София", "Локомотив Пловдив",
                    "Славия София", "Черно море", "Арда", "Ботев Враца",
                    "ЦСКА София", "Септември София", "Спартак Варна", "Ботев Пловдив",
                    "Берое", "Хебър", "ЦСКА 1948", "Миньор Перник"
                };

            List<string> teamLogos = new()
                {
                    "/logos/ludogorec.png", "/logos/krumovgrad.png", "/logos/levski.png", "/logos/lokomotivplovdiv.png",
                    "/logos/slavia.png", "/logos/chernomore.png", "/logos/arda.png", "/logos/botevvraca.png",
                    "/logos/cskasofia.png", "/logos/septemvri.png", "/logos/spartakvarna.png", "/logos/botevplovdiv.png",
                    "/logos/beroe.png", "/logos/hebar.png", "/logos/cska1948.png", "/logos/minyor.png"
                };

            for (int i = 0; i < teamNames.Count; i++)
            {
                context.Teams.Add(new Team
                {
                    Name = teamNames[i],
                    CoachName = "Н/Д",
                    FeePaid = false,
                    LogoUrl = teamLogos[i]
                });
            }
            context.SaveChanges();
        }

        public static void SeedTournamentsAsync(TurnirDbContext context)
        {
            if (!context.Tournaments.Any())
            {
                var tournaments = new List<Tournament>
                {
                    new Tournament
                    {
                        Name = "Елиминации",
                        Type = TournamentType.Knockout, // 1
                        TypeName = TournamentType.Knockout.ToString(),
                        StartDate = new DateTime(2025, 7, 1),
                        IsActive = false,
                        IsOpenForApplications = false
                    },
                    new Tournament
                    {
                        Name = "Двойна елиминация",
                        Type = TournamentType.DoubleElimination, // 2
                        TypeName = TournamentType.DoubleElimination.ToString(),
                        StartDate = new DateTime(2025, 10, 1),
                        IsActive = false,
                        IsOpenForApplications = false
                    },
                    new Tournament
                    {
                        Name = "Всеки срещу всеки 2025",
                        Type = TournamentType.RoundRobin, // 3
                        TypeName = TournamentType.RoundRobin.ToString(),
                        StartDate = new DateTime(2025, 6, 1),
                        IsActive = true,
                        IsOpenForApplications = true
                    },
                    new Tournament
                    {
                        Name = "Групи + елиминации",
                        Type = TournamentType.GroupAndKnockout, // 4
                        TypeName = TournamentType.GroupAndKnockout.ToString(),
                        StartDate = new DateTime(2025, 9, 1),
                        IsActive = false,
                        IsOpenForApplications = false
                    },
                    new Tournament
                    {
                        Name = "Швейцарска система",
                        Type = TournamentType.Swiss, // 5
                        TypeName = TournamentType.Swiss.ToString(),
                        StartDate = new DateTime(2025, 8, 1),
                        IsActive = false,
                        IsOpenForApplications = false
                    }
                };

                context.Tournaments.AddRange(tournaments);
                context.SaveChangesAsync();
            }
        }
    }
}
