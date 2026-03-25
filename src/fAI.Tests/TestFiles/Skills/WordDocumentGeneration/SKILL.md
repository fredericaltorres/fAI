---
name: word-document-generation
description: Generate, create, edit, or manipulate Word documents (.docx files). Use this skill when the user wants to produce a Word document, report, letter, memo, invoice, template, or any formatted document output. Trigger when the user mentions .docx, Word, document generation, or asks to export content into a downloadable file with headings, tables, or structured formatting.
---

# Word Document Generation

A skill for creating and editing Word documents (.docx) programmatically.

## Overview

Use this skill to generate well-structured Word documents from scratch or from existing content. Output should always be a valid `.docx` file the user can open in Microsoft Word or compatible software.

## Supported Features

- Headings (H1–H6)
- Paragraphs and body text
- Bold, italic, and underline formatting
- Bullet and numbered lists
- Tables with headers and rows
- Page headers and footers
- Page numbers
- Title pages

## Workflow

1. Clarify the document type (report, letter, invoice, template, etc.)
2. Gather content — either from the user's input or generate it based on their topic
3. Identify any formatting requirements (fonts, margins, branding)
4. Generate the `.docx` file using the appropriate library
5. Present the file to the user as a download

## Libraries

| Language | Library         | Notes                        |
|----------|-----------------|------------------------------|
| Python   | `python-docx`   | Preferred. Pip installable.  |
| C#       | `DocumentFormat.OpenXml` | For .NET environments |
| Node.js  | `docx`          | npm installable              |

## Python Example
```python
from docx import Document

doc = Document()
doc.add_heading('My Report', level=1)
doc.add_paragraph('This is the introduction paragraph.')

table = doc.add_table(rows=1, cols=2)
table.rows[0].cells[0].text = 'Column A'
table.rows[0].cells[1].text = 'Column B'

doc.save('output.docx')
```

## Notes

- Always save the output file to the working directory before presenting it to the user
- If the user provides a template `.docx`, load it with `Document('template.docx')` instead of `Document()` to preserve styles
- For large documents, build section by section and confirm structure with the user before generating