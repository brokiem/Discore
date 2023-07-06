using System.Collections.Generic;
using System.Text.Json;

namespace Discore
{
    public class DiscordMemberListUpdateOperator
    {
        /// <summary>
        ///     Operator type for the list
        /// </summary>
        public string Type { get; }

        /// <summary>
        ///     Range being operated upon
        /// </summary>
        public IReadOnlyList<int[]>? Range { get; }

        /// <summary>
        ///     The list of items related to the range given
        /// </summary>
        public IReadOnlyList<DiscordMemberListItem>? Items { get; }

        /// <summary>
        ///     The item being acted upon
        /// </summary>
        public int? Index { get; }

        /// <summary>
        ///     New item for the index
        /// </summary>
        public DiscordMemberListItem? Item { get; }

        internal DiscordMemberListUpdateOperator(JsonElement json, Snowflake guildId)
        {
            Type = json.GetProperty("op").GetString()!;

            if (json.TryGetProperty("range", out JsonElement range))
            {
                var ranges = new List<int[]>();
                foreach (JsonElement r in range.EnumerateArray())
                {
                    int[] rangeArray = new int[2];
                    rangeArray[0] = r[0].GetInt32();
                    rangeArray[1] = r[1].GetInt32();
                    ranges.Add(rangeArray);
                }

                Range = ranges;
            }

            if (json.TryGetProperty("items", out JsonElement items))
            {
                var itemList = new List<DiscordMemberListItem>();
                foreach (JsonElement i in items.EnumerateArray())
                {
                    itemList.Add(new DiscordMemberListItem(i, guildId));
                }

                Items = itemList;
            }

            if (json.TryGetProperty("index", out JsonElement index))
            {
                Index = index.GetInt32();
            }

            if (json.TryGetProperty("item", out JsonElement item))
            {
                Item = new DiscordMemberListItem(item, guildId);
            }
        }
    }
}
