using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Media;


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
        private SoundPlayer backgroundMusicPlayer;
        private bool isMusicPlaying = true;
        private Button musicToggleButton;
        private Panel dimPanel;
        private List<PictureBox> backgroundThumbnails = new List<PictureBox>();
        private Image[] backgroundImages = new Image[5]; // Массив для хранения фоновых изображений

        public Form1()
        {
            InitializeComponent();
            this.Text = "Кликомания!"; // Установка заголовка формы
            LoadScores();
            this.BackColor = Color.LightBlue; // Сплошной цвет фона
            LoadBackgroundImages(); // Загрузка фонов
            ShowMainMenu();
            this.Resize += (s, e) => { RecenterMainMenu(); };
            this.KeyDown += Form1_KeyDown;
            backgroundMusicPlayer = new SoundPlayer("dystopia.wav");
            backgroundMusicPlayer.PlayLooping(); // Запуск музыки в цикле
            InitializeDimPanel(); // Инициализация панели затемнения
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

        private void LoadBackgroundImages()
        {
            backgroundImages[0] = Resources.background1; // Измените на имя ваших ресурсов
            backgroundImages[1] = Resources.background2;
            backgroundImages[2] = Resources.background3;
            backgroundImages[3] = Resources.background4;
            backgroundImages[4] = Resources.background5;
        }

        private void ClearBackgroundThumbnails()
        {
            foreach (var thumbnail in backgroundThumbnails)
            {
                Controls.Remove(thumbnail);
                thumbnail.Dispose(); // Освобождение ресурсов
            }
            backgroundThumbnails.Clear();
        }

        private void ShowMainMenu()
        {
            Controls.Clear();
            ClearBackgroundThumbnails(); // Очистка старых миниатюр
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

            // Создание и настройка кнопки управления музыкой
            musicToggleButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(10, this.ClientSize.Height - 60),
                BackColor = Color.LightPink,
                BackgroundImageLayout = ImageLayout.Zoom
            };
            musicToggleButton.BackgroundImage = Resources.soundon; // Иконка микрофона
            musicToggleButton.Click += musicToggleButton_Click;
            Controls.Add(musicToggleButton);

            // Установка привязки кнопки к левому нижнему углу
            musicToggleButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // Обработка изменения размера формы
            this.Resize += (s, e) =>
            {
                // Обновление расположения кнопки в случае изменения размера формы
                musicToggleButton.Location = new Point(10, this.ClientSize.Height - musicToggleButton.Height - 10);
            };

            // Загрузка и размещение новых миниатюр
            LoadBackgroundThumbnails();

            // Подпись "Выберите фон"
            Label chooseBackgroundLabel = new Label
            {
                Text = "Выберите фон",
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(chooseBackgroundLabel);

            // Миниатюры фонов
            int thumbnailWidth = 40;
            int thumbnailHeight = 40;
            int thumbnailSpacing = 10; // Расстояние между миниатюрами

            // Перемещение элементов в методе обработки изменения размера
            this.Resize += (s, e) =>
            {
                RecenterMainMenu();
                RepositionBackgroundElements(chooseBackgroundLabel, thumbnailWidth, thumbnailHeight, thumbnailSpacing);
            };

            // Изначальное размещение элементов
            RepositionBackgroundElements(chooseBackgroundLabel, thumbnailWidth, thumbnailHeight, thumbnailSpacing);

            Label footerLabel = new Label
            {
                Text = "by Terai <33",
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            footerLabel.Location = new Point(this.ClientSize.Width - footerLabel.Width - 10, this.ClientSize.Height - footerLabel.Height - 10);
            Controls.Add(footerLabel);

            // Обработка изменения размера формы
            this.Resize += (s, e) =>
            {
                footerLabel.Location = new Point(this.ClientSize.Width - footerLabel.Width - 10, this.ClientSize.Height - footerLabel.Height - 10);
            };
        }

        private void LoadBackgroundThumbnails()
        {
            int thumbnailWidth = 40;
            int thumbnailHeight = 40;
            int thumbnailSpacing = 10;

            for (int i = 0; i < backgroundImages.Length; i++)
            {
                PictureBox thumbnail = new PictureBox
                {
                    Image = backgroundImages[i],
                    Size = new Size(thumbnailWidth, thumbnailHeight),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Создание локальной копии переменной `i`
                int index = i;

                thumbnail.Click += (s, e) =>
                {
                    ApplyBackground(backgroundImages[index]);
                };

                backgroundThumbnails.Add(thumbnail);
                Controls.Add(thumbnail);
            }
        }

        private void RepositionBackgroundElements(Label chooseBackgroundLabel, int thumbnailWidth, int thumbnailHeight, int thumbnailSpacing)
        {
            int thumbnailCount = backgroundThumbnails.Count;
            int totalThumbnailWidth = thumbnailWidth * thumbnailCount + thumbnailSpacing * (thumbnailCount - 1);

            // Позиция миниатюр и подписи
            int startX = (this.ClientSize.Width - totalThumbnailWidth) / 2;
            int startY = this.ClientSize.Height - thumbnailHeight - 20;

            // Позиция подписи
            chooseBackgroundLabel.Location = new Point((this.ClientSize.Width - chooseBackgroundLabel.Width) / 2, startY - chooseBackgroundLabel.Height - 10);

            // Позиция миниатюр
            for (int i = 0; i < backgroundThumbnails.Count; i++)
            {
                backgroundThumbnails[i].Location = new Point(startX + i * (thumbnailWidth + thumbnailSpacing), startY);
            }
        }

        private void ApplyBackground(Image backgroundImage)
        {
            this.BackgroundImage = backgroundImage;
            this.BackgroundImageLayout = ImageLayout.Stretch; // или другой режим размещения
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

        private void ToggleMusic()
        {
            if (isMusicPlaying)
            {
                backgroundMusicPlayer.Stop();
                musicToggleButton.BackgroundImage = Resources.soundoff; // Иконка зачеркнутого микрофона
                // Обновите текст кнопки или другой элемент интерфейса, чтобы показать, что музыка выключена
            }
            else
            {
                backgroundMusicPlayer.PlayLooping();
                musicToggleButton.BackgroundImage = Resources.soundon; // Иконка микрофона
            }
            isMusicPlaying = !isMusicPlaying;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            InitializeGame();
        }

        private void ScoresButton_Click(object sender, EventArgs e)
        {
            ShowScores();
        }

        private void musicToggleButton_Click(object sender, EventArgs e)
        {
            ToggleMusic();
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

            // Кнопка "Очистить рейтинг"
            Button clearButton = new Button
            {
                Text = "Очистить рейтинг",
                Location = new System.Drawing.Point((this.ClientSize.Width - 100) / 2, title.Bottom + 10 + scores.Count * 30 + 10),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(200, 50)
            };
            clearButton.Click += ClearScores;
            scoresPanel.Controls.Add(clearButton);

            // Кнопка "Назад"
            Button backButton = new Button
            {
                Text = "Назад",
                Location = new System.Drawing.Point((this.ClientSize.Width - 150) / 2, clearButton.Bottom + 10),
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(100, 50)
            };
            backButton.Click += (s, e) => ShowMainMenu();
            scoresPanel.Controls.Add(backButton);

            // Центрирование содержимого панели
            CenterPanelContent(scoresPanel);

            this.Resize += (s, e) => { CenterPanelContent(scoresPanel); };
        }

        private void ClearScores(object sender, EventArgs e)
        {
            scores.Clear();
            if (File.Exists("scores.txt"))
            {
                File.Delete("scores.txt");
            }
            ShowScores(); // Обновление экрана с рейтингом
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

        private void InitializeDimPanel()
        {
            dimPanel = new Panel
            {
                BackColor = Color.FromArgb(128, 0, 0, 0), // Полупрозрачный черный
                Size = this.ClientSize,
                Location = new Point(0, 0),
                Visible = false // Сначала скрыт
            };
            this.Controls.Add(dimPanel);
            this.Resize += (s, e) => dimPanel.Size = this.ClientSize; // Подгонка размера при изменении размеров формы
        }

        private void ShowPauseMenu()
        {

            // Отображение панели затемнения
            dimPanel.Visible = true;
            this.Controls.Add(dimPanel); // Принудительно добавляем на передний план
            dimPanel.BringToFront(); // Перемещение панели на передний план
            dimPanel.Controls.Clear();

            // Очистка панели перед добавлением новых кнопок
            dimPanel.Controls.Clear();

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
                Size = new Size(buttonWidth, buttonHeight)
            };
            continueButton.Click += (s, e) => TogglePauseMenu();
            dimPanel.Controls.Add(continueButton);

            // Создание и настройка кнопки "Главное меню"
            Button mainMenuButton = new Button
            {
                Text = "Главное меню",
                Location = new System.Drawing.Point(centerX, startY + buttonHeight + 10),
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight)
            };
            mainMenuButton.Click += (s, e) =>
            {
                isPaused = false;
                dimPanel.Visible = false; // Скрытие панели затемнения
                ShowMainMenu();
            };
            dimPanel.Controls.Add(mainMenuButton);

            // Создание и настройка кнопки "Выход"
            Button exitButton = new Button
            {
                Text = "Выход",
                Location = new System.Drawing.Point(centerX, startY + (buttonHeight + 10) * 2),
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight),
            };
            exitButton.Click += (s, e) => this.Close();
            dimPanel.Controls.Add(exitButton);

            // Убедитесь, что кнопки находятся поверх панели затемнения
            dimPanel.BringToFront();
        }

        private void TogglePauseMenu()
        {
            if (isPaused)
            {
                gameTimer.Start();
                isPaused = false;
                dimPanel.Visible = false; // Скрытие панели затемнения
            }
            else
            {
                gameTimer.Stop();
                ShowPauseMenu();
                isPaused = true;
            }
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

            // Использование MeasureString для вычисления точной ширины текста
            using (Graphics g = prompt.CreateGraphics())
            {
                SizeF textSize = g.MeasureString(textLabel.Text, textLabel.Font);
                textLabel.Location = new Point(
                    (prompt.ClientSize.Width - (int)textSize.Width) / 2,
                    20
                );
            }

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
                using (Graphics g = prompt.CreateGraphics())
                {
                    SizeF textSize = g.MeasureString(textLabel.Text, textLabel.Font);
                    textLabel.Location = new Point(
                        (prompt.ClientSize.Width - (int)textSize.Width) / 2,
                        20
                    );
                }
                textBox.Left = (prompt.ClientSize.Width - textBox.Width) / 2;
                confirmation.Left = (prompt.ClientSize.Width - confirmation.Width) / 2;
            };

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
