---
name: data-analysis-and-insights
description: Perform data analysis, statistical summaries, trend detection, and insight generation from structured data. Use this skill when the user wants to analyze a dataset, find patterns, generate charts, compute statistics, clean data, or summarize findings from CSV, Excel, JSON, or database sources. Trigger when the user mentions data analysis, insights, trends, statistics, visualizations, dashboards, or asks questions like "what does this data show" or "analyze my data".
---

# Data Analysis And Insights

A skill for analyzing structured data and generating clear, actionable insights.

## Overview

Use this skill to load, clean, explore, and summarize datasets. Output can range from
plain text summaries and statistical tables to charts and exportable reports. Always
aim to surface meaningful insights, not just raw numbers.

## Supported Input Formats

| Format     | Notes                                      |
|------------|--------------------------------------------|
| CSV        | Most common. Use pandas `read_csv()`       |
| Excel      | `.xlsx` / `.xls`. Use `read_excel()`       |
| JSON       | Flat or nested. Use `read_json()`          |
| SQL        | Query via `sqlalchemy` or `sqlite3`        |
| Clipboard  | User pastes raw data directly into chat    |

## Workflow

1. **Load** — Read the data from the provided source
2. **Inspect** — Check shape, column names, data types, and null values
3. **Clean** — Handle missing values, fix types, remove duplicates
4. **Explore** — Compute descriptive statistics and identify distributions
5. **Analyze** — Detect trends, correlations, outliers, and patterns
6. **Visualize** — Generate charts where helpful (bar, line, scatter, heatmap)
7. **Summarize** — Write a plain-English summary of key findings
8. **Export** — Optionally save results to CSV, Excel, or a report

## Key Analysis Types

- **Descriptive** — Mean, median, mode, standard deviation, min/max
- **Trend Analysis** — Changes over time, moving averages, growth rates
- **Correlation** — Relationships between numeric columns
- **Distribution** — Histograms, skewness, outlier detection (IQR / Z-score)
- **Segmentation** — Group-by summaries, category breakdowns
- **Ranking** — Top N / bottom N by a given metric

## Python Example
```python