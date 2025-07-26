using AutoMapper;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;
using Synapse_API.Models.Dto.QuizDTOs;
using Synapse_API.Repositories.Course.Quiz;
using Synapse_API.Services.AIServices;
using Synapse_API.Configuration_Services;
using Synapse_API.Utils;
using System.Text.Json;

namespace Synapse_API.Services.CourseServices.QuizServices
{
    public class QuizService
    {
        private readonly GeminiService _geminiService;
        private readonly QuizRepository _quizRepository;
        private readonly IMapper _mapper;
        private readonly IOptions<ApplicationSettings> _appSettings;

        public QuizService(GeminiService geminiService, QuizRepository quizRepository, IMapper mapper, IOptions<ApplicationSettings> appSettings)
        {
            _geminiService = geminiService;
            _quizRepository = quizRepository;
            _mapper = mapper;
            _appSettings = appSettings;
        }
        public async Task<QuizDto> CreateQuiz(CreateQuizDto createQuizDto)
        {
            var quizEntity = _mapper.Map<Models.Entities.Quiz>(createQuizDto);
            var quizNew = await _quizRepository.CreateQuiz(quizEntity);
            return _mapper.Map<QuizDto>(quizNew);
        }

        public async Task<QuizDto> CreateQuizFromAI(QuizGenerationResponse quizGenerationResponse, int topicID)
        {
            var quizEntity = _mapper.Map<Models.Entities.Quiz>(quizGenerationResponse);
            quizEntity.TopicID = topicID;
            var quizNew = await _quizRepository.CreateQuiz(quizEntity);
            return _mapper.Map<QuizDto>(quizNew);
        }

