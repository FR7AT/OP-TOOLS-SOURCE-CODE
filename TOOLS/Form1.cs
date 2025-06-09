using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.IO;
using System.Net;
using System.Diagnostics;
using Microsoft.Win32;
using System.Net.Http;
using Guna.UI2.WinForms;
using System.Drawing.Drawing2D;

namespace TOOLS {

    public partial class Form1 : MetroForm {

        private const string CurrentVersion = "1.0.0";
        private const string VersionCheckUrl = "https://raw.githubusercontent.com/FR7AT/OP-TOOLS/main/optools.txt";
        private const string UpdateDownloadUrl = "https://raw.githubusercontent.com/FR7AT/OP-TOOLS/main/";
        private const string UpdateFileName = "OP_TOOLS";

        private Form customToolTip;
        private Label toolTipLabel;
        private Timer hideToolTipTimer;
        private Dictionary<Guna2Button, string> buttonToolTips;

        public Form1() {

            try {

                Task.Run(() => CheckVersion());
            }
            catch (Exception ex) {
                ShowCenteredMessage($"\n\nError: {ex.Message}", "Error", MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            string adminFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "farhat.txt");
            bool allowOpenBrowser = true;

            if (File.Exists(adminFilePath)) {
                string adminContent = File.ReadAllText(adminFilePath).Trim();
                if (adminContent == "farhat") {
                    allowOpenBrowser = false;
                }
            }

            if (allowOpenBrowser) {
                OpenBrowserWithUrl("https://www.opal3ab.com/discord");
            }

            InitializeComponent();
            Task.Run(() => LoadAndCustomizeForm());

            Task.Run(() => InitializeCustomToolTip());

            Task.Run(() => AssignToolTipsToButtons());

            metroProgressBar1.Visible = false;
            lblPercentage.Visible = false;

            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.Text = "OP TOOLS";

            metroTabControl1.SelectedTab = metroTabPage3;
            metroTabControl2.SelectedTab = metroTabPage4;

            metroTabControl1.SelectedTab = metroTabControl1.TabPages["metroTabPage1"];

            this.Resizable = false;
            this.MaximizeBox = false;
            this.Size = new Size(630, 400);

            guna2Button10.Visible = false;
            guna2Button18.Visible = false;
            guna2Button12.Visible = false;
            guna2Button16.Visible = false;
            guna2Button15.Visible = false;
            guna2Button13.Visible = false;

            if (File.Exists(adminFilePath)) {
                string adminContent = File.ReadAllText(adminFilePath).Trim();
                if (adminContent == "farhatandhosaa") {
                    guna2Button10.Visible = true;
                    guna2Button18.Visible = true;
                    guna2Button12.Visible = true;
                    guna2Button16.Visible = true;
                    guna2Button15.Visible = true;
                    guna2Button13.Visible = true;
                }
            }

        }

        private void InitializeCustomToolTip() {

            customToolTip = new Form {

                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                BackColor = Color.Black,
                Opacity = 0.8,
                Size = new Size(300, 40)
            };

            int cornerRadius = 13;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(customToolTip.Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(customToolTip.Width - cornerRadius, customToolTip.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(0, customToolTip.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseFigure();

            customToolTip.Region = new Region(path);

            toolTipLabel = new Label {

                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White
            };

            customToolTip.Controls.Add(toolTipLabel);
            customToolTip.Visible = false;

            hideToolTipTimer = new Timer();
            hideToolTipTimer.Interval = 4000;
            hideToolTipTimer.Tick += (s, e) => {

                customToolTip.Visible = false;
                hideToolTipTimer.Stop();
            };
        }

        private void AssignToolTipsToButtons() {

            buttonToolTips = new Dictionary<Guna2Button, string> {

           { guna2Button2, "يقوم بنسخ موارد اللعبة إلى مجلد الشير 1 ويجب أن يكون المحاكي شغال" },
           { guna2Button9, "يقوم بنسخ موارد اللعبة من مجلد الشير 1 إلى اللعبة ويجب أن يكون المحاكي شغال" },
           { guna2Button3, "يقوم بفتح المحاكي ويجب عليك اختيار نوع المحاكي" },
           { guna2Button4, "يقوم بتنظيف اللعبة ويجب عليك اختيار النسخة التي تريد تنظيفها مع المحاكي شغال" },
           { guna2Button6, "يقوم بفك حظر الجهاز أو المحاكي ويجب أن يكون المحاكي مغلق عند التنفيذ" },
           { guna2Button5, "يقوم بإغلاق المحاكي" },
           { guna2Button7, "اصلاح مشكلة 98% ويجب تنفيذه والمحاكي مغلق" },
           { guna2Button8, "يقوم برسترت الجيست ويجب تنفيذه والمحاكي شغال" },
           { guna2Button1, "يقوم بتثبيت التطبيق ويجب أن يكون التطبيق بجانب الأداة والمحاكي شغال" },
           { guna2Button11, "يقوم بتعديل دقة ببجي الكورية إلى 1080 ويجب تنفيذه والمحاكي مغلق" },
           { guna2Button14, "يقوم بتشغيل جميع أزرار التحكم الخاصة بببجي الكورية" },
           { guna2Button17, "يقوم بإرجاع التحكم الأصلي الخاص بالمحاكي" },
           { guna2Button19, "يقوم بإيقاف تقارير أخطاء ويندوز 10 إلى مايكروسوفت" },
           { guna2Button20, "يقوم بعمل رسترت للإنترنت بالكامل" },
           { guna2Button21, "يقوم بتنظيف ملفات التمب" },
           { guna2Button24, "يقوم بتغيير الماك أدرس الخاص بك" },
           { guna2Button23, "يقوم بتنظيف الهارد ديسك الخاص بك من الملفات المؤقتة" },
           { guna2Button22, "يقوم بإضافة ملف الهوست الأصلي لويندوز" },
           { guna2Button27, "يقوم بحذف ملفات الكاش الخاصة بالمتصفحات" },
           { guna2Button26, "يقوم بعمل رسترت لجدار الحماية الخاص بك" },
           { guna2Button25, "يقوم بتنظيف جميع مسارات محاكي جيم لوب" },
            };

            foreach (var item in buttonToolTips) {

                var button = item.Key;
                var toolTipText = item.Value;

                button.MouseEnter += (s, e) => {

                    Point location = button.PointToScreen(new Point(0, button.Height));
                    customToolTip.Location = location;

                    toolTipLabel.Text = toolTipText;
                    customToolTip.Visible = true;

                    hideToolTipTimer.Stop();
                    hideToolTipTimer.Start();
                };

                button.MouseLeave += (s, e) => {

                    customToolTip.Visible = false;
                    hideToolTipTimer.Stop();
                };
            }
        }

        private void OpenBrowserWithUrl(string url) {
            
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void CheckVersion() {
            
            string latestVersion = GetLatestVersion();
            
            if (!IsVersionCompatible(CurrentVersion, latestVersion)) {
                ShowUpdateMessage(latestVersion);
            }
        }

        private string GetLatestVersion() {
            
            using (HttpClient client = new HttpClient()) {
                return client.GetStringAsync(VersionCheckUrl).Result.Trim();
            }
        }

        private bool IsVersionCompatible(string currentVersion, string latestVersion) {
            
            Version current = new Version(currentVersion);
            Version latest = new Version(latestVersion);
            return current >= latest;
        }

        private void ShowUpdateMessage(string latestVersion) {
            
            using (Form dummyForm = new Form()) {
                dummyForm.StartPosition = FormStartPosition.CenterScreen;
                dummyForm.Size = new Size(350, 200);
                dummyForm.ShowInTaskbar = false;
                dummyForm.Opacity = 0;
                dummyForm.Show();

                var result = MetroFramework.MetroMessageBox.Show(
                    dummyForm,
                    $"Your version ({CurrentVersion}) is outdated. Latest version: {latestVersion}.\nWould you like to download the update?",
                    "Update Required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes) {
                    DownloadAndInstallUpdate(latestVersion);
                }
                else {
                    ShowCenteredMessage("The application will now close because the version is outdated.", "Version Outdated", MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }
        }

        private void DownloadAndInstallUpdate(string latestVersion) {
            
            try {

                string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{UpdateFileName} {latestVersion}.exe");

                using (HttpClient client = new HttpClient()) {
                    using (var response = client.GetAsync(UpdateDownloadUrl + UpdateFileName + ".exe", HttpCompletionOption.ResponseHeadersRead).Result) {
                        response.EnsureSuccessStatusCode();
                        using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                            response.Content.CopyToAsync(fileStream).Wait();
                        }
                    }
                }

                ShowCenteredMessage(
                    $"Update downloaded as {Path.GetFileName(savePath)}. The application will now close to install the update.",
                    "Update Ready",
                    MessageBoxIcon.Information);

                Process.Start(savePath);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                ShowCenteredMessage(
                    $"Error downloading update: {ex.Message}",
                    "Error",
                    MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private void ShowCenteredMessage(string message, string title, MessageBoxIcon icon) {
            
            using (Form dummyForm = new Form()) {

                dummyForm.StartPosition = FormStartPosition.CenterScreen;
                dummyForm.Size = new Size(350, 200);
                dummyForm.ShowInTaskbar = false;
                dummyForm.Opacity = 0;
                dummyForm.Show();

                MetroFramework.MetroMessageBox.Show(dummyForm, message, title, MessageBoxButtons.OK, icon);
            }
        }

        static string GenerateRandomNumber(int length) {
            
            var random = new Random(Guid.NewGuid().GetHashCode());
            var randomValue = new char[length];
            for (int i = 0; i < length; i++) {
                randomValue[i] = (char)('0' + random.Next(0, 10));
            }
            return new string(randomValue);
        }

        private void DisableMetroUI() {

            foreach (Control control in this.Controls) {
                if (control is MetroFramework.Controls.MetroButton || control is MetroFramework.Controls.MetroTile || control is MetroFramework.Controls.MetroTabControl) {
                    control.Enabled = false;
                }
            }
        }

        private void EnableMetroUI() {

            foreach (Control control in this.Controls) {
                if (control is MetroFramework.Controls.MetroButton || control is MetroFramework.Controls.MetroTile || control is MetroFramework.Controls.MetroTabControl) {
                    control.Enabled = true;
                }
            }
        }

        private async void LoadAndCustomizeForm() {
            
            await downloadFiles();
        }

        private string selectedPackage = "";
        string randomDeviceId = GenerateRandomNumber(17);
        string randomAndroidId = GenerateRandomNumber(16);

        private async Task downloadFiles() {
            
            await Task.Run(() => {


            });
        }

        private async void guna2Button3_Click(object sender, EventArgs e) {

            if (!metroRadioButton1.Checked && !metroRadioButton2.Checked) {
                return;
            }

            string originalText = guna2Button3.Text;
            guna2Button3.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button3.Enabled = false;
            guna2Button3.Text = "Processing";

            string selectedFilePath = "";

            if (metroRadioButton1.Checked) {

                selectedFilePath = "AndroidEmulatorEn.exe";
            }
            else if (metroRadioButton2.Checked) {
                selectedFilePath = "AndroidEmulatorEx.exe";
            }

            if (!string.IsNullOrEmpty(selectedFilePath)) {
                await ProcessAddresses2(selectedFilePath);
                guna2Button3.Text = "Done";
                await Task.Delay(600);
                guna2Button3.Text = originalText;
            }
            guna2Button3.Enabled = true;
        }

        private void metroRadioButton2_CheckedChanged(object sender, EventArgs e) {

        }

        private void metroRadioButton1_CheckedChanged(object sender, EventArgs e) {

        }
       
        private async Task ProcessAddresses2(string fileName) {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button7_Click(object sender, EventArgs e) {

            string originalText = guna2Button7.Text;
            guna2Button7.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button7.Enabled = false;
            guna2Button7.Text = "Processing";
            await ProcessAddresses1();
            guna2Button7.Text = "Done";
            await Task.Delay(600);
            guna2Button7.Text = originalText;
            guna2Button7.Enabled = true;
        }

        private async Task ProcessAddresses1() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button5_Click(object sender, EventArgs e) {

            string originalText = guna2Button5.Text;
            guna2Button5.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button5.Enabled = false;
            guna2Button5.Text = "Processing";
            await ProcessAddresses3();
            guna2Button5.Text = "Done";
            await Task.Delay(600);
            guna2Button5.Text = originalText;
            guna2Button5.Enabled = true;
        }

        private async Task ProcessAddresses3() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button4_Click(object sender, EventArgs e) {

            if (string.IsNullOrEmpty(selectedPackage)) {
                return;
            }

            string originalText = guna2Button4.Text;
            guna2Button4.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button4.Enabled = false;
            guna2Button4.Text = "Processing";
            await ProcessAddresses5();
            guna2Button4.Text = "Done";
            await Task.Delay(600);
            guna2Button4.Text = originalText;
            guna2Button4.Enabled = true;
        }

        private async Task ProcessAddresses5() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }
        
        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e) {

            string selectedOption = metroComboBox1.SelectedItem?.ToString();
            
            switch (selectedOption) {
                case "Global":
                    selectedPackage = "com.tencent.ig";
                    break;
                case "Korea":
                    selectedPackage = "com.pubg.krmobile";
                    break;
                case "Taiwan":
                    selectedPackage = "com.rekoo.pubgm";
                    break;
                case "Vietnam":
                    selectedPackage = "com.vng.pubgmobile";
                    break;
                default:
                    selectedPackage = ""; 
                    MessageBox.Show("Invalid selection. Please select a valid package.");
                    break;
            }
        }

        private async void guna2Button6_Click(object sender, EventArgs e) {

            string originalText = guna2Button6.Text;
            guna2Button6.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button6.Enabled = false;
            guna2Button6.Text = "Processing";
            await ProcessAddresses6();
            guna2Button6.Text = "Done";
            await Task.Delay(600);
            guna2Button6.Text = originalText;
            guna2Button6.Enabled = true;
        }

        private async Task ProcessAddresses6() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button8_Click(object sender, EventArgs e) {

            if (string.IsNullOrEmpty(selectedPackage)) {
                return;
            }

            string originalText = guna2Button8.Text;
            guna2Button8.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button8.Enabled = false;
            guna2Button8.Text = "Processing";
            await ProcessAddresses7();
            guna2Button8.Text = "Done";
            await Task.Delay(600);
            guna2Button8.Text = originalText;
            guna2Button8.Enabled = true;
        }

        private async Task ProcessAddresses7() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private void metroLink1_Click(object sender, EventArgs e) {

            string url = "https://www.opal3ab.com";

            try {
                
                Process.Start(new ProcessStartInfo {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch {
            }
        }

        private void metroLink3_Click(object sender, EventArgs e) {

            string url = "https://www.opal3ab.com";

            try {
                
                Process.Start(new ProcessStartInfo {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch {
            }

        }

        private void metroLink6_Click(object sender, EventArgs e) {

            string url = "https://opal3ab.com";

            try {
                
                Process.Start(new ProcessStartInfo {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch {
            }
        }

        private async void metroLink5_Click(object sender, EventArgs e) {

            await ProcessAddresses10();
        }

        private async Task ProcessAddresses10() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button20_Click(object sender, EventArgs e) {

            string originalText = guna2Button20.Text;
            guna2Button20.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button20.Enabled = false;
            guna2Button20.Text = "Processing";
            await ProcessAddresses11();
            guna2Button20.Text = "Done";
            await Task.Delay(600);
            guna2Button20.Text = originalText;
            guna2Button20.Enabled = true;
        }

        private async Task ProcessAddresses11() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button26_Click(object sender, EventArgs e) {

            string originalText = guna2Button26.Text;
            guna2Button26.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button26.Enabled = false;
            guna2Button26.Text = "Processing";
            await ProcessAddresses12();
            guna2Button26.Text = "Done";
            await Task.Delay(600);
            guna2Button26.Text = originalText;
            guna2Button26.Enabled = true;
        }

        private async Task ProcessAddresses12() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button23_Click(object sender, EventArgs e) {

            string originalText = guna2Button23.Text;
            guna2Button23.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button23.Enabled = false;
            guna2Button23.Text = "Processing";
            await ProcessAddresses13();
            guna2Button23.Text = "Done";
            await Task.Delay(600);
            guna2Button23.Text = originalText;
            guna2Button23.Enabled = true;
        }

        private async Task ProcessAddresses13() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button21_Click(object sender, EventArgs e) {

            string originalText = guna2Button21.Text;
            guna2Button21.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button21.Enabled = false;
            guna2Button21.Text = "Processing";
            await ProcessAddresses14();
            guna2Button21.Text = "Done";
            await Task.Delay(600);
            guna2Button21.Text = originalText;
            guna2Button21.Enabled = true;
        }

        private async Task ProcessAddresses14() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button22_Click(object sender, EventArgs e) {

            string originalText = guna2Button22.Text;
            guna2Button22.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button22.Enabled = false;
            guna2Button22.Text = "Processing";
            await ProcessAddresses15();
            guna2Button22.Text = "Done";
            await Task.Delay(600);
            guna2Button22.Text = originalText;
            guna2Button22.Enabled = true;
        }

        private async Task ProcessAddresses15() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button25_Click(object sender, EventArgs e) {

            string originalText = guna2Button25.Text;
            guna2Button25.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button25.Enabled = false;
            guna2Button25.Text = "Processing";
            await ProcessAddresses16();
            guna2Button25.Text = "Done";
            await Task.Delay(600);
            guna2Button25.Text = originalText;
            guna2Button25.Enabled = true;
        }

        private async Task ProcessAddresses16() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink7_Click(object sender, EventArgs e) {

            await ProcessAddresses17();
        }

        private async Task ProcessAddresses17() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink9_Click(object sender, EventArgs e) {

            await ProcessAddresses18();
        }

        private async Task ProcessAddresses18() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink8_Click(object sender, EventArgs e) {

            await ProcessAddresses19();
        }

        private async Task ProcessAddresses19() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink10_Click(object sender, EventArgs e) {

            await ProcessAddresses20();
        }

        private async Task ProcessAddresses20() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink11_Click(object sender, EventArgs e) {

            await ProcessAddresses21();
        }

        private async Task ProcessAddresses21() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink12_Click(object sender, EventArgs e) {

            await ProcessAddresses22();
        }

        private async Task ProcessAddresses22() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink18_Click(object sender, EventArgs e) {

            await ProcessAddresses23();
        }

        private async Task ProcessAddresses23() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink16_Click(object sender, EventArgs e) {

            await ProcessAddresses24();
        }

        private async Task ProcessAddresses24() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink17_Click(object sender, EventArgs e) {

            await ProcessAddresses25();
        }

        private async Task ProcessAddresses25() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink14_Click(object sender, EventArgs e) {

            await ProcessAddresses26();
        }

        private async Task ProcessAddresses26() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink15_Click(object sender, EventArgs e) {

            await ProcessAddresses27();
        }

        private async Task ProcessAddresses27() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink13_Click(object sender, EventArgs e) {
           
            await ProcessAddresses28();
        }

        private async Task ProcessAddresses28() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink4_Click(object sender, EventArgs e) {

            await ProcessAddresses29();
        }

        private async Task ProcessAddresses29() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink23_Click(object sender, EventArgs e) {

            await ProcessAddresses30();
        }

        private async Task ProcessAddresses30() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink22_Click(object sender, EventArgs e) {

            await ProcessAddresses31();
        }

        private async Task ProcessAddresses31() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink21_Click(object sender, EventArgs e) {

            await ProcessAddresses32();
        }

        private async Task ProcessAddresses32() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink20_Click(object sender, EventArgs e) {
            
            await ProcessAddresses33();
        }

        private async Task ProcessAddresses33() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink19_Click(object sender, EventArgs e) {

            await ProcessAddresses34();
        }

        private async Task ProcessAddresses34() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink24_Click(object sender, EventArgs e) {

            await ProcessAddresses35();
        }

        private async Task ProcessAddresses35() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink28_Click(object sender, EventArgs e) {

            await ProcessAddresses36();
        }

        private async Task ProcessAddresses36() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink27_Click(object sender, EventArgs e) {

            await ProcessAddresses37();
        }

        private async Task ProcessAddresses37() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink26_Click(object sender, EventArgs e) {

            await ProcessAddresses38();
        }

        private async Task ProcessAddresses38() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink25_Click(object sender, EventArgs e) {
            
            await ProcessAddresses39();
        }

        private async Task ProcessAddresses39() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink29_Click(object sender, EventArgs e) {
            
            await ProcessAddresses40();
        }

        private async Task ProcessAddresses40() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private void metroLink31_Click(object sender, EventArgs e) {

            this.Close();

        }

        private void metroLink30_Click(object sender, EventArgs e) {
            this.WindowState = FormWindowState.Minimized;
        }

        private void metroProgressBar1_Click(object sender, EventArgs e) {

        }

        private async void metroLink2_Click(object sender, EventArgs e) {
            
            await ProcessAddresses41();
        }

        private async Task ProcessAddresses41() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink40_Click(object sender, EventArgs e) {

            await ProcessAddresses42();
        }

        private async Task ProcessAddresses42() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink32_Click(object sender, EventArgs e) {

            await ProcessAddresses43();
        }

        private async Task ProcessAddresses43() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink35_Click(object sender, EventArgs e) {

            await ProcessAddresses44();
        }

        private async Task ProcessAddresses44() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink33_Click(object sender, EventArgs e) {

            await ProcessAddresses45();
        }

        private async Task ProcessAddresses45() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink39_Click(object sender, EventArgs e) {

            await ProcessAddresses46();
        }

        private async Task ProcessAddresses46() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink34_Click(object sender, EventArgs e) {

            await ProcessAddresses47();
        }

        private async Task ProcessAddresses47() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink36_Click(object sender, EventArgs e) {
            
            await ProcessAddresses49();
        }

        private async Task ProcessAddresses49() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink37_Click(object sender, EventArgs e) {

            await ProcessAddresses50();
        }

        private async Task ProcessAddresses50() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink38_Click(object sender, EventArgs e) {
            
            await ProcessAddresses51();
        }

        private async Task ProcessAddresses51() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink41_Click(object sender, EventArgs e) {
            
            await ProcessAddresses52();
        }

        private async Task ProcessAddresses52() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink42_Click(object sender, EventArgs e) {
            
            await ProcessAddresses53();
        }

        private async Task ProcessAddresses53() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink53_Click(object sender, EventArgs e) {
            
            await ProcessAddresses60();
        }

        private async Task ProcessAddresses60() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink48_Click(object sender, EventArgs e) {

            await ProcessAddresses61();
        }

        private async Task ProcessAddresses61() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink51_Click(object sender, EventArgs e) {

            await ProcessAddresses62();
        }

        private async Task ProcessAddresses62() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink49_Click(object sender, EventArgs e) {

            await ProcessAddresses63();
        }

        private async Task ProcessAddresses63() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink50_Click(object sender, EventArgs e) {

            await ProcessAddresses64();
        }

        private async Task ProcessAddresses64() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink52_Click(object sender, EventArgs e) {

            await ProcessAddresses65();
        }

        private async Task ProcessAddresses65() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink54_Click(object sender, EventArgs e) {
            
            await ProcessAddresses66();
        }

        private async Task ProcessAddresses66() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void metroLink47_Click(object sender, EventArgs e) {

            await ProcessAddresses67();
        }

        private async Task ProcessAddresses67() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button10_Click(object sender, EventArgs e) {

            string originalText = guna2Button10.Text;
            guna2Button10.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button10.Enabled = false;
            guna2Button10.Text = "Processing";
            await ProcessAddresses72();
            guna2Button10.Text = "Done";
            await Task.Delay(600);
            guna2Button10.Text = originalText;
            guna2Button10.Enabled = true;
        }

        private async Task ProcessAddresses72() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button18_Click(object sender, EventArgs e) {

            string originalText = guna2Button18.Text;
            guna2Button18.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button18.Enabled = false;
            guna2Button18.Text = "Processing";
            await ProcessAddresses73();
            guna2Button18.Text = "Done";
            await Task.Delay(600);
            guna2Button18.Text = originalText;
            guna2Button18.Enabled = true;
        }

        private async Task ProcessAddresses73() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button11_Click(object sender, EventArgs e) {

            string originalText = guna2Button11.Text;
            guna2Button11.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button11.Enabled = false;
            guna2Button11.Text = "Processing";
            await ProcessAddresses69();
            guna2Button11.Text = "Done";
            await Task.Delay(600);
            guna2Button11.Text = originalText;
            guna2Button11.Enabled = true;
        }

        private async Task ProcessAddresses69() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button14_Click(object sender, EventArgs e) {

            string originalText = guna2Button14.Text;
            guna2Button14.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button14.Enabled = false;
            guna2Button14.Text = "Processing";
            await ProcessAddresses90();
            guna2Button14.Text = "Done";
            await Task.Delay(600);
            guna2Button14.Text = originalText;
            guna2Button14.Enabled = true;
        }

        private async Task ProcessAddresses90() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button12_Click(object sender, EventArgs e) {

            string originalText = guna2Button12.Text;
            guna2Button12.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button12.Enabled = false;
            guna2Button12.Text = "Processing";
            await ProcessAddresses70();
            guna2Button12.Text = "Done";
            await Task.Delay(600);
            guna2Button12.Text = originalText;
            guna2Button12.Enabled = true;
        }

        private async Task ProcessAddresses70() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button16_Click(object sender, EventArgs e) {

            string originalText = guna2Button16.Text;
            guna2Button16.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button16.Enabled = false;
            guna2Button16.Text = "Processing";
            await ProcessAddresses71();
            guna2Button16.Text = "Done";
            await Task.Delay(600);
            guna2Button16.Text = originalText;
            guna2Button16.Enabled = true;
        }

        private async Task ProcessAddresses71() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button15_Click(object sender, EventArgs e) {

            string originalText = guna2Button15.Text;
            guna2Button15.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button15.Enabled = false;
            guna2Button15.Text = "Processing";
            await ProcessAddresses91();
            guna2Button15.Text = "Done";
            await Task.Delay(600);
            guna2Button15.Text = originalText;
            guna2Button15.Enabled = true;
        }

        private async Task ProcessAddresses91() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button13_Click(object sender, EventArgs e) {

            string originalText = guna2Button13.Text;
            guna2Button13.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button13.Enabled = false;
            guna2Button13.Text = "Processing";
            await ProcessAddresses92();
            guna2Button13.Text = "Done";
            await Task.Delay(600);
            guna2Button13.Text = originalText;
            guna2Button13.Enabled = true;
        }

        private async Task ProcessAddresses92() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button17_Click(object sender, EventArgs e) {

            string originalText = guna2Button17.Text;
            guna2Button17.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button17.Enabled = false;
            guna2Button17.Text = "Processing";
            await ProcessAddresses93();
            guna2Button17.Text = "Done";
            await Task.Delay(600);
            guna2Button17.Text = originalText;
            guna2Button17.Enabled = true;
        }

        private async Task ProcessAddresses93() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button1_Click_1(object sender, EventArgs e) {

            string originalText = guna2Button1.Text;
            guna2Button1.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button1.Enabled = false;
            guna2Button1.Text = "Processing";
            await ProcessAddresses59();
            guna2Button1.Text = "Done";
            await Task.Delay(600);
            guna2Button1.Text = originalText;
            guna2Button1.Enabled = true;
        }

        private async Task ProcessAddresses59() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button2_Click(object sender, EventArgs e) {
            
            if (string.IsNullOrEmpty(selectedPackage)) {
                return;
            }

            string originalText = guna2Button2.Text;
            guna2Button2.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button2.Enabled = false;
            guna2Button2.Text = "Processing";
            await ProcessAddresses8();
            guna2Button2.Text = "Done";
            await Task.Delay(600);
            guna2Button2.Text = originalText;
            guna2Button2.Enabled = true;
        }

        private async Task ProcessAddresses8() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button9_Click(object sender, EventArgs e) {

            if (string.IsNullOrEmpty(selectedPackage)) {
                return;
            }

            string originalText = guna2Button9.Text;
            guna2Button9.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button9.Enabled = false;
            guna2Button9.Text = "Processing";
            await ProcessAddresses9();
            guna2Button9.Text = "Done";
            await Task.Delay(600);
            guna2Button9.Text = originalText;
            guna2Button9.Enabled = true;
        }

        private async Task ProcessAddresses9() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button19_Click(object sender, EventArgs e) {

            string originalText = guna2Button19.Text;
            guna2Button19.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button19.Enabled = false;
            guna2Button19.Text = "Processing";
            await ProcessAddresses97();
            guna2Button19.Text = "Done";
            await Task.Delay(600);
            guna2Button19.Text = originalText;
            guna2Button19.Enabled = true;
        }
        
        private async Task ProcessAddresses97() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button24_Click(object sender, EventArgs e) {

            string originalText = guna2Button24.Text;
            guna2Button24.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button24.Enabled = false;
            guna2Button24.Text = "Processing";
            await ProcessAddresses98();
            guna2Button24.Text = "Done";
            await Task.Delay(600);
            guna2Button24.Text = originalText;
            guna2Button24.Enabled = true;
        }

        private async Task ProcessAddresses98() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }

        private async void guna2Button27_Click(object sender, EventArgs e) {

            string originalText = guna2Button27.Text;
            guna2Button27.DisabledState.ForeColor = System.Drawing.Color.White;
            guna2Button27.Enabled = false;
            guna2Button27.Text = "Processing";
            await ProcessAddresses99();
            guna2Button27.Text = "Done";
            await Task.Delay(600);
            guna2Button27.Text = originalText;
            guna2Button27.Enabled = true;
        }

        private async Task ProcessAddresses99() {

            await Task.Run(async () => {

                DisableMetroUI();

                await Task.Delay(4000);

                EnableMetroUI();

            });
        }
    }
}
