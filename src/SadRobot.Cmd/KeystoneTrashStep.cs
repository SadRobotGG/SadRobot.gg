using System.Collections.Generic;

namespace SadRobot.Cmd
{
    public class KeystoneTrashStep
    {
        public int Id { get; set; }

        public int TreeId { get; set; }

        public IList<KeystoneNpc> Trash { get; set; }
    }
}