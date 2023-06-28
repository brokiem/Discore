using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0060 // Remove unused parameter

namespace Discore.WebSocket.Internal
{
    partial class GatewaySocket
    {
        static readonly Random rnd = new Random();

        #region Receiving
        [Payload(GatewayOPCode.Dispatch)]
        void HandleDispatchPayload(JsonElement payload, JsonElement data)
        {
            sequence = payload.GetProperty("s").GetInt32();
            string eventName = payload.GetProperty("t").GetString()!;

            OnDispatch?.Invoke(this, new DispatchEventArgs(eventName, data));
        }

        [Payload(GatewayOPCode.Hello)]
        async Task HandleHelloPayload(JsonElement payload, JsonElement data)
        {
            if (!receivedHello)
            {
                receivedHello = true;

                // Set heartbeat interval
                heartbeatInterval = data.GetProperty("heartbeat_interval").GetInt32();
                log.LogVerbose($"[Hello] heartbeat_interval = {heartbeatInterval}ms");

                // Begin heartbeat loop
                heartbeatCancellationSource = new CancellationTokenSource();
                heartbeatTask = HeartbeatLoop();

                // Notify so the IDENTIFY or RESUME payloads are sent
                if (OnHello != null)
                    await OnHello.Invoke();
            }
            else
                log.LogWarning("Received more than one HELLO payload.");
        }

        [Payload(GatewayOPCode.Heartbeat)]
        async Task HandleHeartbeatPayload(JsonElement payload, JsonElement data)
        {
            // The gateway can request a heartbeat in certain (unlisted) scenarios.
            log.LogVerbose("[Heartbeat] Gateway requested heartbeat.");

            await SendHeartbeatPayload();
        }

        [Payload(GatewayOPCode.HeartbeatAck)]
        void HandleHeartbeatAckPayload(JsonElement payload, JsonElement data)
        {
            receivedHeartbeatAck = true;
        }

        [Payload(GatewayOPCode.Reconnect)]
        void HandleReconnectPayload(JsonElement payload, JsonElement data)
        {
            // Resume
            log.LogInfo("[Reconnect] Performing resume...");
            OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(false));
        }

