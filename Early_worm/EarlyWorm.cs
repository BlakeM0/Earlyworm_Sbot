using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.IO;

public class CPHInline
{
    // =========================================================================
    // CONFIGURATION — Edit your message templates here
    // =========================================================================
    //
    // Available placeholders per message:
    //
    //   MSG_ALREADY_OPEN         (no placeholders)
    //   MSG_ENTRIES_OPEN         (no placeholders)
    //   MSG_ENTRIES_CLOSED       {count}
    //   MSG_ALREADY_ENTERED      {user}
    //   MSG_JOINED               {user}, {count}
    //   MSG_NO_ENTRIES           (no placeholders)
    //   MSG_WINNER               {winner}, {total}
    //   MSG_NO_ENTRIES_COUNT     (no placeholders)
    //   MSG_ENTRY_COUNT_ONE      (no placeholders)
    //   MSG_ENTRY_COUNT_MANY     {count}
    //   MSG_ODDS_NONE            {user}
    //   MSG_ODDS_NOT_ENTERED     {user}
    //   MSG_ODDS                 {user}, {userEntries}, {total}, {odds}
    //   MSG_RESET                (no placeholders)
    //   MSG_AUTO_CLOSING         (no placeholders)
    //   MSG_MONTHLY_WINNER       {winner}, {userEntries}, {total}
    //   MSG_MONTHLY_ARCHIVED     (no placeholders)
    //   MSG_MONTHLY_NO_ENTRIES   (no placeholders)
    //   MSG_MONTHLY_NOT_FOUND    (no placeholders)
    //
    // =========================================================================
    private const string MSG_ALREADY_OPEN = "Entries are already open! Use `!earlyworm reset` if you need to restart.";
    private const string MSG_ENTRIES_OPEN = "🐛 Early Worm entries are OPEN! Type !rub to enter.";
    private const string MSG_ENTRIES_CLOSED = "Entries closed! {count} worms in the dirt 🐛";
    private const string MSG_ALREADY_ENTERED = "@{user} you've already rubbed in the dirt 🐛";
    private const string MSG_JOINED = "@{user} joined the Early Worm draw! ({count} entries)";
    private const string MSG_NO_ENTRIES = "No worms entered!";
    private const string MSG_WINNER = "🎉 Early Worm Winner: @{winner}! (1 in {total} odds) 🐛";
    private const string MSG_NO_ENTRIES_COUNT = "No worms have entered yet! 🐛";
    private const string MSG_ENTRY_COUNT_ONE = "Current entries: 1 worm 🐛";
    private const string MSG_ENTRY_COUNT_MANY = "Current entries: {count} worms 🐛";
    private const string MSG_ODDS_NONE = "@{user} no worms have entered yet 🐛";
    private const string MSG_ODDS_NOT_ENTERED = "@{user} you haven't entered! Use !rub 🐛";
    private const string MSG_ODDS = "@{user} your odds of winning are {userEntries} in {total} ({odds}%) 🐛";
    private const string MSG_RESET = "⚠️ Early Worm entries have been reset.";
    private const string MSG_AUTO_CLOSING = "🐛 Early Worm entries will close in 20 minutes!";
    private const string MSG_MONTHLY_WINNER = "🎁 MONTHLY MERCH WINNER: @{winner}! ({userEntries} entries out of {total})";
    private const string MSG_MONTHLY_ARCHIVED = "Monthly Early Worm entries have been archived.";
    private const string MSG_MONTHLY_NO_ENTRIES = "Monthly draw has no entries!";
    private const string MSG_MONTHLY_NOT_FOUND = "No entries found for the monthly draw!";
    // =========================================================================
    // OVERLAY TOGGLE — Set to true to enable the OBS spin overlay,
    //                  false for chat-only announcements
    // =========================================================================
    private const bool USE_OVERLAY = true;
    // =========================================================================
    // Set to true to automatically draw a winner when the timer closes entries.
    // Set to false to close entries only — streamer calls the draw manually.
    // =========================================================================
    private const bool AUTO_DRAW = false;
    // =========================================================================
    private static readonly object _fileLock = new object ();
    private string listFile;
    private string vipRemoveFile;
    private string archiveFolder;
    private string overlayFile;
    public CPHInline()
    {
        string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Early_worm");
        if (!Directory.Exists(baseDir))
            Directory.CreateDirectory(baseDir);
        listFile = Path.Combine(baseDir, "Early_Worm_List.txt");
        vipRemoveFile = Path.Combine(baseDir, "VIP_To_Remove.txt");
        archiveFolder = Path.Combine(baseDir, "Archive");
        string overlayDir = Path.Combine(baseDir, "Overlay");
        if (!Directory.Exists(overlayDir))
            Directory.CreateDirectory(overlayDir);
        overlayFile = Path.Combine(overlayDir, "EarlyWormData.json");
    }

