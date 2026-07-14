import time
import physics


def run_simulation(bodies, steps, dt):
    physics.initialize_accelerations(bodies)

    for body in bodies:
        body.xhist.append(body.pos[0])
        body.yhist.append(body.pos[1])

    energy_0   = physics.total_energy(bodies)
    momentum_0 = physics.angular_momentum(bodies)

    for i in range(steps):
        physics.update_nbody(bodies, dt)

        for body in bodies:
            body.xhist.append(body.pos[0])
            body.yhist.append(body.pos[1])

        if i % 500 == 0:
            E = physics.total_energy(bodies)
            L = physics.angular_momentum(bodies)
            E_drift = abs((E - energy_0) / energy_0) * 100 if energy_0 != 0 else 0
            L_drift = abs((L - momentum_0) / momentum_0) * 100 if momentum_0 != 0 else 0
            print(f"step {i:5d} | E={E:.6f} drift={E_drift:.12f}% | L={L:.6f} drift={L_drift:.12f}%")

    return bodies


def run_live_simulation(bodies, dt, bridge, max_steps=None,
                        print_every=500, broadcast_every=10,
                        target_speed=1.0):
    """
    Live simulation loop driven by the TCP bridge.

    broadcast_every: only send a TCP packet every N steps.
    target_speed: simulated years per real second. Default 1.0 means
        1 simulated year takes 1 real second (Earth orbits in ~1 s).
        Set to 0.05 for 1 sim-year per 20 real seconds (comfortable to watch).
        Set to None to run at full CPU speed (original behaviour).
    """
    physics.initialize_accelerations(bodies)

    for body in bodies:
        body.xhist.append(body.pos[0])
        body.yhist.append(body.pos[1])

    energy_0   = physics.total_energy(bodies)
    momentum_0 = physics.angular_momentum(bodies)

    i              = 0
    sim_time       = 0.0
    last_body_count = len(bodies)

    # throttle: compute wall-clock seconds per physics step
    # step_wall = dt / target_speed  (e.g. dt=0.0001, speed=0.05 → 2 ms/step)
    step_wall = (dt / target_speed) if target_speed else None
    _t0 = time.perf_counter()    # reference time for drift correction

    while max_steps is None or i < max_steps:

        # 1. apply Unity commands
        bodies = bridge.apply_pending_commands(bodies)

        # 2. reseed if body count changed due to spawn/remove command
        if len(bodies) != last_body_count:
            physics.initialize_accelerations(bodies)
            last_body_count = len(bodies)

        # 3. advance physics — returns merge events that happened this tick
        merge_events = physics.update_nbody(bodies, dt)
        sim_time    += dt
        if merge_events:
            print("Broadcast merge events:", merge_events)

        # 4. reseed if a merge just happened inside update_nbody
        if len(bodies) != last_body_count:
            physics.initialize_accelerations(bodies)
            last_body_count = len(bodies)

        # 5. record history
        for body in bodies:
            body.xhist.append(body.pos[0])
            body.yhist.append(body.pos[1])

        # 6. broadcast — throttled so TCP buffer doesn't back up
        should_broadcast = (
            i % broadcast_every == 0
                or len(merge_events) > 0
                )

        if should_broadcast:
            E = physics.total_energy(bodies)
            L = physics.angular_momentum(bodies)

            bridge.broadcast_state(
                bodies=bodies,
                sim_time=sim_time,
                step=i,
                energy=E,
                angular_momentum=L,
                merge_events=merge_events
            )
            if merge_events:
                print("Broadcast merge events:", merge_events)

            if i % print_every == 0:
                E_drift = abs((E - energy_0) / energy_0) * 100 if energy_0 != 0 else 0
                L_drift = abs((L - momentum_0) / momentum_0) * 100 if momentum_0 != 0 else 0
                print(
                    f"step {i:5d} | t={sim_time:.4f}yr | bodies={len(bodies):2d} | "
                    f"E drift={E_drift:.8f}% | L drift={L_drift:.8f}%"
                )

        # ── throttle ─────────────────────────────────────────────────────────
        if step_wall is not None:
            # drift-corrected sleep: target wall time for this step
            target_wall = _t0 + (i + 1) * step_wall
            slack = target_wall - time.perf_counter()
            if slack > 0:
                time.sleep(slack)

        i += 1

    return bodies