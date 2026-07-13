import socket
import threading
import json
import queue
import numpy as np


class UnityBridge:
    def __init__(self, host="127.0.0.1", port=9000):
        self.host = host
        self.port = port

        self._server_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self._server_sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self._server_sock.bind((host, port))
        self._server_sock.listen(1)
        self._server_sock.settimeout(0.5)

        self._client_sock  = None
        self._client_lock  = threading.Lock()
        self._recv_buffer  = ""
        self._command_queue = queue.Queue()

        self._running      = False
        self._accept_thread = None
        self._recv_thread  = None

    def start(self):
        self._running = True
        self._accept_thread = threading.Thread(target=self._accept_loop, daemon=True)
        self._accept_thread.start()
        print(f"UnityBridge listening on {self.host}:{self.port}")

    def stop(self):
        self._running = False
        with self._client_lock:
            if self._client_sock:
                self._client_sock.close()
        self._server_sock.close()
        if self._accept_thread:
            self._accept_thread.join(timeout=1.0)
        if self._recv_thread:
            self._recv_thread.join(timeout=1.0)

    def _accept_loop(self):
        while self._running:
            try:
                client, addr = self._server_sock.accept()
                print(f"UnityBridge: Unity connected from {addr}")

                # join old recv thread before starting new one — prevents
                # two threads fighting over _client_sock on reconnect
                if self._recv_thread and self._recv_thread.is_alive():
                    self._recv_thread.join(timeout=1.0)

                with self._client_lock:
                    self._client_sock = client
                    self._recv_buffer = ""  # clear stale partial data on reconnect

                self._recv_thread = threading.Thread(
                    target=self._recv_loop, daemon=True
                )
                self._recv_thread.start()

            except socket.timeout:
                continue
            except OSError:
                break

    def _recv_loop(self):
        sock = self._client_sock
        sock.settimeout(0.5)

        while self._running:
            try:
                chunk = sock.recv(65536)
                if not chunk:
                    print("UnityBridge: Unity disconnected")
                    break

                self._recv_buffer += chunk.decode()

                while "\n" in self._recv_buffer:
                    line, self._recv_buffer = self._recv_buffer.split("\n", 1)
                    if not line.strip():
                        continue
                    try:
                        self._command_queue.put(json.loads(line))
                    except json.JSONDecodeError as e:
                        print(f"UnityBridge: malformed packet ignored ({e})")

            except socket.timeout:
                continue
            except OSError:
                break

        with self._client_lock:
            self._client_sock = None
            self._recv_buffer = ""  # clear on disconnect so reconnect starts clean

    def apply_pending_commands(self, bodies):
        while not self._command_queue.empty():
            try:
                command = self._command_queue.get_nowait()
            except queue.Empty:
                break

            cmd = command.get("cmd")
            if   cmd == "edit":   self._apply_edit(bodies, command)
            elif cmd == "spawn":  self._apply_spawn(bodies, command)
            elif cmd == "remove": self._apply_remove(bodies, command)
            else: print(f"UnityBridge: unknown command '{cmd}', ignored")

        return bodies

    def _find_body(self, bodies, name):
        for b in bodies:
            if b.name == name:
                return b
        return None

    def _apply_edit(self, bodies, command):
        body = self._find_body(bodies, command.get("name"))
        if body is None:
            print(f"UnityBridge edit: '{command.get('name')}' not found")
            return
        if "mass" in command: body.mass = float(command["mass"])
        if "vel"  in command: body.vel  = np.array(command["vel"],  dtype=float)
        if "pos"  in command: body.pos  = np.array(command["pos"],  dtype=float)
        if "rad"  in command: body.rad  = float(command["rad"])
        print(f"UnityBridge: edited '{body.name}'")

    def _apply_spawn(self, bodies, command):
        from bodeis import Body
        name = command.get("name", f"body_{len(bodies)}")
        if self._find_body(bodies, name):
            print(f"UnityBridge spawn: '{name}' already exists, ignored")
            return
        bodies.append(Body(
            name,
            float(command.get("mass", 1e-10)),
            tuple(command.get("pos",  [0.0, 0.0])),
            tuple(command.get("vel",  [0.0, 0.0])),
            float(command.get("rad",  1e-6))
        ))
        print(f"UnityBridge: spawned '{name}'")

    def _apply_remove(self, bodies, command):
        body = self._find_body(bodies, command.get("name"))
        if body is None:
            print(f"UnityBridge remove: '{command.get('name')}' not found")
            return
        bodies.remove(body)
        print(f"UnityBridge: removed '{body.name}'")

    def _send_tcp(self, message_bytes):
        with self._client_lock:
            if self._client_sock is None:
                return
            try:
                self._client_sock.sendall(message_bytes)
            except OSError as e:
                print(f"UnityBridge: send failed ({e})")
                self._client_sock = None

    def broadcast_state(self, bodies, sim_time, step, energy, angular_momentum,
                        merge_events=None):
        payload = {
            "step":             step,
            "sim_time":         sim_time,
            "energy":           energy,
            "angular_momentum": angular_momentum,
            "merge_events":     merge_events or [],
            "bodies": [
                
                    {
                        "name": b.name,
                        "parent": b.parent,
                        "type": b.body_type,
                        "status": b.status,

                        "pos": [float(b.pos[0]), float(b.pos[1])],
                        "vel": [float(b.vel[0]), float(b.vel[1])],
                        "acc": [float(b.acc[0]), float(b.acc[1])],

                        "mass": float(b.mass),
                        "rad": float(b.rad),

                        "escape_velocity": b.escape_velocity,
                        "orbital_velocity": b.orbital_velocity,
                        "orbital_period": b.orbital_period,
                        "total_energy": b.total_energy
                    
                }
                for b in bodies
            ]
        }
        self._send_tcp((json.dumps(payload) + "\n").encode())