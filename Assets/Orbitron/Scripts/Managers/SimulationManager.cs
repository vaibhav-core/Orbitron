using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;
    [SerializeField] private PlanetBrowserUI planetBrowserUI;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateSimulation(SimulationState state)
    {
        if (verboseLogging)
          //  Debug.Log($"[SimulationManager] step={state.step} t={state.simTime:F4}yr");

        // handle merges first so dead bodies are removed before positions update
       if (state.mergeEvents != null)
{
    Debug.Log($"Merge Events: {state.mergeEvents?.Length}");
    foreach (MergeEvent e in state.mergeEvents)
    {
        PlanetManager.Instance?.HandleMerge(e);

        CollisionHistoryUI.Instance?.AddCollision(
            state.simTime,
            e.removed[0],
            e.removed[1],
            e.created
        );
    }
}
        
       

        PlanetManager.Instance?.UpdateBodies(state);
        planetBrowserUI.RefreshPlanetList(state);
    }
}