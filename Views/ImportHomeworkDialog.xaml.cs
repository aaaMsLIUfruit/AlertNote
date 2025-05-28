using System.Windows;
using Microsoft.Web.WebView2.Core;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Collections.Generic;
using StickyAlerts.Services;

namespace StickyAlerts.Views
{
    public partial class ImportHomeworkDialog : Window
    {
        public ImportHomeworkDialog()
        {
            InitializeComponent();
            webView.Source = new System.Uri("https://cslabcg.whu.edu.cn/indexcs/simple.jsp?loginErr=0");
        }

        private DateTime ParseDeadline(string deadlineText)
        {
            int days = 0, hours = 0;
            var dayMatch = Regex.Match(deadlineText, @"(\d+)天");
            var hourMatch = Regex.Match(deadlineText, @"(\d+)小时");
            if (dayMatch.Success) days = int.Parse(dayMatch.Groups[1].Value);
            if (hourMatch.Success) hours = int.Parse(hourMatch.Groups[1].Value);
            return DateTime.Now.AddDays(days).AddHours(hours);
        }

        private async void ImportHomework_Click(object sender, RoutedEventArgs e)
        {
            string js = @"
                (function() {
                    var result = [];
                    var div = document.getElementById('activeEXPsDIV');
                    if(div) {
                        var links = div.querySelectorAll('a');
                        for (var i = 0; i < links.length; i++) {
                            var name = links[i].innerText.trim();
                            var next = links[i].nextElementSibling;
                            var deadline = '';
                            if(next && next.tagName.toLowerCase() === 'span') {
                                deadline = next.innerText.trim();
                            }
                            if(name && deadline) {
                                result.push({name: name, deadline: deadline});
                            }
                        }
                    }
                    return JSON.stringify(result);
                })();
            ";
            string json = await webView.ExecuteScriptAsync(js);
            if (json.StartsWith("\"") && json.EndsWith("\""))
                json = json.Substring(1, json.Length - 2);
            json = json.Replace("\\\"", "\"").Replace("\\\\", "\\");

            List<LabInfo> labs = null;
            try
            {
                labs = JsonSerializer.Deserialize<List<LabInfo>>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("反序列化失败：" + ex.Message + "\n内容：" + json);
                return;
            }

            if (labs == null || labs.Count == 0)
            {
                MessageBox.Show("未能在当前页面提取到实验信息，请确认页面内容！", "未提取到实验", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 生成便签
            var alertService = App.Host.Services.GetService(typeof(IAlertService)) as IAlertService;
            if (alertService == null)
            {
                MessageBox.Show("便签服务未初始化，无法生成便签！");
                return;
            }
            int count = 0;
            foreach (var lab in labs)
            {
                string title = lab.name;
                string note = lab.deadline;
                DateTime deadline = ParseDeadline(lab.deadline);
                alertService.Add(title, note, deadline, "作业", true, true, false);
                count++;
            }
            MessageBox.Show($"已为你生成{count}个便签！", "导入成功");
        }
    }

    public class LabInfo
    {
        public string name { get; set; }
        public string deadline { get; set; }
    }
}
