import json
import os
from datetime import datetime


def save(bodies, path=None):
    if path is None:
        os.makedirs("saves", exist_ok=True)
        path = f"saves/{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"

    data = {
        "meta": {
            "timestamp": datetime.now().isoformat(),
            "body_count": len(bodies)
        },
        "bodies": [
            {
                "name": b.name,
                "mass": b.mass,
                "pos":  [b.pos[0],  b.pos[1]],
                "vel":  [b.vel[0],  b.vel[1]],
                "acc":  [b.acc[0],  b.acc[1]],
                "rad":  b.rad
            }
            for b in bodies
        ]
    }

    with open(path, 'w') as f:
        json.dump(data, f, indent=2)

    print(f"Saved → {path}")
    return path


def load(path):
    with open(path, 'r') as f:
        data = json.load(f)

    from bodeis import Body
    import numpy as np

    bodies = []
    for b in data["bodies"]:
        body     = Body(b["name"], b["mass"], tuple(b["pos"]), tuple(b["vel"]), b["rad"])
        body.acc = np.array(b["acc"])
        bodies.append(body)

    print(f"Loaded {len(bodies)} bodies from {path}")
    return bodies
