using EkubCircle.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(EkubDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Users.AnyAsync())
        {
            return; // DB has been seeded already
        }

        // 1. Seed Users
        // Hash for "Admin123!" and "Ekub123!"
        string adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        string userPasswordHash = BCrypt.Net.BCrypt.HashPassword("Ekub123!");

        var admin = new User
        {
            FullName = "Hackathon Admin",
            Email = "admin@hackathon.local",
            PasswordHash = adminPasswordHash,
            Role = "Admin",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var organizer = new User
        {
            FullName = "Abebe Bikila",
            Email = "organizer@ekub.local",
            PasswordHash = userPasswordHash,
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        var member1 = new User
        {
            FullName = "Hana Girma",
            Email = "member1@ekub.local",
            PasswordHash = userPasswordHash,
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-19)
        };

        var member2 = new User
        {
            FullName = "Dawit Tadesse",
            Email = "member2@ekub.local",
            PasswordHash = userPasswordHash,
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-18)
        };

        var member3 = new User
        {
            FullName = "Meron Bekele",
            Email = "member3@ekub.local",
            PasswordHash = userPasswordHash,
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-17)
        };

        var member4 = new User
        {
            FullName = "Selam Fikre",
            Email = "member4@ekub.local",
            PasswordHash = userPasswordHash,
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-16)
        };

        context.Users.AddRange(admin, organizer, member1, member2, member3, member4);
        await context.SaveChangesAsync();

        // 2. Seed Active Circle: "Bole Tech Savings Circle"
        var activeCircle = new Circle
        {
            Name = "Bole Tech Savings Circle",
            ContributionAmount = 1000m,
            MeetingLabel = "Weekly",
            Status = CircleStatus.Active,
            CreatedByUserId = organizer.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            StartedAt = DateTime.UtcNow.AddDays(-14)
        };

        // Seed Forming Circle: "Arat Kilo Traders Ekub" (For demonstrating Circle Creation, Member Invite & Start)
        var formingCircle = new Circle
        {
            Name = "Arat Kilo Traders Ekub",
            ContributionAmount = 2500m,
            MeetingLabel = "Monthly",
            Status = CircleStatus.Forming,
            CreatedByUserId = organizer.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        context.Circles.AddRange(activeCircle, formingCircle);
        await context.SaveChangesAsync();

        // 3. Seed Circle Members for Active Circle
        var cmOrganizer = new CircleMember
        {
            CircleId = activeCircle.Id,
            UserId = organizer.Id,
            MemberOrder = 1,
            RoleInCircle = CircleRole.Organizer,
            HasReceived = true, // Received round 1 pot
            JoinedAt = DateTime.UtcNow.AddDays(-14)
        };

        var cmHana = new CircleMember
        {
            CircleId = activeCircle.Id,
            UserId = member1.Id,
            MemberOrder = 2,
            RoleInCircle = CircleRole.Member,
            HasReceived = false,
            JoinedAt = DateTime.UtcNow.AddDays(-14)
        };

        var cmDawit = new CircleMember
        {
            CircleId = activeCircle.Id,
            UserId = member2.Id,
            MemberOrder = 3,
            RoleInCircle = CircleRole.Member,
            HasReceived = false,
            JoinedAt = DateTime.UtcNow.AddDays(-14)
        };

        var cmMeron = new CircleMember
        {
            CircleId = activeCircle.Id,
            UserId = member3.Id,
            MemberOrder = 4,
            RoleInCircle = CircleRole.Member,
            HasReceived = false,
            JoinedAt = DateTime.UtcNow.AddDays(-14)
        };

        var cmSelam = new CircleMember
        {
            CircleId = activeCircle.Id,
            UserId = member4.Id,
            MemberOrder = 5,
            RoleInCircle = CircleRole.Member,
            HasReceived = false,
            JoinedAt = DateTime.UtcNow.AddDays(-14)
        };

        // Members for Forming Circle
        var cmForming1 = new CircleMember
        {
            CircleId = formingCircle.Id,
            UserId = organizer.Id,
            MemberOrder = 0,
            RoleInCircle = CircleRole.Organizer,
            HasReceived = false,
            JoinedAt = DateTime.UtcNow.AddDays(-2)
        };

        var cmForming2 = new CircleMember
        {
            CircleId = formingCircle.Id,
            UserId = member1.Id,
            MemberOrder = 0,
            RoleInCircle = CircleRole.Member,
            HasReceived = false,
            JoinedAt = DateTime.UtcNow.AddDays(-1)
        };

        context.CircleMembers.AddRange(cmOrganizer, cmHana, cmDawit, cmMeron, cmSelam, cmForming1, cmForming2);
        await context.SaveChangesAsync();

        // 4. Seed Rounds for Active Circle (5 members -> 5 rounds)
        var round1 = new Round
        {
            CircleId = activeCircle.Id,
            RoundNumber = 1,
            ReceiverMemberId = cmOrganizer.Id,
            Status = RoundStatus.PaidOut,
            PotAmount = 5000m,
            PaidOutAt = DateTime.UtcNow.AddDays(-7)
        };

        var round2 = new Round
        {
            CircleId = activeCircle.Id,
            RoundNumber = 2,
            ReceiverMemberId = cmHana.Id,
            Status = RoundStatus.Open,
            PotAmount = 3000m,
            PaidOutAt = null
        };

        var round3 = new Round
        {
            CircleId = activeCircle.Id,
            RoundNumber = 3,
            ReceiverMemberId = cmDawit.Id,
            Status = "Pending",
            PotAmount = 0m
        };

        var round4 = new Round
        {
            CircleId = activeCircle.Id,
            RoundNumber = 4,
            ReceiverMemberId = cmMeron.Id,
            Status = "Pending",
            PotAmount = 0m
        };

        var round5 = new Round
        {
            CircleId = activeCircle.Id,
            RoundNumber = 5,
            ReceiverMemberId = cmSelam.Id,
            Status = "Pending",
            PotAmount = 0m
        };

        context.Rounds.AddRange(round1, round2, round3, round4, round5);
        await context.SaveChangesAsync();

        // 5. Seed Payments for Round 1 (All 5 paid -> 5000 Birr pot disbursed)
        var p1_1 = new Payment { RoundId = round1.Id, MemberId = cmOrganizer.Id, Amount = 1000m, PaymentType = PaymentType.Normal, PaidAt = DateTime.UtcNow.AddDays(-8), RecordedByUserId = organizer.Id };
        var p1_2 = new Payment { RoundId = round1.Id, MemberId = cmHana.Id, Amount = 1000m, PaymentType = PaymentType.Normal, PaidAt = DateTime.UtcNow.AddDays(-8), RecordedByUserId = organizer.Id };
        var p1_3 = new Payment { RoundId = round1.Id, MemberId = cmDawit.Id, Amount = 1000m, PaymentType = PaymentType.Normal, PaidAt = DateTime.UtcNow.AddDays(-7), RecordedByUserId = organizer.Id };
        var p1_4 = new Payment { RoundId = round1.Id, MemberId = cmMeron.Id, Amount = 1000m, PaymentType = PaymentType.Normal, PaidAt = DateTime.UtcNow.AddDays(-7), RecordedByUserId = organizer.Id };
        var p1_5 = new Payment { RoundId = round1.Id, MemberId = cmSelam.Id, Amount = 1000m, PaymentType = PaymentType.Normal, PaidAt = DateTime.UtcNow.AddDays(-7), RecordedByUserId = organizer.Id };

        // Seed Payments for Round 2 (3 paid: Abebe, Hana, Dawit. Meron and Selam unpaid -> ready for demo!)
        var p2_1 = new Payment { RoundId = round2.Id, MemberId = cmOrganizer.Id, Amount = 1000m, PaymentType = PaymentType.Normal, PaidAt = DateTime.UtcNow.AddDays(-1), RecordedByUserId = organizer.Id };
        var p2_2 = new Payment { RoundId = round2.Id, MemberId = cmHana.Id, Amount = 1000m, PaymentType = PaymentType.Normal, PaidAt = DateTime.UtcNow.AddDays(-1), RecordedByUserId = organizer.Id };
        var p2_3 = new Payment { RoundId = round2.Id, MemberId = cmDawit.Id, Amount = 1000m, PaymentType = PaymentType.Normal, PaidAt = DateTime.UtcNow.AddHours(-3), RecordedByUserId = organizer.Id };

        context.Payments.AddRange(p1_1, p1_2, p1_3, p1_4, p1_5, p2_1, p2_2, p2_3);
        await context.SaveChangesAsync();
    }
}
