using Xunit;

// Integration/concurrency tests share the same real LocalDB database and SQL Server app-locks
// (sp_getapplock keyed by HotelId+RoomTypeId). Running test classes in parallel caused genuine
// SQL Server deadlocks between unrelated tests contending for the same lock resource, so test
// classes run sequentially here - the concurrency test itself still exercises real parallel
// requests internally via Task.WhenAll, which is the actual behavior under test.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
