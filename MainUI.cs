﻿using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using MaterialSkin.Controls;
using MaterialSkin;
using System.Drawing;
using System.Windows.Forms;
using Level = GateBot.LogManager.Level;

namespace GateBot
{
    public partial class MainUI : MaterialForm
    {
        

        private readonly MaterialSkinManager materialSkinManager;
        public static IWebDriver _driver = null;

        private readonly Config _config;

        private string serverName;
        private string serverIP;

        private string mainHandle;
        private readonly Timer timer1;

        /// Option
        private bool disablePopup;

        public MainUI()
        {
            LogManager.InitializeLogFile();
            LogManager.LogMessage("프로그램 초기화 시작", Level.Info);
            InitializeComponent();

            ConfigManager configManager = new ConfigManager();
            _config = configManager.LoadedConfig;

            // 폼 닫기 이벤트 연결
            this.FormClosing += MainUI_FormClosing;

            // Material SKIN
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue900, Primary.Blue700, Accent.LightBlue200, TextShade.WHITE);

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(700, 700);

            // Util_Control.MoveControl(TabSelector1, 150, 30);

            // DisablePopupCheckBox1.Checked = true;

            timer1 = new Timer();
            timer1.Interval = 5000; // 5초마다 팝업 탐색

            timer1.Tick += Timer1_Tick;
            timer1.Start();

            LogManager.LogMessage("프로그램 초기화 완료", Level.Info);
        }

        private async void Timer1_Tick(object sender, EventArgs e)
        {
            if (_driver != null && !string.IsNullOrEmpty(mainHandle) && _driver.WindowHandles.Contains(mainHandle) && disablePopup)
            {
                try
                {
                    bool alertHandled = await Util_Option.HandleWindows(_driver, mainHandle, _config);
                    if (alertHandled)
                    {
                        Console.WriteLine("경고창 처리 성공");
                        // 경고창 처리 성공 시 추가 작업 수행
                    }
                    else
                    {
                        Console.WriteLine("경고창 처리 실패 또는 없음");
                        // 경고창 처리 실패 시 추가 작업 수행
                    }
                }
                catch (NoSuchElementException ex)
                {
                    LogManager.LogException(ex, Level.Error);
                }
                catch (NoSuchWindowException ex)
                {
                    LogManager.LogException(ex, Level.Error);
                }
                catch (NoAlertPresentException)
                {
                    //
                }
                catch (Exception ex)
                {
                    LogManager.LogException(ex, Level.Error);
                }
            }
        }




        private async void StartBtn1_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("StartBtn Click", Level.Info);
            try
            {
                // _config = ConfigManager.LoadConfig(); // Config 파일 로드

                _driver = await Task.Run(() => Util.InitializeDriver(_config)); // 비동기로 드라이버 초기화


                _driver.Navigate().GoToUrl(_config.Url); // 사용자가 입력한 사이트로 이동

                mainHandle = Util.FindWindowHandleByUrl(_driver, _config.Url);

                Util_Control.MoveFormToTop(this);
            }
            catch (Exception ex)
            {
                // MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogManager.LogException(ex, Level.Error);
            }
        }

        private void ConnectBtn1_Click(object sender, EventArgs e)
        {
            try
            {
                if (_driver == null)
                {
                    MessageBox.Show("드라이버가 초기화되지 않았습니다. 먼저 시작 버튼을 눌러주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Util_Control.ClickElementByXPath(_driver, "/html/body/div/div[2]/button[3]"); // 고급
                Util_Control.ClickElementByXPath(_driver, "/html/body/div/div[3]/p[2]/a"); // 안전하지않음으로이동

                Util.InputKeys("{Tab},SPACE,{Tab},SPACE"); // MPO Helper

                Util_Control.MoveFormToTop(this);

            }
            catch (Exception ex)
            {
                // MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogManager.LogException(ex, Level.Error);
            }
        }



        

        private void LoginBtn1_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("LoginBtn Click", Level.Info);
            string gateID = GateIDTxt1.Text; // ID 값 가져오기
            string gatePW = GatePWTxt1.Text; // PW 값 가져오기

            // Util.FocusMainWindow(MainHandle);

            // iframe으로 이동
            Util_Control.SendKeysToElement(_driver, "//*[@id='USERID']", gateID);
            Util_Control.SendKeysToElement(_driver, "//*[@id='PASSWD']", gatePW);

            Util_Control.ClickElementByXPath(_driver, "//*[@id='login_submit']");
        }

        private void TestBtn1_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("TestBtn Click", Level.Info);
            Util.FocusMainWindow(mainHandle);
            Util.InvestigateIframesAndCollectClickableElements(_driver);
        }

        private void SearchBtn1_Click(object sender, EventArgs e)
        {
            try
            {
                LogManager.LogMessage("SearchBtn Click", Level.Info);
                Util.ValidateServerInfo(SearchTxt1.Text, out serverName, out serverIP);

                if (!string.IsNullOrEmpty(serverIP))
                {
                    // IP 주소인 경우
                    Util_Control.SendKeysToElement(_driver, "//*[@id='id_IPADDR']", serverIP);
                    Util_Control.SendKeysToElement(_driver, "//*[@id='id_DEVNAME']", "");
                }
                else if (!string.IsNullOrEmpty(serverName))
                {
                    // 서버 이름인 경우
                    Util_Control.SendKeysToElement(_driver, "//*[@id='id_DEVNAME']", serverName);
                    Util_Control.SendKeysToElement(_driver, "//*[@id='id_IPADDR']", "");
                }

                Util_Control.ClickElementByXPath(_driver, "//*[@id='access_control']/table/tbody/tr[2]/td/a");
            }
            catch (ArgumentException ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show(ex.Message, "알림");
            }
            catch (NoSuchElementException ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show("요소를 찾을 수 없습니다.", "오류");
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Critical);
                MessageBox.Show("예상치 못한 오류가 발생했습니다.", "오류");
            }
        }

        private void DisablePopupCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            LogManager.LogMessage("DisablePopupCheckBox CheckedChanged", Level.Info);
            disablePopup = DisablePopupCheckBox1.Checked;
        }

        

        private void btnLoadServers1_Click(object sender, EventArgs e)
        {
            try
            {
                string htmlUrl = "https://10.94.25.179/main/main.php"; // 웹 페이지 HTML 가져오기
                string html = Util_ServerList.GetHtmlFromWeb(htmlUrl);
                var serverNames = Util_ServerList.ParseServerNamesFromHtml(html); // HTML 파싱 및 서버 이름 추출
                Util_ServerList.AddServersToComboBox(ComboBoxServerList1, serverNames); // 콤보박스에 서버 이름 추가
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
            }
        }







        private void MainUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 드라이버 종료
            if (_driver != null)
            {
                Util.CloseDriver(_driver);
                _driver = null;  // 드라이버 객체 해제
            }
            // 프로그램 완전 종료
            LogManager.LogMessage("프로그램 종료", Level.Info);
            Environment.Exit(0);
        }
    }
}