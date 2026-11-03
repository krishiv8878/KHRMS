using System;

namespace KHRMS.Services
{
    public static class FieldMaskingHelper
    {
        public static string? MaskAccountNumber(long? accountNumber)
        {
            if (!accountNumber.HasValue) return null;
            return MaskAccountNumber(accountNumber.Value.ToString());
        }

        public static string? MaskAccountNumber(string? str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            var trimmed = str.Trim();
            if (trimmed.Length <= 4) return new string('*', trimmed.Length);
            return new string('*', trimmed.Length - 4) + trimmed.Substring(trimmed.Length - 4);
        }

        public static string? MaskIfsc(string? ifsc)
        {
            if (string.IsNullOrWhiteSpace(ifsc)) return ifsc;
            var trimmed = ifsc.Trim();
            if (trimmed.Length <= 4) return "****";
            return trimmed.Substring(0, 4) + "****" + (trimmed.Length > 7 ? trimmed.Substring(trimmed.Length - 3) : "");
        }

        public static string? MaskPassport(string? passport)
        {
            if (string.IsNullOrWhiteSpace(passport)) return passport;
            var trimmed = passport.Trim();
            if (trimmed.Length <= 4) return "****";
            return new string('*', trimmed.Length - 4) + trimmed.Substring(trimmed.Length - 4);
        }

        public static string? MaskPan(string? pan)
        {
            if (string.IsNullOrWhiteSpace(pan)) return pan;
            var trimmed = pan.Trim();
            if (trimmed.Length <= 4) return "*****";
            return "*****" + trimmed.Substring(Math.Max(0, trimmed.Length - 4));
        }

        public static decimal MaskSalary(decimal salary) => 0m;
        public static string MaskSalaryDisplay(decimal salary) => "₹ **,**,***";
    }
}
