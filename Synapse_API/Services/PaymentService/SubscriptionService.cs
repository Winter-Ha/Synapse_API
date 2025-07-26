using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;
using Synapse_API.Models.Enums;

namespace Synapse_API.Services.PaymentService
{
    namespace Synapse_API.Services
    {
        public class SubscriptionService
        {
            private readonly SynapseDbContext _context; 

            public SubscriptionService(SynapseDbContext context)
            {
                _context = context;
            }

            // Kiểm tra xem người dùng có phải là Premium hay không
            private async Task<bool> IsUserPremium(int userId)
            {
                var subscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.UserID == userId && s.IsActive);

                return subscription != null && subscription.PlanType == PlanType.Premium;
            }

            // Kiểm tra giới hạn tạo Khóa học (Course)
            public async Task<(bool, string)> CheckCourseLimit(int userId)
            {
                if (await IsUserPremium(userId))
                {
                    return (false, string.Empty); // Premium không có giới hạn
                }

                var courseCount = await _context.Courses.CountAsync(c => c.UserID == userId);
                if (courseCount >= 2)
                {
                    return (true, "Bạn đã đạt giới hạn 2 khóa học cho tài khoản miễn phí. Vui lòng nâng cấp.");
                }

                return (false, string.Empty);
            }

            // Kiểm tra giới hạn tạo Chủ đề (Topic)
            public async Task<(bool, string)> CheckTopicLimit(int userId, int courseId)
            {
                if (await IsUserPremium(userId))
                {
                    return (false, string.Empty); // Premium không có giới hạn
                }

                var topicCount = await _context.Topics.CountAsync(t => t.CourseID == courseId);
                if (topicCount >= 2)
                {
                    return (true, "Bạn đã đạt giới hạn 2 chủ đề cho mỗi khóa học. Vui lòng nâng cấp.");
                }

                return (false, string.Empty);
            }

            // Kiểm tra giới hạn tạo Bài trắc nghiệm (Quiz)
            public async Task<(bool, string)> CheckQuizLimit(int userId)
            {
                if (await IsUserPremium(userId))
                {
                    return (false, string.Empty); // Premium không có giới hạn
                }

                // Đếm tổng số quiz của người dùng qua các topic và course
                var quizCount = await _context.Quizzes
                    .Where(q => q.Topic.Course.UserID == userId)
                    .CountAsync();

                if (quizCount >= 1)
                {
                    return (true, "Bạn đã đạt giới hạn 1 bài trắc nghiệm cho tài khoản miễn phí. Vui lòng nâng cấp.");
                }

                return (false, string.Empty);
            }
        }
    }
}
