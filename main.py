import numpy as np
from bodeis import Body
import physics

dt=1

def update(frame):

    earth_dot.set_data(
        [earth_x[frame]],
        [earth_y[frame]]
    )

    moon_dot.set_data(
        [moon_x[frame]],
        [moon_y[frame]]
    )

    earth_trail.set_data(
        earth_x[:frame],
        earth_y[:frame]
    )

    moon_trail.set_data(
        moon_x[:frame],
        moon_y[:frame]
    )

    return (
        earth_dot,
        moon_dot,
        earth_trail,
        moon_trail
    )


earth_x = []
earth_y = []

moon_x = []
moon_y = []

earth = Body(
    "earth",
    100,
    (100,50),
    (0.0428,-0.3212),
    10
)

moon = Body(
    "moon",
    40,
    (250,70),
    (-0.107,0.803),
    5
)

bodies=[earth,moon]

time=0


for i in range(5000):

    dist=physics.update(earth,moon,dt)


 
    earth_x.append(earth.pos[0])
    earth_y.append(earth.pos[1])

    moon_x.append(moon.pos[0])
    moon_y.append(moon.pos[1])

import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation

fig, ax = plt.subplots()

ax.set_aspect('equal')

ax.set_xlim(
    min(min(earth_x), min(moon_x)) - 20,
    max(max(earth_x), max(moon_x)) + 20
)

ax.set_ylim(
    min(min(earth_y), min(moon_y)) - 20,
    max(max(earth_y), max(moon_y)) + 20
)

earth_dot, = ax.plot([], [], 'bo', markersize=8)
moon_dot, = ax.plot([], [], 'ro', markersize=5)

earth_trail, = ax.plot([], [], 'b-', alpha=0.5)
moon_trail, = ax.plot([], [], 'r-', alpha=0.5)
ani = FuncAnimation(
    fig,
    update,
    frames=len(earth_x),
    interval=10,
    blit=True
)

plt.show()