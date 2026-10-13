using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json.Nodes;

namespace KHRMS.AI
{
    /// <summary>
    /// Pipeline policy that preserves and re-injects Google Gemini thought_signatures.
    /// Gemini's OpenAI-compatible endpoint returns a thought_signature in extra_content for tool calls,
    /// and requires it to be present in subsequent turns when returning tool results.
    /// Since OpenAI .NET SDK drops unknown extra_content properties, this policy ensures they are preserved.
    /// </summary>
    public class GeminiThoughtSignaturePolicy : PipelinePolicy
    {
        private static readonly Dictionary<string, JsonNode> Signatures = new();

        public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
        {
            ProcessAsync(message, pipeline, currentIndex).AsTask().GetAwaiter().GetResult();
        }

        public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
        {
            // 1. Before sending: if request has tool_calls, inject extra_content if missing
            if (message.Request.Content != null)
            {
                using var ms = new MemoryStream();
                message.Request.Content.WriteTo(ms, default);
                var reqBytes = ms.ToArray();
                var reqText = Encoding.UTF8.GetString(reqBytes);

                try
                {
                    var doc = JsonNode.Parse(reqText);
                    if (doc?["messages"] is JsonArray msgArray)
                    {
                        bool modified = false;
                        foreach (var m in msgArray)
                        {
                            if (m?["tool_calls"] is JsonArray tcArray)
                            {
                                foreach (var tc in tcArray)
                                {
                                    var id = tc?["id"]?.GetValue<string>();
                                    if (!string.IsNullOrEmpty(id) && tc["extra_content"] == null)
                                    {
                                        lock (Signatures)
                                        {
                                            if (Signatures.TryGetValue(id, out var extraContent))
                                            {
                                                tc["extra_content"] = extraContent.DeepClone();
                                                modified = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (modified)
                        {
                            var newBytes = Encoding.UTF8.GetBytes(doc.ToJsonString());
                            message.Request.Content = BinaryContent.Create(BinaryData.FromBytes(newBytes));
                        }
                    }
                }
                catch
                {
                    // If JSON parsing fails, continue without modification
                }
            }

            // 2. Call next policy in pipeline
            await ProcessNextAsync(message, pipeline, currentIndex);

            // 3. After receiving: save any thought_signatures from tool_calls
            if (message.Response?.Content != null)
            {
                try
                {
                    var respText = message.Response.Content.ToString();
                    var doc = JsonNode.Parse(respText);
                    if (doc?["choices"] is JsonArray choices)
                    {
                        foreach (var choice in choices)
                        {
                            if (choice?["message"]?["tool_calls"] is JsonArray tcArray)
                            {
                                foreach (var tc in tcArray)
                                {
                                    var id = tc?["id"]?.GetValue<string>();
                                    var extra = tc?["extra_content"];
                                    if (!string.IsNullOrEmpty(id) && extra != null)
                                    {
                                        lock (Signatures)
                                        {
                                            Signatures[id] = extra.DeepClone();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // If parsing response fails, continue without throwing
                }
            }
        }
    }
}
