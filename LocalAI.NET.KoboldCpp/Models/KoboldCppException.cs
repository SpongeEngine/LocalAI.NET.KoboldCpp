namespace LocalAI.NET.KoboldCpp.Models
{
    public class KoboldCppException : Exception
    {
        public string Provider { get; }
        public int? StatusCode { get; }
        public string? ResponseContent { get; }

        public KoboldCppException(
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

        public KoboldCppException(string message) : base(message)
        {
            Provider = "KoboldCpp";
        }

        public KoboldCppException(string message, Exception innerException) 
            : base(message, innerException)
        {
            Provider = "KoboldCpp";
        }
    }
}