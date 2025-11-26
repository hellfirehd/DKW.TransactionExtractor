# DKW Transaction Extractor Documentation

Welcome to the DKW Transaction Extractor documentation. This directory contains all technical and user-facing documentation for the project.

## ?? Documentation Index

### ?? User Guides
- **[Classification Guide](CLASSIFICATION_GUIDE.md)** - Complete guide to the transaction classification system
  - Configuration and setup
  - Matcher types (ExactMatch, Contains, Regex)
  - Interactive matcher creation
  - Smart merging behavior
  - Output formats

### ?? Features
- **[Extracted Text Feature](features/EXTRACTED_TEXT_FEATURE.md)** - Debug feature for viewing raw PDF text
- **[Logging Quick Reference](features/LOGGING_QUICK_REFERENCE.md)** - Quick guide to logging configuration
- **[Serilog File Logging](features/SERILOG_FILE_LOGGING.md)** - File-based logging setup and configuration
- **[Supplemental Details Filtering](features/SUPPLEMENTAL_DETAILS_FILTERING.md)** - Handling itemized purchase details
- **[Transaction Exclusion Feature](features/TRANSACTION_EXCLUSION_FEATURE.md)** - Excluding fees, interest, and refunds

### ??? Architecture & Design
- **[Category Service Refactoring](architecture/CATEGORY_SERVICE_REFACTORING.md)** - Service layer design and dependency injection

### ?? Development & Bug Fixes
- **[Leap Year Fix](development/LEAP_YEAR_FIX.md)** - Handling February 29th transactions
- **[Line Ending Normalization Fix](development/LINE_ENDING_NORMALIZATION_FIX.md)** - PDF text extraction normalization
- **[ParseContext Refactoring](development/PARSECONTEXT_REFACTORING.md)** - Parser context improvements

## ?? Quick Links

### For Users
- [Getting Started](../README.md) - Project overview and quick start
- [Configuration](CLASSIFICATION_GUIDE.md#configuration) - How to configure the application
- [Output Formats](CLASSIFICATION_GUIDE.md#output-files) - CSV and JSON output formats
- [Troubleshooting](../README.md#troubleshooting) - Common issues and solutions

### For Developers
- [Coding Standards](../.github/copilot/instructions.md) - Naming conventions and best practices
- [Matcher System](CLASSIFICATION_GUIDE.md#matcher-types) - Understanding the matcher architecture
- [Extensibility](CLASSIFICATION_GUIDE.md#extensibility) - Adding new matcher types
- [Testing Guidelines](../.github/copilot/instructions.md#testing) - Unit test standards

## ?? Documentation Standards

All documentation in this workspace follows these standards:

1. **Location**: All markdown documentation goes in `./docs/` (not in root)
2. **Organization**:
   - `./docs/` - User guides and general documentation
   - `./docs/features/` - Feature-specific documentation
   - `./docs/architecture/` - Design documents and architecture decisions
   - `./docs/development/` - Bug fixes, refactoring notes, and technical changes
3. **Formatting**: Use clear headers, code examples, and tables where appropriate
4. **Examples**: Include working examples for all configuration options
5. **Updates**: Keep documentation in sync with code changes

## ??? Complete Directory Structure

```
docs/
??? README.md                                    # This file - documentation index
??? CLASSIFICATION_GUIDE.md                      # User guide for classification system
??? features/                                    # Feature documentation
?   ??? EXTRACTED_TEXT_FEATURE.md
?   ??? LOGGING_QUICK_REFERENCE.md
?   ??? SERILOG_FILE_LOGGING.md
?   ??? SUPPLEMENTAL_DETAILS_FILTERING.md
?   ??? TRANSACTION_EXCLUSION_FEATURE.md
??? architecture/                                # Design documents
?   ??? CATEGORY_SERVICE_REFACTORING.md
??? development/                                 # Bug fixes and technical notes
    ??? LEAP_YEAR_FIX.md
    ??? LINE_ENDING_NORMALIZATION_FIX.md
    ??? PARSECONTEXT_REFACTORING.md
```

## ?? Contributing to Documentation

When adding new features or making changes:

1. **Features**: Document in `features/` with clear examples
2. **Architecture Changes**: Add design doc to `architecture/` for significant refactorings
3. **Bug Fixes**: Document in `development/` with problem description and solution
4. **Update Index**: Update this README.md if adding new top-level documents
5. **Code Examples**: Include working code snippets and configuration samples
6. **Keep Current**: Update docs in the same PR as code changes

### Documentation Template

When creating new documentation:

```markdown
# Feature/Fix Name

## Overview
Brief description of what this feature/fix does.

## Problem (for fixes)
Description of the issue being addressed.

## Solution
How it works and why this approach was chosen.

## Usage/Configuration
Code examples and configuration options.

## Testing
How to verify it works.

## References
Links to related docs or external resources.
```

## ?? Additional Resources

- [GitHub Repository](https://github.com/hellfirehd/DKW.TransactionExtractor)
- [Issue Tracker](https://github.com/hellfirehd/DKW.TransactionExtractor/issues)
- [Coding Standards](../.github/copilot/instructions.md)
- [Project README](../README.md)

---

**Last Updated**: 2025-01-26  
**Maintained by**: Project Contributors
