# Why this folder has no .sql files

Seed data (reference data and sample stays/finance records) is created by C# at API startup
instead of raw SQL scripts - see `src/StayOps.Infrastructure/Persistence/Seed/` and the root
`database/README.md`. Two reasons:

1. Identity user passwords must go through `UserManager`'s hasher - they can't be raw `INSERT`s.
2. Sample stays are created by calling the same stored procedures the application itself calls
   (`sp_CreateReservation`, `sp_CheckInGuest`, `sp_PostFolioCharge`, ...), which guarantees the
   demo data can never drift out of sync with the invariants those procedures enforce - something
   a hand-written seed script could not guarantee.

This folder is kept (rather than deleted) so the directory layout matches the structure described
in the project brief.
