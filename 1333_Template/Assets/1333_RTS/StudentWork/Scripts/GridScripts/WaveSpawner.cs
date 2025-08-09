using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameStateHandler gameStateHandler;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject[] waveAnnouncements; // Index 0 = Wave 1, etc.

    [Header("Base Placement Info")]
    [SerializeField] private int baseOriginX = 0;
    [SerializeField] private int baseOriginY = 5;
    [SerializeField] private int baseSize = 10;

    private float timer = 10f;
    private int wave = 0;
    private bool spawning = false;

    private void Start()
    {
        UpdateCountdownUI();
        StartCoroutine(WaveRoutine());
    }

    private void Update()
    {
        if (!spawning) return;

        timer -= Time.deltaTime;
        timer = Mathf.Max(0, timer);
        UpdateCountdownUI();
    }

    private IEnumerator WaveRoutine()
    {
        spawning = true;

        yield return new WaitForSeconds(10f); // Initial phase

        for (int i = 1; i <= 4; i++)
        {
            wave = i;
            SpawnWave(wave);
            ShowWaveAnnouncement(wave);
            timer = 30f;
            yield return new WaitForSeconds(30f);
        }

        spawning = false;
        countdownText.text = "GG";
        
        if (gridManager.FriendlyBase != null)
        {
            gameStateHandler.TriggerWin();
        }
    }

    private void SpawnWave(int waveNumber)
    {
        List<GridNode> spawnPoints = new List<GridNode>();

        switch (waveNumber)
        {
            case 1:
                spawnPoints = GetRectangle(baseOriginX, baseOriginY - 3, baseSize, 3);
                SpawnEnemies(spawnPoints, 5);
                break;
            case 2:
                spawnPoints = GetRectangle(baseOriginX, baseOriginY + baseSize, baseSize, 3);
                SpawnEnemies(spawnPoints, 10);
                break;
            case 3:
                spawnPoints = GetRectangle(baseOriginX, baseOriginY - 6, baseSize, 3);
                SpawnEnemies(spawnPoints, 15);
                break;
            case 4:
                var top = GetRectangle(baseOriginX, baseOriginY + baseSize, baseSize, 3);
                var bottom = GetRectangle(baseOriginX, baseOriginY - 6, baseSize, 3);
                top.AddRange(bottom);
                SpawnEnemies(top, 20);
                break;
        }
    }

    private void SpawnEnemies(List<GridNode> area, int count)
    {
        Pathfinding finder = FindObjectOfType<Pathfinding>();

        for (int i = 0; i < count; i++)
        {
            GridNode node = area[Random.Range(0, area.Count)];
            Vector3 pos = node.WorldPosition + Vector3.up * (gridManager.GridSettings.NodeSize * 0.5f);
            GameObject obj = Instantiate(enemyPrefab, pos, Quaternion.identity);

            if (obj.TryGetComponent<UnitAI>(out var ai)) ai.Initialise(gridManager, finder);
            if (obj.TryGetComponent<UnitMover>(out var mover)) mover.Inject(gridManager, finder);
        }
    }

    private List<GridNode> GetRectangle(int startX, int startY, int width, int height)
    {
        List<GridNode> nodes = new List<GridNode>();

        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                var node = gridManager.GetNodeAt(x, y);
                if (node != null && node.Walkable)
                    nodes.Add(node);
            }
        }

        return nodes;
    }

    private void ShowWaveAnnouncement(int waveNumber)
    {
        if (waveAnnouncements == null || waveNumber < 1 || waveNumber > waveAnnouncements.Length)
            return;

        GameObject announcementGO = waveAnnouncements[waveNumber - 1];
        if (announcementGO == null) return;

        announcementGO.SetActive(true);
        StartCoroutine(HideAnnouncementAfterDelay(announcementGO));
    }

    private IEnumerator HideAnnouncementAfterDelay(GameObject go)
    {
        yield return new WaitForSeconds(3f);
        if (go != null)
            go.SetActive(false);
    }

    private void UpdateCountdownUI()
    {
        if (countdownText != null)
            countdownText.text = Mathf.CeilToInt(timer).ToString();
    }
}
