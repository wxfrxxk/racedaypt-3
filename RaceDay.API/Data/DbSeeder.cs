using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Data
{
    // Applies migrations and inserts realistic demo data the first time the API starts.
    public static class DbSeeder
    {
        public static void MigrateAndSeed(RaceDayContext db)
        {
            // SQL Server may still be starting (e.g. in a container), so retry a few times.
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    db.Database.Migrate();
                    break;
                }
                catch (Exception) when (attempt < 8)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                }
            }

            if (db.Roles.Any())
            {
                return;
            }

            var organiserRole = new Role { RoleName = "Organiser" };
            var participantRole = new Role { RoleName = "Participant" };
            db.Roles.AddRange(organiserRole, participantRole);
            db.SaveChanges();

            User NewUser(string first, string last, string email, string phone, string password, int roleId) => new()
            {
                FirstName = first,
                LastName = last,
                Email = email,
                PhoneNumber = phone,
                Password = PasswordHelper.Hash(password),
                RoleID = roleId
            };

            var thabo = NewUser("Thabo", "Mokoena", "thabo@raceday.co.za", "0821234567", "Organiser#123", organiserRole.RoleID);
            var lerato = NewUser("Lerato", "Naidoo", "lerato@raceday.co.za", "0839876543", "Organiser#123", organiserRole.RoleID);
            var sipho = NewUser("Sipho", "Dlamini", "sipho@example.com", "0711112222", "Participant#123", participantRole.RoleID);
            var aisha = NewUser("Aisha", "Khan", "aisha@example.com", "0723334444", "Participant#123", participantRole.RoleID);
            var jabu = NewUser("Jabu", "Nkosi", "jabu@example.com", "0745556666", "Participant#123", participantRole.RoleID);
            var megan = NewUser("Megan", "van der Merwe", "megan@example.com", "0767778888", "Participant#123", participantRole.RoleID);
            db.Users.AddRange(thabo, lerato, sipho, aisha, jabu, megan);
            db.SaveChanges();

            DateTime today = DateTime.Today;

            Event NewEvent(string name, string description, int daysFromNow, string location, string type, decimal km, int organiserId) => new()
            {
                EventName = name,
                Description = description,
                EventDate = today.AddDays(daysFromNow),
                Location = location,
                EventType = type,
                Distance = km,
                OrganiserID = organiserId
            };

            var trail = NewEvent("Table Mountain Trail Run", "A scenic trail run on the slopes of Table Mountain with sweeping views of Cape Town.", 21, "Cape Town, Western Cape", "Run", 21.1m, thabo.UserID);
            var walk = NewEvent("Sea Point Promenade Walk", "A relaxed charity walk along the Sea Point promenade, open to all ages.", 35, "Sea Point, Cape Town", "Walk", 5m, thabo.UserID);
            var cycle = NewEvent("Winelands Cycle Challenge", "A road cycling challenge through the Stellenbosch winelands.", 49, "Stellenbosch, Western Cape", "Cycle", 65m, lerato.UserID);
            var soweto = NewEvent("Soweto Spring 10K", "A fast and friendly 10 km road race through the streets of Soweto.", -14, "Soweto, Gauteng", "Run", 10m, lerato.UserID);
            db.Events.AddRange(trail, walk, cycle, soweto);
            db.SaveChanges();

            Category Cat(string name, Event e) => new() { CategoryName = name, EventID = e.EventID };

            var trail21 = Cat("21km Open", trail);
            var trail10 = Cat("10km Fun Run", trail);
            var walk5 = Cat("5km Family Walk", walk);
            var walk3 = Cat("3km Social Walk", walk);
            var cycle65 = Cat("65km Senior", cycle);
            var cycle35 = Cat("35km Under 20", cycle);
            var sowetoSenior = Cat("10km Senior", soweto);
            var sowetoVet = Cat("10km Veteran", soweto);
            db.Categories.AddRange(trail21, trail10, walk5, walk3, cycle65, cycle35, sowetoSenior, sowetoVet);
            db.SaveChanges();

            Registration Reg(User u, Event e, Category c, string status, int daysAgo) => new()
            {
                UserID = u.UserID,
                EventID = e.EventID,
                CategoryID = c.CategoryID,
                Status = status,
                RegistrationDate = today.AddDays(-daysAgo)
            };

            var regs = new[]
            {
                Reg(sipho, trail, trail21, "Confirmed", 5),
                Reg(aisha, trail, trail10, "Pending", 3),
                Reg(jabu, walk, walk5, "Confirmed", 4),
                Reg(megan, cycle, cycle65, "Pending", 2),
                Reg(sipho, soweto, sowetoSenior, "Confirmed", 30),
                Reg(aisha, soweto, sowetoSenior, "Confirmed", 29),
                Reg(jabu, soweto, sowetoSenior, "Confirmed", 28),
                Reg(megan, soweto, sowetoVet, "Confirmed", 27)
            };
            db.Registrations.AddRange(regs);
            db.SaveChanges();

            db.Results.AddRange(
                new Result { RegistrationID = regs[4].RegistrationID, FinishTime = new TimeSpan(0, 44, 12), Position = 2 },
                new Result { RegistrationID = regs[5].RegistrationID, FinishTime = new TimeSpan(0, 41, 55), Position = 1 },
                new Result { RegistrationID = regs[6].RegistrationID, FinishTime = new TimeSpan(0, 47, 30), Position = 3 },
                new Result { RegistrationID = regs[7].RegistrationID, FinishTime = new TimeSpan(0, 52, 8), Position = 4 });
            db.SaveChanges();
        }
    }
}
