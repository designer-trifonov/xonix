namespace YG
{
    public partial class SavesYG
    {
        public int    savedScore       = 0;
        public int    savedLevel       = 0;    // 0 = нет сохранения
        public int    savedLives       = 0;
        public int    savedDifficulty  = 1;    // Difficulty.Medium
        public float  savedLastFillPct = 0f;
        public string savedGridCells   = "";

        // Состояние гиппо
        public string savedHippoCell   = "";   // "x,y" — клетка на сетке
        public string savedHippoDir    = "";   // "dx,dy" — направление движения
        public int    savedIsDrawing   = 0;    // 0/1

        // Трейл (упорядоченный список клеток)
        public string savedTrailPoints = "";   // "x1,y1;x2,y2;..."

        // Арбузы
        public string savedBalls       = "";   // "px,py,dx,dy|..."
        public float  savedBallSpeed   = 0f;
    }
}
