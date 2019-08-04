﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbchat_sharp.API
{
    /// <summary>
    /// Facebook messenger group class
    /// </summary>
    public class FB_Group : FB_Thread
    {
        /// Unique list (set) of the group thread"s participant user IDs
        public ISet<string> participants { get; set; }
        /// Dict, containing user nicknames mapped to their IDs
        public Dictionary<string, string> nicknames { get; set; }
        /// A `ThreadColor`. The groups"s message color
        public string color { get; set; }
        /// The groups"s default emoji
        public JToken emoji { get; set; }
        /// Set containing user IDs of thread admins
        public ISet<string> admins { get; set; }
        /// True if users need approval to join
        public bool approval_mode { get; set; }
        /// Set containing user IDs requesting to join
        public ISet<string> approval_requests { get; set; }
        /// Link for joining group
        public string join_link { get; set; }

        /// <summary>
        /// Represents a Facebook group. Inherits `Thread`
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="photo"></param>
        /// <param name="name"></param>
        /// <param name="message_count"></param>
        /// <param name="last_message_timestamp"></param>
        /// <param name="plan"></param>
        /// <param name="participants"></param>
        /// <param name="nicknames"></param>
        /// <param name="color"></param>
        /// <param name="emoji"></param>
        /// <param name="admins"></param>
        /// <param name="approval_mode"></param>
        /// <param name="approval_requests"></param>
        /// <param name="join_link"></param>
        public FB_Group(string uid, string photo = null, string name = null, int message_count = 0, string last_message_timestamp = null, FB_Plan plan = null, ISet<string> participants = null, Dictionary<string, string> nicknames = null, string color = null, JToken emoji = null, ISet<string> admins = null, bool approval_mode = false, ISet<string> approval_requests = null, string join_link = null)
            : base(ThreadType.GROUP, uid, photo, name, message_count: message_count, last_message_timestamp: last_message_timestamp, plan: plan)
        {
            this.participants = participants ?? new HashSet<string>();
            this.nicknames = nicknames ?? new Dictionary<string, string>();
            this.color = color;
            this.emoji = emoji;
            this.admins = admins ?? new HashSet<string>();
            this.approval_mode = approval_mode;
            this.approval_requests = approval_requests ?? new HashSet<string>();
            this.join_link = join_link;
        }

        public static FB_Group _from_graphql(JToken data)
        {
            if (data["image"] == null || data["image"].Type == JTokenType.Null)
                data["image"] = new JObject(new JProperty("uri", ""));
            var c_info = FB_Group._parse_customization_info(data);

            var last_message_timestamp = data["last_message"]?["nodes"]?.FirstOrDefault()?["timestamp_precise"]?.Value<string>();
            var plan = data["event_reminders"]?["nodes"]?.FirstOrDefault() != null ? FB_Plan._from_graphql(data["event_reminders"]?["nodes"]?.FirstOrDefault()) : null;

            return new FB_Group(
                uid: data["thread_key"]?["thread_fbid"]?.Value<string>(),
                participants: new HashSet<string>(data["all_participants"]?["nodes"]?.Select(node => node["messaging_actor"]?["id"]?.Value<string>())),
                nicknames: (Dictionary<string, string>)c_info.GetValueOrDefault("nicknames"),
                color: (string)c_info.GetValueOrDefault("color"),
                emoji: (JToken)c_info.GetValueOrDefault("emoji"),
                admins: new HashSet<string>(data["thread_admins"]?.Select(node => node["id"]?.Value<string>())),
                approval_mode: data["approval_mode"]?.Value<bool>() ?? false,
                approval_requests: data["group_approval_queue"] != null ? new HashSet<string>(data["group_approval_queue"]?["nodes"]?.Select(node => node["requester"]?["id"]?.Value<string>())) : null,
                photo: data["image"]?["uri"]?.Value<string>(),
                name: data["name"]?.Value<string>(),
                message_count: data["messages_count"]?.Value<int>() ?? 0,
                last_message_timestamp: last_message_timestamp,
                plan: plan);
        }
    }

    /// <summary>
    /// Represents a Facebook room. Inherits `Group`
    /// </summary>
    public class FB_Room : FB_Group
    {
        /// True is room is not discoverable
        public bool privacy_mode { get; set; }

        /// <summary>
        /// Represents a Facebook room. Inherits `Group`
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="photo"></param>
        /// <param name="name"></param>
        /// <param name="message_count"></param>
        /// <param name="last_message_timestamp"></param>
        /// <param name="plan"></param>
        /// <param name="participants"></param>
        /// <param name="nicknames"></param>
        /// <param name="color"></param>
        /// <param name="emoji"></param>
        /// <param name="admins"></param>
        /// <param name="approval_mode"></param>
        /// <param name="approval_requests"></param>
        /// <param name="join_link"></param>
        /// <param name="privacy_mode"></param>
        public FB_Room(string uid, string photo = null, string name = null, int message_count = 0, string last_message_timestamp = null, FB_Plan plan = null, ISet<string> participants = null, Dictionary<string, string> nicknames = null, string color = null, string emoji = null, ISet<string> admins = null, bool approval_mode = false, ISet<string> approval_requests = null, string join_link = null, bool privacy_mode = false) 
            : base(uid, photo, name, message_count, last_message_timestamp, plan, participants, nicknames, color, emoji, admins, approval_mode, approval_requests, join_link)
        {
            this.type = ThreadType.ROOM;
            this.privacy_mode = privacy_mode;
        }
    }
}