        [Payload(GatewayOPCode.InvalidSession)]
        void HandleInvalidSessionPayload(JsonElement payload, JsonElement data)
        {
            bool isResumable = data.GetBoolean();

            if (isResumable)
            {
                // Resume
                log.LogInfo("[InvalidSession] Resuming...");
                OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(false, 5000));
            }
            else
            {
                // Start new session
                log.LogInfo("[InvalidSession] Starting new session...");
                OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(true, rnd.Next(1000, 5001)));
            }
        }
        #endregion

        #region Sending
        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        async Task SendPayload(GatewayOPCode op, Action<Utf8JsonWriter> builder)
        {
            // TODO: There must be a more memory efficient way of doing this

            // Create payload bytes
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            writer.WriteStartObject();

            writer.WriteNumber("op", (int)op);
            writer.WritePropertyName("d");
            builder(writer);

            writer.WriteEndObject();

            writer.Flush();

            byte[] payload = stream.ToArray();

            // Check with the payload rate limiter
            await outboundPayloadRateLimiter.Invoke().ConfigureAwait(false);
            // Send payload
            await SendAsync(payload).ConfigureAwait(false);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        Task SendHeartbeatPayload()
        {
            return SendPayload(GatewayOPCode.Heartbeat, writer => writer.WriteNumberValue(sequence));
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public async Task SendIdentifyPayload(
            string token,
            GatewayIntent intents,
            int? largeThreshold, 
            int shardId, 
            int totalShards)
        {
            void BuildPayload(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();

                writer.WriteString("token", token);
                writer.WriteBoolean("compress", true);
                /*writer.WriteNumber("intents", (int)intents);

                if (largeThreshold != null)
                {
                    writer.WriteNumber("large_threshold", largeThreshold.Value);
                }

                if (totalShards > 1)
                {
                    writer.WriteStartArray("shard");
                    writer.WriteNumberValue(shardId);
                    writer.WriteNumberValue(totalShards);
                    writer.WriteEndArray();
                }*/

                // TODO: don't hardcode this
                writer.WriteStartObject("properties");
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
                writer.WriteEndObject();

                writer.WriteStartObject("client_state");
                writer.WriteNumber("api_code_version", 0);
                writer.WriteStartObject("guild_versions");
                writer.WriteEndObject();
                writer.WriteString("highest_last_message_id", "0");
                writer.WriteString("private_channels_version", "0");
                writer.WriteNumber("read_state_version", 0);
                writer.WriteNumber("user_guild_settings_version", -1);
                writer.WriteNumber("user_settings_version", -1);
                writer.WriteEndObject();

                writer.WriteNumber("capabilities", 8189);

                writer.WriteStartObject("presence");
                writer.WriteString("status", "unknown");
                writer.WriteNumber("since", 0);
                writer.WriteStartArray("activities");
                writer.WriteEndArray();
                writer.WriteBoolean("afk", false);
                writer.WriteEndObject();

                writer.WriteEndObject();
            }

            log.LogVerbose("[Identify] Sending payload...");

            // Make sure we don't send IDENTIFY's too quickly
            await identifyRateLimiter.Invoke(CancellationToken.None).ConfigureAwait(false);
            // Send IDENTIFY
            await SendPayload(GatewayOPCode.Identify, BuildPayload).ConfigureAwait(false);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendResumePayload(string token, string sessionId, int sequence)
        {
            void BuildPayload(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();

                writer.WriteString("token", token);
                writer.WriteString("session_id", sessionId);
                writer.WriteNumber("seq", sequence);

                writer.WriteEndObject();
            }

            log.LogVerbose("[Resume] Sending payload...");

            return SendPayload(GatewayOPCode.Resume, BuildPayload);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public async Task SendPresenceUpdate(PresenceOptions options)
        {
            void BuildPayload(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();

                writer.WriteNumber("since", options.AfkSince);
                writer.WriteBoolean("afk", options.Afk);
                writer.WriteString("status", Utils.UserStatusToString(options.Status) ?? "online");

                writer.WriteStartArray("activities");
                if (options.Activities != null)
                {
                    foreach (ActivityOptions activity in options.Activities)
                    {
                        writer.WriteStartObject();
                        writer.WriteString("name", activity.Name);
                        writer.WriteNumber("type", (int)activity.Type);
                        writer.WriteString("url", activity.Url);
                        writer.WriteEndObject();
                    }
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
            }

            // Check with the presence update limiter
            await presenceUpdateRateLimiter.Invoke().ConfigureAwait(false);
            // Send status update
            await SendPayload(GatewayOPCode.PresenceUpdate, BuildPayload).ConfigureAwait(false);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendRequestGuildMembersPayload(Snowflake guildId, string query, int limit)
        {
            void BuildPayload(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();

                writer.WriteSnowflake("guild_id", guildId);
                writer.WriteString("query", query);
                writer.WriteNumber("limit", limit);

                writer.WriteEndObject();
            }

            return SendPayload(GatewayOPCode.RequestGuildMembers, BuildPayload);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendVoiceStateUpdatePayload(Snowflake guildId, Snowflake? channelId, bool isMute, bool isDeaf)
        {
            void BuildPayload(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();

                writer.WriteSnowflake("guild_id", guildId);
                writer.WriteSnowflake("channel_id", channelId);
                writer.WriteBoolean("self_mute", isMute);
                writer.WriteBoolean("self_deaf", isDeaf);

                writer.WriteEndObject();
            }

            return SendPayload(GatewayOPCode.VoiceStateUpdate, BuildPayload);
        }
        #endregion
    }
}

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0051 // Remove unused private members
