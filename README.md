# Orbitron

A high-precision N-body gravitational simulator with a real-time 3D Solar System visualizer. The project is split into two tightly coupled layers: a Python physics engine and a Unity 3D renderer, communicating over a local TCP bridge.

---

## Project Status

| Layer | Status |
|---|---|
| Physics engine | ✅ Complete |
| Diagnostics | ✅ Complete |
| Save / Load | ✅ Complete |
| Matplotlib preview | ✅ Complete |
| TCP bridge (Python ↔ Unity) | ✅ Complete |
| Unity 3D visualization | ✅ Complete |
| Planet spawner UI | ✅ Complete |
| Camera controller | ✅ Complete |
| Runtime body editing | ✅ Complete |
| Orbital normalizer / unit converter | ✅ Complete |

---

## Architecture

```
┌─────────────────────────────────┐         TCP 127.0.0.1:9000
│        Python Engine            │ ──────────────────────────► │ Unity Renderer │
│  physics · simulation · save    │ ◄────────────────────────── │  C# / URP      │
└─────────────────────────────────┘   newline-delimited JSON
```

- **Python** is the authority on all physics. It owns positions, velocities, accelerations, collisions, and time.
- **Unity** is a pure renderer. It receives state packets every N simulation steps and renders them. It can also send commands back (spawn, edit, remove a body) which Python applies on the next tick.
- Both sides are **stateless about each other** — Unity reconnects automatically after Python restarts, and vice versa.

---

## Physics

### Units
All simulation runs in **astronomical units**:
- Distance — AU (astronomical units)
- Mass — solar masses
- Time — years
- G = 4π² (exact in these units)

### Integrator — Velocity Verlet
A 2nd-order symplectic Velocity Verlet integrator. Symplectic integrators conserve the geometric structure of Hamiltonian systems, keeping energy and angular momentum bounded over arbitrarily long simulations rather than accumulating drift.

```
pos  +=  vel·dt  +  ½·acc·dt²
acc_new  =  F(pos_new) / m
vel  +=  ½·(acc_old + acc_new)·dt
acc  =  acc_new
```

### Gravity
- Newtonian gravity with softening: `F = G·m1·m2 / (r² + ε²)`
- Softening constant `ε² = 1e-6` prevents force singularities at close range
- Newton's 3rd law optimization — each pair computed once, equal and opposite forces applied to both bodies, halving force evaluations

### Collision Detection
- Distance² check avoids sqrt: `dist² < (r1 + r2)²`
- `merged_pairs` set prevents a body from being consumed twice in one tick
- Cluster merging via graph flood-fill — handles A+B+C+D all colliding simultaneously in one tick
- Merge conserves mass, momentum, and volume (`r³ = r1³ + r2³`)
- Merge events are broadcast to Unity so it can instantly destroy the consumed body's GameObject

### Adaptive Timestep
Automatically shrinks `dt` when bodies are close and forces are large, expands when the system is calm. Controlled by `eta` parameter.

```python
dt = adaptive_dt(bodies, dt_max=0.001, dt_min=1e-6, eta=0.01)
```

### Initialization
`initialize_accelerations(bodies)` must be called once before the main loop to seed correct accelerations for Verlet's first step. Without this, tick 0 silently degrades to an Euler step.

---

## Diagnostics

Energy and angular momentum tracked every 500 steps. After 50,000 steps (5 simulated years):

```
Energy drift:           ~2e-9 %
Angular momentum drift: ~1e-12 %
```

Both are at float64 machine precision — the theoretical minimum. NASA JPL requires < 1e-6%, Orbitron runs 500× better.

---

## TCP Bridge

### Python side — `unity_bridge.py`

`UnityBridge` is a threaded TCP server that runs alongside the simulation loop:

| Method | Description |
|---|---|
| `start()` | Opens port 9000, begins accepting Unity connections in a daemon thread |
| `broadcast_state(bodies, ...)` | Serializes full simulation state to newline-terminated JSON and sends to Unity |
| `apply_pending_commands(bodies)` | Drains Unity's command queue and mutates the bodies list before the next physics tick |
| `stop()` | Graceful shutdown, joins threads |

