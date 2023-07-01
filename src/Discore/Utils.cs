using System.Text.Json;

namespace Discore
{
    static class Utils
    {
        public static DiscordUserStatus? ParseUserStatus(string str)
        {
            return str switch
            {
                "offline" => DiscordUserStatus.Offline,
                "invisible" => DiscordUserStatus.Invisible,
                "dnd" => DiscordUserStatus.DoNotDisturb,
                "idle" => DiscordUserStatus.Idle,
                "online" => DiscordUserStatus.Online,
                _ => null,
            };
        }

        public static string? UserStatusToString(DiscordUserStatus status)
        {
            return status switch
            {
                DiscordUserStatus.Offline => "offline",
                DiscordUserStatus.Invisible => "invisible",
                DiscordUserStatus.DoNotDisturb => "dnd",
                DiscordUserStatus.Idle => "idle",
                DiscordUserStatus.Online => "online",
                _ => null,
            };
        }

        public static void AppendClientProperties(Utf8JsonWriter writer)
        {
            writer.WriteString("os", "Windows");
            writer.WriteString("browser", "Discord Client");
            writer.WriteString("release_channel", "stable");
            writer.WriteString("client_version", "1.0.9013");
            writer.WriteString("os_version", "10.0.19045");
            writer.WriteString("os_arch", "x64");
            writer.WriteString("system_locale", "en-US");
            writer.WriteString("browser_user_agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/1.0.9013 Chrome/108.0.5359.215 Electron/22.3.2 Safari/537.36");
            writer.WriteString("browser_version", "22.3.2");
            writer.WriteNumber("client_build_number", 204762);
            writer.WriteNumber("native_build_number", 33666);
            writer.WriteNull("client_event_source");
        }

        public static void AppendClientState(Utf8JsonWriter writer)
        {
            writer.WriteNumber("api_code_version", 0);
            writer.WriteStartObject("guild_versions");
            writer.WriteEndObject();
            writer.WriteString("highest_last_message_id", "0");
            writer.WriteString("private_channels_version", "0");
            writer.WriteNumber("read_state_version", 0);
            writer.WriteNumber("user_guild_settings_version", -1);
            writer.WriteNumber("user_settings_version", -1);
        }
    }
}
