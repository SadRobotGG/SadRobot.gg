using System.Collections.Generic;

namespace SadRobot.Core.Models
{
    public class JournalSection
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        public ushort JournalEncounterId { get; set; }
        public byte Order { get; set; }
        public ushort ParentSectionId { get; set; }
        public ushort FirstChildSectionId { get; set; }
        public ushort NextSiblingSectionId { get; set; }
        public byte SectionType { get; set; }
        public uint IconCreatureDisplayId { get; set; }
        public int UiModelSceneId { get; set; }
        public int SpellId { get; set; }
        public int IconFileDataId { get; set; }
        public ushort Flags { get; set; }
        public ushort IconFlags { get; set; }
        public byte DifficultyMask { get; set; }
        public IList<JournalSection> Children { get; set; }
    }
}