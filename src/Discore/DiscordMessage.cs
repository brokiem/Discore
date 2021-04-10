using System;
using System.Collections.Generic;
using System.Text.Json;

#nullable enable

namespace Discore
{
    /// <summary>
    /// Represents a message sent in a channel within Discord.
    /// </summary>
    public sealed class DiscordMessage : DiscordIdEntity
    {
        public const int MAX_CHARACTERS = 2000;

        /// <summary>
        /// Gets the ID of the channel this message is in.
        /// </summary>
        public Snowflake ChannelId { get; }
        /// <summary>
        /// Gets the author of this message.
        /// </summary>
        public DiscordUser Author { get; }
        /// <summary>
        /// If this message originated from a guild, gets the member properties of the author.
        /// <para/>
        /// Only available if this message originated from a MessageCreate or MessageUpdate Gateway event.
        /// </summary>
        public DiscordMessageMember? Member { get; }
        /// <summary>
        /// Gets the contents of this message.
        /// </summary>
        public string Content { get; }
        /// <summary>
        /// Gets the time this message was first sent.
        /// </summary>
        public DateTime Timestamp { get; }
        /// <summary>
        /// Gets the time of the last edit to this message.
        /// </summary>
        public DateTime? EditedTimestamp { get; }
        /// <summary>
        /// Gets whether or not this message was sent with the /tts command.
        /// </summary>
        public bool TextToSpeech { get; }
        /// <summary>
        /// Gets whether or not this message mentioned everyone via @everyone.
        /// </summary>
        public bool MentionEveryone { get; }
        /// <summary>
        /// Gets a list of all user-specific mentions in this message.
        /// </summary>
        public IReadOnlyList<DiscordUser> Mentions { get; }
        /// <summary>
        /// Gets a list of all the IDs of mentioned roles in this message.
        /// </summary>
        public IReadOnlyList<Snowflake> MentionedRoleIds { get; }
        /// <summary>
        /// Gets a list of all channels mentioned in this message.
        /// May be null.
        /// <para/>
        /// Note: This will only ever be set for crossposted messages.
        /// </summary>
        public IReadOnlyList<DiscordChannelMention>? MentionedChannels { get; }
        /// <summary>
        /// Gets a list of all attachments in this message.
        /// </summary>
        public IReadOnlyList<DiscordAttachment> Attachments { get; }
        /// <summary>
        /// Gets a list of all embedded attachments in this message.
        /// </summary>
        public IReadOnlyList<DiscordEmbed> Embeds { get; }
        /// <summary>
        /// Gets a list of all reactions to this message.
        /// </summary>
        public IReadOnlyList<DiscordReaction>? Reactions { get; }
        /// <summary>
        /// Used for validating if a message was sent.
        /// </summary>
        public Snowflake? Nonce { get; }
        /// <summary>
        /// Gets whether or not this message is pinned in the containing channel.
        /// </summary>
        public bool IsPinned { get; }
        /// <summary>
        /// If this message was generated by a webhook, gets the ID of that webhook.
        /// </summary>
        public Snowflake? WebhookId { get; }
        /// <summary>
        /// Gets the type of message.
        /// </summary>
        public DiscordMessageType Type { get; }
        /// <summary>
        /// Gets the activity information for a Rich Presence-related chat message.
        /// </summary>
        public DiscordMessageActivity? Activity { get; }
        /// <summary>
        /// Gets the application information for a Rich Persence-related chat message.
        /// </summary>
        public DiscordMessageApplication? Application { get; }
        /// <summary>
        /// Gets the reference data sent with crossposted messages.
        /// </summary>
        public DiscordMessageReference? MessageReference { get; }
        /// <summary>
        /// Flags describing extra features of the message.
        /// </summary>
        public DiscordMessageFlags Flags { get; }

