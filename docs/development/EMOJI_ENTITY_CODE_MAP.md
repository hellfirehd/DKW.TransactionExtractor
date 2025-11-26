# Emoji to HTML Entity Code Map

This document provides the standard mappings for all emojis and extended characters used in documentation.

---

## &#9888; IMPORTANT: Code Blocks Exception

**HTML Entity Codes DO NOT render inside code blocks** (fenced code blocks delimited by `` ``` ``).

### Inside Code Blocks:
```
&#10004; will display as literal text: &#10004;
Not as a check mark
```

### Outside Code Blocks:
&#10004; will render as: ✅ (the actual character)

**Rule**: Use raw characters or text descriptions ONLY inside code blocks.

---

## Standard Mappings

| Display   | Actual  | HTML Entity | Unicode | Description |
|-----------|---|-------------|---------|-------------|
| &#9733;   | ⭐ | `&#9733;`   | U+2605  | Gold Star |
| &#10004;  | ✔ | `&#10004;`  | U+2714  | Heavy Check Mark |
| &#10060;  | ❌ | `&#10060;`  | U+274C  | Cross Mark |
| &#9888;   | ⚠ | `&#9888;`   | U+26A0  | Warning Sign |
| &#9989;   | ✅ | `&#9989;`   | U+2705  | Check Mark Button |
| &#10149;  | ➥ | `&#10149;`  | U+27A5  | Heavy Arrow Pointing Right |
| &#128165; | 💥 | `&#128165;` | U+1F4A5 | Bomb/Explosion |
| &#128640; | 🚀 | `&#128640;` | U+1F680 | Rocket |
| &#8987;   | ⌛ | `&#8987;`   | U+231B  | Hourglass |
| &#128308; | 🔴 | `&#128308;` | U+ | Red Circle |
| &#128994; | 🟢 | `&#128994;` | U+1F7E2 | Green Circle (low priority) |
| &#128309; | 🔵 | `&#128309;` | U+ | Blue Circle |
| &#128221; | 📝 | `&#128221;` | U+1F4DD | Memo |
| &#128241; | 📱 | `&#128241;` | U+1F4F1 | Mobile Phone |
| &#128187; | 💻 | `&#128187;` | U+1F4BB | Laptop |
| &#128230; | 📦 | `&#128230;` | U+1F4F6 | Satellite Antenna |
| &#128424; | 🖨 | `&#128424;` | U+1F528 | Printer |
| &#128274; | 🔒 | `&#128274;` | U+1F512 | Lock |
| &#128275; | 🔓 | `&#128275;` | U+1F513 | Unlock |
| &#128341; | 🕕 | `&#128341;` | U+1F595 | Colck |
| &#128161; | 💡 | `&#128161;` | U+1F4A1 | Light Bulb |
| &#128203; | 📋 | `&#128203;` | U+1F4CB | Clipboard |
| &#128204; | 📌 | `&#128204;` | U+1F4CC | Pushpin |
| &#128209; | 📑 | `&#128209;` | U+1F4D1 | Bookmark Tabs |
| &#128278; | 🔖 | `&#128278;` | U+1F4D1 | Bookmark |
| &#128214; | 📖 | `&#128214;` | U+1F4D6 | Open Book |
| &#128269; | 🔍 | `&#128269;` | U+1F50D | Magnifying Glass |
| &#128248; | 📸 | `&#128248;` | U+1F4F8 | Camera |
| &#128302; | 🔮 | `&#128302;` | U+1F56E | Snow Globe |
| &#128375; | 🕷 | `&#128375;` | U+1F5A7 | Spider |


---

## Priority Indicators

| Indicator | HTML Entity | Display | Usage |
|-----------|-------------|---------|-------|
| Gold Star | `&#9733;` | &#9733; | Featured/Important |
| High Priority | `&#129992;` | &#129992; | Critical/Urgent (Red Circle) |
| Medium Priority | `&#129995;` | &#129995; | Important (Yellow Circle) |
| Low Priority | `&#128994;` | &#128994; | Optional/Nice-to-have (Green Circle) |

---

## Status Indicators

| Status | HTML Entity | Display | Meaning |
|--------|-------------|---------|---------|
| Complete | `&#10004;` | &#10004; | Done/Approved |
| Incomplete | `&#10060;` | &#10060; | Not done/Failed |
| Warning | `&#9888;` | &#9888; | Caution/Review needed |
| Info | `&#9733;` | &#9733; | Important/Featured |

---

## Color Circle Reference

**For accurate color circles, use these codes:**

| Color | Code | Display | Unicode |
|-------|------|---------|---------|
| Red | `&#129992;` | &#129992; | U+1F7E0 (Red Circle) |
| Yellow | `&#129995;` | &#129995; | U+1F7E3 (Yellow Circle) |
| Green | `&#128994;` | &#128994; | U+1F7E2 (Green Circle) |
| Orange | `&#129993;` | &#129993; | U+1F7E1 (Orange Circle) |

**Note**: The original codes (&#128997;, &#128995;, &#128994;) may render differently depending on platform and font.

---

## Usage Guidelines

1. **Always use HTML Entity Codes** in markdown files (outside code blocks)
2. **Never use raw emojis** (they don't render consistently)
3. **Inside code blocks**: Use text descriptions or raw characters if needed
4. **Document new codes** when adding to this file
5. **Use UTF-8 with BOM** encoding for markdown files

---

## Code Block Special Case

### &#9888; Important Rule
HTML entity codes do NOT render inside fenced code blocks (`` ``` ``).

### Example - WRONG (Don't do this):
```markdown
Status: &#10004; Task completed
Problem: &#10060; Build failed
```
Result: Shows literal `&#10004;` text, not the character.

### Example - RIGHT (Do this instead):

**In regular markdown:**
```markdown
Status: &#10004; Task completed
Problem: &#10060; Build failed
```

**In code blocks (use text descriptions):**
```markdown
// [CHECK] Task completed
// [FAIL] Build failed
// [WARN] Needs review
```

Or use actual code comments as they would appear:

```csharp
// Status: OK - Task completed
// Status: FAIL - Build failed
// Status: WARN - Needs review
```

Or simply avoid using fenced code blocks if they will not contain actual code. For example:

Status: &#10004; Task completed

Problem: &#10060; Build failed

---

## Example Markdown Usage

### For Regular Documentation:

BEGIN EXAMPLE:

## Status Report

- &#10004; Task 1 - Completed
- &#10060; Task 2 - Failed
- &#9888; Task 3 - Needs review

## Priority Breakdown

High Priority: &#128997;
- Issue #1
- Issue #2

Medium Priority: &#128995;
- Issue #3
- Issue #4

Low Priority: &#128994;
- Issue #5

END EXAMPLE

### For Code Examples (use text instead):

```csharp
public class Result
{
    // [OK] Successful operation
    public Boolean IsSuccess { get; set; }
    
    // [FAIL] Operation failed
    public String? ErrorMessage { get; set; }
}
```

---

**Last Updated**: 2025-01-26  
**Applies to**: All markdown files in `docs/` directory  
**Critical Note**: Entity codes don't work in code blocks - use text descriptions instead
