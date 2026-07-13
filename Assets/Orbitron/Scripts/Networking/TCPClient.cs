using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;

public class TCPClient : MonoBehaviour
{
    public static TCPClient Instance { get; private set; }

    [Header("Connection")]
    [SerializeField] private string host = "127.0.0.1";
    [SerializeField] private int port    = 9000;

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;

    private TcpClient       _client;
    private NetworkStream   _stream;
    private Thread          _receiveThread;
    private bool            _running;
    private bool            _reconnecting;

    private readonly ConcurrentQueue<string> _messageQueue = new();
    private readonly StringBuilder           _incoming     = new();
    private readonly byte[]                  _buffer       = new byte[65536];

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => Connect();

    private void Update()
    {
        DrainMainThreadQueue();   // flush reconnect callbacks from receive thread

        while (_messageQueue.TryDequeue(out string msg))
        {
            try
            {
                SimulationState state = JsonConvert.DeserializeObject<SimulationState>(msg);
                if (state == null) continue;

                if (verboseLogging)
                    Debug.Log($"[TCPClient] step={state.step} bodies={state.bodies?.Length}");

                SimulationManager.Instance?.UpdateSimulation(state);
            }
            catch (Exception e)
            {
                //Debug.LogError($"[TCPClient] JSON parse error: {e.Message}");
            }
        }
    }

    private void OnApplicationQuit()
    {
        _running = false;
        _receiveThread?.Join(500);
        _stream?.Close();
        _client?.Close();
    }

    // ── Connection ────────────────────────────────────────────────────────────

    private void Connect()
    {
        try
        {
            _client = new TcpClient();
            _client.ReceiveTimeout = 5000;   // 5s read timeout — prevents infinite block
            _client.Connect(host, port);
            _stream  = _client.GetStream();
            _running = true;
            _incoming.Clear();               // clear any stale data from a previous session

            _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            _receiveThread.Start();

            Debug.Log($"[TCPClient] Connected to Python at {host}:{port}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[TCPClient] Connection failed: {e.Message}. Retrying in 3s...");
            Invoke(nameof(Connect), 3f);    // auto-retry — handles Python not being ready yet
        }
    }

    private void Reconnect()
    {
        if (_reconnecting) return;
        _reconnecting = true;

        _running = false;
        _stream?.Close();
        _client?.Close();

        Debug.LogWarning("[TCPClient] Disconnected from Python. Reconnecting in 3s...");
        Invoke(nameof(DoReconnect), 3f);
    }

    private void DoReconnect()
    {
        _reconnecting = false;
        Connect();
    }

    // ── Receive loop (background thread) ─────────────────────────────────────

    private void ReceiveLoop()
    {
        while (_running)
        {
            try
            {
                int bytesRead = _stream.Read(_buffer, 0, _buffer.Length);

                if (bytesRead <= 0)
                {
                    // Python closed the connection cleanly
                    MainThreadDispatch(Reconnect);
                    break;
                }

                _incoming.Append(Encoding.UTF8.GetString(_buffer, 0, bytesRead));

                // parse all complete newline-delimited messages
                while (true)
                {
                    string current = _incoming.ToString();
                    int newline    = current.IndexOf('\n');
                    if (newline == -1) break;

                    string message = current.Substring(0, newline);
                    _incoming.Remove(0, newline + 1);

                    if (!string.IsNullOrWhiteSpace(message))
                        _messageQueue.Enqueue(message);
                }
            }
            catch (System.IO.IOException)
            {
                // read timeout or connection reset
                MainThreadDispatch(Reconnect);
                break;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TCPClient] Receive error: {e.Message}");
                MainThreadDispatch(Reconnect);
                break;
            }
        }
    }

    // Unity API (Invoke, etc.) can only be called from the main thread.
    // This helper queues a callback to run on the next Update().
    private readonly ConcurrentQueue<Action> _mainThreadQueue = new();

    private void MainThreadDispatch(Action action) => _mainThreadQueue.Enqueue(action);

    // called in Update to drain main-thread callbacks from the receive thread
    private void DrainMainThreadQueue()
    {
        while (_mainThreadQueue.TryDequeue(out Action action))
            action?.Invoke();
    }

    // ── Send ──────────────────────────────────────────────────────────────────

    private void Send(string json)
    {
        if (_client == null || !_client.Connected)
        {
            Debug.LogWarning("[TCPClient] Not connected — command dropped.");
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(json + "\n");
            _stream.Write(data, 0, data.Length);
            _stream.Flush();

            if (verboseLogging)
                Debug.Log($"[TCPClient] Sent -> {json}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPClient] Send failed: {e.Message}");
        }
    }

    public void SendSpawnCommand(PlanetData data)
    {
        // Denormalize position from Unity world units back to AU before
        // sending to Python — Python physics works in AU, not Unity units.
        var (posX, posY) = Normalizer.DenormalizePosition(data.initialPosition);
        var (velX, velY) = Normalizer.DenormalizeVelocity(data.initialVelocity);
        float radAU      = Normalizer.DenormalizeRadius((float)data.radius);

        SpawnCommand command = new SpawnCommand
        {
            name = data.bodyName,
            mass = data.mass,
            rad  = radAU,
            pos  = new float[] { posX, posY },
            vel  = new float[] { velX, velY }
        };

        Send(JsonConvert.SerializeObject(command));
    }

    public void SendEditCommand(EditCommand command)
    {
        Send(JsonConvert.SerializeObject(command));
    }

    public void SendRemoveCommand(string bodyName)
    {
        Send(JsonConvert.SerializeObject(new RemoveCommand { name = bodyName }));
    }
}