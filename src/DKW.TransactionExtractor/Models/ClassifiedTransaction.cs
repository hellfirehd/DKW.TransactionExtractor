namespace DKW.TransactionExtractor.Models;

public class ClassifiedTransaction
{
    public Transaction Transaction { get; set; } = new();
    public String CategoryId { get; set; } = String.Empty;
    public String CategoryName { get; set; } = String.Empty;
    public String? Comment { get; set; }
}
