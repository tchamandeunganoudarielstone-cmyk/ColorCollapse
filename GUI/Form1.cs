using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using NAudio.Wave;

namespace GUI
{
    public partial class Form1 : Form
    {
        GameLogic logique = new GameLogic();
        Button[,] boutons = new Button[10, 10];
        Label lblScore = new Label();
        Label lblMeilleur = new Label();
        Button btnNouveau = new Button();

        private WaveOutEvent musicPlayer = new WaveOutEvent();
        private AudioFileReader? musicReader;
        private WaveOutEvent sfxPlayer = new WaveOutEvent();

        public Form1()
        {
            InitializeComponent();
            this.Text = "Color Collapse";
            this.Size = new Size(520, 650);
            this.MinimumSize = new Size(520, 650);
            this.MaximumSize = new Size(520, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Icon = new Icon("ico.ico");

            InitialiserInterface();
            CreerGrille();
            ChargerMeilleurScore();
            DemarrerMusique();
        }
        private void DemarrerMusique()
        {
            if (!File.Exists("music1.wav")) return;
            musicReader = new AudioFileReader("music1.wav");
            musicReader.Volume = 0.5f;
            musicPlayer = new WaveOutEvent();
            musicPlayer.Init(musicReader);
            musicPlayer.PlaybackStopped += (s, e) =>
            {
                if (musicReader != null)
                {
                    musicReader.Position = 0;
                    musicPlayer.Play();
                }
            };
            musicPlayer.Play();
        }

        
        private void JouerSonElimination()
        {
            if (!File.Exists("music2.wav")) return;
            if (musicReader != null)
                musicReader.Volume = 0.1f;

            var sfxReader = new AudioFileReader("music2.wav");
            sfxPlayer = new WaveOutEvent();
            sfxPlayer.Init(sfxReader);
            sfxPlayer.PlaybackStopped += (s, e) =>
            {
                if (musicReader != null)
                    musicReader.Volume = 0.5f;
                sfxReader.Dispose();
            };
            sfxPlayer.Play();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            DialogResult result = MessageBox.Show(
                "Voulez-vous vraiment quitter ?",
                "Quitter",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                musicPlayer.Stop();
                musicReader?.Dispose();
            }
        }

        private void InitialiserInterface()
        {
            lblScore.Text = "Score : 0";
            lblScore.Left = 10;
            lblScore.Top = 10;
            lblScore.Width = 150;
            lblScore.ForeColor = Color.White;
            lblScore.Font = new Font("Arial", 11, FontStyle.Bold);

            lblMeilleur.Text = "Meilleur : 0";
            lblMeilleur.Left = 200;
            lblMeilleur.Top = 10;
            lblMeilleur.Width = 150;
            lblMeilleur.ForeColor = Color.Gold;
            lblMeilleur.Font = new Font("Arial", 11, FontStyle.Bold);

            btnNouveau.Text = "Nouvelle partie";
            btnNouveau.Left = 10;
            btnNouveau.Top = 560;
            btnNouveau.Width = 150;
            btnNouveau.BackColor = Color.FromArgb(50, 50, 50);
            btnNouveau.ForeColor = Color.White;
            btnNouveau.FlatStyle = FlatStyle.Flat;
            btnNouveau.Click += (s, e) => {
                logique.score = 0;
                lblScore.Text = "Score : 0";
                this.Controls.Clear();
                this.Controls.Add(lblScore);
                this.Controls.Add(lblMeilleur);
                this.Controls.Add(btnNouveau);
                boutons = new Button[10, 10];
                CreerGrille();
            };

            this.Controls.Add(lblScore);
            this.Controls.Add(lblMeilleur);
            this.Controls.Add(btnNouveau);
        }

        private void CreerGrille()
        {
            logique.GenererGrille();
            for (int ligne = 0; ligne < 10; ligne++)
            {
                for (int colonne = 0; colonne < 10; colonne++)
                {
                    Button btn = new Button();
                    btn.Width = 50;
                    btn.Height = 50;
                    btn.Left = colonne * 50;
                    btn.Top = ligne * 50 + 40;
                    btn.Tag = new Point(ligne, colonne);
                    btn.BackColor = ObtenirCouleur(logique.grille[ligne, colonne]);
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = Color.FromArgb(30, 30, 30);
                    btn.FlatAppearance.BorderSize = 2;
                    btn.Click += BoutonClick;
                    boutons[ligne, colonne] = btn;
                    this.Controls.Add(btn);
                }
            }
        }

        private Color ObtenirCouleur(GameLogic.Couleur couleur)
        {
            switch (couleur)
            {
                case GameLogic.Couleur.Rouge: return Color.FromArgb(220, 50, 50);
                case GameLogic.Couleur.Bleu: return Color.FromArgb(50, 100, 220);
                case GameLogic.Couleur.Vert: return Color.FromArgb(50, 200, 80);
                case GameLogic.Couleur.Jaune: return Color.FromArgb(240, 200, 30);
                default: return Color.White;
            }
        }

        private void ChargerMeilleurScore()
        {
            if (File.Exists("bestscore.txt"))
            {
                string contenu = File.ReadAllText("bestscore.txt");
                if (int.TryParse(contenu, out int score))
                {
                    logique.meilleurScore = score;
                    lblMeilleur.Text = "Meilleur : " + score;
                }
            }
        }

        private void BoutonClick(object? sender, EventArgs e)
        {
            Button btn = (Button)sender!;
            Point pos = (Point)btn.Tag!;
            int ligne = pos.X;
            int colonne = pos.Y;

            List<Point> cases = logique.TrouverCasesConnectees(ligne, colonne);

            if (cases.Count < 2)
                return;

            foreach (Point p in cases)
            {
                logique.grille[p.X, p.Y] = (GameLogic.Couleur)(-1);
                boutons[p.X, p.Y].Visible = false;
            }

            JouerSonElimination();

            logique.score += cases.Count;
            lblScore.Text = "Score : " + logique.score;

            if (logique.score > logique.meilleurScore)
            {
                logique.meilleurScore = logique.score;
                lblMeilleur.Text = "Meilleur : " + logique.meilleurScore;
                File.WriteAllText("bestscore.txt", logique.meilleurScore.ToString());
            }

            if (logique.EstPartieTerminer())
            {
                DialogResult result = MessageBox.Show(
                    "Fin de partie ! Score : " + logique.score + "\nVoulez-vous recommencer ?",
                    "Partie terminée",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );
                if (result == DialogResult.Yes)
                {
                    logique.score = 0;
                    lblScore.Text = "Score : 0";
                    this.Controls.Clear();
                    this.Controls.Add(lblScore);
                    this.Controls.Add(lblMeilleur);
                    this.Controls.Add(btnNouveau);
                    boutons = new Button[10, 10];
                    CreerGrille();
                }
            }
        }
    }
}