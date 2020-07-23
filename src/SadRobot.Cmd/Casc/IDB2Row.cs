namespace SadRobot.Cmd.Casc
{
    public interface IDB2Row
    {
        int GetId();
        void SetId(int id);
        T GetField<T>(int fieldIndex, int arrayIndex = -1);
        IDB2Row Clone();
    }
}