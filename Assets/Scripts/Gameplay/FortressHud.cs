using UnityEngine;
using UnityEngine.UI;

public sealed class FortressHud : MonoBehaviour
{
    public WaveGameController game;
    public Text lifeText;
    public Text waveText;
    public Text statusText;
    public Text branchText;
    public Button waveButton;
    public Text buttonText;
    private Font runtimeFont;

    private void Awake()
    {
        runtimeFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" }, 24);
        if (runtimeFont != null)
            foreach (Text label in GetComponentsInChildren<Text>(true)) label.font = runtimeFont;
        waveButton.onClick.AddListener(OnWaveButton);
    }

    private void OnEnable()
    {
        if (game != null) game.StateChanged += Refresh;
    }

    private void Start() { Refresh(); }

    private void OnDisable()
    {
        if (game != null) game.StateChanged -= Refresh;
    }

    private void OnDestroy()
    {
        if (runtimeFont != null) Destroy(runtimeFont);
    }

    private void OnWaveButton()
    {
        if (game.IsGameOver || game.AllWavesFinished) game.RestartLevel();
        else game.StartNextWave();
    }

    public void Refresh()
    {
        if (game == null) return;
        lifeText.text = "生命  " + game.Lives + " / " + game.startingLives;
        lifeText.color = game.Lives <= 5 ? new Color32(255, 125, 113, 255) : new Color32(245, 231, 208, 255);
        waveText.text = "波次  " + game.CurrentWave + " / " + game.TotalWaves;
        branchText.text = (game.BranchOpen ? game.branchDisplayName + "已开放" : game.branchDisplayName + "第 " + game.branchUnlockWave + " 波开放") + " · 击败 " + game.DefeatedEnemies;
        if (game.IsGameOver) statusText.text = game.homeDisplayName + "失守";
        else if (game.AllWavesFinished) statusText.text = "全部波次结束";
        else if (game.IsWaveActive) statusText.text = (game.IsSpawning ? "敌人来袭" : "等待剩余敌人") + " · 场上 " + game.AliveCount;
        else statusText.text = game.CurrentWave == 0 ? game.preparationHint : "本波结束，准备下一波";
        buttonText.text = game.IsGameOver || game.AllWavesFinished ? "重新开始" : "开始第 " + (game.CurrentWave + 1) + " 波";
        waveButton.interactable = game.IsGameOver || game.AllWavesFinished || game.CanStartWave;
    }
}