    public bool StartEarlyWorm()
    {
        bool open = CPH.GetGlobalVar<bool>("earlyworm_open", true);
        if (open)
        {
            CPH.SendMessage(MSG_ALREADY_OPEN);
            return true;
        }

        List<string> entries = new List<string>();
        CPH.SetGlobalVar("earlyworm_entries", entries, true);
        CPH.SetGlobalVar("earlyworm_open", true, true);
        CPH.SendMessage(MSG_ENTRIES_OPEN);
        return true;
    }

    public bool StopEarlyWorm()
    {
        CPH.SetGlobalVar("earlyworm_open", false, true);
        int count = GetEntries().Count;
        CPH.SendMessage(MSG_ENTRIES_CLOSED.Replace("{count}", count.ToString()));
        return true;
    }

    public bool JoinEarlyWorm()
    {
        CPH.TryGetArg("user", out string user);
        user = user.ToLower();
        bool open = CPH.GetGlobalVar<bool>("earlyworm_open", true);
        if (!open)
            return true;
        List<string> entries = GetEntries();
        if (entries.Contains(user))
        {
            CPH.SendMessage(MSG_ALREADY_ENTERED.Replace("{user}", user));
            return true;
        }

        entries.Add(user);
        CPH.SetGlobalVar("earlyworm_entries", entries, true);
        CPH.SendMessage(MSG_JOINED.Replace("{user}", user).Replace("{count}", entries.Count.ToString()));
        return true;
    }

    public bool DrawEarlyWormWinner()
    {
        List<string> entries = GetEntries();
        if (entries.Count == 0)
        {
            CPH.SendMessage(MSG_NO_ENTRIES);
            return true;
        }

        SecureShuffle(entries);
        string winner = entries.First();
        int total = entries.Count;
        // Trigger the overlay spin — write before announcing so OBS shows it first
        if (USE_OVERLAY)
        {
            WriteOverlayData("spin", entries, winner);
            CPH.Wait(7500);
        }

        CPH.SendMessage(MSG_WINNER.Replace("{winner}", winner).Replace("{total}", total.ToString()));
        HandleVip(winner);
        LogWinner(winner);
        ResetEarlyWorm();
        return true;
    }

    public bool RemoveOldVips()
    {
        if (!File.Exists(vipRemoveFile))
            return true;
        try
        {
            var users = File.ReadAllLines(vipRemoveFile);
            foreach (var user in users)
            {
                if (!string.IsNullOrWhiteSpace(user))
                    CPH.TwitchRemoveVip(user.Trim());
            }

            File.WriteAllText(vipRemoveFile, "");
        }
        catch (Exception ex)
        {
            CPH.LogError($"RemoveOldVips failed: {ex.Message}");
        }

        return true;
    }

    public bool ShowEntryCount()
    {
        List<string> entries = GetEntries();
        int count = entries.Count;
        if (count == 0)
            CPH.SendMessage(MSG_NO_ENTRIES_COUNT);
        else if (count == 1)
            CPH.SendMessage(MSG_ENTRY_COUNT_ONE);
        else
            CPH.SendMessage(MSG_ENTRY_COUNT_MANY.Replace("{count}", count.ToString()));
        return true;
    }

