using Synapse_API.Repositories;
using Synapse_API.Models.Dto.EventDTOs;
using AutoMapper;
using Synapse_API.Models.Entities;
using Synapse_API.Models.Enums;
using Synapse_API.Repositories.Profile;
using Synapse_API.Repositories.Course;
using Microsoft.Extensions.Options;
using Synapse_API.Configuration_Services;
using Synapse_API.Utils;

namespace Synapse_API.Services.EventServices
{
    public class EventService
    {
        private readonly EventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly UserProfileRepository _userProfileRepository;
        private readonly CourseRepository _courseRepository;
        private readonly TopicRepository _topicRepository;
        private readonly EventReminderService _reminderService;
        private readonly IOptions<ApplicationSettings> _appSettings;

        public EventService(EventRepository eventRepository, IMapper mapper, UserProfileRepository userProfileRepository, CourseRepository courseRepository, TopicRepository topicRepository, EventReminderService reminderService, IOptions<ApplicationSettings> appSettings)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _userProfileRepository = userProfileRepository;
            _courseRepository = courseRepository;
            _topicRepository = topicRepository;
            _reminderService = reminderService;
            _appSettings = appSettings;
        }

        public async Task<EventDto> GetEventById(int id)
        {
            var e = await _eventRepository.GetEventById(id);
            var eDto = _mapper.Map<EventDto>(e);
            return eDto;
        }
        public async Task<IEnumerable<EventDto>> GetEventsByStudentId(int studentId)
        {
            var events = await _eventRepository.GetEventsByStudentId(studentId);
            var eventsDto = _mapper.Map<IEnumerable<EventDto>>(events);
            return eventsDto;
        }
        public async Task<IEnumerable<EventDto>> GetParentEventsByStudentId(int studentId)
        {
            var events = await _eventRepository.GetParentEventsByStudentId(studentId);
            var eventsDto = _mapper.Map<IEnumerable<EventDto>>(events);
            return eventsDto;
        }
        public async Task<EventDto> GetEventAndItsSubEventById(int eventId)
        {
            var events = await _eventRepository.GetEventAndItsSubEventById(eventId);
            var eventsDto = _mapper.Map<EventDto>(events);
            return eventsDto;
        }

        public async Task<IEnumerable<EventDto>> GetAllEvents()
        {
            var events = await _eventRepository.GetAllEvents();
            var eventsDto = _mapper.Map<IEnumerable<EventDto>>(events);
            return eventsDto;
        }

        public async Task<EventDto> CreateEvent(CreateEventDto ce)
        {
            var e = _mapper.Map<Event>(ce);
            var eDto = await _eventRepository.CreateEvent(e);

            // Tạo default reminders
            if (e.StartTime > DateTime.Now)
            {
                await _reminderService.CreateDefaultRemindersAsync(eDto.EventID);
            }

            return _mapper.Map<EventDto>(eDto);
        }

        public async Task<EventDto> UpdateEvent(UpdateEventDto ue)
        {
            var eventEntity = await _eventRepository.GetEventById(ue.EventID);
            _mapper.Map(ue, eventEntity);
            var updated = await _eventRepository.UpdateEvent(eventEntity);
            return _mapper.Map<EventDto>(updated);
        }

        public async Task UpdateChildEventIsCompleted(int id, bool isCompleted)
        {
            var child = await _eventRepository.GetEventById(id);
            if (child == null) return;
            child.IsCompleted = isCompleted;
            await _eventRepository.UpdateEvent(child);
        }


        public async Task<bool> DeleteChileEventsByParentEventId(int parentEventId)
        {
            var subEvents = await _eventRepository.DeleteChildEventsByParentEventId(parentEventId);
            return subEvents;
        }

        public async Task<EventDto> DeleteEvent(int id)
        {
            var e = await _eventRepository.DeleteEvent(id);
            return _mapper.Map<EventDto>(e);
        }

        // Gen sub events để practice Topic
        public async Task<List<StudySessionDto>> GenerateStudyPlan(GenerateStudyPlanDto generateDto)
        {
            var examEvent = await GetExamEvent(generateDto.ExamEventID);
            var userProfile = await GetUserProfile(generateDto.UserID);
            var course = await GetCourse(generateDto.CourseID);

            // delete sessions hoc cu
            await _eventRepository.DeleteStudySessionsByParentEvent(generateDto.ExamEventID);

            // Gen sub event
            var (studySessions, studySessionDtos) = await GenerateStudySessions(
                generateDto, examEvent, userProfile, course);

            var createdEvents = await _eventRepository.CreateMultipleEvents(studySessions);

            return studySessionDtos;
        }

        private async Task<Event> GetExamEvent(int examEventId)
        {
            var examEvent = await _eventRepository.GetEventById(examEventId);
            if (examEvent == null)
            {
                throw new Exception(AppConstants.ErrorMessages.Event.EventNotFound);
            }
            if (examEvent.EventType != EventType.Exam)
            {
                throw new Exception(AppConstants.ErrorMessages.Event.EventTypeNotExam);
            }
            return examEvent;
        }

