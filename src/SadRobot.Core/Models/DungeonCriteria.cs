namespace SadRobot.Core.Models
{
    public class DungeonCriteria
    {
        public DungeonCriteria(int scenarioStepId, int criteriaTreeId, string name, int affix = 0, int faction = 0)
        {
            ScenarioStepId = scenarioStepId;
            CriteriaTreeId = criteriaTreeId;
            Name = name;
            Affix = affix;
            Faction = faction;
        }

        /// <summary>
        /// This maps to the ID in ScenarioStep
        /// </summary>
        public int ScenarioStepId { get; set; }

        /// <summary>
        /// This maps to ID in CriteriaTree, which we can traverse to find all the instance mobs and bosses
        /// </summary>
        public int CriteriaTreeId { get; set; }

        /// <summary>
        /// The criteria set name e.g. "8.0 Dungeon - The Underrot - Challenge"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The affix this criteria maps to; 0 normally, or 5 for Teeming
        /// </summary>
        public int Affix { get; set; }

        /// <summary>
        /// The faction this criteria maps to; 0 for both (default), 1 for Alliance and 2 for Horde. Currently
        /// only applicable for Siege of Boralus.
        /// </summary>
        public int Faction { get; set; }

        /// <summary>
        /// The enemy forces count required to complete
        /// </summary>
        public int EnemyForcesCount { get; set; }
    }
}