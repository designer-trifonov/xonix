using TMPro;
using UnityEngine;

namespace HippoGame.UI
{
    public class LeaderboardSlotUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _scoreText;

        public void Setup(int rank, string playerName, int score)
        {
            _rankText.text  = rank.ToString();
            _nameText.text  = playerName;
            _scoreText.text = score.ToString();
        }
    }
}
