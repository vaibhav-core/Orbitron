import numpy as np

class Body:

    def __init__(self, name, mass, pos, vel, rad):

        self.name = name
        self.mass = mass

        self.pos = np.array(pos, dtype=float)
        self.vel = np.array(vel, dtype=float)
        self.xhist=[]
        self.yhist=[]

        self.rad = rad


class space_craft:
    def __init__(self,name,launch,mass,target):
        self.name = name
        self.launch=launch
        self.mass=mass
        self.target=target
        