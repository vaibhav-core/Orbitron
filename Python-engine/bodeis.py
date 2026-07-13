import numpy as np

class Body:
    def __init__(self, name, mass, pos, vel, rad, body_type="Planet"):
        self.name      = name
        self.mass      = mass
        self.pos       = np.array(pos, dtype=float)
        self.vel       = np.array(vel, dtype=float)
        self.acc       = np.zeros(2)
        self.rad       = rad
        self.body_type = body_type   # "Star" | "Planet" | "Moon" | "Asteroid"

        # computed each tick by update_body_properties
        self.parent          = None   # name of dominant gravitational body
        self.orbital_velocity = 0.0
        self.orbital_period   = 0.0
        self.escape_velocity  = 0.0
        self.total_energy     = 0.0
        self.status           = "Stable"   # "Stable" | "Escaping" | "Colliding" | "Merged"
        self.status_timer     = 0

        self.xhist = []
        self.yhist = []