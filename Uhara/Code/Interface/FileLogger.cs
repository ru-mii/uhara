using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

public class FileLogger : MainShared
{
    string FilePath;
    int MaxLines;
    int RemoveLines;
    bool Enabled;

    int CurrentLines;

    public void Start(string filePath, int maxLines = 6000, int removeLinesCountWhenExceeded = 1000)
    {
        try
        {
            FilePath = filePath;
            MaxLines = maxLines;
            RemoveLines = removeLinesCountWhenExceeded;

            // ---
            if (RemoveLines > MaxLines)
                throw new Exception("Max lines can't exceed remove lines");

            // ---
            if (File.Exists(FilePath)) CurrentLines = File.ReadAllLines(FilePath).Length;

            // ---
            Enabled = true;
        }
        catch { }
    }

    public void Stop()
    {
        try
        {
            Enabled = false;
        }
        catch { }
    }

    public void Log(string message)
    {
        try
        {
            if (!Enabled) return;

            // ---
            string gameName = ProcessInstance.ProcessName;
            if (string.IsNullOrEmpty(gameName)) throw new Exception("Couldn't retrieve game name for logging");

            string stamp = "00:00:00.000";
            {
                TimeSpan ts = new TimeSpan();
                if (CurrentState.CurrentTimingMethod == TimingMethod.GameTime && CurrentState.CurrentTime.GameTime.HasValue)
                    ts = CurrentState.CurrentTime.GameTime.Value;

                else if (CurrentState.CurrentTimingMethod == TimingMethod.RealTime && CurrentState.CurrentTime.RealTime.HasValue)
                    ts = CurrentState.CurrentTime.RealTime.Value;

                stamp = string.Format(CultureInfo.InvariantCulture,
                "{0:00}:{1:00}:{2:00}.{3:000}",
                ts.Hours,
                ts.Minutes,
                ts.Seconds,
                ts.Milliseconds);
            }

            int liveSplitPid = Process.GetCurrentProcess().Id;
            int gamePid = ProcessInstance != null ? ProcessInstance.Id : 0;

            message = $"LS[{liveSplitPid}]" + $"GM[{gamePid}] | " + gameName + " | " + stamp + " | " + message;

            // ---
            File.AppendAllLines(FilePath, new string[] { message });
            CurrentLines++;

            if (CurrentLines > MaxLines)
            {
                string[] oldArray = File.ReadAllLines(FilePath);
                string[] newArray = new string[oldArray.Length - RemoveLines];
                Array.Copy(oldArray, RemoveLines, newArray, 0, newArray.Length);
                File.WriteAllText(FilePath, string.Join("\r\n", newArray));
                CurrentLines -= RemoveLines;
            }
        }
        catch { }
    }
}