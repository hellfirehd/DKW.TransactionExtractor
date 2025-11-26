# DKW.TransactionExtractor - Coding Standards

This document outlines the coding standards and best practices for the DKW.TransactionExtractor project.

## Table of Contents
- [General Principles](#general-principles)
- [Control Flow](#control-flow)
- [Error Handling](#error-handling)
- [Naming Conventions](#naming-conventions)
- [Code Organization](#code-organization)

## General Principles

### Prefer Clarity Over Cleverness
Write code that is easy to read and maintain. If you need to explain how code works, consider refactoring it.

### Single Responsibility Principle
Each class and method should have one clear purpose. If a method does multiple things, break it into smaller methods.

## Control Flow

### ? Avoid Recursion

**Prefer iterative (loop-based) solutions over recursive solutions.**

#### Rationale
- **Stack Safety**: Prevents stack overflow with deep call chains or unexpected input
- **Debuggability**: Easier to step through and understand execution flow
- **Performance**: No call stack overhead or function call penalties
- **Explicit State**: Loop variables make state changes visible
- **Maintainability**: More developers are comfortable with loops than recursion

#### Bad Example ?
```csharp
private CategorySelectionResult SelectCategory(Transaction transaction)
{
    var input = Console.ReadLine();
    
    if (!IsValid(input))
    {
        Console.WriteLine("Invalid. Try again.");
        return SelectCategory(transaction); // ? Recursive retry
    }
    
    return ProcessInput(input);
}
```

**Problems:**
- Stack grows with each retry
- Hard to add retry limits
- State is implicit in call stack
- Difficult to unit test

#### Good Example ?
```csharp
private CategorySelectionResult SelectCategory(Transaction transaction)
{
    var input = Console.ReadLine();
    
    if (!IsValid(input))
    {
        return CategorySelectionResult.Invalid; // ? Return sentinel value
    }
    
    return ProcessInput(input);
}

// Caller handles retry with a loop
public CategorySelectionResult PromptForCategory(...)
{
    while (true)
    {
        var result = SelectCategory(transaction);
        
        if (result.IsInvalid)
        {
            Console.WriteLine("Invalid. Try again.");
            continue; // ? Explicit retry logic
        }
        
        return result;
    }
}
```

**Benefits:**
- No stack growth
- Easy to add retry counters: `var attempts = 0; while (attempts++ < maxRetries)`
- Clear control flow
- Testable in isolation

#### Acceptable Uses of Recursion

Recursion is appropriate when:

1. **Tree/Graph Traversal**: Natural fit for hierarchical data structures
   ```csharp
   void TraverseTree(Node node)
   {
       if (node == null) return;
       ProcessNode(node);
       TraverseTree(node.Left);
       TraverseTree(node.Right);
   }
   ```

2. **Divide-and-Conquer Algorithms**: QuickSort, MergeSort, etc.
   ```csharp
   void QuickSort(int[] arr, int low, int high)
   {
       if (low < high)
       {
           int pivot = Partition(arr, low, high);
           QuickSort(arr, low, pivot - 1);
           QuickSort(arr, pivot + 1, high);
       }
   }
   ```

3. **Bounded Depth**: When recursion depth is provably limited and small
   ```csharp
   // Maximum depth is configuration nesting level (typically 2-3)
   void MergeConfiguration(Config parent, Config child, int depth)
   {
       if (depth > 5) throw new InvalidOperationException("Config too deep");
       // ... merge logic with bounded recursive calls
   }
   ```

**Rule of Thumb**: If you're using recursion for retry logic, user input validation, or menu navigation, use a loop instead.

## Error Handling

### Use Result Types Over Exceptions for Expected Failures

For expected failures (invalid input, not found, etc.), return a result object instead of throwing exceptions.

#### Good Example ?
```csharp
public record CategorySelectionResult(
    String CategoryId,
    String CategoryName,
    Boolean AddRule,
    Boolean RequestedExit = false)
{
    public static CategorySelectionResult Invalid => new("__INVALID__", String.Empty, false);
    public Boolean IsInvalid => CategoryId == "__INVALID__";
}
```

#### When to Use Exceptions
- Truly exceptional conditions (file system errors, network failures)
- Programming errors (null arguments, invalid state)
- Third-party library failures

## Naming Conventions

### Use Descriptive Names
- **Classes**: `CategorySelectionResult` not `Result`
- **Methods**: `SelectExistingCategory` not `Select`
- **Variables**: `sortedCategories` not `items`

### Boolean Properties
- Use `Is`, `Has`, `Can`, `Should` prefixes
- Examples: `IsValid`, `HasChildren`, `CanExecute`, `ShouldRetry`

### Sentinel Values
- Use descriptive constants with `__` prefix for internal sentinel values
- Examples: `__INVALID__`, `__GO_BACK__`, `__NOT_FOUND__`

## Code Organization

### File Structure
- One public type per file
- File name matches the primary type name
- Related types (records, enums) can share a file if small and tightly coupled

### Namespace Organization
```
DKW.TransactionExtractor/
??? Classification/          # Feature-based grouping
?   ??? ITransactionClassifier.cs
?   ??? TransactionClassifier.cs
?   ??? CategorySelectionResult.cs
?   ??? ...
??? Formatting/             # Feature-based grouping
?   ??? ITransactionFormatter.cs
?   ??? CsvFormatter.cs
?   ??? JsonFormatter.cs
??? Models/                 # Shared models
    ??? Transaction.cs
    ??? Category.cs
```

### Dependency Injection
- Prefer constructor injection
- Keep constructors simple (assignment only)
- Use primary constructors in .NET 8+ when appropriate

```csharp
public class TransactionClassifier(
    ILogger<TransactionClassifier> logger,
    ICategoryRepository repository) : ITransactionClassifier
{
    // Automatically creates private readonly fields
}
```

## Testing

### Unit Test Naming
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    // Act
    // Assert
}
```

### Test Organization
- One test class per production class
- Group related tests with nested classes or theory data

## Code Review Checklist

Before submitting a PR, verify:

- [ ] No recursion used for retry/validation logic (use loops + sentinel values)
- [ ] No hard-coded strings (use constants or configuration)
- [ ] Descriptive variable and method names
- [ ] Each method has a single, clear responsibility
- [ ] Result types used for expected failures
- [ ] Unit tests added for new functionality
- [ ] No compiler warnings
- [ ] Code builds successfully

## References

- [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Clean Code by Robert C. Martin](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)

---

**Questions?** Open an issue or discuss in PR comments.