**State packet schema:**
```json
{
  "step": 1200,
  "sim_time": 0.12,
  "energy": -39.47,
  "angular_momentum": 28.32,
  "merge_events": [],
  "bodies": [
    { "name": "earth", "pos": [0.98, 0.12], "vel": [-0.71, 6.21],
      "acc": [-0.0003, -0.00012], "mass": 3.003e-6, "rad": 4.3e-5 }
  ]
}
```

**Command packets (Unity → Python):**
```json
{ "cmd": "spawn", "name": "rogue", "mass": 1e-5, "pos": [3.0, 0.0], "vel": [0.0, 4.5], "rad": 5e-5 }
{ "cmd": "edit",  "name": "earth", "mass": 6e-6 }
{ "cmd": "remove","name": "mars" }
```

### Unity side — `TCPClient.cs`

`TCPClient` is a MonoBehaviour that connects to Python and maintains a persistent background receive thread:

- Parses newline-delimited JSON on a background thread into a `ConcurrentQueue<string>`
- Drains the queue on Unity's main thread in `Update()` (thread-safe, no lock contention)
- Deserializes packets into `SimulationState` via Newtonsoft.Json
- Auto-reconnects every 3 seconds if Python restarts
- Sends spawn / edit / remove commands to Python via `SendSpawnCommand`, `SendEditCommand`, `SendRemoveCommand`

---

## Unity Layer

### Scripts

| Script | Role |
|---|---|
| `TCPClient.cs` | Background TCP receive thread + Unity main-thread dispatch |
| `SimulationManager.cs` | Routes incoming state to PlanetManager and UIManager |
| `PlanetManager.cs` | Owns all planet GameObjects; spawns, moves, scales, destroys them |
| `Normalizer.cs` | All unit conversions between AU and Unity world-space |
| `UIManager.cs` | Planet creator panel toggle (`M` key), pauses simulation during UI |
| `CreatePlanetUI.cs` | Form for spawning custom bodies at runtime |
| `CameraController.cs` | Orbit camera with zoom and pan |
| `EarthLighting.cs` | Positions directional light to always face Earth |

### Normalizer — Unit Conversion

`Normalizer.cs` is the single source of truth for all AU ↔ Unity unit conversions:

| Quantity | Rule |
|---|---|
| Position | 1 AU = 100 Unity units (linear) |
| Velocity | Same scale as position |
| Radius | Power-curve compressed (`r^0.45`) so small planets are visible without the Sun being enormous |
| Speed display | AU/yr × 4.7404 = km/s |
| Mass display | Switches between Earth masses (M⊕) and solar masses (M☉) automatically |
| Sim time display | Formats as hours / days / months / years depending on magnitude |

### Camera Controls

| Input | Action |
|---|---|
| Right-click + drag | Orbit |
| Scroll wheel | Zoom |
| Middle-click + drag | Pan |

### UI Controls

| Input | Action |
|---|---|
| `M` | Toggle planet creator panel (pauses simulation) |
| `S` (Python window) | Save current simulation state to JSON |

---

## File Structure

```
Orbitron/
├── Python-engine/
│   ├── bodeis.py          # Body class (name, mass, pos, vel, acc, rad, xhist, yhist)
│   ├── physics.py         # Gravity, Velocity Verlet, collision, diagnostics, adaptive dt
│   ├── simulation.py      # run_simulation() (offline) + run_live_simulation() (TCP mode)
│   ├── animation.py       # Matplotlib preview renderer
│   ├── save.py            # JSON save / load
│   ├── unity_bridge.py    # Threaded TCP server, state broadcast, command dispatch
│   └── main.py            # Entry point — initializes solar system, starts bridge + simulation
│
├── Assets/
│   └── Orbitron/
│       ├── Scripts/
│       │   ├── Managers/
│       │   │   ├── PlanetManager.cs    # Planet GameObject lifecycle
│       │   │   ├── SimulationManager.cs
│       │   │   └── UIManager.cs        # Panel toggle + time-scale pause
│       │   ├── Networking/
│       │   │   ├── TCPClient.cs        # Background TCP receive + main-thread dispatch
│       │   │   └── Messages/           # Serializable data models (SimulationState, etc.)
│       │   ├── Renderer/
│       │   │   ├── Normalizer.cs       # AU ↔ Unity unit conversions
│       │   │   └── EarthLighting.cs    # Sun-facing directional light
│       │   └── UI/
│       │       └── CreatePlanetUI.cs   # Runtime body spawner form
│       ├── Scenes/
│       │   └── simulation.unity        # Main scene
│       ├── Prefabs/                    # Planet prefabs
│       ├── textures/                   # Planet texture maps
│       └── materials/                  # Planet materials
│
└── saves/                              # Auto-generated JSON snapshots
```

