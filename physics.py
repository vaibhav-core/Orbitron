import numpy as np
from bodeis import Body

# Gravitational constant (set to 1 for simulation units)
G = 1

# Softening length squared — precomputed for efficiency
# Prevents force singularities at very close range
# Set to ~1% of your smallest expected orbital radius
SOFTENING2 = 1e-4   # ε² = (0.01)²


def merge(body1, body2):
    """Inelastic merge conserving mass, momentum, and volume."""
    new_mass = body1.mass + body2.mass
    inv_mass = 1.0 / new_mass

    new_pos = (body1.mass * body1.pos + body2.mass * body2.pos) * inv_mass
    new_vel = (body1.mass * body1.vel + body2.mass * body2.vel) * inv_mass
    new_acc = (body1.mass * body1.acc + body2.mass * body2.acc) * inv_mass

    # Volume-conserving radius: r³ = r1³ + r2³
    rad_new = (body1.rad ** 3 + body2.rad ** 3) ** (1.0 / 3.0)

    body3 = Body(f"{body1.name}+{body2.name}", new_mass, new_pos, new_vel, rad_new)
    body3.acc = new_acc
    return body3


def _compute_accelerations(bodies, skip_set):
    """
    Compute net acceleration for every body using Newton's 3rd law:
    compute each pair once, apply equal-and-opposite forces to both.

    This halves the force evaluations vs the naive O(n²) double loop.
    skip_set: bodies scheduled for removal — excluded from force calc.

    Returns a list of acceleration vectors, index-aligned with `bodies`.
    """
    n = len(bodies)
    accels = [np.zeros(2) for _ in range(n)]

    # Build index list of surviving bodies only
    alive = [i for i, b in enumerate(bodies) if b not in skip_set]

    for idx in range(len(alive)):
        i = alive[idx]
        bi = bodies[i]

        for jdx in range(idx + 1, len(alive)):
            j = alive[jdx]
            bj = bodies[j]

            r = bj.pos - bi.pos

            # Softened inverse-distance-cubed: avoids sqrt + division separately
            dist2 = r[0]*r[0] + r[1]*r[1] + SOFTENING2
            inv_dist3 = dist2 ** (-1.5)          # 1 / (r² + ε²)^(3/2)

            # F/m directly → acceleration, scaled by G
            # a_i += G * mj * r̂ / |r|²  =  G * mj * r / |r|³
            factor = G * inv_dist3
            ai_contrib = factor * bj.mass * r
            aj_contrib = factor * bi.mass * r    # equal magnitude, opposite direction

            accels[i] += ai_contrib
            accels[j] -= aj_contrib              # Newton's 3rd law, no extra eval

    return accels


def _detect_collisions(bodies):
    """
    Detect overlapping body pairs and build merge lists.
    Uses merged_pairs to ensure each body participates in at most one merge per tick.

    Returns (to_add, to_remove_set).
    """
    to_add     = []
    to_remove  = set()
    merged_idx = set()   # indices already consumed this tick

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

            if dist2 < touch * touch:           # avoid sqrt for collision test
                print(f"Collision: {body1.name} + {body2.name}")
                to_add.append(merge(body1, body2))
                to_remove.add(body1)
                to_remove.add(body2)
                merged_idx.add(i)
                merged_idx.add(j)
                break                           # body1 is consumed; move to next i

    return to_add, to_remove


def update_nbody(bodies, dt):
    """
    Velocity Verlet N-body integrator with collision merging.

    Velocity Verlet is 2nd-order symplectic — it conserves energy to machine
    precision over arbitrarily long simulations, making it suitable for
    millions-of-years timescales.

    Algorithm per step:
        1. Collision detection  (before integration, on current positions)
        2. pos  +=  vel·dt  +  ½·acc·dt²        (half-step position)
        3. acc_new  =  F(pos_new) / m            (forces at new positions)
        4. vel  +=  ½·(acc + acc_new)·dt         (full-step velocity)
        5. acc  =  acc_new                        (store for next step)
        6. Apply merges
    """

    # ── Step 1: Collision detection ───────────────────────────────────────────
    to_add, to_remove = _detect_collisions(bodies)

    dt2_half = 0.5 * dt * dt
    dt_half  = 0.5 * dt

    # ── Step 2: Position half-step  x += v·dt + ½·a·dt² ─────────────────────
    for body in bodies:
        if body in to_remove:
            continue
        body.pos += body.vel * dt + body.acc * dt2_half

    # ── Step 3: Accelerations at new positions ────────────────────────────────
    new_accels = _compute_accelerations(bodies, to_remove)

    # ── Step 4: Velocity full-step  v += ½·(a_old + a_new)·dt ───────────────
    for i, body in enumerate(bodies):
        if body in to_remove:
            continue
        body.vel += (body.acc + new_accels[i]) * dt_half

    # ── Step 5: Store new accelerations for next tick ─────────────────────────
    for i, body in enumerate(bodies):
        if body in to_remove:
            continue
        body.acc = new_accels[i]

    # ── Step 6: Apply merges ──────────────────────────────────────────────────
    for body in to_remove:
        if body in bodies:
            bodies.remove(body)

    bodies.extend(to_add)


def update(body1, body2, dt):
    """
    Velocity Verlet update for a standalone 2-body system.
    Returns the distance between the two bodies after the step.
    """
    dt2_half = 0.5 * dt * dt
    dt_half  = 0.5 * dt

    # Step 2: position half-step
    body1.pos += body1.vel * dt + body1.acc * dt2_half
    body2.pos += body2.vel * dt + body2.acc * dt2_half

    # Step 3: new forces at updated positions
    r     = body2.pos - body1.pos
    dist2 = r[0]*r[0] + r[1]*r[1]
    dist  = np.sqrt(dist2)

    inv_dist3  = (dist2 + SOFTENING2) ** (-1.5)
    factor     = G * inv_dist3

    new_acc1 =  factor * body2.mass * r
    new_acc2 = -factor * body1.mass * r

    # Step 4: velocity full-step
    body1.vel += (body1.acc + new_acc1) * dt_half
    body2.vel += (body2.acc + new_acc2) * dt_half

    # Step 5: store
    body1.acc = new_acc1
    body2.acc = new_acc2

    return dist