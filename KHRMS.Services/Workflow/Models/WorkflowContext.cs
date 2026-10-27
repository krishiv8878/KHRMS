using System.Collections.Generic;

namespace KHRMS.Services.Workflow.Models
{
    public class WorkflowContext
    {
        public long RequesterId { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Location { get; set; }
        public long? ProjectId { get; set; }
        public Dictionary<string, object?> Attributes { get; set; } = new(System.StringComparer.OrdinalIgnoreCase);

        public T? GetAttribute<T>(string key)
        {
            if (Attributes.TryGetValue(key, out var val) && val != null)
            {
                if (val is T typedVal) return typedVal;
                try
                {
                    var converted = System.Convert.ChangeType(val, typeof(T));
                    return (T)converted;
                }
                catch
                {
                    return default;
                }
            }
            return default;
        }
    }
}
