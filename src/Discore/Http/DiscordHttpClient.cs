using Discore.Http.Internal;
using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Discore.Http
{
    public partial class DiscordHttpClient : IDisposable
    {
        /// <summary>
        /// Gets or sets whether to resend requests that get rate-limited.
        /// </summary>
        public bool RetryWhenRateLimited
        {
            get => api.RetryOnRateLimit;
            set => api.RetryOnRateLimit = value;
        }

        readonly ApiClient api;

        public DiscordHttpClient(string botToken)
        {
            api = new ApiClient(botToken);
        }

        string BuildJsonContent(Action<Utf8JsonWriter> builder)
        {
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                builder(writer);
            }

            return Encoding.UTF8.GetString(buffer.WrittenSpan.ToArray());
        }

        DiscordChannel DeserializeChannelData(JsonElement data)
        {
            var type = (DiscordChannelType)data.GetProperty("type").GetInt32();

            if (type == DiscordChannelType.DirectMessage)
                return new DiscordDMChannel(data);
            else if (type == DiscordChannelType.GuildText)
                return new DiscordGuildTextChannel(data);
            else if (type == DiscordChannelType.GuildVoice)
                return new DiscordGuildVoiceChannel(data);
            else if (type == DiscordChannelType.GuildCategory)
                return new DiscordGuildCategoryChannel(data);
            else if (type == DiscordChannelType.GuildNews)
                return new DiscordGuildNewsChannel(data);
            else if (type == DiscordChannelType.GuildStore)
                return new DiscordGuildStoreChannel(data);
            else
                return new DiscordGuildUnknownChannel(data);
                //throw new NotSupportedException($"{type} isn't a known type of {nameof(DiscordChannel)}.");
        }

        public void Dispose()
        {
            api.Dispose();
        }
    }
}