        // TODO: add guild_id, mentions.member, stickers, referenced_message, interaction

        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="author"/>, <paramref name="content"/>,
        /// <paramref name="mentionedRoleIds"/>, <paramref name="attachments"/>,
        /// or <paramref name="embeds"/> is null.
        /// </exception>
        public DiscordMessage(
            Snowflake id,
            Snowflake channelId, 
            DiscordUser author, 
            DiscordMessageMember? member, 
            string content, 
            DateTime timestamp, 
            DateTime? editedTimestamp, 
            bool textToSpeech, 
            bool mentionEveryone, 
            IReadOnlyList<DiscordUser> mentions, 
            IReadOnlyList<Snowflake> mentionedRoleIds, 
            IReadOnlyList<DiscordChannelMention>? mentionedChannels, 
            IReadOnlyList<DiscordAttachment> attachments, 
            IReadOnlyList<DiscordEmbed> embeds, 
            IReadOnlyList<DiscordReaction>? reactions, 
            Snowflake? nonce, 
            bool isPinned, 
            Snowflake? webhookId, 
            DiscordMessageType type, 
            DiscordMessageActivity? activity, 
            DiscordMessageApplication? application, 
            DiscordMessageReference? messageReference, 
            DiscordMessageFlags flags)
            : base(id)
        {
            ChannelId = channelId;
            Author = author ?? throw new ArgumentNullException(nameof(author));
            Member = member;
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Timestamp = timestamp;
            EditedTimestamp = editedTimestamp;
            TextToSpeech = textToSpeech;
            MentionEveryone = mentionEveryone;
            Mentions = mentions ?? throw new ArgumentNullException(nameof(mentions));
            MentionedRoleIds = mentionedRoleIds ?? throw new ArgumentNullException(nameof(mentionedRoleIds));
            MentionedChannels = mentionedChannels;
            Attachments = attachments ?? throw new ArgumentNullException(nameof(attachments));
            Embeds = embeds ?? throw new ArgumentNullException(nameof(embeds));
            Reactions = reactions;
            Nonce = nonce;
            IsPinned = isPinned;
            WebhookId = webhookId;
            Type = type;
            Activity = activity;
            Application = application;
            MessageReference = messageReference;
            Flags = flags;
        }

        internal DiscordMessage(JsonElement json)
            : base(json)
        {
            ChannelId = json.GetProperty("channel_id").GetSnowflake();
            Content = json.GetProperty("content").GetString()!;
            Timestamp = json.GetProperty("timestamp").GetDateTime();
            EditedTimestamp = json.GetPropertyOrNull("edited_timestamp")?.GetDateTimeOrNull();
            TextToSpeech = json.GetProperty("tts").GetBoolean();
            MentionEveryone = json.GetProperty("mention_everyone").GetBoolean();
            Nonce = json.GetPropertyOrNull("nonce")?.GetSnowflake();
            IsPinned = json.GetProperty("pinned").GetBoolean();
            WebhookId = json.GetPropertyOrNull("webhook_id")?.GetSnowflake();
            Type = (DiscordMessageType)json.GetProperty("type").GetInt32();
            Flags = (DiscordMessageFlags)(json.GetPropertyOrNull("flags")?.GetInt32() ?? 0);

            // Author
            Author = new DiscordUser(json.GetProperty("author"), isWebhookUser: WebhookId != null);

            // Member
            JsonElement? memberJson = json.GetPropertyOrNull("member");
            Member = memberJson == null ? null : new DiscordMessageMember(memberJson.Value);

            // Activity
            JsonElement? activityJson = json.GetPropertyOrNull("activity");
            Activity = activityJson == null ? null : new DiscordMessageActivity(activityJson.Value);

            // Application
            JsonElement? applicationJson = json.GetPropertyOrNull("application");
            Application = applicationJson == null ? null : new DiscordMessageApplication(applicationJson.Value);

            // Message reference
            JsonElement? messageReferenceJson = json.GetPropertyOrNull("message_reference");
            MessageReference = messageReferenceJson == null ? null : new DiscordMessageReference(messageReferenceJson.Value);

            // Mentions
            JsonElement mentionsJson = json.GetProperty("mentions");
            var mentions = new DiscordUser[mentionsJson.GetArrayLength()];

            for (int i = 0; i < mentions.Length; i++)
                mentions[i] = new DiscordUser(mentionsJson[i], isWebhookUser: false);

            Mentions = mentions;

            // Mentioned roles
            JsonElement mentionRolesJson = json.GetProperty("mention_roles");
            var mentionRoles = new Snowflake[mentionRolesJson.GetArrayLength()];

            for (int i = 0; i < mentionRoles.Length; i++)
                mentionRoles[i] = mentionRolesJson[i].GetSnowflake();

            MentionedRoleIds = mentionRoles;

            // Mentioned channels
            JsonElement? mentionChannelsJson = json.GetPropertyOrNull("mention_channels");

            if (mentionChannelsJson != null)
            {
                JsonElement _mentionChannelsJson = mentionChannelsJson.Value;
                var mentionChannels = new DiscordChannelMention[_mentionChannelsJson.GetArrayLength()];

                for (int i = 0; i < mentionChannels.Length; i++)
                    mentionChannels[i] = new DiscordChannelMention(_mentionChannelsJson[i]);

                MentionedChannels = mentionChannels;
            }

            // Attachments
            JsonElement attachmentsJson = json.GetProperty("attachments");
            var attachments = new DiscordAttachment[attachmentsJson.GetArrayLength()];

            for (int i = 0; i < attachments.Length; i++)
                attachments[i] = new DiscordAttachment(attachmentsJson[i]);

            Attachments = attachments;

            // Embeds
            JsonElement embedsJson = json.GetProperty("embeds");
            var embeds = new DiscordEmbed[embedsJson.GetArrayLength()];

            for (int i = 0; i < embeds.Length; i++)
                embeds[i] = new DiscordEmbed(embedsJson[i]);

            Embeds = embeds;

            // Reactions
            JsonElement? reactionsJson = json.GetPropertyOrNull("reactions");

            if (reactionsJson != null)
            {
                JsonElement _reactionsJson = reactionsJson.Value;
                var reactions = new DiscordReaction[_reactionsJson.GetArrayLength()];

                for (int i = 0; i < reactions.Length; i++)
                    reactions[i] = new DiscordReaction(_reactionsJson[i]);

                Reactions = reactions;
            }
        }

