import numpy as np
import keyboard as kb

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

G = 1

earth_x = []
earth_y = []

moon_x = []
moon_y = []

earth = np.array([100.0,50.0])
moon = np.array([250.0,70.0])

me = 100
mm = 40
re=7
rm=4

time=0

dt = 1

vel_e = np.array([0.0428,-0.3212])
vel_m = np.array([-0.107,0.803])

for i in range(5000):
    r = moon - earth
    dist = np.linalg.norm(r)
    ur = r/dist
    time+=1
    if dist<=(re+rm):
        print(time)
        print(f"collsion at {dist}")      
        break


    Fg = (G*me*mm)/(dist**2)

    F_vect = Fg * ur

    acc_e = F_vect/me
    acc_m = -F_vect/mm

    vel_e += acc_e*dt
    vel_m+= acc_m*dt

    earth_x.append(earth[0])
    earth_y.append(earth[1])

    moon_x.append(moon[0])
    moon_y.append(moon[1])
    
    
    earth += vel_e*dt
    moon += vel_m*dt

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