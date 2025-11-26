# Versioning Workflow

This project uses [GitVersion](https://gitversion.net/) for automatic version management based on Git tags and commits.

## Version Format

**Calendar-Semantic Versioning**: `YYYY.MINOR.PATCH`

- **YYYY**: Year of release (e.g., 2025)
- **MINOR**: Incremented for new features or significant changes (0, 1, 2, ...)
- **PATCH**: Incremented for bug fixes and minor improvements (0, 1, 2, ...)

### Examples
- `2025.1.0` - First minor release in 2025
- `2025.1.1` - Patch release
- `2025.2.0` - Second minor release in 2025
- `2025.1.1-alpha.5` - Fifth commit after 2025.1.0 tag (pre-release)

## How It Works

GitVersion automatically calculates the version based on:
1. **Git tags**: Define release versions
2. **Commit count**: Auto-increments pre-release versions
3. **Branch name**: Determines pre-release tag (e.g., `alpha`, `beta`)

### Version Lifecycle

```
main branch commits
?
?? Tag: 2025.1.0          ? Version: 2025.1.0
?
?? Commit 1 after tag     ? Version: 2025.1.1-alpha.1
?? Commit 2 after tag     ? Version: 2025.1.1-alpha.2
?? Commit 3 after tag     ? Version: 2025.1.1-alpha.3
?
?? Tag: 2025.1.1          ? Version: 2025.1.1
?
?? Commit 1 after tag     ? Version: 2025.1.2-alpha.1
```

## Creating a Release

### 1. Commit Your Changes
```bash
git add .
git commit -m "Add new feature"
git push
```

### 2. Tag the Release
When you're ready to release, tag the commit:

```bash
# For a minor version bump (new features)
git tag 2025.1.0
git push --tags

# For a patch version bump (bug fixes)
git tag 2025.1.1
git push --tags
```

### 3. Update CHANGELOG.md
After tagging, update the CHANGELOG.md to move items from `[Unreleased]` to the new version section.

## Version Increment Guidelines

### When to bump MINOR version (X.Y.0):
- ? New features
- ? Significant functionality changes
- ? Breaking changes
- Example: `2025.1.0` ? `2025.2.0`

### When to bump PATCH version (X.Y.Z):
- ? Bug fixes
- ? Performance improvements
- ? Documentation updates
- ? Minor enhancements
- Example: `2025.1.0` ? `2025.1.1`

### When to bump YEAR:
- ? New calendar year begins
- Example: `2025.12.3` ? `2026.1.0`

## Commit Message Conventions (Optional)

You can use commit message tags to automatically control version increments:

```bash
# Patch increment (default)
git commit -m "Fix parsing bug +semver:patch"

# Minor increment
git commit -m "Add new feature +semver:minor"

# Major/Breaking increment
git commit -m "Breaking change to API +semver:major"
```

**Note**: These are optional. Manual tagging is the primary release mechanism.

## Checking Current Version

### During Build
The version is automatically embedded in the assembly during build.

### At Runtime
The version is logged when the application starts:
```
[2025-11-27 14:30:00.123] [INF] Starting Transaction Extractor v2025.1.0+abc1234
```

### Using Git
```bash
# Show the last tag
git describe --tags --abbrev=0

# Show the current GitVersion-calculated version
dotnet gitversion
```

## Pre-release Versions

Between tags, GitVersion automatically generates pre-release versions:
- Format: `{NextVersion}-alpha.{CommitsSinceLastTag}`
- Example: `2025.1.1-alpha.5` (5 commits after 2025.1.0 tag)

These versions help track development builds between releases.

## Configuration

GitVersion is configured in `GitVersion.yml` at the repository root:
- **Mode**: Mainline (continuous delivery)
- **Tag prefix**: None (empty)
- **Increment**: Patch (default)
- **Branch**: main (release branch)

## Workflow Summary

1. **Develop**: Make commits on `main` branch
2. **Test**: Run tests and verify functionality
3. **Release**: Tag the commit with desired version
4. **Document**: Update CHANGELOG.md with the release notes
5. **Repeat**: Continue development; versions auto-increment between tags

## Troubleshooting

### Version shows as "0.0.0" or "0.1.0"
- **Cause**: No Git tags exist yet
- **Solution**: Create the first tag: `git tag 2025.1.0 && git push --tags`

### Version not updating after tag
- **Cause**: Local repository may be out of sync
- **Solution**: Pull tags: `git fetch --tags`

### Build fails with GitVersion error
- **Cause**: GitVersion.yml syntax error or missing Git repository
- **Solution**: Verify GitVersion.yml syntax and ensure `.git` directory exists

## Resources

- [GitVersion Documentation](https://gitversion.net/docs/)
- [Mainline Mode](https://gitversion.net/docs/reference/modes/mainline)
- [Calendar Versioning](https://calver.org/)
- [Keep a Changelog](https://keepachangelog.com/)