    public bool ShowRubOdds()
    {
        CPH.TryGetArg("user", out string user);
        List<string> entries = GetEntries();
        int totalEntries = entries.Count;
        if (totalEntries == 0)
        {
            CPH.SendMessage(MSG_ODDS_NONE.Replace("{user}", user));
            return true;
        }

        int userEntries = entries.Count(x => x.Equals(user, StringComparison.OrdinalIgnoreCase));
        if (userEntries == 0)
        {
            CPH.SendMessage(MSG_ODDS_NOT_ENTERED.Replace("{user}", user));
            return true;
        }

        double odds = (double)userEntries / totalEntries * 100;
        CPH.SendMessage(MSG_ODDS.Replace("{user}", user).Replace("{userEntries}", userEntries.ToString()).Replace("{total}", totalEntries.ToString()).Replace("{odds}", odds.ToString("F2")));
        return true;
    }

    public bool ResetEarlyWorm()
    {
        List<string> entries = new List<string>();
        CPH.SetGlobalVar("earlyworm_entries", entries, true);
        CPH.SetGlobalVar("earlyworm_open", false, true);
        if (USE_OVERLAY)
            WriteOverlayData("reset", entries, "");
        CPH.SendMessage(MSG_RESET);
        return true;
    }

    public bool RunAutomaticEarlyWormStart()
    {
        StartEarlyWorm();
        CPH.SendMessage(MSG_AUTO_CLOSING);
        CPH.EnableTimer("EarlyWormClose");
        return true;
    }

    public bool RunAutomaticEarlyWormStop()
    {
        CPH.DisableTimer("EarlyWormClose");
        StopEarlyWorm();
        if (AUTO_DRAW) DrawEarlyWormWinner();
        return true;
    }

    public bool DrawMonthlyWinner()
    {
        if (!File.Exists(listFile))
        {
            CPH.SendMessage(MSG_MONTHLY_NOT_FOUND);
            return true;
        }

        List<string> entries = File.ReadAllLines(listFile).Select(line => line.Split('|')[0]).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (entries.Count == 0)
        {
            CPH.SendMessage(MSG_MONTHLY_NO_ENTRIES);
            return true;
        }

        SecureShuffle(entries);
        string winner = entries.First();
        int total = entries.Count;
        int userEntries = entries.Count(x => x.Equals(winner, StringComparison.OrdinalIgnoreCase));
        // Trigger the overlay spin for the monthly draw too
        if (USE_OVERLAY)
        {
            WriteOverlayData("spin", entries, winner);
            CPH.Wait(7500);
            ;
        }

        CPH.SendMessage(MSG_MONTHLY_WINNER.Replace("{winner}", winner).Replace("{userEntries}", userEntries.ToString()).Replace("{total}", total.ToString()));
        LogWinner($"This months winner: {winner}");
        CPH.LogInfo($"This months winner is {winner} with {userEntries} entries out of {total}");
        ResetMonthlyEntries();
        return true;
    }

    public bool ResetMonthlyEntries()
    {
        if (File.Exists(listFile))
        {
            try
            {
                if (!Directory.Exists(archiveFolder))
                    Directory.CreateDirectory(archiveFolder);
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd");
                string archiveFile = Path.Combine(archiveFolder, $"{timestamp}_Early_Worm_List.txt");
                if (File.Exists(archiveFile))
                    File.Delete(archiveFile);
                File.Move(listFile, archiveFile);
                CPH.SendMessage(MSG_MONTHLY_ARCHIVED);
                CPH.LogInfo($"Monthly Early Worm entries have been archived to {archiveFile}");
            }
            catch (Exception ex)
            {
                CPH.LogError($"ResetMonthlyEntries failed: {ex.Message}");
                return true; // Exit early — don't wipe the list if archive failed
            }
        }
        else
        {
            CPH.SendMessage(MSG_MONTHLY_NO_ENTRIES);
        }

        File.WriteAllText(listFile, "");
        return true;
    }

    private List<string> GetEntries()
    {
        var entries = CPH.GetGlobalVar<List<string>>("earlyworm_entries", true);
        if (entries == null)
            CPH.LogWarn("earlyworm_entries was null — returning empty list");
        return entries ?? new List<string>();
    }

