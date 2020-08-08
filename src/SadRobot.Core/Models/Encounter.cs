using System.Collections.Generic;

namespace SadRobot.Core.Models
{
    public class Encounter
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float MapX { get; set; }
        public float MapY { get; set; }
        public int InstanceId { get; set; }
        public int EncounterId { get; set; }
        public int Order { get; set; }
        public int FirstSectionId { get; set; }
        public int UiMapId { get; set; }
        public int MapDisplayConditionId { get; set; }
        public int Flags { get; set; }
        public int Difficulty { get; set; }
        public IList<JournalSection> Sections { get; set; }
    }
}