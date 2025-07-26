namespace Synapse_API.Utils
{
    public static class AppConstants
    {
        public static class Cache
        {
            public const int UserCacheMinutes = 10;
            public const int TokenCacheMinutes = 15;
        }

        public static class Validation
        {
            public const int MinPasswordLength = 6;
            public const int MaxPasswordLength = 100;
            public const int MaxFileSize = 10 * 1024 * 1024; // 10MB
            public const int TokenCodeFrom = 10000;
            public const int TokenCodeTo = 999999;
        }

        public static class ErrorMessages
        {
            public static class General
            {
                public const string InternalServerError = "An unexpected error occurred. Please try again later.";
                public const string BadRequest = "The request was invalid or cannot be served.";
                public const string NotFound = "The requested resource was not found.";
                public const string Unauthorized = "You don't have permission to access this resource";
            }
            public static class Auth
            {
                public const string InvalidCredentials = "Incorrect email or password";
                public const string AccountInactive = "The account can no longer log in to the site due to a violation of community standards.";
                public const string EmailAlreadyExists = "Email already exists.";
                public const string ErrorCodeInactive = "INACTIVE_ACCOUNT";
                public const string ErrorCodeCredentials = "INVALID_CREDENTIALS";
                public const string ErrorResetPass = "Password must be at least 6 characters long.";
                public const string EmailAlreadyExist = "Email already in use";
                public const string EmailNotRegistered = "This email is not registered for Synapse Learn";
                public const string ErrorResetPassword = "Failed to send reset email. Please try again later.";
                public const string ErrorVerifyResetToken = "Invalid or expired token.";
                public const string ErrorResetPasswordHasToken = "Invalid or expired token. Please request a new one.";
            }
            public static class User
            {
                public const string UserIdMissing = "User ID missing from token.";
                public const string ProfileUpdateFailed = "Failed to update profile.";
                public const string UserNotFound = "User not found.";
                public const string FailedToChangePassword = "Failed to change password";
                public const string ProfileNotFound = "User or profile not found.";
            }
            public static class Topic
            {
                public const string TopicCreationFailed = "Failed to create topic.";
                public const string TopicUpdateFailed = "Failed to update topic.";
                public const string TopicDeletionFailed = "Failed to delete topic.";
                public const string TopicNotFound = "Topic not found.";
            }
            public static class Course
            {
                public const string CourseCreationFailed = "Failed to create course.";
                public const string CourseUpdateFailed = "Failed to update course.";
                public const string CourseDeletionFailed = "Failed to delete course.";
                public const string CourseNotFound = "Course not found.";
                public const string UpdateDataIsNull = "Update data is null.";
                public const string GetTopicFailed = "Failed to get topics";
            }
            public static class FileUpload
            {
                public const string FileTooLarge = "File size exceeds the maximum limit.";
                public const string InvalidFileType = "Invalid file type. Only PDF, TXT are allowed.";
            }
            public static class DocumentProcessing
            {
                public const string DocumentUrlEmpty = "Document URL is empty.";
                public const string DocumentProcessingFailed = "Failed to process document.";
                public const string EmbeddingFailed = "Failed to embed document.";
            }
            public static class Quiz
            {
                public const string QuizCreationFailed = "Failed to create quiz.";
                public const string QuizUpdateFailed = "Failed to update quiz.";
                public const string QuizDeletionFailed = "Failed to delete quiz.";
                public const string QuizNotFound = "Quiz not found.";
            }
            public static class Event
            {
                public const string EventCreationFailed = "Failed to create event.";
                public const string EventUpdateFailed = "Failed to update event.";
                public const string EventDeletionFailed = "Failed to delete event.";
                public const string EventNotFound = "Event not found.";
                public const string EventTypeNotExam = "Event is not EXAM-Typed.";
            }
            public static class Reminder
            {
                public const string CreateReminderFailed = "Failed to create reminder.";
                public const string ReminderNotFound = "Reminder not found.";
                public const string ReminderUpdateFailed = "Failed to update reminder.";
                public const string ReminderDeletionFailed = "Failed to delete reminder.";
                public const string ReminderTimeMustBeInTheFuture = "Reminder time must be in the future.";
                public const string ReminderTimeMustBeBeforeEventStartTime = "Reminder time must be before the event start time.";
            }
            public static class AiResponse
            {
                public const string ResponseError = "Can not generate quiz.";
            }
            
        }

        public static class SuccessMessages
        {
            public static class User
            {
                public const string ProfileUpdated = "Profile updated successfully.";
                public const string PasswordChanged = "Password changed successfully.";
                public const string UserDeleted = "User deleted successfully.";
            }
            public static class Reminder
            {
                public const string ReminderCreated = "Reminder created successfully.";
                public const string ReminderUpdated = "Reminder updated successfully.";
                public const string ReminderDeleted = "Reminder deleted successfully.";
            }
            public static class Course
            {
                public const string CourseDeletionSuccess = "Course deleted successfully.";
                public const string CourseCreationSuccess = "Course created successfully.";
                public const string CourseUpdateSuccess = "Course updated successfully.";
                public const string CourseGetAllSuccess = "Course get all successfully.";
                public const string GetAllCourseSuccessful = "Lấy danh sách khóa học thành công";
                public const string DeleteCourseSuccessful = "Delete Successful";
            }

            public static class Auth
            {
                public const string LoginRegistrationSuccessful = "Registration and login successful";
                public const string RegistrationSuccessful = "Registration successful";
                public const string LoginSuccessful = "Login successful";
                public const string SendEmailSuccessful = "If an account with this email exists, a password reset code has been sent.";
                public const string VerifyTokenSuccessful = "Token is valid.";
                public const string ResetPasswordSuccessful = "Password has been reset successfully.";
            }
            
        }
        public static class Roles
        {
            public const string Student = "Student";
            public const string Admin = "Admin";
        }

        public static class DefaultValues
        {
            public const int MinutesPerHour = 60;
            public const int HoursPerDay = 24;
            public const string Minutes = "minutes";
            public const string Hours = "hours";
            public const string Days = "days";
        }
        public static class ComparePoint
        {
            public const int CompareRateWeak = 150;
            public const int CompareRateGood = 80;
            public const int CompareRateBad = 60;
            public const int CompareTrendScoreGood = 8;
            public const int CompareTrendScoreBad = 6;
            public const string DefaultStudyTime = "Evening";
            public const int DefaultStudyHours = 2;
            public const int DefaultReminderMinutes = 15;
            public const string DefaultPasswordLoginWithGG = "123123";
            public static readonly DateTime ExpiryDate = DateTime.Now.AddMinutes(10);
        }
    }
}