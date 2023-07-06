using System;
using System.Text.Json;

namespace Discore
{
    public class DiscordGuildMemberListGroup
    {
        /// <summary>
        ///     Group id, can be Snowflake OR "online" OR "offline"
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     The amount of members in that group
        /// </summary>
        public int Count { get; }

        internal DiscordGuildMemberListGroup(JsonElement json)
        {
            Id = json.GetProperty("id").GetString()!;
            Count = json.GetProperty("count").GetInt32();
        }

        internal DiscordGuildMemberListGroup(string id, int count)
        {
            Id = id;
            Count = count;
        }
    }
}
