from simulation import run_live_simulation
from unity_bridge import UnityBridge

dt = 0.0001

#───────────────────────────speed knob ────────────────────────────────────────────────────────────────
# TARGET_SPEED = simulated years per real second.
#   0.05  → 1 sim-year  = 20 s real  (Earth orbit lasts ~20 s) ← default
#   0.1   → 1 sim-year  = 10 s real
#   1.0   → 1 sim-year  =  1 s real  (very fast, hard to follow)
#   None  → full CPU speed (original, useful for stress-testing physics)
TARGET_SPEED = 0.05
# Start with an empty universe
bodies = []

bridge = UnityBridge(host="127.0.0.1", port=9000)
bridge.start()

try:
    run_live_simulation(
        bodies,
        dt=dt,
        bridge=bridge,
        max_steps=None,target_speed=TARGET_SPEED
        
    )
except KeyboardInterrupt:
    print("\nStopped.")
finally:
    bridge.stop()