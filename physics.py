import numpy as np
from bodeis import Body

G          = 4 * np.pi**2
SOFTENING2 = 1e-6


def merge(body1, body2):
    new_mass = body1.mass + body2.mass
    inv_mass = 1.0 / new_mass
    new_pos  = (body1.mass * body1.pos + body2.mass * body2.pos) * inv_mass
    new_vel  = (body1.mass * body1.vel + body2.mass * body2.vel) * inv_mass
    new_acc  = (body1.mass * body1.acc + body2.mass * body2.acc) * inv_mass
    rad_new  = (body1.rad**3 + body2.rad**3) ** (1.0 / 3.0)
    body3        = Body(f"{body1.name}+{body2.name}", new_mass, new_pos, new_vel, rad_new)
    body3.acc    = new_acc
    return body3


def _compute_accelerations(bodies, skip_set):
    accels = [np.zeros(2) for _ in range(len(bodies))]
    alive  = [i for i, b in enumerate(bodies) if b not in skip_set]

    for idx in range(len(alive)):
        i  = alive[idx]
        bi = bodies[i]
        for jdx in range(idx + 1, len(alive)):
            j         = alive[jdx]
            bj        = bodies[j]
            r         = bj.pos - bi.pos
            dist2     = r[0]*r[0] + r[1]*r[1] + SOFTENING2
            inv_dist3 = dist2 ** (-1.5)
            factor    = G * inv_dist3
            accels[i] += factor * bj.mass * r
            accels[j] -= factor * bi.mass * r

    return accels


def _detect_collisions(bodies):
    to_add     = []
    to_remove  = set()
    merged_idx = set()

    for i, body1 in enumerate(bodies):
        if i in merged_idx:
            continue
        for j in range(i + 1, len(bodies)):
            if j in merged_idx:
                continue
            body2 = bodies[j]
            r     = body2.pos - body1.pos
            dist2 = r[0]*r[0] + r[1]*r[1]
            touch = body1.rad + body2.rad
            if dist2 < touch * touch:
                print(f"Collision: {body1.name} + {body2.name}")
                to_add.append(merge(body1, body2))
                to_remove.add(body1)
                to_remove.add(body2)
                merged_idx.add(i)
                merged_idx.add(j)
                break

    return to_add, to_remove


def _cluster_collisions(bodies):
    n     = len(bodies)
    touch = [set() for _ in range(n)]

    for i in range(n):
        for j in range(i + 1, n):
            r     = bodies[j].pos - bodies[i].pos
            dist2 = r[0]*r[0] + r[1]*r[1]
            t     = bodies[i].rad + bodies[j].rad
            if dist2 < t * t:
                touch[i].add(j)
                touch[j].add(i)

    visited = set()
    groups  = []

    for i in range(n):
        if i in visited or not touch[i]:
            continue
        cluster = set()
        stack   = [i]
        while stack:
            node = stack.pop()
            if node in cluster:
                continue
            cluster.add(node)
            stack.extend(touch[node] - cluster)
        visited |= cluster
        groups.append(list(cluster))

    to_add    = []
    to_remove = set()

    for group in groups:
        merged = bodies[group[0]]
        for idx in group[1:]:
            merged = merge(merged, bodies[idx])
            print(f"Cluster merge: {merged.name}")
        to_add.append(merged)
        for idx in group:
            to_remove.add(bodies[idx])

    return to_add, to_remove


def initialize_accelerations(bodies):
    accels = _compute_accelerations(bodies, skip_set=set())
    for i, body in enumerate(bodies):
        body.acc = accels[i]


def kinetic_energy(bodies):
    return sum(0.5 * b.mass * (b.vel[0]**2 + b.vel[1]**2) for b in bodies)


def potential_energy(bodies):
    pe = 0.0
    for i in range(len(bodies)):
        for j in range(i + 1, len(bodies)):
            r     = bodies[j].pos - bodies[i].pos
            dist  = np.sqrt(r[0]*r[0] + r[1]*r[1] + SOFTENING2)
            pe   -= G * bodies[i].mass * bodies[j].mass / dist
    return pe


def total_energy(bodies):
    return kinetic_energy(bodies) + potential_energy(bodies)


def angular_momentum(bodies):
    L = 0.0
    for b in bodies:
        L += b.mass * (b.pos[0] * b.vel[1] - b.pos[1] * b.vel[0])
    return L


def adaptive_dt(bodies, dt_max=0.001, dt_min=1e-6, eta=0.01):
    min_timescale = float('inf')
    alive = [(i, j) for i in range(len(bodies)) for j in range(i+1, len(bodies))]
    for i, j in alive:
        r     = bodies[j].pos - bodies[i].pos
        dist2 = r[0]*r[0] + r[1]*r[1] + SOFTENING2
        dist  = dist2 ** 0.5
        acc_i = np.linalg.norm(bodies[i].acc)
        acc_j = np.linalg.norm(bodies[j].acc)
        if acc_i > 0:
            min_timescale = min(min_timescale, np.sqrt(dist / acc_i))
        if acc_j > 0:
            min_timescale = min(min_timescale, np.sqrt(dist / acc_j))
    dt = eta * min_timescale
    return float(np.clip(dt, dt_min, dt_max))


def update_nbody(bodies, dt, use_clusters=False):
    if use_clusters:
        to_add, to_remove = _cluster_collisions(bodies)
    else:
        to_add, to_remove = _detect_collisions(bodies)

    dt2_half = 0.5 * dt * dt
    dt_half  = 0.5 * dt

    for body in bodies:
        if body in to_remove:
            continue
        body.pos += body.vel * dt + body.acc * dt2_half

    new_accels = _compute_accelerations(bodies, to_remove)

    for i, body in enumerate(bodies):
        if body in to_remove:
            continue
        body.vel += (body.acc + new_accels[i]) * dt_half

    for i, body in enumerate(bodies):
        if body in to_remove:
            continue
        body.acc = new_accels[i]

    for body in to_remove:
        if body in bodies:
            bodies.remove(body)

    bodies.extend(to_add)


def update(body1, body2, dt):
    dt2_half = 0.5 * dt * dt
    dt_half  = 0.5 * dt

    body1.pos += body1.vel * dt + body1.acc * dt2_half
    body2.pos += body2.vel * dt + body2.acc * dt2_half

    r         = body2.pos - body1.pos
    dist2     = r[0]*r[0] + r[1]*r[1]
    dist      = np.sqrt(dist2)
    inv_dist3 = (dist2 + SOFTENING2) ** (-1.5)
    factor    = G * inv_dist3

    new_acc1  =  factor * body2.mass * r
    new_acc2  = -factor * body1.mass * r

    body1.vel += (body1.acc + new_acc1) * dt_half
    body2.vel += (body2.acc + new_acc2) * dt_half

    body1.acc  = new_acc1
    body2.acc  = new_acc2

    return dist