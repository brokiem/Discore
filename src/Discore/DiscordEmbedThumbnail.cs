﻿namespace Discore
{
    /// <summary>
    /// A thumbnail of a <see cref="DiscordEmbed"/>.
    /// </summary>
    public sealed class DiscordEmbedThumbnail
    {
        /// <summary>
        /// Gets the url of the thumbnail.
        /// </summary>
        public string Url { get; }
        /// <summary>
        /// Gets the proxy url of the thumbnail.
        /// </summary>
        public string ProxyUrl { get; }
        /// <summary>
        /// Gets the pixel-width of the thumbnail.
        /// </summary>
        public int Width { get; }
        /// <summary>
        /// Gets the pixel-height of the thumbnail.
        /// </summary>
        public int Height { get; }

        internal DiscordEmbedThumbnail(DiscordApiData data)
        {
            Url = data.GetString("url");
            ProxyUrl = data.GetString("proxy_url");
            Width = data.GetInteger("width").Value;
            Height = data.GetInteger("height").Value;
        }

        internal DiscordApiData Serialize()
        {
            DiscordApiData data = DiscordApiData.CreateContainer();
            data.Set("url", Url);
            data.Set("proxy_url", ProxyUrl);
            data.Set("width", Width);
            data.Set("height", Height);
            return data;
        }

        public override string ToString()
        {
            return Url;
        }
    }
}
