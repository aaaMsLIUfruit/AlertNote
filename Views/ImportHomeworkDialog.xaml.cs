using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using StickyAlerts.Services;
using StickyAlerts.ViewModels;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StickyAlerts.Views
{
    public partial class ImportHomeworkDialog : Window
    {
        private readonly HttpClientHandler _httpClientHandler = new HttpClientHandler { UseCookies = true, AllowAutoRedirect = false };
        private readonly HttpClient _httpClient;
        private string _loginUrl = "https://cslabcg.whu.edu.cn/login/loginproc.jsp";
        private string _homeworkUrl = "https://cslabcg.whu.edu.cn/indexcs/simple.jsp?loginErr=0";

        public ImportHomeworkDialog()
        {
            InitializeComponent();
            _httpClient = new HttpClient(_httpClientHandler);
            Loaded += ImportHomeworkDialog_Loaded;
        }

        private async void ImportHomeworkDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCaptchaAsync();
        }

        private async void RefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            await LoadCaptchaAsync();
        }

        private async Task LoadCaptchaAsync()
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync("https://cslabcg.whu.edu.cn/cgjiaoyan?t=" + DateTime.Now.Ticks);
                using var ms = new MemoryStream(bytes);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                CaptchaImage.Source = bitmap;
            }
            catch
            {
                MessageBox.Show("验证码加载失败！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoginAndImport_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string captcha = CaptchaBox.Text.Trim();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(captcha))
            {
                MessageBox.Show("请填写完整信息！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                // 1. 先访问首页
                await _httpClient.GetAsync("https://cslabcg.whu.edu.cn/indexcs/simple.jsp?loginErr=0");
                // 2. 获取验证码图片（加时间戳防缓存）
                var bytes = await _httpClient.GetByteArrayAsync("https://cslabcg.whu.edu.cn/cgjiaoyan?t=" + DateTime.Now.Ticks);
                // 3. 设置常见浏览器头部（补齐所有关键Header）
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
                _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9");
                _httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { MaxAge = TimeSpan.Zero };
                _httpClient.DefaultRequestHeaders.Remove("Origin");
                _httpClient.DefaultRequestHeaders.Add("Origin", "https://cslabcg.whu.edu.cn");
                _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://cslabcg.whu.edu.cn/indexcs/simple.jsp?loginErr=0");
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");
                // 4. 登录
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("IndexStyle", "1"),
                    new KeyValuePair<string, string>("stid", username),
                    new KeyValuePair<string, string>("pwd", password),
                    new KeyValuePair<string, string>("captchaCode", captcha)
                });
                var response = await _httpClient.PostAsync(_loginUrl, content);
                var location = response.Headers.Location?.ToString() ?? "";
                var html = await response.Content.ReadAsStringAsync();
               
                // 判断是否重定向（302），并根据Location判断登录结果
                if (response.StatusCode == System.Net.HttpStatusCode.Found && response.Headers.Location != null)
                {
                    if (location.Contains("loginErr"))
                    {
                        MessageBox.Show("登录失败，请检查账号、密码和验证码！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        await LoadCaptchaAsync();
                        return;
                    }
                    else
                    {
                        // 登录成功
                        await ImportHomeworksAsync();
                        MessageBox.Show("作业导入完成！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("登录失败，未知错误！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    await LoadCaptchaAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("导入失败：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ImportHomeworksAsync()
        {
            // 1. 访问首页，提取所有课程链接
            var homeResponse = await _httpClient.GetAsync("https://cslabcg.whu.edu.cn/indexcs/simple.jsp?loginErr=0");
            var homeHtml = await homeResponse.Content.ReadAsStringAsync();
            var courseMatches = Regex.Matches(homeHtml, "href=\"(courselist\\.jsp\\?courseID=\\d+)\"");
            foreach (Match match in courseMatches)
            {
                string courseUrl = "https://cslabcg.whu.edu.cn/" + match.Groups[1].Value;
                // 2. 访问课程页面，查找"在线作业"链接
                var courseResponse = await _httpClient.GetAsync(courseUrl);
                var courseHtml = await courseResponse.Content.ReadAsStringAsync();
                var homeworkMenuMatch = Regex.Match(courseHtml, "href=\"(/includes/redirect\\.jsp\\?tab=-2)\"");
                if (homeworkMenuMatch.Success)
                {
                    string homeworkMenuUrl = "https://cslabcg.whu.edu.cn" + homeworkMenuMatch.Groups[1].Value;
                    // 3. 访问"在线作业"页面，提取作业列表
                    var homeworkListResponse = await _httpClient.GetAsync(homeworkMenuUrl);
                    var homeworkListHtml = await homeworkListResponse.Content.ReadAsStringAsync();
                    var hwMatches = Regex.Matches(homeworkListHtml, "href=\"(index\\.jsp\\?courseID=\\d+&assignID=\\d+)\".*?>([^<]+作业)<", RegexOptions.Singleline);
                    foreach (Match hw in hwMatches)
                    {
                        string hwUrl = "https://cslabcg.whu.edu.cn/" + hw.Groups[1].Value;
                        string hwName = hw.Groups[2].Value.Trim();
                        // 4. 进入作业详情页，提取截止时间
                        var hwDetailResponse = await _httpClient.GetAsync(hwUrl);
                        var hwDetailHtml = await hwDetailResponse.Content.ReadAsStringAsync();
                        var deadlineMatch = Regex.Match(hwDetailHtml, "作业时间：<b>.*?</b> 至 <b>(?<deadline>[\\d\\- :]+)</b>");
                        string deadline = deadlineMatch.Success ? deadlineMatch.Groups["deadline"].Value.Trim() : "";
                        // 5. 生成便签
                        if (DateTime.TryParse(deadline, out DateTime deadlineTime))
                        {
                            var alertService = App.Host.Services.GetService(typeof(IAlertService)) as IAlertService;
                            alertService?.Add(hwName, string.Empty, deadlineTime, true, true, false);
                        }
                    }
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 