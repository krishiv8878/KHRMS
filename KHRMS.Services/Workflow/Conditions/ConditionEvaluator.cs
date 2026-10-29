using System;
using System.Text.Json;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;
using Serilog;

namespace KHRMS.Services.Workflow.Conditions
{
    public class ConditionEvaluator : IConditionEvaluator
    {
        public bool Evaluate(string? conditionJson, WorkflowContext context)
        {
            if (string.IsNullOrWhiteSpace(conditionJson))
            {
                // No condition specified means step applies unconditionally
                return true;
            }

            try
            {
                using var doc = JsonDocument.Parse(conditionJson);
                var root = doc.RootElement;

                // Check for compound conditions
                if (root.TryGetProperty("conditions", out var conditionsElem) && conditionsElem.ValueKind == JsonValueKind.Array)
                {
                    string logic = root.TryGetProperty("logic", out var logicProp) ? logicProp.GetString()?.ToLowerInvariant() ?? "and" : "and";
                    bool isAnd = logic != "or";

                    foreach (var cond in conditionsElem.EnumerateArray())
                    {
                        bool singleResult = EvaluateSingleCondition(cond, context);
                        if (isAnd && !singleResult) return false;
                        if (!isAnd && singleResult) return true;
                    }
                    return isAnd;
                }

                return EvaluateSingleCondition(root, context);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to evaluate workflow condition JSON: {ConditionJson}. Defaulting to true.", conditionJson);
                return true;
            }
        }

        private bool EvaluateSingleCondition(JsonElement element, WorkflowContext context)
        {
            if (!element.TryGetProperty("field", out var fieldProp))
            {
                return true;
            }

            string field = fieldProp.GetString() ?? string.Empty;
            string op = element.TryGetProperty("operator", out var opProp) ? opProp.GetString() ?? "Equal" : "Equal";
            
            // Try to extract value
            string? targetStr = null;
            decimal? targetNum = null;
            bool? targetBool = null;

            if (element.TryGetProperty("value", out var valProp))
            {
                switch (valProp.ValueKind)
                {
                    case JsonValueKind.Number:
                        if (valProp.TryGetDecimal(out var d)) targetNum = d;
                        targetStr = valProp.GetRawText();
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        targetBool = valProp.GetBoolean();
                        targetStr = valProp.GetRawText();
                        break;
                    case JsonValueKind.String:
                        targetStr = valProp.GetString();
                        if (decimal.TryParse(targetStr, out var parsedD)) targetNum = parsedD;
                        if (bool.TryParse(targetStr, out var parsedB)) targetBool = parsedB;
                        break;
                }
            }

            // Get actual value from context attributes or standard fields
            object? actualObj = null;
            if (context.Attributes.TryGetValue(field, out var attrVal))
            {
                actualObj = attrVal;
            }
            else if (string.Equals(field, "Department", StringComparison.OrdinalIgnoreCase))
            {
                actualObj = context.Department;
            }
            else if (string.Equals(field, "Location", StringComparison.OrdinalIgnoreCase))
            {
                actualObj = context.Location;
            }
            else if (string.Equals(field, "ProjectId", StringComparison.OrdinalIgnoreCase))
            {
                actualObj = context.ProjectId;
            }
            else if (string.Equals(field, "RequestType", StringComparison.OrdinalIgnoreCase))
            {
                actualObj = context.RequestType;
            }

            if (actualObj == null)
            {
                // Field missing in context -> condition cannot be met unless checking null/equality to empty
                return op.Equals("NotEqual", StringComparison.OrdinalIgnoreCase) || op == "!=";
            }

            decimal? actualNum = null;
            if (actualObj is JsonElement actualElem)
            {
                if (actualElem.ValueKind == JsonValueKind.Number && actualElem.TryGetDecimal(out var decVal))
                {
                    actualNum = decVal;
                }
                else if (actualElem.ValueKind == JsonValueKind.String)
                {
                    actualObj = actualElem.GetString();
                }
            }
            else if (decimal.TryParse(actualObj.ToString(), out var decVal))
            {
                actualNum = decVal;
            }

            string actualStr = actualObj.ToString() ?? string.Empty;

            // Numeric comparisons if both sides can be numeric
            if (actualNum.HasValue && targetNum.HasValue)
            {
                return op.ToLowerInvariant() switch
                {
                    ">" or "greaterthan" => actualNum.Value > targetNum.Value,
                    ">=" or "greaterthanorequal" or "gte" => actualNum.Value >= targetNum.Value,
                    "<" or "lessthan" => actualNum.Value < targetNum.Value,
                    "<=" or "lessthanorequal" or "lte" => actualNum.Value <= targetNum.Value,
                    "==" or "equal" or "equals" or "eq" => actualNum.Value == targetNum.Value,
                    "!=" or "notequal" or "neq" => actualNum.Value != targetNum.Value,
                    _ => actualNum.Value == targetNum.Value
                };
            }

            // Boolean comparisons
            if (bool.TryParse(actualStr, out var actualB) && targetBool.HasValue)
            {
                return op.ToLowerInvariant() switch
                {
                    "!=" or "notequal" or "neq" => actualB != targetBool.Value,
                    _ => actualB == targetBool.Value
                };
            }

            // String comparisons
            return op.ToLowerInvariant() switch
            {
                "==" or "equal" or "equals" or "eq" => string.Equals(actualStr, targetStr, StringComparison.OrdinalIgnoreCase),
                "!=" or "notequal" or "neq" => !string.Equals(actualStr, targetStr, StringComparison.OrdinalIgnoreCase),
                "contains" => targetStr != null && actualStr.Contains(targetStr, StringComparison.OrdinalIgnoreCase),
                _ => string.Equals(actualStr, targetStr, StringComparison.OrdinalIgnoreCase)
            };
        }
    }
}
