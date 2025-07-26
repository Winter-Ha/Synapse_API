using System;

namespace Synapse_API.Models.Dto.EventDTOs
{
    public class StudySessionDto
    {
        public int EventID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TopicID { get; set; }
        public string TopicName { get; set; }
    }
} 