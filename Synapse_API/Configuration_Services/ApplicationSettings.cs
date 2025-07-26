using Synapse_API.Utils;

namespace Synapse_API.Configuration_Services
{
    public class ApplicationSettings
    {
        public FileUploadSettings FileUpload { get; set; }
        public QuizSettings Quiz { get; set; }
        public StudyPlanSettings StudyPlan { get; set; }
        public ReminderSettings Reminder { get; set; }
        public BackgroundJobSettings BackgroundJob { get; set; }
        public AISettings AI { get; set; }
        public PasswordSettings Password { get; set; }
    }

    public class FileUploadSettings
    {
        public string[] AllowedExtensions { get; set; } = { ".pdf", ".txt" };
        public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB
    }

    public class QuizSettings
    {
        public int MaxScore { get; set; } = 10;
        public int ScoreDecimalPlaces { get; set; } = 2;
        public Dictionary<string, decimal> FeedbackThresholds { get; set; } = new()
        {
            { "Excellent", 9.0m },
            { "VeryGood", 8.0m },
            { "Good", 7.0m },
            { "Fair", 6.0m },
            { "Average", 5.0m }
        };
        public Dictionary<string, Dictionary<string, string>> FeedbackMessages { get; set; } = new();
    }


    public class StudyPlanSettings
    {
        public int DefaultDailyStudyHours { get; set; } = 2;
        public string DefaultPreferredTime { get; set; } = "Evening";
        public Dictionary<string, int> StudyTimeHours { get; set; } = new()
        {
            { "Morning", 8 },
            { "Afternoon", 14 },
            { "Evening", 19 }
        };
        public int DefaultPriority { get; set; } = 1;
        public int MinTopicPracticePerDay { get; set; } = 1;
        public int NextDayIfConflictEvent { get; set; } = 1;
        public int NextHourIfConflictStartTime { get; set; } = 1;
        public int DefaultDaysBeforeExam { get; set; } = 7;
        public int MaxScheduleConflictHour { get; set; } = 23;
        public int MinScheduleStartHour { get; set; } = 8;
        public string StudySessionTitleFormat { get; set; } = "Practice {0}";
        public string StudySessionDescriptionFormat { get; set; } = "Practice {0} for {1}";
    }

    public class ReminderSettings
    {
        public int[] DefaultValuesMinutesBefore { get; set; } = { 15, 60 };
        public int DefaultValueMinutesBefore { get; set; } = 15;
    }

    public class BackgroundJobSettings
    {
        public int ReminderCheckIntervalMinutes { get; set; } = 1;
    }

    public class AISettings
    {
        public string MimeType { get; set; } = "application/json";
        public string PromptInstruction { get; set; } = "Based on the content of the document, create a multiple choice test with quizTitle {0} containing {1} questions. {2}.";
    }
}

public class PasswordSettings
{
    public string DefaultPassword { get; set; } = "123123";
    public int MinLength { get; set; } = 6;
}
