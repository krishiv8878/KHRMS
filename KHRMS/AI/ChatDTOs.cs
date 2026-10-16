using System.Text.Json.Serialization;

namespace KHRMS.AI
{
    // ─── Inbound ────────────────────────────────────────────────────────────────

    /// <summary>Single message in the conversation history sent from the client.</summary>
    public class ChatRequest
    {
        public List<ChatMessageDto> Messages { get; set; } = [];
    }

    public class ChatMessageDto
    {
        /// <summary>"user" | "assistant"</summary>
        public string Role    { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    // ─── Outbound ───────────────────────────────────────────────────────────────

    public class ChatResponse
    {
        public string         Reply { get; set; } = string.Empty;
        public ChatDraft?     Draft { get; set; }
        public TokenUsageDto? Usage { get; set; }
    }

    public class TokenUsageDto
    {
        public long   InputTokens        { get; set; }
        public long   OutputTokens       { get; set; }
        public long   TotalTokens        { get; set; }
        public int    SystemPromptTokens { get; set; }
        public int    HistoryTokens      { get; set; }
        public string ToolsExecuted      { get; set; } = string.Empty;
    }

    /// <summary>
    /// Machine-readable action the employee must explicitly confirm.
    /// The Angular widget renders this as a confirm-card; on confirm it calls ConfirmEndpoint.
    /// </summary>
    public class ChatDraft
    {
        /// <summary>"leave" | "regularization"</summary>
        public string ActionType       { get; set; } = string.Empty;

        /// <summary>Human-readable one-liner to show the employee.</summary>
        public string Summary          { get; set; } = string.Empty;

        /// <summary>Raw payload the Angular widget will POST to ConfirmEndpoint.</summary>
        public object Payload          { get; set; } = new();

        /// <summary>Relative REST endpoint to call on confirm (e.g. "LeaveRequest/AddLeaveRequest").</summary>
        public string ConfirmEndpoint  { get; set; } = string.Empty;
    }

    public class PrepareLeaveDto
    {
        public long LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveMode { get; set; } = "Full Day";
        public string? LeaveReason { get; set; }
    }

    public class PrepareRegularizationDto
    {
        public string RequestType { get; set; } = "Regularization";
        public DateTime RequestedDate { get; set; }
        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        public string? Reason { get; set; }
    }
}
