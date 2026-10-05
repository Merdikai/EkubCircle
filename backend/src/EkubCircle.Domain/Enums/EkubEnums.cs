namespace EkubCircle.Domain.Enums;

public static class CircleStatus
{
    public const string Draft = "Draft";
    public const string Forming = "Forming";
    public const string Active = "Active";
    public const string Completed = "Completed";
}

public static class CircleFrequency
{
    public const string Weekly = "Weekly";
    public const string Monthly = "Monthly";
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
    public const string Drawn = "Drawn";
    public const string Closed = "Closed";
    public const string PaidOut = "PaidOut";
}

public static class PaymentType
{
    public const string Normal = "Normal";
    public const string Extra = "Extra";
    public const string Contribution = "Normal";
    public const string Payout = "Payout";
}

public static class PaymentStatus
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";
    public const string Failed = "Failed";
}

public static class JoinRequestStatus
{
    public const string Pending = "Pending";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
}

public static class UserRole
{
    public const string Member = "Member";
    public const string Organizer = "Organizer";
    public const string Admin = "Admin";
    public const string User = "User";
}
