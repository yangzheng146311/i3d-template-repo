using i3D;
using System;
using System.Collections.Generic;

public partial class SimDataService : Item
{
    public struct DeviceInfo
    {
        public string Name;
        public string Zone;
        public Vector3 Position;
        public bool Online;
        public float Temperature;
        public float Humidity;
        public float Pressure;
        public float Current;
        public float Rpm;
        public float Vibration;
        public float Yield;
        public float GoodRate;
        public float PowerConsumption;
        public int HealthScore;
        public DateTime NextMaintenance;
        public float RunHours;
    }

    public struct AlertInfo
    {
        public string Title;
        public string Description;
        public string Zone;
        public string DeviceName;
        public DateTime Time;
        public int Level;
        public string Status;
        public string Suggestion;
    }

    public struct SafetyEvent
    {
        public string Type;
        public string Zone;
        public string Description;
        public DateTime Time;
    }

    private const int HISTORY_LEN = 60;
    private const int DEVICE_COUNT = 6;

    private static readonly Vector3[] DEVICE_POSITIONS = {
        new(0.228f, 0.916f, -0.799f),
        new(0.585f, 0.287f, -0.058f),
        new(-0.664f, 0.287f, -0.835f),
        new(-0.664f, 0.061f, -0.064f),
        new(1.79f, 0.061f, 0.518f),
        new(1.79f, 0.061f, -0.16f)
    };

    private static readonly string[] DEVICE_NAMES = {
        "冷却塔", "泵站", "泵站2", "泵站3", "流水线1号", "流水线2号"
    };

    private static readonly string[] DEVICE_ZONES = {
        "A", "B", "B", "B", "C", "C"
    };

    private static readonly bool[] DEVICE_ONLINE = {
        true, true, false, true, true, true
    };

    public DeviceInfo[] Devices = new DeviceInfo[DEVICE_COUNT];
    public float[][] TempHistory = new float[DEVICE_COUNT][];
    public float[][] HumidityHistory = new float[DEVICE_COUNT][];
    public float[][] PressureHistory = new float[DEVICE_COUNT][];
    public float[] DailyYieldHistory = new float[24];
    public float[] EnergyHistory = new float[24];
    public List<AlertInfo> Alerts = new();
    public List<SafetyEvent> SafetyEvents = new();
    public int AlertCount => Alerts.Count;

    private const int MAX_ALERTS = 20;

    private static readonly string[] ALERT_TITLES = {
        "温度超标", "网络中断", "设备离线", "压力异常",
        "电流过载", "振动超标", "湿度异常", "转速异常"
    };
    private static readonly string[] ALERT_DESCS = {
        "温度持续升高，已超过安全阈值",
        "与主控服务器连接超时",
        "无响应，已离线超过10分钟",
        "压力超出正常范围",
        "电流超过额定值",
        "振动幅度超过安全标准",
        "湿度偏离正常范围",
        "转速异常波动"
    };
    private static readonly string[] ALERT_SUGGESTIONS = {
        "检查散热系统，确认冷却液循环正常",
        "检查网络线缆连接，重启网络模块",
        "现场检查设备供电和通讯状态",
        "检查管道密封性，排查泄漏点",
        "降低负载运行，检查电机绝缘",
        "检查轴承磨损，校准动平衡",
        "检查除湿设备运行状态",
        "检查传动系统，确认皮带张紧度"
    };
    private static readonly int[] ALERT_LEVELS = { 1, 1, 2, 1, 2, 1, 1, 2 };

    public float[,] GasConcentration = new float[3, 3];
    public int PersonnelCount = 23;

    public event Action OnDataUpdated;

    private double _elapsed;
    private const double UPDATE_INTERVAL = 1.0;
    private static readonly Random _rng = new();

    public override void iStart()
    {
        for (int i = 0; i < DEVICE_COUNT; i++)
        {
            TempHistory[i] = new float[HISTORY_LEN];
            HumidityHistory[i] = new float[HISTORY_LEN];
            PressureHistory[i] = new float[HISTORY_LEN];
        }

        InitDevices();
        InitAlerts();
        InitSafetyEvents();
        InitGasData();
        GenerateInitialHistory();
        GenerateDailyHistory();
        AddToGroup("sim_data");
        SetUpdate(true);
        S3.Print("[SimDataService] initialized " + DEVICE_COUNT + " devices, " + Alerts.Count + " alerts");
    }

