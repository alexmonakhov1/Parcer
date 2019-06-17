using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Parcer
{
    public partial class Form1 : Form
    {
        List<string> list = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonGetValue_Click_1(object sender, EventArgs e)
        {
            if (comboBoxValue.SelectedItem == null)
            {
                labelValue.Text = "Вы не выбрали валюту";
                labelValue.ForeColor = System.Drawing.Color.Red;
                labelValue.Visible = true;
            }

            else
            {
                var jObject = JObject.Parse(AppSettings.Json);

                //забираем значение из value
                var CharCode = jObject["Valute"][comboBoxValue.SelectedItem.ToString()]["Value"];

                labelValue.Text = "Текущий курс: " + CharCode.ToString();
                labelValue.ForeColor = System.Drawing.Color.Black;
                labelValue.Visible = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            // try-catch для безинтернетных
            try
            {
                request = (HttpWebRequest)WebRequest.Create(AppSettings.URL);
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(WebException ex)
            {
                MessageBox.Show(ex.Message + ".");
                Application.Exit();
            }

            if (response != null)
            {
                //получаем полный JSON
                StreamReader myStreamReader = new StreamReader(response.GetResponseStream());
                AppSettings.Json = myStreamReader.ReadToEnd();

                var jObject = JObject.Parse(AppSettings.Json);

                Regex regex = new Regex(@"[A-Z]{3}");

                //здесь забираем все совпадения по [A-Z]{3} в Valute
                MatchCollection matches = regex.Matches(jObject["Valute"].ToString());

                if (matches.Count > 0)
                {
                    //закидываем все совпадения в лист
                    foreach (Match match in matches)
                        list.Add(match.ToString());

                    //удаляем повторения
                    list = list.Distinct().ToList();

                    //заносим названия валют в список
                    foreach (var item in list)
                        comboBoxValue.Items.Add(item);
                }
                else
                {
                    DialogResult result = MessageBox.Show("Не удалось получить значения с удаленного сервера. \nПопробовать еще раз?", "Ошибка", MessageBoxButtons.YesNo);

                    //повторный бесконечный вызов для попытки парсинга Джейсона
                    if (result == DialogResult.Yes)
                        Form1_Load(sender, e);
                    else
                    {
                        MessageBox.Show("Ошибка! Не удалось получить названия валют.\nПриложение будет закрыто.");
                        Close();
                    }
                }
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