        /// <summary>
        /// Updates a message with a partial version of the same message. 
        /// <para/>
        /// This is primarily used for obtaining the full message from a message update event, 
        /// which only supplies the changes rather than the full message.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the IDs of each message do not match.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static DiscordMessage Update(DiscordMessage message, DiscordPartialMessage withPartial)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (withPartial == null)
                throw new ArgumentNullException(nameof(withPartial));

            if (message.Id != withPartial.Id)
                throw new ArgumentException("Cannot update a message with a different message. The message IDs must match.");

            return new DiscordMessage(
                id:                 message.Id,
                channelId:          message.ChannelId,
                author:             withPartial.Author ?? message.Author,
                member:             withPartial.Member ?? message.Member,
                content:            withPartial.Content ?? message.Content,
                timestamp:          withPartial.Timestamp ?? message.Timestamp,
                editedTimestamp:    withPartial.EditedTimestamp ?? message.EditedTimestamp,
                textToSpeech:       withPartial.TextToSpeech ?? message.TextToSpeech,
                mentionEveryone:    withPartial.MentionEveryone ?? message.MentionEveryone,
                mentions:           withPartial.Mentions ?? message.Mentions,
                mentionedRoleIds:   withPartial.MentionedRoleIds ?? message.MentionedRoleIds,
                mentionedChannels:  withPartial.MentionedChannels ?? message.MentionedChannels,
                attachments:        withPartial.Attachments ?? message.Attachments,
                embeds:             withPartial.Embeds ?? message.Embeds,
                reactions:          withPartial.Reactions ?? message.Reactions,
                nonce:              withPartial.Nonce ?? message.Nonce,
                isPinned:           withPartial.IsPinned ?? message.IsPinned,
                webhookId:          withPartial.WebhookId ?? message.WebhookId,
                type:               withPartial.Type ?? message.Type,
                activity:           withPartial.Activity ?? message.Activity,
                application:        withPartial.Application ?? message.Application,
                messageReference:   withPartial.MessageReference ?? message.MessageReference,
                flags:              withPartial.Flags ?? message.Flags);
        }
    }
}

#nullable restore
