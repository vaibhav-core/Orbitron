import math
from bodeis import Body
from simulation import run_live_simulation
from unity_bridge import UnityBridge

dt = 0.0001

# ── Speed knob ────────────────────────────────────────────────────────────────
# TARGET_SPEED = simulated years per real second.
#   0.05  → 1 sim-year  = 20 s real  (Earth orbit lasts ~20 s) ← default
#   0.1   → 1 sim-year  = 10 s real
#   1.0   → 1 sim-year  =  1 s real  (very fast, hard to follow)
#   None  → full CPU speed ( useful for stress-testing physics)
TARGET_SPEED = 0.05

sun     = Body("sun",     1.0,       (0.000, 0),  (0,  0.000),   0.00465 )
mercury = Body("mercury", 1.651e-7,  (0.387, 0),  (0,  10.1001), 0.000016)
venus   = Body("venus",   2.447e-6,  (0.723, 0),  (0,  7.3894),  0.000040)
earth   = Body("earth",   3.003e-6,  (1.000, 0),  (0,  6.2832),  0.000043)
mars    = Body("mars",    3.213e-7,  (1.524, 0),  (0,  5.0896),  0.000023)
jupiter = Body("jupiter", 9.545e-4,  (5.203, 0),  (0,  2.7546),  0.000477)
saturn  = Body("saturn",  2.858e-4,  (9.537, 0),  (0,  2.0346),  0.000403)
uranus  = Body("uranus",  4.366e-5,  (19.19, 0),  (0,  1.4343),  0.000171)
neptune = Body("neptune", 5.151e-5,  (30.07, 0),  (0,  1.1458),  0.000165)

bodies = [sun, mercury, venus, earth, mars, jupiter, saturn, uranus, neptune]

total_momentum = sum(b.mass * b.vel for b in bodies)
sun.vel -= total_momentum / sun.mass

bridge = UnityBridge(host="127.0.0.1", port=9000)
bridge.start()

try:
    run_live_simulation(bodies, dt=dt, bridge=bridge, max_steps=None,
                        target_speed=TARGET_SPEED)
except KeyboardInterrupt:
    print("\nStopped.")
finally:
    bridge.stop()