---

## Solar System Values

| Body | Mass (M☉) | Distance (AU) | Velocity (AU/yr) | Radius (AU) |
|---|---|---|---|---|
| Sun | 1.0 | 0.000 | 0.000 | 0.00465 |
| Mercury | 1.651e-7 | 0.387 | 10.1001 | 1.6e-5 |
| Venus | 2.447e-6 | 0.723 | 7.3894 | 4.0e-5 |
| Earth | 3.003e-6 | 1.000 | 6.2832 | 4.3e-5 |
| Mars | 3.213e-7 | 1.524 | 5.0896 | 2.3e-5 |
| Jupiter | 9.545e-4 | 5.203 | 2.7546 | 4.77e-4 |
| Saturn | 2.858e-4 | 9.537 | 2.0346 | 4.03e-4 |
| Uranus | 4.366e-5 | 19.19 | 1.4343 | 1.71e-4 |
| Neptune | 5.151e-5 | 30.07 | 1.1458 | 1.65e-4 |

Orbital velocities calculated from `v = √(G·M/r)` with G = 4π².  
Total system momentum zeroed at initialization to cancel sun drift from Jupiter's gravitational pull.

---

## Save System

Press `S` during the Matplotlib animation to save the current simulation state.

```
saves/20250602_143022.json
```

```json
{
  "meta": {
    "timestamp": "2025-06-02T14:30:22",
    "body_count": 9
  },
  "bodies": [
    {
      "name": "earth",
      "mass": 0.000003003,
      "pos": [0.98, 0.12],
      "vel": [-0.71, 6.21],
      "acc": [-0.0003, -0.00012],
      "rad": 0.000043
    }
  ]
}
```

Loading a save resumes from the exact state including accelerations — no re-initialization needed.

---

## Quick Start

### Prerequisites
- Python 3.9+
- NumPy, Matplotlib
- Unity 2022 LTS or later (URP)
- Newtonsoft.Json Unity package

### Run

**1. Start the Python engine:**
```bash
pip install numpy matplotlib
cd Python-engine
python main.py
```
Python opens a TCP server on `127.0.0.1:9000` and begins simulating.

**2. Open the Unity project:**
Open the `Orbitron/` folder as a Unity project and press Play. Unity connects automatically and renders the simulation in real time.

**3. Matplotlib-only preview (no Unity required):**
```python
# In simulation.py, call run_simulation() instead of run_live_simulation()
# then pass results to animation.py
```

### Runtime controls
| Action | How |
|---|---|
| Spawn a new body | Press `M` → fill form → Spawn |
| Save state | Press `S` in the Python terminal window |
| Quit | Close Unity or press `Ctrl+C` in the Python terminal |

---

## Roadmap

- [x] Velocity Verlet integrator
- [x] Adaptive timestep
- [x] Cluster collision merging
- [x] Matplotlib preview
- [x] JSON save / load
- [x] TCP bridge (Python ↔ Unity)
- [x] Unity 3D planet rendering
- [x] Runtime body spawner UI
- [x] Camera controller
- [x] Unit normalizer (AU ↔ Unity world-space)
- [ ] NASA 3D planet models (replacing sphere primitives)
- [ ] Orbit trail particle system (Niagara)
- [ ] ISS, Hubble, Voyager 1 & 2, James Webb via TLE data
- [ ] Starlink constellation subset (~50 satellites)
- [ ] Day/night shader for Earth with city lights
- [ ] Atmospheric scattering (limb glow)
- [ ] Time controls (speed up, slow down, reverse) in Unity UI
- [ ] Asteroid belt
- [ ] Save / load from within the Unity UI

---

## Competition

Built for a competition with a deadline of **July 31, 2025**.  
Physics layer completed June 2025. Unity visualization + TCP bridge completed July 2025.