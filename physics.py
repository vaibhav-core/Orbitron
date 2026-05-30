import numpy as np

G=1

def gravity(body1,body2):
    r=body2.pos-body1.pos

    dist=np.linalg.norm(r)
    ur=r/dist

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