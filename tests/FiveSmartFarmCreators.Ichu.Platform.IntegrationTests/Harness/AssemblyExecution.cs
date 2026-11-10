using Xunit;

// The API and its database are shared, so tests run one at a time: concurrent first requests race on the schema and concurrent device writes conflict.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
