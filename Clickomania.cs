using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Clickomania
{
    public partial class Form1 : Form
    {
        private Random random = new Random();
        private int score = 0;
        private int timeLeft = 30; // время игры 30 секунд
        private System.Windows.Forms.Timer gameTimer; // уточнение пространства имен
        private List<(string name, int score)> scores = new List<(string name, int score)>();

        public Form1()
        {
            InitializeComponent();
            LoadScores();
            this.BackColor = Color.LightBlue; // Сплошной цвет фона
            ShowMainMenu();
            this.Resize += (s, e) => { RecenterMainMenu(); };
        }

        private void LoadScores()
        {
            if (File.Exists("scores.txt"))
            {
                var lines = File.ReadAllLines("scores.txt");
                foreach (var line in lines)
                {
                    var parts = line.Split(' ');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int parsedScore))
                    {
                        scores.Add((parts[0], parsedScore));
                    }
                }
            }
        }

        private void ShowMainMenu()
        {
            Controls.Clear();
            int buttonWidth = 200;
            int buttonHeight = 50;
            int centerX = (this.ClientSize.Width - buttonWidth) / 2;
            int startY = (this.ClientSize.Height - (buttonHeight * 3 + 20)) / 2;

            // Создание и настройка кнопки "Start"
            Button startButton = new Button
            {
                Text = "Start",
                Location = new System.Drawing.Point(centerX, startY),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight)
            };
            startButton.Click += StartButton_Click;
            Controls.Add(startButton);

            // Создание и настройка кнопки "Рейтинг"
            Button scoresButton = new Button
            {
                Text = "Рейтинг",
                Location = new System.Drawing.Point(centerX, startY + buttonHeight + 10),
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight)
            };
            scoresButton.Click += ScoresButton_Click;
            Controls.Add(scoresButton);

            // Создание и настройка кнопки "Выход"
            Button exitButton = new Button
            {
                Text = "Выход",
                Location = new System.Drawing.Point(centerX, startY + (buttonHeight + 10) * 2),
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight)
            };
            exitButton.Click += (s, e) => this.Close();
            Controls.Add(exitButton);
        }

        private void RecenterMainMenu()
        {
            int buttonWidth = 200;
            int buttonHeight = 50;
            int centerX = (this.ClientSize.Width - buttonWidth) / 2;
            int startY = (this.ClientSize.Height - (buttonHeight * 3 + 20)) / 2;

            foreach (Control control in Controls)
            {
                if (control is Button)
                {
                    if (control.Text == "Start")
                    {
                        control.Location = new System.Drawing.Point(centerX, startY);
                    }
                    else if (control.Text == "Рейтинг")
                    {
                        control.Location = new System.Drawing.Point(centerX, startY + buttonHeight + 10);
                    }
                    else if (control.Text == "Выход")
                    {
                        control.Location = new System.Drawing.Point(centerX, startY + (buttonHeight + 10) * 2);
                    }
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            InitializeGame();
        }

        private void ScoresButton_Click(object sender, EventArgs e)
        {
            ShowScores();
        }

        private void InitializeGame()
        {
            Controls.Clear();

            // Инициализация таймера
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 1000; // каждую секунду
            gameTimer.Tick += GameTimer_Tick;

            // Создание и настройка метки для счета
            Label scoreLabel = new Label
            {
                Text = "Score: 0",
                Location = new System.Drawing.Point(10, 10),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            scoreLabel.Name = "scoreLabel";
            Controls.Add(scoreLabel);

            // Создание и настройка метки для оставшегося времени
            Label timerLabel = new Label
            {
                Text = "Time left: 30",
                Location = new System.Drawing.Point(10, 40),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            timerLabel.Name = "timerLabel";
            Controls.Add(timerLabel);

            // Установка фона формы
            this.BackColor = Color.LightBlue; // Сплошной цвет фона

            // Начать игру
            StartGame();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                timeLeft--;
                Controls["timerLabel"].Text = "Time left: " + timeLeft;

                // Добавьте логику появления объектов для кликов
                CreateClickableObject();
            }
            else
            {
                gameTimer.Stop();
                EnterNameAndSaveScore();
            }
        }

        private void StartGame()
        {
            score = 0;
            timeLeft = 30;
            gameTimer.Start();
            Controls["scoreLabel"].Text = "Score: 0";
            Controls["timerLabel"].Text = "Time left: 30";
        }

        private void EnterNameAndSaveScore()
        {
            string name = Prompt.ShowDialog("Введите ваше имя", "Игра окончена! Ваш результат: " + score);
            if (!string.IsNullOrWhiteSpace(name))
            {
                scores.Add((name, score));
                scores = scores.OrderByDescending(s => s.score).Take(10).ToList();
                SaveScores();
                ShowMainMenu();
            }
        }

        private void SaveScores()
        {
            var lines = scores.Select(s => s.name + " " + s.score).ToArray();
            File.WriteAllLines("scores.txt", lines);
        }

        private void ShowScores()
        {
            Controls.Clear();

            // Заголовок
            Label title = new Label
            {
                Text = "Таблица Рекордов",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            title.Location = new Point((this.ClientSize.Width - title.Width) / 2, (this.ClientSize.Height - (scores.Count * 30 + 40)) / 2);
            Controls.Add(title);

            for (int i = 0; i < scores.Count; i++)
            {
                Label scoreLabel = new Label
                {
                    Text = $"{i + 1}. {scores[i].name} - {scores[i].score}",
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleCenter // Центрирование текста
                };
                scoreLabel.Location = new Point((this.ClientSize.Width - scoreLabel.Width) / 2, title.Bottom + 10 + i * 30);
                Controls.Add(scoreLabel);
            }

            // Кнопка "Назад"
            Button backButton = new Button
            {
                Text = "Назад",
                Location = new System.Drawing.Point((this.ClientSize.Width - 100) / 2, title.Bottom + 10 + scores.Count * 30 + 10),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(100, 50)
            };
            backButton.Click += (s, e) => ShowMainMenu();
            Controls.Add(backButton);

            this.Resize += (s, e) => { RecenterScores(); };
        }

        private void RecenterScores()
        {
            Label title = Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Таблица Рекордов");
            if (title != null)
            {
                title.Location = new Point((this.ClientSize.Width - title.Width) / 2, (this.ClientSize.Height - (scores.Count * 30 + 40)) / 2);
                int startY = title.Bottom + 10;
                for (int i = 0; i < scores.Count; i++)
                {
                    Control scoreLabel = Controls.OfType<Label>().ElementAt(i + 1); // +1 чтобы пропустить заголовок
                    scoreLabel.Location = new Point((this.ClientSize.Width - scoreLabel.Width) / 2, startY + i * 30);
                }
                Control backButton = Controls.OfType<Button>().FirstOrDefault(b => b.Text == "Назад");
                if (backButton != null)
                {
                    backButton.Location = new Point((this.ClientSize.Width - 100) / 2, startY + scores.Count * 30 + 10);
                }
            }
        }

        private void CreateClickableObject()
        {
            Button clickableButton = new Button
            {
                Text = "",
                Size = new Size(50, 50),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Location = new Point(random.Next(this.ClientSize.Width - 50), random.Next(this.ClientSize.Height - 50))
            };
            clickableButton.FlatAppearance.BorderSize = 0;

            // Создание кругов разного цвета
            clickableButton.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(Brushes.Red, 0, 0, 50, 50);
                e.Graphics.FillEllipse(Brushes.White, 10, 10, 30, 30);
                e.Graphics.FillEllipse(Brushes.Red, 20, 20, 10, 10);
            };

            clickableButton.Click += (s, e) =>
            {
                score++;
                Controls["scoreLabel"].Text = "Score: " + score;
                Controls.Remove(clickableButton);
            };
            Controls.Add(clickableButton);
        }

        public static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                Form prompt = new Form
                {
                    Width = 400,
                    Height = 200,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen,
                    BackColor = Color.FromArgb(34, 34, 34)
                };
                Label textLabel = new Label() { Left = 50, Top = 20, Text = text, ForeColor = Color.White, AutoSize = true };
                TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 300, Font = new Font("Arial", 12, FontStyle.Regular) };
                Button confirmation = new Button()
                {
                    Text = "OK",
                    Left = 150,
                    Width = 100,
                    Top = 100,
                    DialogResult = DialogResult.OK,
                    BackColor = Color.LightGreen,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }
    }
}
