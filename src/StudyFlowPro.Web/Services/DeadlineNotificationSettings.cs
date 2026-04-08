namespace StudyFlowPro.Web.Services;

public sealed class DeadlineNotificationSettings
{
    public const string SectionName = "DeadlineNotifications";

    public int UpcomingWindowDays { get; set; } = 2;

    public int RefreshIntervalSeconds { get; set; } = 45;
}
