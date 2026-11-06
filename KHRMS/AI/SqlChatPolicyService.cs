using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using Serilog;

namespace KHRMS.AI
{
    /// <summary>
    /// INT-04: Database-backed policy retrieval service for the Gemini AI Chatbot.
    /// Queries the CompanyPolicies table via IUnitOfWork and searches active policies.
    /// Falls back to default organizational policies if no custom policies are published in the database.
    /// </summary>
    public sealed class SqlChatPolicyService : IChatPolicyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly InMemoryChatPolicyService FallbackService = new();

        public SqlChatPolicyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IEnumerable<PolicyEntry>> SearchAsync(string query, int maxResults = 3)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<PolicyEntry>();

            try
            {
                var dbPolicies = (await _unitOfWork.CompanyPolicies.GetAll())
                    .Where(p => !p.IsDeleted && p.IsActive)
                    .ToList();

                if (dbPolicies.Count > 0)
                {
                    var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                     .Where(t => t.Length > 1)
                                     .ToArray();

                    if (terms.Length == 0) terms = new[] { query.Trim() };

                    var scoredList = dbPolicies
                        .Select(p =>
                        {
                            var cleanContent = StripHtml(p.ContentHtml);
                            int score = 0;
                            foreach (var term in terms)
                            {
                                if (p.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) score += 3;
                                if (p.Category.Contains(term, StringComparison.OrdinalIgnoreCase)) score += 2;
                                if (cleanContent.Contains(term, StringComparison.OrdinalIgnoreCase)) score += 1;
                            }
                            return new
                            {
                                Entry = new PolicyEntry(p.Title, p.Category, cleanContent),
                                Score = score
                            };
                        })
                        .Where(x => x.Score > 0)
                        .OrderByDescending(x => x.Score)
                        .Take(maxResults)
                        .Select(x => x.Entry)
                        .ToList();

                    if (scoredList.Count > 0)
                    {
                        Log.Information("SqlChatPolicyService - Found {Count} matching SQL policies for query '{Query}'", scoredList.Count, query);
                        return scoredList;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "SqlChatPolicyService - Error querying database policies for query '{Query}'. Falling back to default policies.", query);
            }

            // Fallback to baseline policies if database has no active policies or no matches
            return await FallbackService.SearchAsync(query, maxResults);
        }

        private static string StripHtml(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            // Decode common HTML entities and strip tags
            var text = Regex.Replace(html, "<[^>]*>", " ");
            text = text.Replace("&nbsp;", " ")
                       .Replace("&amp;", "&")
                       .Replace("&lt;", "<")
                       .Replace("&gt;", ">")
                       .Replace("&quot;", "\"");
            return Regex.Replace(text, @"\s+", " ").Trim();
        }
    }
}
