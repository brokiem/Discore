using System.Text.Json;

namespace Discore
{
    public class DiscordMemberListItem
    {
        // TODO: Implement groups

        public DiscordGuildMember GuildMember { get; }

        internal DiscordMemberListItem(DiscordGuildMember member)
        {
            GuildMember = member;
        }

        internal DiscordMemberListItem(JsonElement json, Snowflake guildId)
        {
            GuildMember = new DiscordGuildMember(json.GetProperty("member"), guildId);
        }
    }
}
