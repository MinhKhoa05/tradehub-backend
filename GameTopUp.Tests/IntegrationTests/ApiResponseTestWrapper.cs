namespace GameTopUp.Tests.IntegrationTests
{
    /// <summary>
    /// Shared wrapper for deserializing ApiResponse in integration tests.
    /// Follows DRY principle to avoid duplication across multiple test files.
    /// </summary>
    /// <typeparam name="T">The type of the Data property.</typeparam>
    public class ApiResponseTestWrapper<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