    public override void Update(double delta)
    {
        _elapsed += delta;
        if (_elapsed < UPDATE_INTERVAL) return;
        _elapsed = 0;
        RefreshData();
        OnDataUpdated?.Invoke();
    }

    private void InitDevices()
    {
        for (int i = 0; i < DEVICE_COUNT; i++)
        {
            Devices[i] = new DeviceInfo
            {
                Name = DEVICE_NAMES[i],
                Zone = DEVICE_ZONES[i],
                Position = DEVICE_POSITIONS[i],
                Online = DEVICE_ONLINE[i],
                Temperature = 25f,
                Humidity = 45f,
                Pressure = 101.3f,
                Current = 12f,
                Rpm = 1480f,
                Vibration = 0.1f,
                Yield = 100f,
                GoodRate = 97.5f,
                PowerConsumption = 120f,
                HealthScore = 92,
                NextMaintenance = DateTime.Now.AddDays(16),
                RunHours = 128f
            };
        }
    }

    private void InitAlerts()
    {
        AddAlert(0, 0);
        AddAlert(1, 1);
        AddAlert(2, 2);
    }

    private void InitSafetyEvents()
    {
        SafetyEvents.Add(new SafetyEvent
        {
            Type = "未戴安全帽",
            Zone = "A",
            Description = "A区入口检测到人员未佩戴安全帽",
            Time = DateTime.Now.AddHours(-2)
        });
    }

    private void InitGasData()
    {
        for (int z = 0; z < 3; z++)
        {
            GasConcentration[z, 0] = (float)(8 + _rng.NextDouble() * 6);
            GasConcentration[z, 1] = (float)(0.02 + _rng.NextDouble() * 0.08);
            GasConcentration[z, 2] = (float)(0.01 + _rng.NextDouble() * 0.04);
        }
    }

    private void AddAlert(int templateIdx, int deviceIdx)
    {
        var alert = new AlertInfo
        {
            Title = ALERT_TITLES[templateIdx],
            Description = DEVICE_NAMES[deviceIdx] + ALERT_DESCS[templateIdx],
            Zone = DEVICE_ZONES[deviceIdx],
            DeviceName = DEVICE_NAMES[deviceIdx],
            Time = DateTime.Now,
            Level = ALERT_LEVELS[templateIdx],
            Status = "Active",
            Suggestion = ALERT_SUGGESTIONS[templateIdx]
        };
        Alerts.Insert(0, alert);
        if (Alerts.Count > MAX_ALERTS)
            Alerts.RemoveAt(Alerts.Count - 1);
    }

    private void MaybeGenerateAlert()
    {
        if (_rng.NextDouble() > 0.15) return;
        int tIdx = _rng.Next(0, ALERT_TITLES.Length);
        int dIdx = _rng.Next(0, DEVICE_COUNT);
        AddAlert(tIdx, dIdx);
    }

    private void GenerateInitialHistory()
    {
        for (int i = 0; i < DEVICE_COUNT; i++)
        {
            for (int j = 0; j < HISTORY_LEN; j++)
            {
                float phase = j / (float)HISTORY_LEN * Mathf.Pi * 4f;
                TempHistory[i][j] = 25f + Mathf.Sin(phase + i * 0.7f) * 3f + Rand(-0.5f, 0.5f);
                HumidityHistory[i][j] = 45f + Mathf.Sin(phase + i * 0.5f) * 5f + Rand(-1f, 1f);
                PressureHistory[i][j] = 101.3f + Mathf.Sin(phase + i * 0.3f) * 1f + Rand(-0.2f, 0.2f);
            }
            Devices[i].Temperature = TempHistory[i][HISTORY_LEN - 1];
            Devices[i].Humidity = HumidityHistory[i][HISTORY_LEN - 1];
            Devices[i].Pressure = PressureHistory[i][HISTORY_LEN - 1];
        }
    }

    private void GenerateDailyHistory()
    {
        for (int h = 0; h < 24; h++)
        {
            DailyYieldHistory[h] = 80f + Mathf.Sin(h / 24f * Mathf.Pi * 2f) * 25f + Rand(-5f, 5f);
            EnergyHistory[h] = 50f + Mathf.Sin(h / 24f * Mathf.Pi * 2f + 1f) * 30f + Rand(-3f, 3f);
        }
    }

