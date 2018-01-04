namespace quickstartcore.Models
{
    using System.Collections.Concurrent;

    public class UsageResult
    {
        public ConcurrentDictionary<string, double> Result { get; set; }
    }
}
