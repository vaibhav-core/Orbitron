import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation

COLORS = ['#4477ff', '#ff4444', '#44cc44', '#ffaa00', '#cc44ff',
          '#00cccc', '#ff88cc', '#88ff44', '#ff6600', '#aaaaaa']

def animate(bodies, interval=10, trail_length=300, on_key=None):
    fig, ax = plt.subplots(figsize=(8, 8), facecolor='black')
    ax.set_facecolor('black')
    ax.set_aspect('equal')

    all_x = [x for body in bodies for x in body.xhist]
    all_y = [y for body in bodies for y in body.yhist]
    padding = 20
    ax.set_xlim(min(all_x) - padding, max(all_x) + padding)
    ax.set_ylim(min(all_y) - padding, max(all_y) + padding)

    ax.tick_params(colors='white')
    for spine in ax.spines.values():
        spine.set_edgecolor('#333333')

    dots   = []
    trails = []
    labels = []

    for idx, body in enumerate(bodies):
        color = COLORS[idx % len(COLORS)]
        size  = max(4, min(12, body.rad * 0.6))
        dot,   = ax.plot([], [], 'o', color=color, markersize=size, zorder=3)
        trail, = ax.plot([], [], '-', color=color, alpha=0.4, linewidth=0.8, zorder=2)
        label  = ax.text(0, 0, body.name, color=color, fontsize=7,
                         ha='center', va='bottom', zorder=4)
        dots.append(dot)
        trails.append(trail)
        labels.append(label)

    energy_text = ax.text(
        0.02, 0.97, '', transform=ax.transAxes,
        color='white', fontsize=8, va='top', fontfamily='monospace'
    )
    momentum_text = ax.text(
        0.02, 0.92, '', transform=ax.transAxes,
        color='white', fontsize=8, va='top', fontfamily='monospace'
    )

    frames = min(len(body.xhist) for body in bodies)

    E0 = None
    L0 = None

    def update(frame):
        nonlocal E0, L0

        artists = []

        for idx, body in enumerate(bodies):
            if frame >= len(body.xhist):
                continue

            x = body.xhist[frame]
            y = body.yhist[frame]

            dots[idx].set_data([x], [y])

            start = max(0, frame - trail_length)
            trails[idx].set_data(body.xhist[start:frame], body.yhist[start:frame])

            labels[idx].set_position((x, y + body.rad * 0.15 + 2))

            artists += [dots[idx], trails[idx], labels[idx]]

        if frame % 50 == 0:
            E = sum(
                0.5 * b.mass * (b.vel[0]**2 + b.vel[1]**2)
                for b in bodies
            )
            L = sum(
                b.mass * (b.pos[0] * b.vel[1] - b.pos[1] * b.vel[0])
                for b in bodies
            )
            if E0 is None:
                E0, L0 = E, L

            E_drift = abs((E - E0) / E0) * 100 if E0 != 0 else 0
            L_drift = abs((L - L0) / L0) * 100 if L0 != 0 else 0

            energy_text.set_text(f"E = {E:.4f}  drift = {E_drift:.4f}%")
            momentum_text.set_text(f"L = {L:.4f}  drift = {L_drift:.4f}%")

        artists += [energy_text, momentum_text]
        return artists

    ani = FuncAnimation(
        fig, update,
        frames=frames,
        interval=interval,
        blit=True
    )

    if on_key is not None:
        fig.canvas.mpl_connect(
            'key_press_event',
            lambda event: on_key(event, bodies)
        )

    plt.tight_layout()
    plt.show()
    return ani