# GitHub Copilot Workspace Instructions

This file provides context and coding standards for GitHub Copilot when working with the DKW Transaction Extractor project.

## Project Overview

**DKW Transaction Extractor** is a .NET console application that extracts, parses, and classifies credit card transactions from PDF statements. It features an interactive classification system with configurable matchers (ExactMatch, Contains, Regex) and outputs results in CSV or JSON format.

## Technology Stack

- **.NET Version**: .NET 10
- **Language**: C# 14.0
- **Project Type**: Console Application
- **Logging**: Serilog
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **Testing**: xUnit

## Coding Standards

### Naming Conventions

1. **Use `String` instead of `string`** - Always use the BCL type name
   ```csharp
   // ? Correct
   public String CategoryName { get; set; }
   
   // ? Incorrect
   public string CategoryName { get; set; }
   ```

2. **Use `Boolean` instead of `bool`** - Consistent with String convention
   ```csharp
   // ? Correct
   public Boolean IsActive { get; set; }
   
   // ? Incorrect
   public bool IsActive { get; set; }
   ```

3. **Use `Int32` instead of `int`** - For consistency
   ```csharp
   // ? Correct
   public Int32 Count { get; set; }
   
   // ? Incorrect
   public int Count { get; set; }
   ```

4. **Descriptive Names** - Clear, self-documenting code
   - Classes: `CategorySelectionResult` not `Result`
   - Methods: `SelectExistingCategory` not `Select`
   - Variables: `sortedCategories` not `items`

5. **Boolean Properties** - Use meaningful prefixes
   - Use `Is`, `Has`, `Can`, `Should` prefixes
   - Examples: `IsValid`, `HasChildren`, `CanExecute`, `ShouldRetry`

6. **Sentinel Values** - Use descriptive constants
   - Use `__` prefix for internal sentinel values
   - Examples: `__INVALID__`, `__GO_BACK__`, `__NOT_FOUND__`

### Control Flow Best Practices

#### Avoid Recursion for Retry Logic

**Never use recursion for:**
- Retry logic
- User input validation
- Menu navigation
- State transitions

**Why?** Stack overflow risk with infinite loops (bad user input, network failures, etc.).

**Use Instead:** Loops with sentinel values or explicit exit conditions.

**Bad Example (Recursion Risk):**
```csharp
public CategorySelectionResult PromptForCategory()
{
    var result = ShowMenu();
    if (result.IsInvalid)
        return PromptForCategory(); // Stack overflow risk!
    return result;
}
```

**Good Example (Loop with Sentinel):**
```csharp
public CategorySelectionResult PromptForCategory()
{
    while (true)
    {
        var result = ShowMenu();
        if (result.IsInvalid)
        {
            Console.WriteLine("Invalid choice. Please try again.");
            continue;
        }
        if (result.IsGoBack)
        {
            continue; // Return to appropriate menu level
        }
        return result;
    }
}
```

#### When Recursion IS Acceptable

1. **Tree/Graph Traversal**: When data structure is inherently recursive
   ```csharp
   void TraverseDirectory(DirectoryInfo dir)
   {
       foreach (var file in dir.GetFiles()) Process(file);
       foreach (var subdir in dir.GetDirectories()) TraverseDirectory(subdir);
   }
   ```

2. **Divide and Conquer**: Classic algorithms (QuickSort, MergeSort)
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
   void MergeConfiguration(Config parent, Config child, int depth)
   {
       if (depth > 5) throw new InvalidOperationException("Config too deep");
       // ... merge logic with bounded recursive calls
   }
   ```

**Rule of Thumb**: If you're using recursion for retry logic, user input validation, or menu navigation, use a loop instead.

### Error Handling

#### Use Result Types Over Exceptions for Expected Failures

For expected failures (invalid input, not found, etc.), return a result object instead of throwing exceptions.

**? Good Example:**
```csharp
public record CategorySelectionResult(
    String CategoryId,
    String CategoryName,
    MatcherCreationRequest? MatcherRequest,
    Boolean RequestedExit = false)
{
    public static CategorySelectionResult Invalid => new("__INVALID__", String.Empty, null);
    public Boolean IsInvalid => CategoryId == "__INVALID__";
}
```

#### When to Use Exceptions
- Truly exceptional conditions (file system errors, network failures)
- Programming errors (null arguments, invalid state)
- Third-party library failures

#### Structured Logging
- Use Serilog with structured logging
- Log at appropriate levels (Debug, Info, Warning, Error)
- Include context in error messages
  ```csharp
  _logger.LogError("Failed to load category '{CategoryId}' from {FilePath}", 
      categoryId, filePath);
  ```

### Architecture Patterns

1. **Service Layer Pattern**
   - Use interfaces for all services (`IXxxService`)
   - Keep services focused on business logic
   - Repositories handle persistence only

2. **Dependency Injection**
   - Constructor injection for all dependencies
   - Register services in `Program.cs`
   - Use appropriate lifetimes (Singleton, Transient, Scoped)
   - Keep constructors simple (assignment only)

3. **Immutability**
   - Prefer `record` types for DTOs and data transfer objects
   - Use `init` instead of `set` where appropriate
   - Make fields `readonly` when possible

4. **SOLID Principles**
   - **Single Responsibility** - Each class does one thing
   - **Open/Closed** - Extend without modifying
   - **Liskov Substitution** - Interfaces are substitutable
   - **Interface Segregation** - Small, focused interfaces
   - **Dependency Inversion** - Depend on abstractions

### Code Organization

1. **File Structure**
   ```
   src/
   ??? DKW.TransactionExtractor/           # Main application
   ?   ??? Classification/                  # Classification subsystem
   ?   ??? Formatting/                      # Output formatting
   ?   ??? Models/                          # Domain models
   ?   ??? Providers/                       # Provider-specific parsers
   ?   ??? Program.cs                       # Entry point
   ??? DKW.TransactionExtractor.Tests/     # Unit tests
   ```

2. **Namespace Alignment**
   - Namespace should match folder structure
   - Example: `DKW.TransactionExtractor.Classification`

3. **One Class Per File**
   - Each class/interface/record in its own file
   - File name matches type name
   - Exception: Related types (small records, enums) can share if tightly coupled

### Documentation

1. **Markdown Files**
   - **Save all documentation in `./docs/`**
   - **DO NOT** put documentation files in the solution root (except README.md, LICENSE.md, CHANGELOG.md)
   - **DO NOT** use extended characters and emojis because they do not display correctly in all environments. Use HTML Entity Codes instead.
   - **IMPORTANT**: HTML Entity Codes do NOT work inside code blocks (fenced code blocks with `` ``` ``). Use text descriptions like `[OK]`, `[FAIL]`, `[WARN]` or actual code comments inside code blocks.
   - Use UTF-8 with BOM for all markdown files
   - Reference: `docs/development/EMOJI_ENTITY_CODE_MAP.md` for all entity code mappings and usage rules
   - Organize by purpose:
     - `./docs/` - User guides and general docs
     - `./docs/architecture/` - Design documents and refactoring notes
     - `./docs/development/` - Bug fixes and technical changes
     - `./docs/features/` - Feature documentation

