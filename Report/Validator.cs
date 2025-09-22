using System.ComponentModel.DataAnnotations;
using TimeZoneConverter;

namespace JobSearcher.Report
{
    public class ValidTimeZoneAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string tzId)
                return new ValidationResult("Time zone must be a string.");

            try
            {
                var windowsId = TZConvert.IanaToWindows(tzId);
                var validWindowsIds = TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id);
                if (!validWindowsIds.Contains(windowsId))
                    return new ValidationResult($"Invalid time zone: {tzId}");
            }
            catch
            {
                return new ValidationResult($"Invalid time zone: {tzId}");
            }

            return ValidationResult.Success;
        }
    }
}
