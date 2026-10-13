namespace KHRMS.AI
{
    // ─── Contract ───────────────────────────────────────────────────────────────

    public interface IChatPolicyService
    {
        Task<IEnumerable<PolicyEntry>> SearchAsync(string query, int maxResults = 3);
    }

    public record PolicyEntry(string Title, string Category, string Content);

    // ─── In-memory implementation (Phase 3 placeholder) ──────────────────────────
    // Replace the static list below with an EF Core query when the CompanyPolicy
    // table is added in a future migration.

    public sealed class InMemoryChatPolicyService : IChatPolicyService
    {
        private static readonly List<PolicyEntry> Policies =
        [
            new("Annual / Earned Leave Entitlement", "Leave",
                "Employees earn 18 days of annual (earned) leave per year, accruing at 1.5 days per month. " +
                "Unused earned leave up to 30 days may be carried forward to the next calendar year."),

            new("Sick Leave", "Leave",
                "Employees are entitled to 10 paid sick leave days per year. " +
                "A medical certificate is required for sick leave exceeding 3 consecutive days."),

            new("Casual Leave", "Leave",
                "Employees receive 12 casual leave days per year. " +
                "A maximum of 3 consecutive casual leave days is allowed without a medical certificate. " +
                "Casual leave must be applied at least 1 working day in advance."),

            new("Maternity Leave", "Leave",
                "Female employees are entitled to 26 weeks of paid maternity leave as per the Maternity Benefit Act. " +
                "This applies to employees who have worked for at least 80 days in the preceding 12 months."),

            new("Paternity Leave", "Leave",
                "Male employees are entitled to 5 paid paternity leave days, to be availed within 6 months of the child's birth."),

            new("Loss of Pay (LOP)", "Leave",
                "Employees may take unpaid leave (LOP) beyond their leave quota. LOP requires prior manager approval."),

            new("Notice Period Policy", "Separation",
                "Probation (0–6 months): 1 month. Confirmed employees (6 months – 3 years): 2 months. " +
                "Senior employees (3+ years): 3 months. Notice can be bought out by mutual agreement."),

            new("Attendance & Work Hours", "Attendance",
                "Standard shift: Monday–Friday, 9 AM to 6 PM. Minimum 8 hours/day required. " +
                "Working less than 4.5 hours counts as half-day. More than 3 late arrivals in a month " +
                "triggers a half-day salary deduction."),

            new("Attendance Regularization", "Attendance",
                "Employees may regularize missed punches or incorrect clock-in/clock-out times. " +
                "Regularization requests must be submitted within 3 working days of the concerned date. " +
                "Requests are subject to manager approval."),

            new("Payroll & Salary", "Payroll",
                "Salary is credited on the last working day of each month. " +
                "Salary components: Basic = 40% of CTC, HRA = 20% of CTC, Special Allowance = remainder. " +
                "Employee EPF contribution: 12% of Basic. Employer EPF: 12%. " +
                "TDS is computed annually and deducted monthly. Payslip is available by the 5th of the following month."),

            new("Public Holidays", "Holiday",
                "The company observes approximately 10–12 public holidays per year, including national holidays " +
                "(Republic Day, Independence Day, Gandhi Jayanti) and applicable state holidays. " +
                "Use the get_upcoming_holidays tool for the current year's exact list."),

            new("Work-from-Home Policy", "Attendance",
                "WFH requires prior manager approval. Attendance must still be punched in remotely. " +
                "Unapproved WFH days are treated as absent unless regularized."),
        ];

        public Task<IEnumerable<PolicyEntry>> SearchAsync(string query, int maxResults = 3)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Task.FromResult(Enumerable.Empty<PolicyEntry>());

            var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var results = Policies
                .Select(p => new
                {
                    Policy = p,
                    Score  = terms.Count(t =>
                        p.Title.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                        p.Category.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                        p.Content.Contains(t, StringComparison.OrdinalIgnoreCase))
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(maxResults)
                .Select(x => x.Policy);

            return Task.FromResult(results);
        }
    }
}
