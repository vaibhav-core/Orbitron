using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonLauncher : MonoBehaviour
{
    public static PythonLauncher Instance { get; private set; }

    [SerializeField] private string backendFolder = "Backend";

    private Process backendProcess;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private string GetExecutablePath(string executable)
    {
        bool isWindows =
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.WindowsEditor;

        string fileName = isWindows
            ? executable + ".exe"
            : executable;

        return Path.Combine(
            Application.dataPath,
            "..",
            backendFolder,
            fileName
        );
    }

    public void LaunchMain()
    {
        Launch("main");
    }

    public void LaunchSolarSystem()
    {
        Launch("solar_system");
    }

    private void Launch(string executable)
    {
        CloseBackend();

        string path = GetExecutablePath(executable);

        if (!File.Exists(path))
        {
            UnityEngine.Debug.LogError($"Backend not found:\n{path}");
            return;
        }

        backendProcess = new Process();

        backendProcess.StartInfo.FileName = path;
        backendProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
        backendProcess.StartInfo.UseShellExecute = false;
        backendProcess.StartInfo.CreateNoWindow = true;

        backendProcess.Start();

        UnityEngine.Debug.Log($"Started backend: {path}");
    }
public void CloseBackend()
{
    if (backendProcess == null)
        return;

    try
    {
        if (!backendProcess.HasExited)
        {
            backendProcess.Kill();
            backendProcess.WaitForExit(3000);
        }
    }
    catch (System.Exception e)
    {
        UnityEngine.Debug.LogWarning(e.Message);
    }

    backendProcess.Dispose();
    backendProcess = null;
}

}