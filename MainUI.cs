﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Drawing;
using MaterialSkin;
using MaterialSkin.Controls;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GateBot
{
    public partial class MainUI : MaterialForm
    {
        private readonly MaterialSkinManager materialSkinManager;

        public static IWebDriver driver = null;


        public MainUI()
        {
            InitializeComponent();
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;

            // 색상 변경
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue900, Primary.Blue700, Accent.LightBlue200, TextShade.WHITE);

            // 폼의 크기를 600 x 900으로 설정
            this.Size = new Size(400, 600);
        }

        private async void StartBtn1_Click(object sender, EventArgs e)
        {
            try
            {
                // 비동기로 드라이버 초기화
                driver = await Task.Run(() => Util.InitializeDriver());

                // 열려 있는 탭에서 URL 확인
                await Task.Run(() => Util.CheckOpenUrls(driver));
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 사용자에게 메시지 표시
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 드라이버 종료
                if (driver != null)
                {
                    Util.CloseDriver(driver);
                }
            }
        }
    }
}
