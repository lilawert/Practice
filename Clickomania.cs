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
        private bool isPaused = false;

        public Form1()
        {
            InitializeComponent();
            this.Text = "Кликомания!"; // Установка заголовка формы
            LoadScores();
            this.BackColor = Color.LightBlue; // Сплошной цвет фона
            ShowMainMenu();
            this.Resize += (s, e) => { RecenterMainMenu(); };
            this.KeyDown += Form1_KeyDown;
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

            // Создание и настройка кнопки "Играть"
            Button startButton = new Button
            {
                Text = "Играть",
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
                    if (control.Text == "Играть")
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
                Text = "Счёт: 0",
                Location = new System.Drawing.Point(10, 10),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            scoreLabel.Name = "scoreLabel";
            Controls.Add(scoreLabel);

            // Создание и настройка метки для оставшегося времени
            Label timerLabel = new Label
            {
                Text = "Оставшееся время: 30",
                Location = new System.Drawing.Point(10, 40),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true
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
            if (!isPaused && timeLeft > 0)
            {
                timeLeft--;
                Controls["timerLabel"].Text = "Оставшееся время: " + timeLeft;

                // Добавьте логику появления объектов для кликов
                CreateClickableObject();
            }
            else if (timeLeft <= 0)
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
            Controls["scoreLabel"].Text = "Счёт: 0";
            Controls["timerLabel"].Text = "Оставшееся время: 30";
        }

        private void EnterNameAndSaveScore()
        {
            string name = Prompt.ShowDialog("Введите ваше имя:", "Игра окончена! Ваш результат: " + score);
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

            // Создание панели для таблицы рекордов
            Panel scoresPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(scoresPanel);

            // Заголовок
            Label title = new Label
            {
                Text = "Таблица Рекордов",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            title.TextAlign = ContentAlignment.MiddleCenter;

            scoresPanel.Controls.Add(title);

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
                scoresPanel.Controls.Add(scoreLabel);
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
            scoresPanel.Controls.Add(backButton);

            // Центрирование содержимого панели
            CenterPanelContent(scoresPanel);

            this.Resize += (s, e) => { CenterPanelContent(scoresPanel); };
        }

        private void CenterPanelContent(Panel panel)
        {
            int totalHeight = 0;
            foreach (Control control in panel.Controls)
            {
                totalHeight += control.Height;
            }

            int yOffset = (panel.ClientSize.Height - totalHeight) / 2;
            foreach (Control control in panel.Controls)
            {
                control.Location = new Point((panel.ClientSize.Width - control.Width) / 2, yOffset);
                yOffset += control.Height + 10; // Добавление отступа между элементами
            }
        }

        private void RecenterScores()
        {
            Panel scoresPanel = Controls.OfType<Panel>().FirstOrDefault();
            if (scoresPanel != null)
            {
                CenterPanelContent(scoresPanel);
            }
        }

        private void CreateClickableObject()
        {
            PictureBox clickableObject = new PictureBox
            {
                Size = new Size(50, 50), // Размер объекта
                BackColor = Color.Transparent,
                Location = new Point(random.Next(0, this.ClientSize.Width - 50), random.Next(0, this.ClientSize.Height - 50))
            };
            Bitmap targetBitmap = new Bitmap(clickableObject.Width, clickableObject.Height);
            using (Graphics g = Graphics.FromImage(targetBitmap))
            {
                g.Clear(Color.Transparent); // Прозрачный фон
                DrawTarget(g, clickableObject.Width / 2, clickableObject.Height / 2, clickableObject.Width / 2);
            }
            clickableObject.Image = targetBitmap;
            clickableObject.Click += (s, e) =>
            {
                score++;
                Controls["scoreLabel"].Text = "Счёт: " + score;
                Controls.Remove(clickableObject);
            };
            Controls.Add(clickableObject);
        }

        private void DrawTarget(Graphics g, int centerX, int centerY, int radius)
        {
            Color[] colors = { Color.Red, Color.White, Color.Red, Color.White, Color.Red }; // Чередующиеся цвета
            for (int i = 0; i < colors.Length; i++)
            {
                using (SolidBrush brush = new SolidBrush(colors[i]))
                {
                    int currentRadius = radius - (radius / colors.Length) * i;
                    g.FillEllipse(brush, centerX - currentRadius, centerY - currentRadius, currentRadius * 2, currentRadius * 2);
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                TogglePauseMenu();
            }
        }

        private void TogglePauseMenu()
        {
            if (isPaused)
            {
                foreach (Control control in Controls.OfType<Button>().Where(b => b.Text == "Продолжить" || b.Text == "Главное меню" || b.Text == "Выход"))
                {
                    control.Visible = false;
                }
                gameTimer.Start();
                isPaused = false;
            }
            else
            {
                gameTimer.Stop();
                ShowPauseMenu();
                isPaused = true;
            }
        }

        private void ShowPauseMenu()
        {
            int buttonWidth = 200;
            int buttonHeight = 50;
            int centerX = (this.ClientSize.Width - buttonWidth) / 2;
            int startY = (this.ClientSize.Height - (buttonHeight * 3 + 20)) / 2;

            // Создание и настройка кнопки "Продолжить"
            Button continueButton = new Button
            {
                Text = "Продолжить",
                Location = new System.Drawing.Point(centerX, startY),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight),
                Visible = true
            };
            continueButton.Click += (s, e) => TogglePauseMenu();
            Controls.Add(continueButton);

            // Создание и настройка кнопки "Главное меню"
            Button mainMenuButton = new Button
            {
                Text = "Главное меню",
                Location = new System.Drawing.Point(centerX, startY + buttonHeight + 10),
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight),
                Visible = true
            };
            mainMenuButton.Click += (s, e) =>
            {
                isPaused = false;
                ShowMainMenu();
            };
            Controls.Add(mainMenuButton);

            // Создание и настройка кнопки "Выход"
            Button exitButton = new Button
            {
                Text = "Выход",
                Location = new System.Drawing.Point(centerX, startY + (buttonHeight + 10) * 2),
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight),
                Visible = true
            };
            exitButton.Click += (s, e) => this.Close();
            Controls.Add(exitButton);
        }
    }

    // Вспомогательный класс для ввода имени
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
                BackColor = Color.LightBlue // Соответствие фону основной формы
            };

            Label textLabel = new Label()
            {
                Text = text,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter // Центрирование текста внутри метки
            };

            // Центрирование метки по форме
            textLabel.Location = new Point(
                (prompt.ClientSize.Width - textLabel.Width) / 2,
                20
            );

            TextBox textBox = new TextBox()
            {
                Left = 20,
                Top = textLabel.Bottom + 10,
                Width = 340,
                Font = new Font("Arial", 12)
            };

            Button confirmation = new Button()
            {
                Text = "Сохранить",
                Left = (prompt.ClientSize.Width - 200) / 2,
                Width = 200,
                Top = textBox.Bottom + 10,
                Height = 50,
                DialogResult = DialogResult.OK,
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 12, FontStyle.Bold),
            };

            confirmation.Click += (sender, e) => { prompt.Close(); };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            // Обновление формы после добавления всех элементов
            prompt.Resize += (s, e) =>
            {
                // Переустановка расположения метки, если размеры формы изменяются
                textLabel.Location = new Point(
                    (prompt.ClientSize.Width - textLabel.PreferredWidth) / 2,
                    20
                );
                textBox.Left = (prompt.ClientSize.Width - textBox.Width) / 2;
                confirmation.Left = (prompt.ClientSize.Width - confirmation.Width) / 2;
            };

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
