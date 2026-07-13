using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;

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
            foreach (MergeEvent e in state.mergeEvents)
                PlanetManager.Instance?.HandleMerge(e);

        }
        //foreach (BodyState body in state.bodies)
    //{
        // Debug.Log
        // (
        //     $"{body.name} | {body.parent} | {body.type} | " +
        //     $"{body.orbital_velocity} | {body.escape_velocity}"
           
        // );
        

        //  Debug.Log($"{body.name} | {body.orbital_period:F3} yr");
   // }

        PlanetManager.Instance?.UpdateBodies(state);
    }
}