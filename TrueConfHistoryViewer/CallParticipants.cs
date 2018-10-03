using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrueConfHistoryViewer {
	public class CallParticipants {
		[JsonProperty("cnt")]
		public int Count { get; set; }

		[JsonProperty("list")]
		public List<CallParticipant> CallParticipantList { get; set; }

		public class CallParticipant : INotifyPropertyChanged {
			public event PropertyChangedEventHandler PropertyChanged;
			private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}

			//Structure
			//Field   Type Description
			//app_id String
			//Identifier of client application.

			//avg_cpu_load    Integer
			//Average CPU load of call participant endpoint in percent during a call.

			//avg_sent_fps Integer
			//Average sent frames count per second of call participant endpoint.

			//bitrate_in Integer
			//Bit rate of incoming traffic to call participant endpoint.

			//bitrate_out Integer
			//Bit rate of outgoing traffic from call participant endpoint.

			//call_id String
			//Identifier of call participant.

			//display_name String
			//Display name of call participant.

			//duration Integer
			//Call sessions duration in Unix timestamp.

			//join_time ObjectDataTime
			//Call sessions join time. It can be null.

			//leave_reason Integer
			//Call sessions join time.

			//leave_time ObjectDataTime
			//Call sessions leave time. It can be null.

			//video_h Integer
			//Height of video frames from call participant camera during a call.

			//video_w Integer
			//Width of video frames from call participant camera during a call.

			[JsonProperty("app_id")]
			public string AppId { get; set; }

			[JsonProperty("avg_cpu_load")]
			public int? AvgCpuLoad { get; set; }

			[JsonProperty("avg_sent_fps")]
			public double? AvgSentFps { get; set; }

			[JsonProperty("bitrate_in")]
			public int? BitrateIn { get; set; }

			[JsonProperty("bitrate_out")]
			public int? BitrateOut { get; set; }

			[JsonProperty("call_id")]
			public string CallId { get; set; }

			[JsonProperty("display_name")]
			public string DisplayName { get; set; }

			[JsonProperty("duration")]
			public int Duration { get; set; }

			[JsonProperty("join_time")]
			public CallRecords.CallRecord.DateTimeInfo JoinTime { get; set; }

			[JsonProperty("leave_reason")]
			public int LeaveReason { get; set; }

			[JsonProperty("leave_time")]
			public CallRecords.CallRecord.DateTimeInfo LeaveTime { get; set; }

			[JsonProperty("video_h")]
			public int? VideoH { get; set; }

			[JsonProperty("video_w")]
			public int? VideoW { get; set; }
		}
	}
}
