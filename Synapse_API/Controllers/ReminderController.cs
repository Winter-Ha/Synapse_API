using Microsoft.AspNetCore.Mvc;
using Synapse_API.Services.EventServices;
using Synapse_API.Services;
using Microsoft.AspNetCore.Authorization;
using Synapse_API.Models.Entities;
using Synapse_API.Models.Dto.ReminderDTOs;
using Synapse_API.Utils;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReminderController : ControllerBase
    {
        private readonly EventReminderService _reminderService;
        private readonly EventService _eventService;
        private readonly UserService _userService;
        public ReminderController(EventReminderService reminderService, UserService userService, EventService eventService)
        {
            _reminderService = reminderService;
            _userService = userService;
            _eventService = eventService;
        }


        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpPost("CreateFor/Event/{eventId}")]
        public async Task<ActionResult> CreateEventReminder(int eventId, [FromBody] CreateReminderRequest request)
        {
            try
            {
                var eventItem = await _eventService.GetEventById(eventId);
                if (eventItem == null)
                    return NotFound(AppConstants.ErrorMessages.Event.EventNotFound);

                int userId = _userService.GetMyUserId(User);
                if (eventItem.UserID != userId)
                    return Forbid(AppConstants.ErrorMessages.General.Unauthorized);

                bool success;
                if (request.ReminderTime.HasValue)
                {
                    success = await _reminderService.CreateReminderAsync(eventId, request.ReminderTime.Value);
                }
                else
                {
                    success = await _reminderService.CreateReminderAsync(eventId, request.MinutesBefore);
                }

                if (success)
                {
                    return Ok(AppConstants.SuccessMessages.Reminder.ReminderCreated);
                }
                else
                {
                    return BadRequest(AppConstants.ErrorMessages.Reminder.CreateReminderFailed);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(AppConstants.ErrorMessages.General.BadRequest);
            }
        }

        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpGet("OfEvent/{eventId}")]
        public async Task<ActionResult<IEnumerable<ReminderDto>>> GetEventReminders(int eventId)
        {
            try
            {
                var eventItem = await _eventService.GetEventById(eventId);
                if (eventItem == null)
                    return NotFound(AppConstants.ErrorMessages.Event.EventNotFound);

                int userId = _userService.GetMyUserId(User);
                if (eventItem.UserID != userId)
                    return Forbid(AppConstants.ErrorMessages.General.Unauthorized);

                var reminders = await _reminderService.GetRemindersByEventIdAsync(eventId);
                return Ok(reminders);
            }
            catch (Exception)
            {
                return BadRequest(AppConstants.ErrorMessages.General.BadRequest);
            }
        }

        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpGet("MyReminders")]
        public async Task<ActionResult<IEnumerable<ReminderDto>>> GetMyReminders(bool includeSent = false)
        {
            try
            {
                int userId = _userService.GetMyUserId(User);
                var reminders = await _reminderService.GetRemindersByUserIdAsync(userId, includeSent);
                return Ok(reminders);
            }
            catch (Exception)
            {
                return BadRequest(AppConstants.ErrorMessages.General.BadRequest);
            }
        }

        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpDelete("{reminderId}")]
        public async Task<ActionResult> DeleteEventReminder(int reminderId)
        {
            try
            {
                var success = await _reminderService.DeleteReminderAsync(reminderId);
                if (success)
                {
                    return Ok(AppConstants.SuccessMessages.Reminder.ReminderDeleted);
                }
                else
                {
                    return NotFound(AppConstants.ErrorMessages.Reminder.ReminderNotFound);
                }
            }
            catch (Exception)
            {
                return BadRequest(AppConstants.ErrorMessages.General.BadRequest);
            }
        }

        [Authorize(Roles = AppConstants.Roles.Student)]
        [HttpPut("UpdateFor/Event/{eventId}")]
        public async Task<ActionResult> UpdateEventReminders(int eventId, [FromBody] UpdateRemindersRequest request)
        {
            try
            {
                var eventItem = await _eventService.GetEventById(eventId);
                if (eventItem == null)
                    return NotFound(AppConstants.ErrorMessages.Event.EventNotFound);

                int userId = _userService.GetMyUserId(User);
                if (eventItem.UserID != userId)
                    return Forbid(AppConstants.ErrorMessages.General.Unauthorized);

                var success = await _reminderService.UpdateRemindersAsync(eventId, request.ReminderMinutesBefore);
                if (success)
                {
                    return Ok(AppConstants.SuccessMessages.Reminder.ReminderUpdated);
                }
                else
                {
                    return BadRequest(AppConstants.ErrorMessages.Reminder.ReminderUpdateFailed);
                }
            }
            catch (Exception)
            {
                return BadRequest(AppConstants.ErrorMessages.General.BadRequest);
            }
        }
    }
}
