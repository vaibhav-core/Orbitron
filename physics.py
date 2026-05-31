import numpy as np

G=1

def gravity(body1,body2):
    r=body2.pos-body1.pos
    dist=np.linalg.norm(r)
    ur=r/dist
    if dist==0:
        return
    Fg=(G*body1.mass*body2.mass)/dist**2

    return Fg*ur , dist

def update(body1,body2,dt):

    Fv,dist=gravity(body1,body2)

    acc1=Fv/body1.mass
    acc2=-Fv/body2.mass

    body1.vel+=acc1*dt
    body2.vel+=acc2*dt

    body2.pos+=body2.vel*dt
    body1.pos+=body1.vel*dt

    return dist

def update_nbody(bodies, dt):

    forces = []

    for body1 in bodies:

        f_net = np.array([0.0, 0.0])

        for body2 in bodies:

            if body1 is body2:
                continue

            f, dist = gravity(body1, body2)

            f_net += f

        forces.append(f_net)

    for i,body in enumerate(bodies):
        acc=forces[i]/body.mass
        body.vel+=acc*dt

    for body in bodies:
        body.pos+=body.vel*dt
