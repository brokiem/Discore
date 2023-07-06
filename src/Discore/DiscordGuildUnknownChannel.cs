using System.Text.Json;

namespace Discore
{
    public class DiscordGuildUnknownChannel : DiscordGuildChannel
    {
        internal DiscordGuildUnknownChannel(JsonElement json, Snowflake? guildId = null) : base(json, DiscordChannelType.Unknown, guildId)
        {
        }
    }
}
