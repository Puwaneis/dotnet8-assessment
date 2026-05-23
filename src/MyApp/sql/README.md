# SQL Scripts

This folder stores manual database change scripts for changes not covered by EF Core migrations (e.g. data fixes, index changes, stored procedures).

## Naming Convention

`YYYY-MM-DD-description.sql`

Example: `2026-05-01-add-product-category-index.sql`

## When to use

- Data migration scripts
- Index additions/removals
- Stored procedures or views
- Any change that cannot be expressed as an EF migration

## Applying scripts

Run scripts manually against the target database:

```bash
sqlcmd -S . -d MyApp -E -i 2026-05-01-add-product-category-index.sql
```
