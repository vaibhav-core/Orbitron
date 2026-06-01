# Orbitron

A high-precision N-body gravitational simulator built in Python, designed as the physics backend for a real-time 3D solar system visualizer being built in Unity.

---

## Project Status

| Layer | Status |
|---|---|
| Physics engine | ✅ Complete |
| Diagnostics | ✅ Complete |
| Save / Load | ✅ Complete |
| Matplotlib preview | ✅ Complete |
| Unity visualization | 🟡 In progress |

---

## Physics

### Units
All simulation runs in **astronomical units**:
- Distance — AU (astronomical units)
- Mass — solar masses
- Time — years
- G = 4π² (exact in these units)

### Integrator — Velocity Verlet
Replaced naive Euler with a 2nd-order symplectic Velocity Verlet integrator. Symplectic integrators conserve the geometric structure of Hamiltonian systems, meaning energy and angular momentum remain bounded over arbitrarily long simulations rather than accumulating drift.

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

Both are at float64 machine precision — the theoretical minimum. NASA JPL requires < 1e-6%, Orbitron runs 500x better.

---

## File Structure

```
Orbitron/
├── bodeis.py          # Body class
├── physics.py         # Gravity, integrator, collision, diagnostics
├── simulation.py      # Simulation loop with diagnostic printing
├── animation.py       # Matplotlib preview renderer
├── save.py            # JSON save / load
├── main.py            # Entry point — bodies, steps, dt
└── saves/             # Auto-generated save files
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
Total system momentum zeroed at initialization to prevent sun drift from Jupiter's gravitational pull.

---

## Save System

Press `S` during the animation to save current simulation state.

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

Loading a save resumes from exact state including accelerations — no re-initialization needed.

---

## Quick Start

```bash
pip install numpy matplotlib
python main.py
```

Press `S` to save state. Close the window to exit.

---

## Roadmap

- [ ] Unity 3D visualization layer
- [ ] UDP bridge Python → Unity
- [ ] NASA 3D planet models via Nanite
- [ ] Orbit trail particle system (Niagara)
- [ ] ISS, Hubble, Voyager 1 & 2, James Webb via TLE data
- [ ] Starlink constellation subset (~50 satellites)
- [ ] Day/night shader for Earth with city lights
- [ ] Atmospheric scattering (limb glow)
- [ ] Time controls (speed up, slow down, reverse)
- [ ] Custom body addition at runtime
- [ ] Asteroid belt
- [ ] Save / load in Unity UI

---

## Competition

Built for a competition with a deadline of July 31, 2025. Physics layer completed June 2025.