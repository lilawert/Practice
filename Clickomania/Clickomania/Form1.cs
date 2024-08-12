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
        private int timeLeft = 30; // ����� ���� 30 ������
        private System.Windows.Forms.Timer gameTimer; // ��������� ������������ ����
        private List<(string name, int score)> scores = new List<(string name, int score)>();
        private bool isPaused = false;
        private SoundPlayer backgroundMusicPlayer;
        private bool isMusicPlaying = true;
        private Button musicToggleButton;
        private Panel dimPanel;
        private List<PictureBox> backgroundThumbnails = new List<PictureBox>();
        private Image[] backgroundImages = new Image[5]; // ������ ��� �������� ������� �����������

        public Form1()
        {
            InitializeComponent();
            this.Text = "����������!"; // ��������� ��������� �����
            LoadScores();
            this.BackColor = Color.LightBlue; // �������� ���� ����
            LoadBackgroundImages(); // �������� �����
            ShowMainMenu();
            this.Resize += (s, e) => { RecenterMainMenu(); };
            this.KeyDown += Form1_KeyDown;
            backgroundMusicPlayer = new SoundPlayer("dystopia.wav");
            backgroundMusicPlayer.PlayLooping(); // ������ ������ � �����
            InitializeDimPanel(); // ������������� ������ ����������
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
            backgroundImages[0] = Resources.background1; // �������� �� ��� ����� ��������
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
                thumbnail.Dispose(); // ������������ ��������
            }
            backgroundThumbnails.Clear();
        }

        private void ShowMainMenu()
        {
            Controls.Clear();
            ClearBackgroundThumbnails(); // ������� ������ ��������
            int buttonWidth = 200;
            int buttonHeight = 50;
            int centerX = (this.ClientSize.Width - buttonWidth) / 2;
            int startY = (this.ClientSize.Height - (buttonHeight * 3 + 20)) / 2;

            // �������� � ��������� ������ "������"
            Button startButton = new Button
            {
                Text = "������",
                Location = new System.Drawing.Point(centerX, startY),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight)
            };
            startButton.Click += StartButton_Click;
            Controls.Add(startButton);

            // �������� � ��������� ������ "�������"
            Button scoresButton = new Button
            {
                Text = "�������",
                Location = new System.Drawing.Point(centerX, startY + buttonHeight + 10),
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight)
            };
            scoresButton.Click += ScoresButton_Click;
            Controls.Add(scoresButton);

            // �������� � ��������� ������ "�����"
            Button exitButton = new Button
            {
                Text = "�����",
                Location = new System.Drawing.Point(centerX, startY + (buttonHeight + 10) * 2),
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight)
            };
            exitButton.Click += (s, e) => this.Close();
            Controls.Add(exitButton);

            // �������� � ��������� ������ ���������� �������
            musicToggleButton = new Button
            {
                Size = new Size(50, 50),
                Location = new Point(10, this.ClientSize.Height - 60),
                BackColor = Color.LightPink,
                BackgroundImageLayout = ImageLayout.Zoom
            };
            musicToggleButton.BackgroundImage = Resources.soundon; // ������ ���������
            musicToggleButton.Click += musicToggleButton_Click;
            Controls.Add(musicToggleButton);

            // ��������� �������� ������ � ������ ������� ����
            musicToggleButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // ��������� ��������� ������� �����
            this.Resize += (s, e) =>
            {
                // ���������� ������������ ������ � ������ ��������� ������� �����
                musicToggleButton.Location = new Point(10, this.ClientSize.Height - musicToggleButton.Height - 10);
            };

            // �������� � ���������� ����� ��������
            LoadBackgroundThumbnails();

            // ������� "�������� ���"
            Label chooseBackgroundLabel = new Label
            {
                Text = "�������� ���",
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(chooseBackgroundLabel);

            // ��������� �����
            int thumbnailWidth = 40;
            int thumbnailHeight = 40;
            int thumbnailSpacing = 10; // ���������� ����� �����������

            // ����������� ��������� � ������ ��������� ��������� �������
            this.Resize += (s, e) =>
            {
                RecenterMainMenu();
                RepositionBackgroundElements(chooseBackgroundLabel, thumbnailWidth, thumbnailHeight, thumbnailSpacing);
            };

            // ����������� ���������� ���������
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

            // ��������� ��������� ������� �����
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

                // �������� ��������� ����� ���������� `i`
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

            // ������� �������� � �������
            int startX = (this.ClientSize.Width - totalThumbnailWidth) / 2;
            int startY = this.ClientSize.Height - thumbnailHeight - 20;

            // ������� �������
            chooseBackgroundLabel.Location = new Point((this.ClientSize.Width - chooseBackgroundLabel.Width) / 2, startY - chooseBackgroundLabel.Height - 10);

            // ������� ��������
            for (int i = 0; i < backgroundThumbnails.Count; i++)
            {
                backgroundThumbnails[i].Location = new Point(startX + i * (thumbnailWidth + thumbnailSpacing), startY);
            }
        }

        private void ApplyBackground(Image backgroundImage)
        {
            this.BackgroundImage = backgroundImage;
            this.BackgroundImageLayout = ImageLayout.Stretch; // ��� ������ ����� ����������
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
                    if (control.Text == "������")
                    {
                        control.Location = new System.Drawing.Point(centerX, startY);
                    }
                    else if (control.Text == "�������")
                    {
                        control.Location = new System.Drawing.Point(centerX, startY + buttonHeight + 10);
                    }
                    else if (control.Text == "�����")
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
                musicToggleButton.BackgroundImage = Resources.soundoff; // ������ ������������ ���������
                // �������� ����� ������ ��� ������ ������� ����������, ����� ��������, ��� ������ ���������
            }
            else
            {
                backgroundMusicPlayer.PlayLooping();
                musicToggleButton.BackgroundImage = Resources.soundon; // ������ ���������
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

            // ������������� �������
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 1000; // ������ �������
            gameTimer.Tick += GameTimer_Tick;

            // �������� � ��������� ����� ��� �����
            Label scoreLabel = new Label
            {
                Text = "����: 0",
                Location = new System.Drawing.Point(10, 10),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            scoreLabel.Name = "scoreLabel";
            Controls.Add(scoreLabel);

            // �������� � ��������� ����� ��� ����������� �������
            Label timerLabel = new Label
            {
                Text = "���������� �����: 30",
                Location = new System.Drawing.Point(10, 40),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            timerLabel.Name = "timerLabel";
            Controls.Add(timerLabel);

            // ��������� ���� �����
            this.BackColor = Color.LightBlue; // �������� ���� ����

            // ������ ����
            StartGame();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!isPaused && timeLeft > 0)
            {
                timeLeft--;
                Controls["timerLabel"].Text = "���������� �����: " + timeLeft;

                // �������� ������ ��������� �������� ��� ������
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
            Controls["scoreLabel"].Text = "����: 0";
            Controls["timerLabel"].Text = "���������� �����: 30";
        }

        private void EnterNameAndSaveScore()
        {
            string name = Prompt.ShowDialog("������� ���� ���:", "���� ��������! ��� ���������: " + score);
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

            // �������� ������ ��� ������� ��������
            Panel scoresPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(scoresPanel);

            // ���������
            Label title = new Label
            {
                Text = "������� ��������",
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
                    TextAlign = ContentAlignment.MiddleCenter // ������������� ������
                };
                scoresPanel.Controls.Add(scoreLabel);
            }

            // ������ "�������� �������"
            Button clearButton = new Button
            {
                Text = "�������� �������",
                Location = new System.Drawing.Point((this.ClientSize.Width - 100) / 2, title.Bottom + 10 + scores.Count * 30 + 10),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(200, 50)
            };
            clearButton.Click += ClearScores;
            scoresPanel.Controls.Add(clearButton);

            // ������ "�����"
            Button backButton = new Button
            {
                Text = "�����",
                Location = new System.Drawing.Point((this.ClientSize.Width - 150) / 2, clearButton.Bottom + 10),
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(100, 50)
            };
            backButton.Click += (s, e) => ShowMainMenu();
            scoresPanel.Controls.Add(backButton);

            // ������������� ����������� ������
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
            ShowScores(); // ���������� ������ � ���������
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
                yOffset += control.Height + 10; // ���������� ������� ����� ����������
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
                Size = new Size(50, 50), // ������ �������
                BackColor = Color.Transparent,
                Location = new Point(random.Next(0, this.ClientSize.Width - 50), random.Next(0, this.ClientSize.Height - 50))
            };
            Bitmap targetBitmap = new Bitmap(clickableObject.Width, clickableObject.Height);
            using (Graphics g = Graphics.FromImage(targetBitmap))
            {
                g.Clear(Color.Transparent); // ���������� ���
                DrawTarget(g, clickableObject.Width / 2, clickableObject.Height / 2, clickableObject.Width / 2);
            }
            clickableObject.Image = targetBitmap;
            clickableObject.Click += (s, e) =>
            {
                score++;
                Controls["scoreLabel"].Text = "����: " + score;
                Controls.Remove(clickableObject);
            };
            Controls.Add(clickableObject);
        }

        private void DrawTarget(Graphics g, int centerX, int centerY, int radius)
        {
            Color[] colors = { Color.Red, Color.White, Color.Red, Color.White, Color.Red }; // ������������ �����
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
                BackColor = Color.FromArgb(128, 0, 0, 0), // �������������� ������
                Size = this.ClientSize,
                Location = new Point(0, 0),
                Visible = false // ������� �����
            };
            this.Controls.Add(dimPanel);
            this.Resize += (s, e) => dimPanel.Size = this.ClientSize; // �������� ������� ��� ��������� �������� �����
        }

        private void ShowPauseMenu()
        {

            // ����������� ������ ����������
            dimPanel.Visible = true;
            this.Controls.Add(dimPanel); // ������������� ��������� �� �������� ����
            dimPanel.BringToFront(); // ����������� ������ �� �������� ����
            dimPanel.Controls.Clear();

            // ������� ������ ����� ����������� ����� ������
            dimPanel.Controls.Clear();

            int buttonWidth = 200;
            int buttonHeight = 50;
            int centerX = (this.ClientSize.Width - buttonWidth) / 2;
            int startY = (this.ClientSize.Height - (buttonHeight * 3 + 20)) / 2;

            // �������� � ��������� ������ "����������"
            Button continueButton = new Button
            {
                Text = "����������",
                Location = new System.Drawing.Point(centerX, startY),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight)
            };
            continueButton.Click += (s, e) => TogglePauseMenu();
            dimPanel.Controls.Add(continueButton);

            // �������� � ��������� ������ "������� ����"
            Button mainMenuButton = new Button
            {
                Text = "������� ����",
                Location = new System.Drawing.Point(centerX, startY + buttonHeight + 10),
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight)
            };
            mainMenuButton.Click += (s, e) =>
            {
                isPaused = false;
                dimPanel.Visible = false; // ������� ������ ����������
                ShowMainMenu();
            };
            dimPanel.Controls.Add(mainMenuButton);

            // �������� � ��������� ������ "�����"
            Button exitButton = new Button
            {
                Text = "�����",
                Location = new System.Drawing.Point(centerX, startY + (buttonHeight + 10) * 2),
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(buttonWidth, buttonHeight),
            };
            exitButton.Click += (s, e) => this.Close();
            dimPanel.Controls.Add(exitButton);

            // ���������, ��� ������ ��������� ������ ������ ����������
            dimPanel.BringToFront();
        }

        private void TogglePauseMenu()
        {
            if (isPaused)
            {
                gameTimer.Start();
                isPaused = false;
                dimPanel.Visible = false; // ������� ������ ����������
            }
            else
            {
                gameTimer.Stop();
                ShowPauseMenu();
                isPaused = true;
            }
        }
    }

    // ��������������� ����� ��� ����� �����
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
                BackColor = Color.LightBlue // ������������ ���� �������� �����
            };

            Label textLabel = new Label()
            {
                Text = text,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter // ������������� ������ ������ �����
            };

            // ������������� MeasureString ��� ���������� ������ ������ ������
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
                Text = "���������",
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

            // ���������� ����� ����� ���������� ���� ���������
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