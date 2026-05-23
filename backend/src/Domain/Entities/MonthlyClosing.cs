namespace Domain.Entities;

public class MonthlyClosing
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime ClosedAtUtc { get; set; }
    public required IReadOnlyCollection<Guid> AccountIds { get; set; }
    public required IReadOnlyCollection<string> Participants { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPerPerson { get; set; }
}
