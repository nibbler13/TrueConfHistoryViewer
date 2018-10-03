using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrueConfHistoryViewer {
	public class CallRecords {
		[JsonProperty("cnt")]
		public int Count { get; set; }

		[JsonProperty("list")]
		public List<CallRecord> CallRecordsList { get; set; }

		public class CallRecord : INotifyPropertyChanged {
			public event PropertyChangedEventHandler PropertyChanged;
			private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}

			public CallParticipants CallParticipantsObject { get; set; }

			//Structure

			//	Field	Type	Description
			//	conference_id	String	
			//	Identifier of call session.

			//	name	String	
			//	Video file name.

			//	named_conf_id	String	
			//	Identifier of conference. It can be null. GET

			//	topic	String	
			//	Topic of conference. It can be null.

			//	owner	String	
			//	Owner of conference. GET

			//	class	Integer	
			//	Class of call session.

			//	type	Integer	
			//	Type of call session.

			//	subtype	Integer	
			//	Subtype of call session.

			//	duration	Integer	
			//	Duration of call session in seconds.

			//	file_size	String	
			//	Video file size in bytes.

			//	start_time	ObjectDataTime	
			//	Call session start time. It can be null.

			//	end_time	ObjectDataTime	
			//	Call session end time. It can be null.

			//	is_public	Boolean	
			//	Flag signaling the allowed public access to conference.

			//	download_url	String	
			//	Video file download link.

			//	is_deleted	Boolean	
			//	Flag signaling the the absence of file in current records directory. You can see records directory on page 'Recordings' of Web-configuration.

			[JsonProperty("conference_id")]
			public string ConferenceId { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("named_conf_id")]
			public string NamedConfId { get; set; }

			[JsonProperty("topic")]
			public string Topic { get; set; }

			private string owner;
			[JsonProperty("owner")]
			public string Owner {
				get {
					return owner;
				}
				set {
					if (value != owner) {
						owner = value;
						OwnerText = Owner.Replace("@ruh93.trueconf.name", "");
					}
				}
			}

			[JsonProperty("class")]
			public int Class { get; set; }

			[JsonProperty("type")]
			public int Type { get; set; }

			[JsonProperty("subtype")]
			public int Subtype { get; set; }

			[JsonProperty("duration")]
			public long Duration { get; set; }

			[JsonProperty("file_size")]
			public long FileSize { get; set; }

			[JsonProperty("start_time")]
			public DateTimeInfo StartTime { get; set; }

			[JsonProperty("end_time")]
			public DateTimeInfo EndTime { get; set; }

			[JsonProperty("is_public")]
			public bool IsPublic { get; set; }

			[JsonProperty("download_url")]
			public string DownloadUrl { get; set; }

			[JsonProperty("is_deleted")]
			public bool IsDeleted { get; set; }

			private string participantCount;
			[JsonProperty("participant_count")]
			public string ParticipantCount {
				get {
					return string.IsNullOrEmpty(participantCount) ? "Неизвестно" : participantCount;
				}
				set {
					if (value != participantCount) {
						participantCount = value;
						NotifyPropertyChanged();
					}
				}
			}

			private string participantList;
			public string ParticipantList {
				get {
					return participantList;
				} 
				set {
					if (value != participantList) {
						participantList = value;
						NotifyPropertyChanged();
					}
				}
			}

			public string DateTimeBegin {
				get {
					return StartTime.Date.HasValue ? StartTime.Date.Value.AddHours(3).ToString("yyyy.MM.dd HH:mm:ss") : string.Empty;
				}
			}

			private string ownerText;
			public string OwnerText {
				get {
					return ownerText;
				}
				set {
					if (value != ownerText) {
						ownerText = value;
						NotifyPropertyChanged();
					}
				}
			}

			public string DurationText {
				get {
					return TimeSpan.FromSeconds(Duration).ToString();
				}
			}

			public string FileSizeText {
				get {
					string[] sizes = { "B", "KB", "MB", "GB", "TB" };
					double len = FileSize;
					int order = 0;
					for (int i = 0; i < 2; i++) {
						order++;
						len = len / 1024;
					}
					//while (len >= 1024 && order < sizes.Length - 1) {
					//	order++;
					//	len = len / 1024;
					//}

					// Adjust the format string to your preferences. For example "{0:0.#}{1}" would
					// show a single decimal place, and no space.
					return String.Format("{0:0.##} {1}", len, sizes[order]);
				}
			}

			public class DateTimeInfo {
				[JsonProperty("date")]
				public DateTime? Date { get; set; }

				[JsonProperty("timezone_type")]
				public int TimezoneType { get; set; }

				[JsonProperty("timezone")]
				public string Timezone { get; set; }
			}
		}
	}
}
