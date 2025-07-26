using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse_API.Services;
using Synapse_API.Services.CourseServices;
using Synapse_API.Models.Dto.CourseDTOs;
using Synapse_API.Utils;
using Synapse_API.Services.PaymentService.Synapse_API.Services;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly CourseService _courseService;
        private readonly UserService _userService;
        private readonly TopicService _topicService;
        private readonly SubscriptionService _subscriptionService;

        public CoursesController(CourseService courseService, UserService userService, TopicService topicService, SubscriptionService subscriptionService)
        {
            _courseService = courseService;
            _userService = userService;
            _topicService = topicService;
            _subscriptionService = subscriptionService;
        }

        [HttpGet("get-all-course")]
        public async Task<IActionResult> GetAllCourseAsync()
        {
            var course = await _courseService.GetAllCoursesAsync();
            return Ok(course);
        }

        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpGet("get-my-course")]
        public async Task<IActionResult> GetMyCourseAsync()
        {
            int userId = _userService.GetMyUserId(User);
            var events = await _courseService.GetCourseByUserId(userId);

            return Ok(events);
        }

        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpGet("get-course-by-id/{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _courseService.GetCourseById(id);
            if (course == null)
            {
                return NotFound(AppConstants.ErrorMessages.Course.CourseNotFound);
            }
            return Ok(course);
        }

        //Thay đổi
        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpPost("create-course")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseRequestDto courseRequest)
        {
            var userId = _userService.GetMyUserId(User);

            // Bắt đầu kiểm tra giới hạn
            var (limitExceeded, message) = await _subscriptionService.CheckCourseLimit(userId);
            if (limitExceeded)
            {
                return StatusCode(402, new { message }); // 402 Payment Required
            }
            try
            {
                var course = await _courseService.CreateCourseAsync(courseRequest, userId);
                return Ok(course);
            }
            catch (Exception)
            {
                return BadRequest(AppConstants.ErrorMessages.Course.CourseCreationFailed);
            }
        }

        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpPut("update-course/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
        {
            if (dto == null)
            {
                return BadRequest(AppConstants.ErrorMessages.General.BadRequest);

            }

            var result = await _courseService.UpdateCourseAsync(id, dto);

            if (result == null)
            {
                return NotFound(AppConstants.ErrorMessages.Course.CourseUpdateFailed);
            }

            return Ok(result);
        }

        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpDelete("delete-course/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var result = await _courseService.DeleteCourseById(id);

            if (!result.Success)
            {
                return NotFound(AppConstants.ErrorMessages.Course.CourseDeletionFailed);
            }

            return NoContent();
        }
        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpGet("{courseId}/events")]
        public async Task<IActionResult> GetParentEventsByCourse(int courseId)
        {
            var result = await _courseService.GetListParentEventByCourseId(courseId);
            return Ok(result);
        }

        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpGet("{courseId}/topics")]
        public async Task<IActionResult> GetTopicByCourseId(int courseId)
        {
            try
            {
                var topics = await _topicService.GetTopicByCourseId(courseId);
                return Ok(topics);
            }
            catch (Exception)
            {
                return BadRequest(AppConstants.ErrorMessages.General.BadRequest);

            }
        }
    }
}
