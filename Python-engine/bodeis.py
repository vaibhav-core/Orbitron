import numpy as np

class Body:
    def __init__(self, name, mass, pos, vel, rad):
        self.name = name
        self.mass = mass
        self.pos  = np.array(pos, dtype=float)
        self.vel  = np.array(vel, dtype=float)
        self.acc  = np.zeros(2)
        self.rad  = rad
        self.xhist = []
        self.yhist = []