namespace Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool Paid { get; set; }
    public bool ParticipatesInDivision { get; set; }

    public void MarkAsPaid()
    {
        Paid = true;
    }
}