2. **Code Comments**
   - Use XML comments (`///`) for public APIs
   - Keep comments concise and meaningful
   - Update comments when refactoring
   - Avoid stating the obvious

3. **README Structure**
   - Root `README.md` - Project overview and quick start
   - `docs/README.md` - Documentation index

### Testing

1. **Unit Test Naming**
   ```csharp
   [Fact]
   public void MethodName_Scenario_ExpectedBehavior()
   {
       // Arrange
       // Act
       // Assert
   }
   ```

2. **Test Organization**
   - One test class per production class
   - Group related tests with nested classes or theory data
   - Write unit tests for business logic
   - Mock dependencies using interfaces

3. **Test Coverage**
   - Focus on business logic and edge cases
   - Don't test framework code or trivial getters/setters
   - Test both happy path and error conditions

### Configuration

- Use `appsettings.json` for configuration
- Bind to strongly-typed option classes
- Validate configuration on startup
- Never hard-code strings (use constants or configuration)

## Project-Specific Patterns

### Matcher System

- Create matchers by implementing `ITransactionMatcher`
- Register in `MatcherFactory.CreateMatcher()`
- Add interactive builder in `MatcherBuilderService`
- Update documentation in `docs/CLASSIFICATION_GUIDE.md`

### Smart Merging

- ExactMatch and Contains: Merge values with same case sensitivity
- Regex: Always create new instances (no merging)

### Interactive Console

- Use `IConsoleInteraction` for user prompts
- Keep UI logic separate from business logic
- Provide clear, numbered options
- Use sentinel values for control flow (GoBack, Invalid, Exit)

## Common Tasks

### Adding a New Matcher Type

1. Create matcher class implementing `ITransactionMatcher`
2. Add case to `MatcherFactory.CreateMatcher()`
3. Add builder method to `MatcherBuilderService.BuildMatcher()`
4. Update `CategoryRepository` merging logic if needed
5. Document in `docs/features/` or `docs/CLASSIFICATION_GUIDE.md`

### Adding a New Output Format

1. Create formatter class implementing `ITransactionFormatter`
2. Register in `Program.cs` based on `OutputFormat` configuration
3. Update `appsettings.json` schema
4. Document in `docs/CLASSIFICATION_GUIDE.md`

### Refactoring Guidelines

- **Minimize technical debt** over backward compatibility (greenfield project)
- Create a plan for multi-step refactorings
- Update documentation alongside code changes
- Build and test after each major change
- Use result types for validation, not recursion

## Code Review Checklist

Before submitting a PR, verify:

- [ ] No recursion used for retry/validation logic (use loops + sentinel values)
- [ ] No hard-coded strings (use constants or configuration)
- [ ] Descriptive variable and method names
- [ ] Each method has a single, clear responsibility
- [ ] Result types used for expected failures
- [ ] Unit tests added for new functionality
- [ ] Documentation updated in `./docs/`
- [ ] No compiler warnings
- [ ] Code builds successfully
- [ ] All tests pass

## Git Workflow

- **Branch**: `main` (primary development branch)
- **Commit Messages**: Clear, descriptive commit messages
- **Documentation**: Update docs in the same commit as code changes

## References

- [Main Documentation](../../docs/README.md)
- [Classification Guide](../../docs/CLASSIFICATION_GUIDE.md)
- [Architecture Decisions](../../docs/architecture/)
- [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Clean Code by Robert C. Martin](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)

---

**Last Updated**: 2025-01-26  
**Maintainer**: GitHub Copilot Workspace Configuration
