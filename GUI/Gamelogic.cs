using System.Drawing;
using System.Collections.Generic;

namespace GUI
{
    internal class GameLogic
    {
        public enum Couleur { Rouge, Bleu, Vert, Jaune }

        public Couleur[,] grille = new Couleur[10, 10];

        public int score = 0;

        public int meilleurScore = 0;

        public void GenererGrille()
        {
            Random random = new Random();
            for (int ligne = 0; ligne < 10; ligne++)
            {
                for (int colonne = 0; colonne < 10; colonne++)
                {
                    grille[ligne, colonne] = (Couleur)random.Next(0, 4);
                }
            }
        }

        public List<Point> TrouverCasesConnectees(int ligne, int colonne)
        {
            List<Point> resultat = new List<Point>();
            bool[,] visite = new bool[10, 10];
            Queue<Point> file = new Queue<Point>();
            Couleur cibleCouleur = grille[ligne, colonne];

            file.Enqueue(new Point(ligne, colonne));
            visite[ligne, colonne] = true;

            int[] dLigne = { -1, 1, 0, 0 };
            int[] dColonne = { 0, 0, -1, 1 };

            while (file.Count > 0)
            {
                Point current = file.Dequeue();
                resultat.Add(current);

                for (int i = 0; i < 4; i++)
                {
                    int nl = current.X + dLigne[i];
                    int nc = current.Y + dColonne[i];

                    if (nl >= 0 && nl < 10 && nc >= 0 && nc < 10
                        && !visite[nl, nc]
                        && grille[nl, nc] == cibleCouleur)
                    {
                        visite[nl, nc] = true;
                        file.Enqueue(new Point(nl, nc));
                    }
                }
            }
            return resultat;
        }
        public bool EstPartieTerminer()
        {
            for (int ligne = 0; ligne < 10; ligne++)
            {
                for (int colonne = 0; colonne < 10; colonne++)
                {
                    if ((int)grille[ligne, colonne] == -1)
                        continue;
                    List<Point> cases = TrouverCasesConnectees(ligne, colonne);
                    if (cases.Count >= 2)
                        return false;
                }
            }
            return true;
        }
    }
}