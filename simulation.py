import physics

def run_simulation(bodies, steps, dt):
    physics.initialize_accelerations(bodies)

    for body in bodies:
        body.xhist.append(body.pos[0])
        body.yhist.append(body.pos[1])

    energy_0 = physics.total_energy(bodies)
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