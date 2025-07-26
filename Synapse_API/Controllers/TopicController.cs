using Microsoft.AspNetCore.Mvc;
using Synapse_API.Services;
using Synapse_API.Services.CourseServices;
using Synapse_API.Models.Dto.TopicDTOs;
using Synapse_API.Services.AmazonServices;
using Microsoft.AspNetCore.Authorization;
using Synapse_API.Services.DocumentServices.Interfaces;
using Microsoft.Extensions.Options;
using Synapse_API.Configuration_Services;
using Synapse_API.Utils;
using Synapse_API.Services.PaymentService.Synapse_API.Services;
namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly TopicService _topicService;
        private readonly UserService _userService;
        private readonly MyS3Service _myS3Service;
        private readonly IOptions<ApplicationSettings> _appSettings;
        private readonly SubscriptionService _subscriptionService;

        public TopicController(TopicService topicService,
        UserService userService,
        MyS3Service myS3Service,
        IOptions<ApplicationSettings> appSettings,
        SubscriptionService subscriptionService)
        {
            _topicService = topicService;
            _userService = userService;
            _myS3Service = myS3Service;
            _appSettings = appSettings;
            _subscriptionService = subscriptionService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllTopics()
        {
            var topics = await _topicService.GetAllTopics();
            return Ok(topics);
        }
        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTopicById(int id)
        {
            var topic = await _topicService.GetTopicById(id);
            if (topic == null)
            {
                return NotFound(AppConstants.ErrorMessages.Topic.TopicNotFound);
            }
            return Ok(topic);
        }
        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpPost()]
        public async Task<IActionResult> CreateTopic([FromForm] CreateTopicRequest topicRequest)
        {
            var userId = _userService.GetMyUserId(User);

            var (limitExceeded, message) = await _subscriptionService.CheckTopicLimit(userId, topicRequest.CourseID);
            if (limitExceeded)
            {
                return StatusCode(402, new { message });
            }
            try
            {
                var fileUrl = await HandleFileUpload(topicRequest.DocumentFile);
                var createdTopic = await _topicService.CreateTopicWithDocument(topicRequest, fileUrl, userId);
                return Ok(createdTopic);
            }
            catch (Exception)
            {
                return BadRequest(AppConstants.ErrorMessages.Topic.TopicCreationFailed);
            }
        }

        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTopic(int id, UpdateTopicRequest topicRequest)
        {
            try
            {
                var fileUrl = await HandleFileUpload(topicRequest.DocumentFile);
                var userId = _userService.GetMyUserId(User);

                var updatedTopic = await _topicService.UpdateTopicWithDocument(id, topicRequest, fileUrl, userId);

                return Ok(updatedTopic);
            }
            catch (Exception)
            {
                return BadRequest(AppConstants.ErrorMessages.Topic.TopicUpdateFailed);
            }
        }
        private async Task<string> HandleFileUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            var allowedExtensions = _appSettings.Value.FileUpload.AllowedExtensions;
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
            {
                throw new ArgumentException(AppConstants.ErrorMessages.FileUpload.InvalidFileType);
            }

            var myEmail = _userService.GetMyEmail(User);
            var fileName = FileNameHelper.SetDocumentName(myEmail, file.FileName);
            return await _myS3Service.UploadObjectAsync(file, fileName);
        }

        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            try
            {
                var topic = await _topicService.DeleteTopic(id);
                return Ok(topic);
            }
            catch (Exception)
            {
                return BadRequest(AppConstants.ErrorMessages.Topic.TopicDeletionFailed);
            }
        }
    }
}