    private void RefreshData()
    {
        float t = (float)Time.GetTicksMsec() / 1000f;
        for (int i = 0; i < DEVICE_COUNT; i++)
        {
            if (!Devices[i].Online) continue;

            ShiftHistory(TempHistory[i]);
            ShiftHistory(HumidityHistory[i]);
            ShiftHistory(PressureHistory[i]);

            float phase = t * 0.1f + i * 0.7f;
            Devices[i].Temperature = 25f + Mathf.Sin(phase) * 3f + Rand(-0.5f, 0.5f);
            Devices[i].Humidity = 45f + Mathf.Sin(phase * 0.8f + 1f) * 5f + Rand(-1f, 1f);
            Devices[i].Pressure = 101.3f + Mathf.Sin(phase * 0.6f + 2f) * 1f + Rand(-0.2f, 0.2f);
            Devices[i].Current = 12f + Mathf.Sin(phase * 0.5f + 3f) * 2f + Rand(-0.3f, 0.3f);
            Devices[i].Rpm = 1480f + Mathf.Sin(phase * 0.4f + 4f) * 50f + Rand(-5f, 5f);
            Devices[i].Vibration = 0.1f + Mathf.Sin(phase * 0.3f + 5f) * 0.05f + Rand(-0.01f, 0.01f);
            Devices[i].Yield = 100f + Mathf.Sin(phase * 0.2f + 6f) * 15f + Rand(-3f, 3f);
            Devices[i].GoodRate = 97.5f + Mathf.Sin(phase * 0.15f + 7f) * 1.5f + Rand(-0.3f, 0.3f);
            Devices[i].PowerConsumption = 120f + Mathf.Sin(phase * 0.25f + 8f) * 40f + Rand(-5f, 5f);
            Devices[i].HealthScore = CalculateDeviceHealth(i);
            Devices[i].RunHours += (float)UPDATE_INTERVAL / 3600f;

            TempHistory[i][HISTORY_LEN - 1] = Devices[i].Temperature;
            HumidityHistory[i][HISTORY_LEN - 1] = Devices[i].Humidity;
            PressureHistory[i][HISTORY_LEN - 1] = Devices[i].Pressure;
        }

        RefreshGasData();

        if (Alerts.Count > 0)
        {
            var top = Alerts[0];
            top.Time = DateTime.Now;
            Alerts[0] = top;
        }

        MaybeGenerateAlert();
    }

    private void RefreshGasData()
    {
        for (int z = 0; z < 3; z++)
        {
            GasConcentration[z, 0] += Rand(-0.3f, 0.3f);
            GasConcentration[z, 0] = Mathf.Clamp(GasConcentration[z, 0], 2f, 50f);
            GasConcentration[z, 1] += Rand(-0.005f, 0.005f);
            GasConcentration[z, 1] = Mathf.Clamp(GasConcentration[z, 1], 0.001f, 0.5f);
            GasConcentration[z, 2] += Rand(-0.003f, 0.003f);
            GasConcentration[z, 2] = Mathf.Clamp(GasConcentration[z, 2], 0.001f, 0.3f);
        }
    }

    private void ShiftHistory(float[] arr)
    {
        for (int i = 0; i < arr.Length - 1; i++)
            arr[i] = arr[i + 1];
    }

    private static float Rand(float min, float max)
    {
        return (float)(min + _rng.NextDouble() * (max - min));
    }

    public int CalculateDeviceHealth(int idx)
    {
        var d = Devices[idx];
        if (!d.Online) return 0;
        int score = 100;
        if (d.Temperature > 35f) score -= (int)((d.Temperature - 35f) * 5);
        if (d.Vibration > 0.15f) score -= (int)((d.Vibration - 0.15f) * 200);
        if (d.Current > 14f) score -= (int)((d.Current - 14f) * 8);
        if (d.Pressure > 103f || d.Pressure < 99f) score -= 5;
        return Mathf.Clamp(score, 0, 100);
    }

    public float[] GetZoneAvgTemp()
    {
        float[] avg = new float[3];
        int[] cnt = new int[3];
        for (int i = 0; i < DEVICE_COUNT; i++)
        {
            int zi = Devices[i].Zone[0] - 'A';
            if (zi >= 0 && zi < 3)
            {
                avg[zi] += Devices[i].Temperature;
                cnt[zi]++;
            }
        }
        for (int i = 0; i < 3; i++)
            if (cnt[i] > 0) avg[i] /= cnt[i];
        return avg;
    }

