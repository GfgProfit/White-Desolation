using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class FPSCounterManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _fpsText;

    [Space]
    [SerializeField] private float _updateInterval = 1f;
    [SerializeField] private bool _showSystemInformation = false;

    private int _frameCount = 0;
    private float _elapsedTime = 0f;

    private float _minFps = float.MaxValue;
    private float _maxFps = float.MinValue;
    private float _currentFps = 0;

    private string _operationSystem;
    private string _processor;
    private int _processorCores;
    private string _videoCard;
    private int _maxMemorySize;
    private MEMORYSTATUSEX memStatus;

    private void Start()
    {
        if (_fpsText == null)
        {
            return;
        }

        memStatus = new();

        SetupSystemInfo();
    }

    private void Update()
    {
        _frameCount++;
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _updateInterval)
        {
            _currentFps = _frameCount / _elapsedTime;

            _minFps = Mathf.Min(_minFps, _currentFps);
            _maxFps = Mathf.Max(_maxFps, _currentFps);

            DisplayInfo();

            _frameCount = 0;
            _elapsedTime = 0f;
        }
    }

    private void SetupSystemInfo()
    {
        _operationSystem = SystemInfo.operatingSystem;
        _processor = SystemInfo.processorType;
        _processorCores = SystemInfo.processorCount;
        _videoCard = SystemInfo.graphicsDeviceName;
        _maxMemorySize = SystemInfo.systemMemorySize;
    }

    private void DisplayInfo()
    {
        if (GlobalMemoryStatusEx(memStatus))
        {
            float totalMemoryInMB = memStatus.ullTotalPhys / (1024f * 1024f);
            float usedMemoryInMB = (memStatus.ullTotalPhys - memStatus.ullAvailPhys) / (1024f * 1024f);
            string currentFPS = $"<color=#27FF00>Current FPS:</color> {Mathf.RoundToInt(_currentFps)}\n";
            string minFPS = $"<color=#FF9090>Min FPS:</color> {Mathf.RoundToInt(_minFps)}\n";
            string maxFPS = $"<color=#94C2FF>Max FPS:</color> {Mathf.RoundToInt(_maxFps)}";
            
            string operationSystem = $"\n<color=#D1B4FF>OS:</color> {_operationSystem}\n";
            string processor = $"<color=#FFD6A3>CPU:</color> {_processor} ({_processorCores} cores)\n";
            string videoCard = $"<color=#D9FF64>GPU:</color> {_videoCard}\n";
            string maxMemorySize = $"<color=#94FFD9>RAM:</color> {Utils.FormatNumber(Mathf.RoundToInt(usedMemoryInMB), '.')} MB / {Utils.FormatNumber(Mathf.RoundToInt(totalMemoryInMB),'.')} MB\n";

            string fps = $"{currentFPS}{minFPS}{maxFPS}";
            string systemInfo = $"{operationSystem}{processor}{videoCard}{maxMemorySize}";

            _fpsText.text = _showSystemInformation ? fps + "\n" + systemInfo : fps;
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class MEMORYSTATUSEX
{
    public uint dwLength;
    public uint dwMemoryLoad;
    public ulong ullTotalPhys;
    public ulong ullAvailPhys;
    public ulong ullTotalPageFile;
    public ulong ullAvailPageFile;
    public ulong ullTotalVirtual;
    public ulong ullAvailVirtual;
    public ulong ullAvailExtendedVirtual;

    public MEMORYSTATUSEX()
    {
        this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
    }
}