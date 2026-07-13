import numpy as np

class Body:
    def __init__(self, name, mass, pos, vel, rad,type):
        self.name = name
        self.mass = mass
        self.pos  = np.array(pos, dtype=float)
        self.vel  = np.array(vel, dtype=float)
        self.acc  = np.zeros(2)
        self.rad  = rad
        self.xhist = []
        self.yhist = []
        self.parent = None
        self.Body_type = type
        self.status = "Initializing"
        self.status_timer=0

        self.escape_velocity = 0.0
        self.orbital_velocity =0.0
        self.orbital_period = 0.0
        self.total_energy = 0.0