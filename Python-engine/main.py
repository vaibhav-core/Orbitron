from simulation import run_live_simulation
from unity_bridge import UnityBridge

dt = 0.0001

# Start with an empty universe
bodies = []

bridge = UnityBridge(host="127.0.0.1", port=9000)
bridge.start()

try:
    run_live_simulation(
        bodies,
        dt=dt,
        bridge=bridge,
        max_steps=None
    )
except KeyboardInterrupt:
    print("\nStopped.")
finally:
    bridge.stop()