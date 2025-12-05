// [assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

// The Clipboard functions from IVSoftware.WinOS.MSTest.Extensions
// don't seem too happy about parallelism. The tests themselves
// are threadsafe and so the clipboard calls (which are for test
// development, not actual testing) will eventually get culled out.
//
// Also remember that whan subscribing to Awaited from the 
// IVSoftware.Portable.Threading bundle it's best to check the
// thread ID and respond only to the events that occur on the
// same thread as the surrent test.
//
// See also: {7E8A9B6F-5AED-48CB-9EB8-EF72D22B9970}
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel, Workers = 1)]
