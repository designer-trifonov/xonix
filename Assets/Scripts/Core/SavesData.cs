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
    }
}
