import numpy as np
from bodeis import Body
import physics

dt=1

def update(frame):

    earth_dot.set_data(
        [earth.xhist[frame]],
        [earth.yhist[frame]]
    )

    moon_dot.set_data(
        [moon.xhist[frame]],
        [moon.yhist[frame]]
    )

    earth_trail.set_data(
        earth.xhist[:frame],
        earth.yhist[:frame]
    )

    moon_trail.set_data(
        moon.xhist[:frame],
        moon.yhist[:frame]
    )

    mars_dot.set_data(
    [mars.xhist[frame]],
    [mars.yhist[frame]]
    )
    mars_trail.set_data(
        mars.xhist[:frame],
        mars.yhist[:frame]
    )
    return (
        earth_dot,
        moon_dot,
        earth_trail,
        moon_trail,
        mars_dot,
        mars_trail

    )
earth = Body(
    "earth",
    100,
    (0,0),
    (0,0),
    10
)

moon = Body(
    "moon",
    20,
    (100,0),
    (0,1.0),
    5
)

mars = Body(
    "mars",
    10,
    (180,0),
    (0,0.75),
    4
)

bodies=[earth,moon,mars]

time=0


for i in range(5000):

    print(i)

    dist=physics.update_nbody(bodies,dt)

    for body in bodies:
         body.xhist.append(body.pos[0])
         body.yhist.append(body.pos[1])

import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation

fig, ax = plt.subplots()

ax.set_aspect('equal')

ax.set_xlim(
    min(min(earth.xhist), min(moon.xhist)) - 20,
    max(max(earth.xhist), max(moon.xhist)) + 20
)

ax.set_ylim(
    min(min(earth.yhist), min(moon.yhist)) - 20,
    max(max(earth.yhist), max(moon.yhist)) + 20
)

earth_dot, = ax.plot([], [], 'bo', markersize=8)
moon_dot, = ax.plot([], [], 'ro', markersize=5)
mars_dot, = ax.plot([], [], 'go', markersize=5)


earth_trail, = ax.plot([], [], 'b-', alpha=0.5)
moon_trail, = ax.plot([], [], 'r-', alpha=0.5)
mars_trail, = ax.plot([], [], 'r-', alpha=0.5)
ani = FuncAnimation(
    fig,
    update,
    frames=len(earth.xhist),
    interval=10,
    blit=True
)

plt.show()