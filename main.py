import math
import physics
import save as savemod
from bodeis import Body
from simulation import run_simulation
from animation import animate

# AU, solar masses, years — G = 4π² in these units
# v = sqrt(G * M_sun / r) = sqrt(4π² / r) for each planet

G  = 4 * math.pi**2
dt = 0.0001
steps = 100000

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

# zero total momentum — prevents sun drift from Jupiter's pull
total_momentum = sum(b.mass * b.vel for b in bodies)
sun.vel -= total_momentum / sun.mass

run_simulation(bodies, steps=50000, dt=0.0001)

def on_key(event, bodies):
    if event.key == 's':
        savemod.save(bodies)

animate(bodies, on_key=on_key)