import physics


def run_simulation(bodies, steps, dt):
    """
    Original batch-mode runner — unchanged behavior.
    Runs the full simulation start to finish with no external interruption.
    Use this for offline experiments / report generation.
    """
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


def run_live_simulation(bodies, dt, bridge, max_steps=None, print_every=500):
    """
    Step-by-step runner for live Unity control.

    Each tick:
      1. drain and apply any pending edit/spawn/remove commands from Unity
      2. advance physics by one dt
      3. record history
      4. broadcast updated positions back to Unity

    Runs until max_steps is reached, or forever if max_steps is None
    (stop with Ctrl+C, or add your own external stop condition).
    """
    physics.initialize_accelerations(bodies)

    for body in bodies:
        body.xhist.append(body.pos[0])
        body.yhist.append(body.pos[1])

    energy_0   = physics.total_energy(bodies)
    momentum_0 = physics.angular_momentum(bodies)

    i = 0
    last_body_count = len(bodies)

    while max_steps is None or i < max_steps:
        bodies = bridge.apply_pending_commands(bodies)

        # spawn/remove changes body count — newly added bodies have acc=zeros
        # by default (set in Body.__init__), which would corrupt Verlet's
        # first step for them. Reseed everyone's acceleration when this happens.
        if len(bodies) != last_body_count:
            physics.initialize_accelerations(bodies)
            last_body_count = len(bodies)

        physics.update_nbody(bodies, dt)

        for body in bodies:
            body.xhist.append(body.pos[0])
            body.yhist.append(body.pos[1])

        bridge.broadcast_positions(bodies)

        if i % print_every == 0:
            E = physics.total_energy(bodies)
            L = physics.angular_momentum(bodies)
            E_drift = abs((E - energy_0) / energy_0) * 100 if energy_0 != 0 else 0
            L_drift = abs((L - momentum_0) / momentum_0) * 100 if momentum_0 != 0 else 0
            print(f"step {i:5d} | bodies={len(bodies):2d} | "
                  f"E={E:.6f} drift={E_drift:.8f}% | L={L:.6f} drift={L_drift:.8f}%")

        i += 1

    return bodies