    private void HandleVip(string winner)
    {
        var userInfo = CPH.TwitchGetUserInfoByLogin(winner) ?? CPH.TwitchGetUserInfoById(winner);
        bool isVip = userInfo != null && userInfo.IsVip;
        bool isMod = userInfo != null && userInfo.IsModerator;
        if (!isVip && !isMod)
        {
            bool success = CPH.TwitchAddVip(winner);
            if (success)
                SafeAppendFile(vipRemoveFile, winner);
        }
    }

    private void LogWinner(string winner)
    {
        string entry = $"{winner}|{DateTime.Now:yyyy-MM-dd HH:mm}";
        SafeAppendFile(listFile, entry);
    }

    private void SecureShuffle(List<string> list)
    {
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[4];
                rng.GetBytes(box);
                int k = (int)(BitConverter.ToUInt32(box, 0) % n);
                n--;
                string temp = list[k];
                list[k] = list[n];
                list[n] = temp;
            }
        }
    }

    // Writes the JSON data file that EarlyWormOverlay.html polls
    // trigger: "spin" to start the animation, "reset" to clear the overlay
    private void WriteOverlayData(string trigger, List<string> entries, string winner)
    {
        try
        {
            // Timestamp is Unix milliseconds — the overlay uses this to ignore
            // triggers that were written before the page last loaded (stale data)
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string entriesJson = string.Join(",", entries.Select(e => $"\"{EscapeJson(e)}\""));
            string json = $"{{\"trigger\":\"{trigger}\",\"entries\":[{entriesJson}],\"winner\":\"{EscapeJson(winner)}\",\"timestamp\":{timestamp}}}";
            lock (_fileLock)
            {
                File.WriteAllText(overlayFile, json);
            }
        }
        catch (Exception ex)
        {
            CPH.LogError($"WriteOverlayData failed: {ex.Message}");
        }
    }

    // Escapes characters that would break the JSON string
    private string EscapeJson(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private void SafeAppendFile(string path, string content)
    {
        lock (_fileLock)
        {
            try
            {
                File.AppendAllText(path, content + Environment.NewLine);
            }
            catch (Exception ex)
            {
                CPH.LogError($"File write failed [{path}]: {ex.Message}");
            }
        }
    }

    // =========================================================================
    // TEST HELPERS — Remove these methods when done testing
    // =========================================================================
    public bool PopulateTestDailyEntries()
    {
        List<string> testUsers = new List<string>
        {
            "user1",
            "user2",
            "user3",
            "user4",
            "user5",
            "user6",
            "user7",
            "user8"
        };
        CPH.SetGlobalVar("earlyworm_entries", testUsers, true);
        CPH.LogInfo($"TEST: Populated {testUsers.Count} daily entries.");
        CPH.SendMessage($"🧪 Test entries loaded! ({testUsers.Count} worms in the draw)");
        return true;
    }

    public bool PopulateTestMonthlyEntries()
    {
        // Each name appears a different number of times to simulate real monthly data
        var testEntries = new (string name, int count)[]
        {
            ("user1", 3),
            ("user2", 2),
            ("user3", 3),
            ("user4", 1),
            ("user5", 2),
            ("user6", 3),
            ("user7", 2),
            ("user8", 1)
        };
        string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Early_worm");
        string testFile = Path.Combine(baseDir, "Early_Worm_List.txt");
        int total = 0;
        try
        {
            // Append test entries to the monthly file in the same format LogWinner uses
            foreach (var(name, count)in testEntries)
            {
                for (int i = 0; i < count; i++)
                {
                    string entry = $"{name}|{DateTime.Now:yyyy-MM-dd HH:mm}";
                    File.AppendAllText(testFile, entry + Environment.NewLine);
                    total++;
                }
            }

            CPH.LogInfo($"TEST: Populated {total} monthly entries across {testEntries.Length} users.");
            CPH.SendMessage($"🧪 Test monthly entries loaded! ({total} total entries)");
        }
        catch (Exception ex)
        {
            CPH.LogError($"PopulateTestMonthlyEntries failed: {ex.Message}");
        }

        return true;
    }
// =========================================================================
// END TEST HELPERS
// =========================================================================
}