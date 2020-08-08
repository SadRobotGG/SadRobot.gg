using System.Collections.Generic;

namespace SadRobot.Core.Models
{
    public class Instance
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MapId { get; set; }
        public int BackgroundImageId { get; set; }
        public int ButtonImageId { get; set; }
        public int ButtonSmallImageId { get; set; }
        public int LoreImageId { get; set; }
        public int Order { get; set; }
        public int TierId { get; set; }
        public int Flags { get; set; }
        public IList<Encounter> Encounters { get; set; }
    }
}