    public float[] GetZoneYield()
    {
        float[] total = new float[3];
        int[] cnt = new int[3];
        for (int i = 0; i < DEVICE_COUNT; i++)
        {
            int zi = Devices[i].Zone[0] - 'A';
            if (zi >= 0 && zi < 3)
            {
                total[zi] += Devices[i].Yield;
                cnt[zi]++;
            }
        }
        for (int i = 0; i < 3; i++)
            if (cnt[i] > 0) total[i] /= cnt[i];
        return total;
    }

    public float[] GetZoneGoodRate()
    {
        float[] avg = new float[3];
        int[] cnt = new int[3];
        for (int i = 0; i < DEVICE_COUNT; i++)
        {
            int zi = Devices[i].Zone[0] - 'A';
            if (zi >= 0 && zi < 3)
            {
                avg[zi] += Devices[i].GoodRate;
                cnt[zi]++;
            }
        }
        for (int i = 0; i < 3; i++)
            if (cnt[i] > 0) avg[i] /= cnt[i];
        return avg;
    }

    public float[] GetZoneEnergy()
    {
        float[] total = new float[3];
        int[] cnt = new int[3];
        for (int i = 0; i < DEVICE_COUNT; i++)
        {
            int zi = Devices[i].Zone[0] - 'A';
            if (zi >= 0 && zi < 3)
            {
                total[zi] += Devices[i].PowerConsumption;
                cnt[zi]++;
            }
        }
        for (int i = 0; i < 3; i++)
            if (cnt[i] > 0) total[i] /= cnt[i];
        return total;
    }

    public float GetOverallOEE()
    {
        return GetAvailability() * GetPerformance() * GetQuality() * 100f;
    }

    public float GetAvailability()
    {
        return (float)GetOnlineCount() / DEVICE_COUNT;
    }

    public float GetPerformance()
    {
        float total = 0;
        int count = 0;
        for (int i = 0; i < DEVICE_COUNT; i++)
        {
            if (Devices[i].Online)
            {
                total += Devices[i].Yield / 120f;
                count++;
            }
        }
        return count > 0 ? total / count : 0;
    }

    public float GetQuality()
    {
        float total = 0;
        int count = 0;
        for (int i = 0; i < DEVICE_COUNT; i++)
        {
            if (Devices[i].Online)
            {
                total += Devices[i].GoodRate;
                count++;
            }
        }
        return count > 0 ? total / count / 100f : 0;
    }

    public float GetTotalYield()
    {
        float total = 0;
        for (int i = 0; i < DEVICE_COUNT; i++)
            total += Devices[i].Yield;
        return total;
    }

    public float GetTotalEnergy()
    {
        float total = 0;
        for (int i = 0; i < DEVICE_COUNT; i++)
            total += Devices[i].PowerConsumption;
        return total;
    }

    public int GetOnlineCount()
    {
        int c = 0;
        for (int i = 0; i < DEVICE_COUNT; i++)
            if (Devices[i].Online) c++;
        return c;
    }

    public int GetOfflineCount() => DEVICE_COUNT - GetOnlineCount();

    public float GetOnlineRate() => (float)GetOnlineCount() / DEVICE_COUNT * 100f;

    public int FindDeviceIndex(string name)
    {
        for (int i = 0; i < DEVICE_COUNT; i++)
            if (Devices[i].Name == name) return i;
        return -1;
    }

    public int GetActiveAlertCount(int level)
    {
        int c = 0;
        for (int i = 0; i < Alerts.Count; i++)
            if (Alerts[i].Level == level && Alerts[i].Status == "Active") c++;
        return c;
    }

    public List<AlertInfo> GetAlertsByLevel(int level)
    {
        var result = new List<AlertInfo>();
        for (int i = 0; i < Alerts.Count; i++)
            if (level < 0 || Alerts[i].Level == level)
                result.Add(Alerts[i]);
        return result;
    }

    public void AcknowledgeAlert(int idx)
    {
        if (idx >= 0 && idx < Alerts.Count)
        {
            var a = Alerts[idx];
            a.Status = "Acknowledged";
            Alerts[idx] = a;
        }
    }

    public float GetGasCO(int zone) => GasConcentration[zone, 0];
    public float GetGasCH4(int zone) => GasConcentration[zone, 1];
    public float GetGasFlammable(int zone) => GasConcentration[zone, 2];
}
