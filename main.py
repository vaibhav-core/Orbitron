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
    1000,        # much heavier — needs to dominate gravitationally
    (0, 0),
    (0, 0),      # stationary at center
    10
)

moon = Body(
    "moon",
    1,           # negligible mass relative to earth
    (100, 0),
    (0, 3.162),  # sqrt(1000 / 100) ≈ 3.162
    5
)

mars = Body(
    "mars",
    1,           # also negligible
    (180, 0),
    (0, 2.357),  # sqrt(1000 / 180) ≈ 2.357
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

all_x = [x for body in bodies for x in body.xhist]
all_y = [y for body in bodies for y in body.yhist]

padding = 20
ax.set_xlim(min(all_x) - padding, max(all_x) + padding)
ax.set_ylim(min(all_y) - padding, max(all_y) + padding)

earth_dot, = ax.plot([], [], 'bo', markersize=8)
moon_dot, = ax.plot([], [], 'ro', markersize=5)
mars_dot, = ax.plot([], [], 'go', markersize=5)


earth_trail, = ax.plot([], [], 'b-', alpha=0.5)
moon_trail, = ax.plot([], [], 'r-', alpha=0.5)
mars_trail, = ax.plot([], [], 'g-', alpha=0.5)
ani = FuncAnimation(
    fig,
    update,
    frames=len(earth.xhist),
    interval=10,
    blit=True
)

plt.show()