        public async Task<QuizGenerationResponse> GenerateQuizFromDocument(
            string documentS3Url,
            string quizTitle,
            int numberOfQuestions,
            string mimeType,
            string promptInstruction = "")
        {
            if (string.IsNullOrEmpty(documentS3Url))
            {
                throw new ArgumentNullException(nameof(documentS3Url), AppConstants.ErrorMessages.DocumentProcessing.DocumentUrlEmpty);
            }
            string googleFileUri = await _geminiService.UploadS3FileToGoogleFileApi(documentS3Url, mimeType);

            string fullPrompt = string
            .Format(_appSettings.Value.AI.PromptInstruction, quizTitle, numberOfQuestions, promptInstruction);

            var parts = new List<IPart>
            {
                new TextData { Text = fullPrompt },
                new FileData { FileUri = googleFileUri  }
            };

            var request = new GenerateContentRequest(parts);

            var generationConfig = new GenerationConfig
            {
                ResponseMimeType = _appSettings.Value.AI.MimeType,
                ResponseSchema = new QuizGenerationResponse()
            };

            var response = await _geminiService.GenerateContentWithConfig(request, generationConfig);

            if (response?.Text == null)
            {
                throw new Exception(AppConstants.ErrorMessages.AiResponse.ResponseError);
            }
            var quizResponse = JsonSerializer.Deserialize<QuizGenerationResponse>(response.Text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return quizResponse;
        }


        public async Task<QuizGenerationResponse> GenerateQuizTEST(
            string documentS3Url,
            string quizTitle,
            int numberOfQuestions,
            string mimeType,
            string promptInstruction = "")
        {
            return JsonSerializer.Deserialize<QuizGenerationResponse>(SampleQuizGenerationResponse(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public string SampleQuizGenerationResponse()
        {
            return @"
            {
              ""quizTitle"": ""MLN"",
              ""questions"": [
                {
                  ""questionText"": ""Theo tài liệu, chủ nghĩa Marx-Lenin là thuật ngữ chính trị để chỉ học thuyết do Karl Marx và Friedrich Engels sáng lập và được ai phát triển kế thừa?"",
                  ""options"": [
                    {
                      ""optionKey"": ""A"",
                      ""optionText"": ""Iosif Vissarionovich Stalin"",
                      ""isCorrect"": false
                    },
                    {
                      ""optionKey"": ""B"",
                      ""optionText"": ""Vladimir Ilyich Lenin"",
                      ""isCorrect"": true
                    },
                    {
                      ""optionKey"": ""C"",
                      ""optionText"": ""Mao Trạch Đông"",
                      ""isCorrect"": false
                    },
                    {
                      ""optionKey"": ""D"",
                      ""optionText"": ""Leon Trotsky"",
                      ""isCorrect"": false
                    }
                  ],
                  ""explanation"": ""Theo đoạn văn, chủ nghĩa Marx-Lenin là học thuyết do Karl Marx và Friedrich Engels sáng lập và được Vladimir Ilyich Lenin phát triển kế thừa. Iosif Vissarionovich Stalin là người đã định nghĩa thuật ngữ này, còn Mao Trạch Đông và Trotsky là những nhân vật có học thuyết khác trong chủ nghĩa cộng sản.""
                },
                {
                  ""questionText"": ""Theo tài liệu, chủ nghĩa Marx-Lenin là thuật ngữ chính trị để chỉ học thuyết do Karl Marx và Friedrich Engels sáng lập và được ai phát triển kế thừa?"",
                  ""options"": [
                    {
                      ""optionKey"": ""A"",
                      ""optionText"": ""Iosif Vissarionovich Stalin"",
                      ""isCorrect"": false
                    },
                    {
                      ""optionKey"": ""B"",
                      ""optionText"": ""Vladimir Ilyich Lenin"",
                      ""isCorrect"": true
                    },
                    {
                      ""optionKey"": ""C"",
                      ""optionText"": ""Mao Trạch Đông"",
                      ""isCorrect"": false
                    },
                    {
                      ""optionKey"": ""D"",
                      ""optionText"": ""Leon Trotsky"",
                      ""isCorrect"": false
                    }
                  ],
                  ""explanation"": ""Theo đoạn văn, chủ nghĩa Marx-Lenin là học thuyết do Karl Marx và Friedrich Engels sáng lập và được Vladimir Ilyich Lenin phát triển kế thừa. Iosif Vissarionovich Stalin là người đã định nghĩa thuật ngữ này, còn Mao Trạch Đông và Trotsky là những nhân vật có học thuyết khác trong chủ nghĩa cộng sản.""
                },
                {
                  ""questionText"": ""Theo tài liệu, chủ nghĩa Marx-Lenin là thuật ngữ chính trị để chỉ học thuyết do Karl Marx và Friedrich Engels sáng lập và được ai phát triển kế thừa?"",
                  ""options"": [
                    {
                      ""optionKey"": ""A"",
                      ""optionText"": ""Iosif Vissarionovich Stalin"",
                      ""isCorrect"": false
                    },
                    {
                      ""optionKey"": ""B"",
                      ""optionText"": ""Vladimir Ilyich Lenin"",
                      ""isCorrect"": true
                    },
                    {
                      ""optionKey"": ""C"",
                      ""optionText"": ""Mao Trạch Đông"",
                      ""isCorrect"": false
                    },
                    {
                      ""optionKey"": ""D"",
                      ""optionText"": ""Leon Trotsky"",
                      ""isCorrect"": false
                    }
                  ],
                  ""explanation"": ""Theo đoạn văn, chủ nghĩa Marx-Lenin là học thuyết do Karl Marx và Friedrich Engels sáng lập và được Vladimir Ilyich Lenin phát triển kế thừa. Iosif Vissarionovich Stalin là người đã định nghĩa thuật ngữ này, còn Mao Trạch Đông và Trotsky là những nhân vật có học thuyết khác trong chủ nghĩa cộng sản.""
                }
             ]
            }";
        }

        public async Task<List<QuizDto>> GetQuizByTopicId(int topicId)
        {
            var quiz = await _quizRepository.GetQuizByTopicId(topicId);
            return _mapper.Map<List<QuizDto>>(quiz);
        }

        public async Task<QuizWithQuestionsDto?> GetQuizWithQuestionsAsync(int quizId)
        {
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);
            if (quiz == null) return null;
            
            return _mapper.Map<QuizWithQuestionsDto>(quiz);
        }

        public async Task<bool> DeleteQuiz(int quizID)
        {
            var quiz = await _quizRepository.GetQuizById(quizID);
            if (quiz == null) return false;
            await _quizRepository.DeleteQuiz(quizID);
            return true;
        }
        public async Task<QuizDto?> GetQuizById(int quizID)
        {
            var quiz = await _quizRepository.GetQuizById(quizID);
            if (quiz == null) return null;
            return _mapper.Map<QuizDto>(quiz);
        }

    }
}