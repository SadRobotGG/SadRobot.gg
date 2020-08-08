using System.Text.Json;
using SadRobot.Core.Text;

namespace SadRobot.Core.Json
{
    public class SnakeCaseNamingStrategy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return StringCasingHelper.ToSnakeCase(name);
        }
    }
}