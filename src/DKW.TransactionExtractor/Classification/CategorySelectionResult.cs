namespace DKW.TransactionExtractor.Classification;

public record CategorySelectionResult(
    String CategoryId,
    String CategoryName,
    MatcherCreationRequest? MatcherRequest,
    String? Comment = null,
    Boolean RequestedExit = false
)
{
    public static CategorySelectionResult GoBack => new("__GO_BACK__", String.Empty, null);
    public static CategorySelectionResult Invalid => new("__INVALID__", String.Empty, null);
    
    public Boolean IsGoBack => CategoryId == "__GO_BACK__";
    public Boolean IsInvalid => CategoryId == "__INVALID__";
    public Boolean HasMatcherRequest => MatcherRequest != null;
}
