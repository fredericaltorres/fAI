# SKILL: Data Analysis And Insights

## Overview
This skill enables structured, rigorous analysis of datasets and business data.
When this skill is active, follow all guidelines below to produce analyses that are
accurate, reproducible, clearly communicated, and decision-ready.

---

## When to Use This Skill
Trigger this skill when the user requests any of the following:
- Analyzing a CSV, Excel, JSON, or database export
- Summarizing trends, patterns, or anomalies in data
- Producing statistical summaries (mean, median, variance, etc.)
- Building or interpreting charts and visualizations
- Drawing business insights or recommendations from data
- Comparing datasets across time periods, segments, or categories

---

## Analysis Workflow
Always follow these steps in order. Do not skip steps.

### Step 1 — Understand the Data
Before any calculation:
- Identify the data source and format (CSV, JSON, SQL result, etc.)
- Determine the number of rows and columns
- List column names, inferred data types, and a sample of values
- Flag any columns with missing, null, or unexpected values

Output a **Data Summary Table**:

| Column Name | Data Type | Non-Null Count | Sample Values       | Notes             |
|-------------|-----------|----------------|---------------------|-------------------|
| date        | DateTime  | 1,200 / 1,200  | 2024-01-01          | No nulls          |
| revenue     | Float     | 1,195 / 1,200  | 42300.50, 38900.00  | 5 nulls — flag    |
| region      | String    | 1,200 / 1,200  | "North", "South"    | 4 unique values   |

---

### Step 2 — Define the Question
Restate the analysis goal in one sentence before proceeding.
If the user's request is ambiguous, ask a clarifying question.

Example:
> **Analysis Goal:** Determine which product category drove the highest revenue growth in Q3 2024 compared to Q2 2024.

---

### Step 3 — Clean the Data
Document every transformation applied:

| Transformation         | Column(s) Affected | Reason                          |
|------------------------|--------------------|---------------------------------|
| Removed null rows      | `revenue`          | 5 rows had no revenue value     |
| Parsed string to date  | `date`             | Stored as "MM/DD/YYYY" string   |
| Normalized case        | `region`           | Mixed "north" / "North" values  |
| Removed duplicates     | All                | 3 exact duplicate rows removed  |

Never modify source data in place. Always describe changes explicitly.

---

### Step 4 — Perform the Analysis
Apply the appropriate method(s) based on the question type:

#### Descriptive Analysis (What happened?)
- Compute: count, sum, mean, median, mode, min, max, standard deviation
- Group by relevant dimensions (time, region, category, etc.)
- Show period-over-period comparisons where applicable

#### Diagnostic Analysis (Why did it happen?)
- Identify correlations between variables (note: correlation ≠ causation)
- Highlight outliers and explain potential causes
- Segment data to isolate contributing factors

#### Trend Analysis (What is changing over time?)
- Use time-series grouping (daily, weekly, monthly, quarterly)
- Calculate growth rates: `(Current - Prior) / Prior × 100`
- Identify seasonality, inflection points, or anomalies

#### Predictive Indicators (What might happen next?)
- Only make predictions when explicitly requested
- Clearly state all assumptions
- Provide confidence ranges, not single-point estimates

---

### Step 5 — Visualize the Findings
For each key finding, recommend the most appropriate chart type:

| Scenario                              | Recommended Chart    |
|---------------------------------------|----------------------|
| Trend over time                       | Line chart           |
| Comparing categories                  | Bar or column chart  |
| Part-to-whole relationships           | Pie or donut chart   |
| Distribution of values                | Histogram            |
| Correlation between two variables     | Scatter plot         |
| Performance vs. target                | Bullet or gauge      |
| Multiple metrics across categories    | Grouped bar chart    |

When producing chart descriptions:
- Label all axes clearly with units
- Include a title: `Figure X: <Description>`
- Always note the data range and any exclusions

---

### Step 6 — Communicate Insights
Structure findings using the **Pyramid Principle** — lead with the conclusion:

```
[HEADLINE FINDING]
One sentence stating the most important result.

[SUPPORTING EVIDENCE]
2–4 bullet points with specific numbers backing the headline.

[SECONDARY FINDINGS]
Additional patterns or anomalies worth noting.

[RECOMMENDATION]
One concrete, actionable next step (if applicable).
```

#### Example Output Format:
```
[HEADLINE FINDING]
North region revenue grew 34% in Q3 2024, outperforming all other regions.

[SUPPORTING EVIDENCE]
- North region: $2.4M in Q3 vs. $1.8M in Q2 (+34%)
- South region: $1.9M in Q3 vs. $2.1M in Q2 (−10%)
- Electronics category drove 61% of North region growth
- Average order value increased from $320 to $410 in the North

[SECONDARY FINDINGS]
- 5 null revenue rows were excluded from the analysis (0.4% of data)
- "Accessories" category saw a 5% dip across all regions

[RECOMMENDATION]
Investigate the North region's sales strategy in Electronics for potential
replication in underperforming regions.
```

---

## Number Formatting Rules
- Currencies: `$1,234,567.89` (always include currency symbol)
- Percentages: `34.2%` (one decimal place unless precision is required)
- Large numbers: use abbreviations at scale — `1.2M`, `450K`, `3.4B`
- Decimals: maximum 2 decimal places in summaries; up to 4 in statistical outputs
- Never present a percentage without also showing the underlying raw numbers

---

## Statistical Integrity Rules
- Always state the sample size (`n = X`)
- Report confidence intervals for any estimate where appropriate
- Never infer causation from correlation alone — flag this explicitly
- Round intermediate calculations only at the final output step
- When data is a sample (not a full population), note that findings are estimates

---

## Limitations & Caveats
Always include a **Limitations** note at the end of any analysis:

```
[LIMITATIONS]
- Analysis covers <date range>; results may not reflect more recent trends.
- <X> rows were excluded due to missing values (<Y>% of total dataset).
- External factors (market conditions, seasonality) are not accounted for.
- Results are based on self-reported data and have not been independently verified.
```

---

## Quality Checklist
Before delivering the analysis, verify:
- [ ] Data Summary Table is present
- [ ] Analysis Goal is clearly stated
- [ ] All data cleaning steps are documented
- [ ] Numbers are formatted consistently throughout
- [ ] Every chart/figure has a title and labeled axes
- [ ] Headline finding leads the output
- [ ] Sample size (`n`) is stated
- [ ] Limitations section is included
- [ ] No causal language used for correlational findings
- [ ] Recommendations are actionable and specific
