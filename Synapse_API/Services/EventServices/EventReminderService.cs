
using Synapse_API.Models.Dto.ReminderDTOs;
using Synapse_API.Models.Entities;
using Synapse_API.Repositories;
using Synapse_API.Repositories.Event;
using Synapse_API.Services.AmazonServices;
using Microsoft.Extensions.Options;
using Synapse_API.Configuration_Services;
using Synapse_API.Utils;

namespace Synapse_API.Services.EventServices
{
    public class EventReminderService
    {
        private readonly EmailService _emailService;
        private readonly EventRepository _eventRepository;
        private readonly ReminderRepository _reminderRepository;
        private readonly IOptions<ApplicationSettings> _appSettings;
        
        public EventReminderService(
            EmailService emailService,
            EventRepository eventRepository,
            ReminderRepository reminderRepository,
            IOptions<ApplicationSettings> appSettings)
        {
            _emailService = emailService;
            _eventRepository = eventRepository;
            _reminderRepository = reminderRepository;
            _appSettings = appSettings;
        }


        public async Task ProcessEventRemindersAsync()
        {
            try
            {
                var currentTime = DateTime.Now;
                
                var dueReminders = await _reminderRepository.GetDueRemindersAsync(currentTime);
                foreach (var reminder in dueReminders)
                {
                    await SendReminderEmailAsync(reminder);
                }
                await _reminderRepository.SaveChangesAsync();
            }
            catch (Exception)
            {
                return;
            }
        }


        private async Task SendReminderEmailAsync(EventReminder reminder)
        {
            try
            {
                var user = reminder.Event.User;
                var eventItem = reminder.Event;

                var timeDifference = eventItem.StartTime - reminder.ReminderTime;
                var reminderTimeText = GetReminderTimeText(timeDifference);

                await _emailService.SendEventReminderEmailAsync(user, eventItem, reminderTimeText);

                reminder.IsSent = true;
            }
            catch (Exception)
            {
                return;
            }
        }


        public async Task CreateDefaultRemindersAsync(int eventId, List<int> reminderMinutesBefore = null)
        {
            reminderMinutesBefore ??= _appSettings.Value.Reminder.DefaultValuesMinutesBefore.ToList();

            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null) return;

            foreach (var minutesBefore in reminderMinutesBefore)
            {
                var reminderTime = eventItem.StartTime.AddMinutes(-minutesBefore);
                
                if (reminderTime > DateTime.Now)
                {
                    var reminder = new EventReminder
                    {
                        EventID = eventId,
                        ReminderTime = reminderTime,
                        IsSent = false
                    };
                    await _reminderRepository.CreateReminder(reminder);
                }
            }
        }


        public async Task CreateCustomReminderAsync(int eventId, DateTime reminderTime)
        {
            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null) 
                throw new ArgumentException(AppConstants.ErrorMessages.Event.EventNotFound);

            if (reminderTime <= DateTime.Now)
                throw new ArgumentException(AppConstants.ErrorMessages.Reminder.ReminderTimeMustBeInTheFuture);

            if (reminderTime >= eventItem.StartTime) 
                throw new ArgumentException(AppConstants.ErrorMessages.Reminder.ReminderTimeMustBeBeforeEventStartTime);

            var reminder = new EventReminder
            {
                EventID = eventId,
                ReminderTime = reminderTime,
                IsSent = false
            };

            await _reminderRepository.CreateReminder(reminder);
        }



        private string GetReminderTimeText(TimeSpan timeDifference)
        {
            if (timeDifference.TotalMinutes < AppConstants.DefaultValues.MinutesPerHour)
                return $"{(int)timeDifference.TotalMinutes} {AppConstants.DefaultValues.Minutes}";
            else if (timeDifference.TotalHours < AppConstants.DefaultValues.HoursPerDay)
                return $"{(int)timeDifference.TotalHours} {AppConstants.DefaultValues.Hours}";
            else
                return $"{(int)timeDifference.TotalDays} {AppConstants.DefaultValues.Days}";
        }



        public async Task<bool> CreateReminderAsync(int eventId, int? minutesBefore)
        {
            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null) return false;

            var reminderTime = eventItem.StartTime.AddMinutes((double)-minutesBefore);
            await CreateCustomReminderAsync(eventId, reminderTime);
            return true;
        }

        public async Task<bool> CreateReminderAsync(int eventId, DateTime reminderTime)
        {
            await CreateCustomReminderAsync(eventId, reminderTime);
            return true;
        }

        public async Task<IEnumerable<ReminderDto>> GetRemindersByEventIdAsync(int eventId)
        {
            var reminders = await _reminderRepository.GetRemindersByEventId(eventId);
            return reminders.Select(r => new ReminderDto
            {
                ReminderID = r.ReminderID,
                ReminderTime = r.ReminderTime,
                IsSent = r.IsSent
            });
        }

        public async Task<IEnumerable<ReminderDto>> GetRemindersByUserIdAsync(int userId, bool includeSent)
        {
            var reminders = await _reminderRepository.GetRemindersByUserId(userId, includeSent);
            return reminders.Select(r => new ReminderDto
            {
                ReminderID = r.ReminderID,
                ReminderTime = r.ReminderTime,
                IsSent = r.IsSent
            });
        }

        public async Task<bool> DeleteReminderAsync(int reminderId)
        {
            return await _reminderRepository.DeleteReminder(reminderId);
        }

        public async Task<bool> UpdateRemindersAsync(int eventId, List<int> reminderMinutesBefore)
        {
            await _reminderRepository.DeletePendingRemindersAsync(eventId);

            await CreateDefaultRemindersAsync(eventId, reminderMinutesBefore);
            return true;
        }
    }
} 