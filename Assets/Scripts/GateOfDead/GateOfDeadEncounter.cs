using UnityEngine;
using KingdomBorder.TwinTowers;

namespace RoyalFortress.GateOfDead
{
    public sealed class GateOfDeadEncounter : MonoBehaviour
    {
        public WaveGameController game;
        public GateOfDeadPresentation presentation;
        public TowerBuildController building;
        public FortressHud hud;
        public bool VictoryStarted { get; private set; }
        public bool VictoryFinished { get; private set; }
        private bool failed;
        private float targetProgress;

        private void Start()
        {
            game.StateChanged += Refresh;
            presentation.onSealed.AddListener(Sealed);
            presentation.showControls = false;
            Refresh();
        }
        private void Update()
        {
            if (!VictoryStarted && !failed)
                presentation.pushProgress = Mathf.MoveTowards(presentation.pushProgress, targetProgress, Time.deltaTime * .25f);
        }
        private void Refresh()
        {
            if (game.IsGameOver)
            {
                failed = true;
                presentation.paused = true;
                StopBuildingAndTowers();
                return;
            }
            targetProgress = .8f * game.CompletedWaves / Mathf.Max(1, game.TotalWaves);
            if (game.AllWavesFinished && game.Lives > 0 && !VictoryStarted)
            {
                VictoryStarted = true;
                presentation.PlaySealingSequence();
                StopBuildingAndTowers();
                hud.statusText.text = "净化力量推进 · 正在封印";
                hud.buttonText.text = "封印中…";
                hud.waveButton.interactable = false;
            }
        }
        private void StopBuildingAndTowers()
        {
            building.Cancel(); building.enabled = false;
            foreach (var tower in building.builtTowersRoot.GetComponentsInChildren<TowerBehaviour>())
                tower.attackEnabled = false;
        }
        private void Sealed()
        {
            if (!VictoryStarted || failed) return;
            VictoryFinished = true;
            hud.statusText.text = "封印完成 · 亡者之门已关闭";
            hud.buttonText.text = "重新开始";
            hud.waveButton.interactable = true;
        }
        private void OnDestroy()
        {
            if (game != null) game.StateChanged -= Refresh;
            if (presentation != null) presentation.onSealed.RemoveListener(Sealed);
        }
    }
}