        private async Task<UserProfile> GetUserProfile(int userId)
        {
            var userProfile = await _userProfileRepository.GetUserProfileByUserId(userId);
            if (userProfile == null)
            {
                throw new Exception(AppConstants.ErrorMessages.User.ProfileNotFound);
            }
            return userProfile;
        }

        private async Task<Course> GetCourse(int courseId)
        {
            var course = await _courseRepository.GetCourseById(courseId);
            if (course == null)
            {
                throw new Exception(AppConstants.ErrorMessages.Course.CourseNotFound);
            }
            return course;
        }

        private async Task<(List<Event> studySessions, List<StudySessionDto> studySessionDtos)> GenerateStudySessions(
            GenerateStudyPlanDto generateDto, Event examEvent, UserProfile userProfile, Course course)
        {
            var studySessions = new List<Event>();
            var studySessionDtos = new List<StudySessionDto>();

            var daysBeforeExam = generateDto.DaysBeforeExam ?? _appSettings.Value.StudyPlan.DefaultDaysBeforeExam;
            var startDate = generateDto.ExamDate.AddDays(-daysBeforeExam);
            var topics = course.Topics.ToList();
            var topicsPerDay = Math.Max(_appSettings.Value.StudyPlan.MinTopicPracticePerDay, topics.Count / daysBeforeExam);

            // Thời gian học 1 ngày
            var dailyStudyHours = userProfile.DailyStudyHours ?? _appSettings.Value.StudyPlan.DefaultDailyStudyHours;
            var preferredTime = userProfile.PreferredStudyTime ?? _appSettings.Value.StudyPlan.DefaultPreferredTime;
            var topicIndex = 0;

            for (int day = 0; day < daysBeforeExam && topicIndex < topics.Count; day++)
            {
                var studyDate = startDate.AddDays(day);
                var startHour = _appSettings.Value.StudyPlan.StudyTimeHours[preferredTime];

                // Kiểm tra xung đột event
                var existingEvents = await _eventRepository.GetEventsByUserAndDateRange(
                    generateDto.UserID,
                    studyDate.Date,
                    studyDate.Date.AddDays(_appSettings.Value.StudyPlan.NextDayIfConflictEvent)
                );
                startHour = AdjustStudyStartHour(existingEvents, startHour, dailyStudyHours);

                // Tạo study session cho {day}
                var topicsForToday = new List<Topic>();
                for (int i = 0; i < topicsPerDay && topicIndex < topics.Count; i++)
                {
                    topicsForToday.Add(topics[topicIndex]);
                    topicIndex++;
                }

                var hoursPerTopic = (double)dailyStudyHours / topicsForToday.Count;
                var currentStartTime = studyDate.Date.AddHours(startHour);

                foreach (var topic in topicsForToday)
                {
                    var studySession = CreateStudySession(generateDto, examEvent, topic, currentStartTime, hoursPerTopic);
                    studySessions.Add(studySession);

                    var sessionDto = CreateStudySessionDto(studySession, topic, hoursPerTopic);
                    studySessionDtos.Add(sessionDto);
                    currentStartTime = currentStartTime.AddHours(hoursPerTopic);
                }
            }

            return (studySessions, studySessionDtos);
        }

        private int AdjustStudyStartHour(IEnumerable<Event> existingEvents, int startHour, int dailyStudyHours)
        {
            while (existingEvents.Any(e =>
                e.StartTime.Hour <= startHour + dailyStudyHours &&
                e.EndTime.Hour >= startHour))
            {
                startHour += _appSettings.Value.StudyPlan.NextHourIfConflictStartTime;
                if (startHour >= _appSettings.Value.StudyPlan.MaxScheduleConflictHour)
                {
                    startHour = _appSettings.Value.StudyPlan.MinScheduleStartHour;
                    break;
                }
            }
            return startHour;
        }

        private Event CreateStudySession(
            GenerateStudyPlanDto generateDto, Event examEvent, Topic topic, DateTime currentStartTime, double hoursPerTopic)
        {
            return new Event
            {
                UserID = generateDto.UserID,
                CourseID = generateDto.CourseID,
                ParentEventID = generateDto.ExamEventID,
                EventType = EventType.StudySession,
                Title = string.Format(_appSettings.Value.StudyPlan.StudySessionTitleFormat, topic.TopicName),
                Description = string.Format(
                    _appSettings.Value.StudyPlan.StudySessionDescriptionFormat,
                    new { topicName = topic.TopicName, examTitle = examEvent.Title }),
                StartTime = currentStartTime,
                EndTime = currentStartTime.AddHours(hoursPerTopic),
                IsCompleted = false
            };
        }

        private StudySessionDto CreateStudySessionDto(
            Event studySession, Topic topic, double hoursPerTopic)
        {
            return new StudySessionDto
            {
                Title = studySession.Title,
                Description = studySession.Description,
                StartTime = studySession.StartTime,
                EndTime = studySession.EndTime,
                TopicID = topic.TopicID,
                TopicName = topic.TopicName,
            };
        }

    }
}
