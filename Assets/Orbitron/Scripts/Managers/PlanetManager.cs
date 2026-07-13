using UnityEngine;
using System.Collections.Generic;

public class PlanetManager : MonoBehaviour
{
    public static PlanetManager Instance { get; private set; }

    [Header("Spawning")]
    [SerializeField] private GameObject defaultPlanetPrefab;

    // tracks all live body GameObjects by name — single source of truth
    private readonly Dictionary<string, GameObject> _bodies =
    new(System.StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, BodyState> _bodyStates =
    new(System.StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    // ── Called by CreatePlanetUI for locally initiated spawns ─────────────────
    // Does NOT create the GameObject immediately — it sends the command to Python
    // and waits for Python to echo it back in the next state broadcast.
    // This prevents the duplicate-spawn race condition.
    public void RequestSpawn(PlanetData data)
    {
        TCPClient.Instance?.SendSpawnCommand(data);
    }

    // ── Called by SimulationManager every TCP tick ────────────────────────────
    public void UpdateBodies(SimulationState state)
    {
        if (state.bodies == null) return;

        foreach (BodyState body in state.bodies)
        
        {
            _bodyStates[body.name] = body;
            Debug.Log($"Cached: '{body.name}'");
            if (!_bodies.TryGetValue(body.name, out GameObject planet))
                planet = SpawnBody(body);

            if (planet == null) continue;

            // position — denormalize double[] to float for Normalizer
            planet.transform.position = Normalizer.NormalizePosition(
                (float)body.pos[0], (float)body.pos[1]
            );

            // scale — update every tick so merges and live edits reflect immediately
            float visualDiameter = Normalizer.NormalizeRadius((float)body.rad);
            planet.transform.localScale = Vector3.one * visualDiameter;
        }
    }

    // ── Called by SimulationManager when a merge event arrives ────────────────
    public void HandleMerge(MergeEvent mergeEvent)
    {
        if (mergeEvent.removed != null)
        {
            foreach (string removedName in mergeEvent.removed)
            {
                if (_bodies.TryGetValue(removedName, out GameObject dead))
                {
                    Destroy(dead);
                    _bodies.Remove(removedName);
                    _bodyStates.Remove(removedName);
                }
            }
        }
        // the created body will be auto-spawned by UpdateBodies on the next tick
        // when it appears in state.bodies for the first time
    }

    // ── Public remove — called by UI if judge removes a body manually ─────────
    public void RequestRemove(string bodyName)
    {
        TCPClient.Instance?.SendRemoveCommand(bodyName);
        // don't destroy the GameObject here — wait for Python to stop sending
        // it in state.bodies, then it just stops being updated. Or handle via
        // a merge event if you want immediate visual removal.
    }

    // ── Internal spawn from state data ───────────────────────────────────────
    private GameObject SpawnBody(BodyState body)
    {
        if (defaultPlanetPrefab == null)
        {
            Debug.LogError("[PlanetManager] defaultPlanetPrefab not assigned.");
            return null;
        }

        Vector3 pos = Normalizer.NormalizePosition((float)body.pos[0], (float)body.pos[1]);
        GameObject go = Instantiate(defaultPlanetPrefab, pos, Quaternion.identity);
        go.name = body.name;

        Debug.Log($"{body.name} rad={body.rad}");

        float visualDiameter = Normalizer.NormalizeRadius((float)body.rad);
        go.transform.localScale = Vector3.one * visualDiameter;

        _bodies[body.name] = go;
        Debug.Log($"[PlanetManager] spawned '{body.name}' rad={body.rad:E2} scale={visualDiameter:F3}");
        return go;
    }

    // ── Utility: get a live body GameObject by name ───────────────────────────
    public GameObject GetBody(string name)
    {
        _bodies.TryGetValue(name, out GameObject go);
        return go;
    }
    public BodyState GetBodyState(string name)
{
    _bodyStates.TryGetValue(name, out BodyState body);
    return body;
}
}