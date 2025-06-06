using System;
using Lucky.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UltimateTicTacToe.Scripts
{
    public class PlayerSettingsController : ManagedBehaviour
    {
        public AI ai;
        public int playerId = 0;
        public PlayerTypes playerType;

        public CanvasGroup canvasGroup;
        public TMP_Text nameText;
        public TMP_Text charText;
        public GameObject difficultyGameObject;
        public Slider difficultySlider;

        private PlayerSettings setting => playerId == 0 ? Settings.Instance.playerSettings0 : Settings.Instance.playerSettings1;


        private void Start()
        {
            ai.on = setting.isAI;
            ai.searchTimes = setting.searchTimes;
            playerType = ai.aiPlayerType = setting.playerType;

            UpdateUI();

            difficultySlider.minValue = 0;
            difficultySlider.maxValue = Settings.AIMaxSeartchTimes;
            difficultySlider.wholeNumbers = true;
            difficultySlider.value = ai.searchTimes;
        }

        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();

            int searchTimes = (int)difficultySlider.value;
            if (playerId == 0)
                Settings.Instance.playerSettings0.searchTimes = searchTimes;
            else
                Settings.Instance.playerSettings1.searchTimes = searchTimes;
            ai.searchTimes = searchTimes;
            canvasGroup.alpha = BoardManager.Instance.curPlayer == playerType ? 1 : 0.3f;
        }

        private void UpdateUI()
        {
            string numberString = (playerId + 1).ToString();
            nameText.text = (ai.on ? "Computer" : "Player ") + numberString;
            charText.text = playerType == PlayerTypes.Cross ? "X" : "O";
            difficultyGameObject.SetActive(ai.on);
        }

        public void Swap()
        {
            if (playerId == 0)
                Settings.Instance.playerSettings0.isAI = !setting.isAI;
            else
                Settings.Instance.playerSettings1.isAI = !setting.isAI;
            BoardManager.Instance.Restart();
        }
    }
}