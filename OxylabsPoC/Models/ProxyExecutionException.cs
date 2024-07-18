namespace OxylabsPoC.Exceptions
{
    public class ProxyExecutionException : Exception
    {
        public ProxyExecutionException(string message, Exception innerException) : base(message, innerException) { }
    }
}