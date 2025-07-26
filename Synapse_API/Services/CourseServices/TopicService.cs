using AutoMapper;
using Synapse_API.Models.Dto.TopicDTOs;
using Synapse_API.Repositories.Course;
using Synapse_API.Services.DocumentServices.Interfaces;

namespace Synapse_API.Services.CourseServices
{
    public class TopicService
    {
        private readonly TopicRepository _topicRepository;
        private readonly IMapper _mapper;
        private readonly IDocumentProcessingService _documentProcessingService;

        public TopicService(TopicRepository topicRepository, 
            IMapper mapper, 
            IDocumentProcessingService documentProcessingService)
        {
            _topicRepository = topicRepository;
            _mapper = mapper;
            _documentProcessingService = documentProcessingService;
        }

        public async Task<List<TopicDto>> GetAllTopics()
        {
            var topics = await _topicRepository.GetAllTopics();
            return _mapper.Map<List<TopicDto>>(topics);
        }

        public async Task<TopicDto> GetTopicById(int id)
        {
            var topic = await _topicRepository.GetTopicById(id);
            if (topic == null)
            {
                return null;
            }
            return _mapper.Map<TopicDto>(topic);
        }

        public async Task<TopicDto> CreateTopic(CreateTopicDto topicDto)
        {
            var topic = _mapper.Map<Models.Entities.Topic>(topicDto);
            topic = await _topicRepository.AddTopic(topic);
            return _mapper.Map<TopicDto>(topic);
        }

        public async Task<TopicDto> UpdateTopic(int id, UpdateTopicDto topicDto)
        {
            var topic = await _topicRepository.GetTopicById(id);
            if (topic == null)
            {
                return null;
            }
            if ((topic.DocumentUrl != string.Empty || topic.DocumentUrl != null)
                && (topicDto.DocumentUrl == string.Empty))
            {
                topicDto.DocumentUrl = topic.DocumentUrl;
            }
            _mapper.Map(topicDto, topic);
            topic = await _topicRepository.UpdateTopic(topic);
            return _mapper.Map<TopicDto>(topic);
        }
        public async Task<TopicDto> DeleteTopic(int id)
        {
            var topic = await _topicRepository.GetTopicById(id);
            if (topic == null)
            {
                return null;
            }
            await _topicRepository.DeleteTopic(topic);
            return _mapper.Map<TopicDto>(topic);
        }
        public async Task<List<TopicDto>> GetTopicByCourseId(int courseId)
        {
            var topics = await _topicRepository.GetTopicByCourseId(courseId);
            return _mapper.Map<List<TopicDto>>(topics);
        }

        public async Task<TopicDto> CreateTopicWithDocument(CreateTopicRequest topicRequest, string fileUrl, int userId)
        {
            var topicDto = new CreateTopicDto
            {
                CourseID = topicRequest.CourseID,
                TopicName = topicRequest.TopicName,
                Description = topicRequest.Description,
                DocumentUrl = fileUrl
            };
            var topic = await CreateTopic(topicDto);

            // nhúng dữ liêu từ tài liệu vào Qdrant
            string docName = string.IsNullOrEmpty(topicRequest.TopicName) ? topicRequest.DocumentFile.FileName : topicRequest.TopicName;
            bool success = await _documentProcessingService.ProcessAndEmbedDocumentAsync(
                topicRequest.DocumentFile,
                userId,
                docName,
                topicRequest.CourseID,
                topic.TopicID
            );
            return topic;
        }
        public async Task<TopicDto> UpdateTopicWithDocument(int topicId, UpdateTopicRequest topicRequest, string fileUrl, int userId)
        {
            var topicDto = new UpdateTopicDto
            {
                TopicName = topicRequest.TopicName,
                Description = topicRequest.Description,
                DocumentUrl = fileUrl
            };
            var topic = await UpdateTopic(topicId, topicDto);

            // nhúng dữ liêu từ tài liệu vào Qdrant
            string docName = string.IsNullOrEmpty(topicRequest.TopicName) ? topicRequest.DocumentFile.FileName : topicRequest.TopicName;
            bool success = await _documentProcessingService.ProcessAndEmbedDocumentAsync(
                topicRequest.DocumentFile,
                userId,
                docName,
                topic.CourseID,
                topic.TopicID
            );
            return topic;
        }
    }
}
