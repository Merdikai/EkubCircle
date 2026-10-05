namespace EkubCircle.Domain.Enums;

public static class CircleStatus
{
    public const string Forming = "Forming";
    public const string Active = "Active";
    public const string Completed = "Completed";
}

public static class CircleRole
{
    public const string Organizer = "Organizer";
    public const string Member = "Member";
}

public static class RoundStatus
{
    public const string Pending = "Pending";
    public const string Open = "Open";
    public const string PaidOut = "PaidOut";
}

public static class PaymentType
{
    public const string Contribution = "Normal";
    public const string Normal = "Normal";
    public const string Extra = "Extra";
    public const string Payout = "Payout";
}

public static class UserRole
{
    public const string Member = "Member";
    public const string Organizer = "Organizer";
    public const string Admin = "Admin";
    public const string User = "User";
}
