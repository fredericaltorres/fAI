# SKILL: Word Document Generation (docx)

## Overview
This skill enables you to create professional, well-structured Microsoft Word documents (.docx).
When this skill is active, you must follow all guidelines below to produce high-quality output
that is consistent, readable, and production-ready.

---

## When to Use This Skill
Trigger this skill when the user requests any of the following:
- Creating or generating a Word document
- Writing a report, memo, letter, proposal, or contract
- Producing formatted business or technical documentation
- Converting structured content into a `.docx` file

---

## Document Structure Rules

### Required Elements
Every document must include the following sections, in order:

1. **Title** — Clear, concise, in Title Case
2. **Metadata block** — Author, Date, Version (table format preferred)
3. **Table of Contents** — Auto-generated from headings (Heading 1 / Heading 2)
4. **Body Sections** — Logical, headed sections using `Heading 1` for major topics
5. **Conclusion or Summary** — Wrap up key points
6. **Appendix** (optional) — Supporting data, references, or raw tables

### Heading Hierarchy
```
Heading 1  →  Major sections       (e.g., "Introduction", "Findings")
Heading 2  →  Sub-sections         (e.g., "Background", "Key Metrics")
Heading 3  →  Granular sub-topics  (use sparingly)
```

### Paragraph Rules
- Maximum 5 sentences per paragraph
- Use **bold** for key terms on first use
- Use *italics* for titles of external works, or light emphasis
- Avoid underlining except for hyperlinks
- Line spacing: 1.15 (body), 1.0 (tables and code blocks)

---

## Formatting Standards

### Fonts
| Element        | Font            | Size | Weight  |
|----------------|-----------------|------|---------|
| Title          | Calibri         | 24pt | Bold    |
| Heading 1      | Calibri Light   | 16pt | Bold    |
| Heading 2      | Calibri Light   | 13pt | Bold    |
| Body text      | Calibri         | 11pt | Regular |
| Captions       | Calibri         | 9pt  | Italic  |
| Code/Mono      | Courier New     | 10pt | Regular |

### Margins
- Top: 1 inch
- Bottom: 1 inch
- Left: 1.25 inches
- Right: 1.25 inches

### Page Numbering
- Format: `Page X of Y`
- Position: Bottom center footer
- Start numbering from page 2 (title page is unnumbered)

---

## Tables
- Always include a header row with bold text and light gray shading (`#F2F2F2`)
- Alternate row shading for readability (`#FFFFFF` / `#F9F9F9`)
- Add a descriptive caption **above** each table: `Table 1: <Description>`
- Avoid merging cells unless strictly necessary

---

## Lists
- Use **bulleted lists** for unordered items (3 or more)
- Use **numbered lists** for sequential steps or ranked items
- Limit nesting to 2 levels
- Do not end list items with a period unless they are full sentences

---

## Images and Figures
- Every image must have a caption **below** it: `Figure 1: <Description>`
- Images should be centered and no wider than the text area
- Preferred formats: PNG, JPEG
- Always provide alt text for accessibility

---

## Tone and Language
- Default tone: **formal and professional**
- Use active voice wherever possible
- Avoid jargon unless the document is explicitly technical
- Define all acronyms on first use: `Application Programming Interface (API)`
- Gender-neutral language throughout

---

## Output Instructions
When generating document content, structure your response as follows:

```
[TITLE]
<Document title>

[METADATA]
Author: <name or "To be completed">
Date: <today's date>
Version: 1.0

[SECTION: <Heading 1 Name>]
<Content>

[SECTION: <Next Heading 1 Name>]
<Content>

[SUMMARY]
<Summary content>
```

Always confirm the document structure before writing content.
Ask for clarification if the document type, audience, or purpose is ambiguous.

---

## Quality Checklist
Before finalizing, verify:
- [ ] Title and metadata are present
- [ ] Table of contents reflects all Heading 1 / Heading 2 entries
- [ ] No section exceeds 800 words without a sub-heading break
- [ ] All tables have captions and header rows
- [ ] All figures have captions and alt text
- [ ] Spelling and grammar pass (formal register)
- [ ] Page numbers are configured
- [ ] Document saved as `.docx` (not `.doc`)
