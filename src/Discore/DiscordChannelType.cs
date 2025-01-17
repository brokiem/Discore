namespace Discore
{
    /// <summary>
    /// The type of a Discord channel.
    /// </summary>
    public enum DiscordChannelType
    {
        Unknown = -1,
        GuildText = 0,
        DirectMessage = 1,
        GuildVoice = 2,
        GuildCategory = 4,
        GuildNews = 5,
        GuildStore = 6
    }
}
