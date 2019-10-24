namespace Logging.Models
{
    public class ErrorLogDetail : LogDetail
    {
        public string Exception { get; set; }

        public string Level { get; set; }

        public string ExceptionBaseType { get; set; }
    }
}
