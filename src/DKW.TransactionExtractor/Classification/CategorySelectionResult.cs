namespace DKW.TransactionExtractor.Classification;

public record CategorySelectionResult(
    String CategoryId,
    String CategoryName,
    Boolean AddRule,
    Boolean RequestedExit = false
)
{
    public static CategorySelectionResult GoBack => new("__GO_BACK__", String.Empty, false);
    public static CategorySelectionResult Invalid => new("__INVALID__", String.Empty, false);
    
    public Boolean IsGoBack => CategoryId == "__GO_BACK__";
    public Boolean IsInvalid => CategoryId == "__INVALID__";
}
