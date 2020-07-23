namespace SadRobot.Cmd
{
    public class MythicKeystone
    {
        public MythicKeystone()
        {
        }

        public MythicKeystone(int id, string slug, int scenarioId)
        {
            Id = id;
            Slug = slug;
            ScenarioId = scenarioId;
        }

        public int Id { get; set; }

        public string Slug { get; set; }

        public string Name { get; set; }


        public int Expansion { get; set; }

        public ushort MapId { get; set; }
        
        public int ScenarioId { get; set; }

        public int CriteriaTreeId { get; set; }

        public int TeemingCriteriaTreeId { get; set; }

        
        public byte Flags { get; set; }

        public ushort BronzeTimer { get; set; }

        public ushort SilverTimer { get; set; }

        public ushort GoldTimer { get; set; }
    }
}