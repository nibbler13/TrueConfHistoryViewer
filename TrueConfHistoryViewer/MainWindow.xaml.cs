using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace TrueConfHistoryViewer {
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public ObservableCollection<CallRecords.CallRecord> CallRecordsCollection { get; set; } = 
			new ObservableCollection<CallRecords.CallRecord>();

		public MainWindow() {
			InitializeComponent();
			DateGridCallRecords.DataContext = this;
			DatePickerBegin.SelectedDate = DateTime.Now;
			DatePickerEnd.SelectedDate = DateTime.Now;
		}

		private async void ButtonSearch_Click(object sender, RoutedEventArgs e) {
			ButtonSearch.IsEnabled = false;
			ButtonExportToExcel.IsEnabled = false;
			CallRecordsCollection.Clear();

			if (CheckBoxDateBegin.IsChecked == true && DatePickerBegin.SelectedDate == null) {
				MessageBox.Show(this, "Необходимо выбрать дату начала", "", MessageBoxButton.OK, MessageBoxImage.Warning);
				ButtonSearch.IsEnabled = true;
				return;
			}

			if (CheckBoxDateEnd.IsChecked == true && DatePickerEnd.SelectedDate == null) {
				MessageBox.Show(this, "Необходимо выбрать дату окончания", "", MessageBoxButton.OK, MessageBoxImage.Warning);
				ButtonSearch.IsEnabled = true;
				return;
			}

			HttpClient httpClient = new HttpClient();
			string rootUrl = "https://portal2.bzklinika.ru";
			string secretKey = @"46fVt9rjbee:yXMJ_hh:3PmkaYL3noXX";
			string apiGetRecordList = "/api/v3.1/logs/records?access_token=" + secretKey;
			string apiGetCall = "/api/v3.1/logs/calls/{{$call_id}}?access_token=" + secretKey;
			string apiGetCallParticipantList = "/api/v3.1/logs/calls/{{$call_id}}/participants?access_token=" + secretKey;
			
			ServicePointManager.Expect100Continue = false;
			string url = rootUrl + apiGetRecordList;
			string data = "&timezone=3&page_size=1000";

			if (CheckBoxDateBegin.IsChecked == true)
				data += "&date_from=" + DatePickerBegin.SelectedDate.Value.ToString("yyyy-MM-dd+00:00:00");

			if (CheckBoxDateEnd.IsChecked == true)
				data += "&date_to=" + DatePickerEnd.SelectedDate.Value.ToString("yyyy-MM-dd+23:59:59");
			
			Uri uri = new Uri(url + data);

			try {
				HttpResponseMessage response = await httpClient.GetAsync(uri);
				response.EnsureSuccessStatusCode();
				string jsonString = await response.Content.ReadAsStringAsync();

				CallRecords callRecords = JsonConvert.DeserializeObject<CallRecords>(jsonString);
				callRecords.CallRecordsList.ForEach(CallRecordsCollection.Add);

				if (callRecords.Count > 1000)
					MessageBox.Show(this, "Отображены только первые 1000 результатов поиска, " +
						"для сужения выборки воспользуйтесь заданием подходящих временных интервалов", 
						"", MessageBoxButton.OK, MessageBoxImage.Information);
			} catch (Exception exc) {
				MessageBox.Show(this, exc.Message + Environment.NewLine + exc.StackTrace, 
					"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			Dictionary<string, string> dict = new Dictionary<string, string>();
			using (PrincipalContext context = new PrincipalContext(ContextType.Domain)) {
				foreach (CallRecords.CallRecord item in CallRecordsCollection) {
					if (dict.ContainsKey(item.OwnerText)) {
						item.OwnerText = dict[item.OwnerText];
					} else {
						UserPrincipal usr = UserPrincipal.FindByIdentity(context, item.OwnerText);
						if (usr != null) {
							dict.Add(item.OwnerText, usr.DisplayName);
							item.OwnerText = usr.DisplayName;
						}
					}
				}
			}

			ButtonExportToExcel.IsEnabled = CallRecordsCollection.Count > 0;
			ButtonSearch.IsEnabled = true;

			if (CallRecordsCollection.Count < 200)
				foreach (CallRecords.CallRecord item in CallRecordsCollection) {
					//string urlGetCall = rootUrl + apiGetCall.Replace("{{$call_id}}", Uri.EscapeDataString(item.ConferenceId));
					//Uri uriGetCall = new Uri(urlGetCall);

					//try {
					//	HttpResponseMessage response = await httpClient.GetAsync(uriGetCall);
					//	response.EnsureSuccessStatusCode();
					//	string jsonString = await response.Content.ReadAsStringAsync();

					//	CallRecords.CallRecord callRecord = JsonConvert.DeserializeObject<CallRecords.CallRecord>(jsonString);
					//	item.ParticipantCount = callRecord.ParticipantCount;
					//} catch (Exception exc) {
					//	MessageBox.Show(this, exc.Message + Environment.NewLine + exc.StackTrace,
					//		"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
					//}


					string urlGetCallParticipantList = rootUrl + apiGetCallParticipantList.Replace("{{$call_id}}", Uri.EscapeDataString(item.ConferenceId));
					Uri uriGetCallParticipantList = new Uri(urlGetCallParticipantList);

					try {
						HttpResponseMessage response = await httpClient.GetAsync(uriGetCallParticipantList);
						response.EnsureSuccessStatusCode();
						string jsonString = await response.Content.ReadAsStringAsync();

						CallParticipants callParticipants = JsonConvert.DeserializeObject<CallParticipants>(jsonString);
						item.CallParticipantsObject = callParticipants;
						item.ParticipantCount = callParticipants.Count.ToString();
						string participantList = string.Empty;
						foreach (CallParticipants.CallParticipant callParticipant in callParticipants.CallParticipantList)
							participantList += callParticipant.CallId + " | " + callParticipant.DisplayName + Environment.NewLine;
						if (participantList.EndsWith(Environment.NewLine))
							participantList = participantList.Substring(0, participantList.Length - Environment.NewLine.Length);
						item.ParticipantList = participantList;
					} catch (Exception exc) {
						MessageBox.Show(this, exc.Message + Environment.NewLine + exc.StackTrace,
							"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				} 
			else
				MessageBox.Show(this, "В результатах запроса более 200 записей, для определения " +
					"количества участников необходимо уменьшить диапазон времени выборки и повторить поиск", 
					"", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void ButtonExportToExcel_Click(object sender, RoutedEventArgs e) {
			try {

				string fileName = "История звонков TrueConf";
				if (CheckBoxDateBegin.IsChecked == true)
					fileName += " с " + DatePickerBegin.SelectedDate.Value.ToShortDateString();

				if (CheckBoxDateEnd.IsChecked == true)
					fileName += " по " + DatePickerEnd.SelectedDate.Value.ToShortDateString();

				if (!CreateNewIWorkbook(fileName, out IWorkbook workbook, out ISheet sheet, out string resultFile))
					return;


				int rowNumber = 1;
				int columnNumber = 0;

				foreach (CallRecords.CallRecord callRecord in CallRecordsCollection) {
					IRow row = sheet.CreateRow(rowNumber);

					string[] values = new string[] {
					callRecord.ConferenceId ?? "",
					callRecord.ParticipantCount ?? "",
					callRecord.ParticipantList ?? "",
					callRecord.Name ?? "",
					callRecord.NamedConfId ?? "",
					callRecord.Topic ?? "",
					callRecord.Owner ?? "",
					callRecord.OwnerText ?? "",
					callRecord.Class.ToString() ?? "",
					callRecord.Type.ToString() ?? "",
					callRecord.Subtype.ToString() ?? "",
					callRecord.Duration.ToString() ?? "",
					callRecord.FileSize.ToString() ?? "",
					callRecord.StartTime.Date.ToString() ?? "",
					callRecord.StartTime.Timezone ?? "",
					callRecord.StartTime.TimezoneType.ToString() ?? "",
					callRecord.EndTime == null ? callRecord.EndTime.Date.ToString() : "",
					callRecord.EndTime == null ? callRecord.EndTime.Timezone : "",
					callRecord.EndTime == null ? callRecord.EndTime.TimezoneType.ToString() : "",
					callRecord.IsPublic.ToString() ?? "",
					callRecord.DownloadUrl ?? "",
					callRecord.IsDeleted.ToString() ?? ""
				};

					foreach (string value in values) {
						ICell cell = row.CreateCell(columnNumber);

						if (double.TryParse(value, out double result)) {
							cell.SetCellValue(result);
							//} else if (DateTime.TryParseExact(value, "dd.MM.yyyy h:mm:ss", 
							//	CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)) {
							//	cell.SetCellValue(date);
						} else {
							cell.SetCellValue(value);
						}

						columnNumber++;
					}

					columnNumber = 0;
					rowNumber++;
				}
				if (SaveAndCloseIWorkbook(workbook, resultFile)) {
					MessageBox.Show(this, "Файл успешно сохранен по адресу: " + resultFile, "", MessageBoxButton.OK, MessageBoxImage.Information);
					Process.Start(resultFile);
				}
			} catch (Exception exc) {
				MessageBox.Show(this, "Не удалось выгрузить информацию: " + Environment.NewLine + exc.Message, "Возникла ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		
		private  bool CreateNewIWorkbook(string resultFilePrefix, out IWorkbook workbook, out ISheet sheet, out string resultFile) {
			string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
			workbook = null;
			sheet = null;
			resultFile = string.Empty;

			try {
				string templateFile = assemblyDirectory + "Template.xlsx";
				foreach (char item in Path.GetInvalidFileNameChars())
					resultFilePrefix = resultFilePrefix.Replace(item, '-');

				if (!File.Exists(templateFile)) {
					MessageBox.Show(this, "Не удалось найти файл шаблона: " + templateFile, 
						"", MessageBoxButton.OK, MessageBoxImage.Error);
					return false;
				}

				string resultPath = Path.Combine(assemblyDirectory, "Results");
				if (!Directory.Exists(resultPath))
					Directory.CreateDirectory(resultPath);

				resultFile = Path.Combine(resultPath, resultFilePrefix + ".xlsx");

				using (FileStream stream = new FileStream(templateFile, FileMode.Open, FileAccess.Read))
					workbook = new XSSFWorkbook(stream);

				sheet = workbook.GetSheet("Данные");

				return true;
			} catch (Exception e) {
				MessageBox.Show(this, e.Message + Environment.NewLine + e.StackTrace, "",
					MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}

		private bool SaveAndCloseIWorkbook(IWorkbook workbook, string resultFile) {
			try {
				using (FileStream stream = new FileStream(resultFile, FileMode.Create, FileAccess.Write))
					workbook.Write(stream);

				workbook.Close();

				return true;
			} catch (Exception e) {
				MessageBox.Show(this, e.Message + Environment.NewLine + e.StackTrace, "",
					MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}


		private void CheckBoxDateBegin_Checked(object sender, RoutedEventArgs e) {
			DatePickerBegin.IsEnabled = true;
		}

		private void CheckBoxDateBegin_Unchecked(object sender, RoutedEventArgs e) {
			DatePickerBegin.IsEnabled = false;
		}

		private void CheckBoxDateEnd_Checked(object sender, RoutedEventArgs e) {
			DatePickerEnd.IsEnabled = true;
		}

		private void CheckBoxDateEnd_Unchecked(object sender, RoutedEventArgs e) {
			DatePickerEnd.IsEnabled = false;
		}

		private void ButtonOpenFile_Click(object sender, RoutedEventArgs e) {
			try {
				CallRecords.CallRecord callRecord = (sender as Button).DataContext as CallRecords.CallRecord;
				Process.Start(@"\\portal2.bzklinika.ru\Recordings\" + callRecord.Name);
			} catch (Exception exc) {
				MessageBox.Show(this, exc.Message + Environment.NewLine + exc.StackTrace, 
					"", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void ButtonOpenFolder_Click(object sender, RoutedEventArgs e) {
			try {
				CallRecords.CallRecord callRecord = (sender as Button).DataContext as CallRecords.CallRecord;
				Process.Start("explorer.exe", "/select,\"\\\\portal2\\Recordings\\" + @callRecord.Name + "\"");
			} catch (Exception exc) {
				MessageBox.Show(this, exc.Message + Environment.NewLine + exc.StackTrace,
					"", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
