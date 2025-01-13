namespace SpongeEngine.KoboldSharp.Models
{
    public class KoboldSharpException : Exception
    {
        public string Provider { get; }
        public int? StatusCode { get; }
        public string? ResponseContent { get; }

        public KoboldSharpException(
            string message,
            string provider,
            int? statusCode = null,
            string? responseContent = null) 
            : base(message)
        {
            Provider = provider;
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }

        public KoboldSharpException(string message) : base(message)
        {
            Provider = "KoboldCpp";
        } 

        public KoboldSharpException(string message, Exception innerException) 
            : base(message, innerException)
        {
            Provider = "KoboldCpp";
        }
    }
}