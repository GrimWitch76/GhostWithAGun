using System.Text;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class SaveStorage
{
    private const string SaveKeyPrefix = "GWAG_SAVE_";
    private const int ChunkSize = 800_000; // ~0.8MB chunks

    public static void WriteJson(string slot, string json)
    {
        Debug.Log("attempting to save in slot " + slot);

        // Delete old chunks
        int i = 0;
        while (PlayerPrefs.HasKey($"{SaveKeyPrefix}{slot}_{i}"))
        {
            PlayerPrefs.DeleteKey($"{SaveKeyPrefix}{slot}_{i}");
            i++;
        }

        // Write in chunks
        var bytes = Encoding.UTF8.GetBytes(json);
        int chunks = Mathf.CeilToInt(bytes.Length / (float)ChunkSize);
        for (int c = 0; c < chunks; c++)
        {
            int offset = c * ChunkSize;
            int len = Mathf.Min(ChunkSize, bytes.Length - offset);
            var part = Encoding.UTF8.GetString(bytes, offset, len);
            PlayerPrefs.SetString($"{SaveKeyPrefix}{slot}_{c}", part);
        }
        PlayerPrefs.SetInt($"{SaveKeyPrefix}{slot}_count", chunks);
        PlayerPrefs.Save(); // flush to IndexedDB
    }

    public static void SaveCurrentNight(int nightId)
    {
        PlayerPrefs.SetInt("CurrentNight", nightId);
    }

    public static int GetCurrentNight()
    {
       return PlayerPrefs.GetInt("CurrentNight", -1);
    }

    public static string ReadJson(string slot)
    {
        Debug.Log("attempting to read from slot " + slot);

        if (!PlayerPrefs.HasKey($"{SaveKeyPrefix}{slot}_count")) return null;
        int chunks = PlayerPrefs.GetInt($"{SaveKeyPrefix}{slot}_count");
        var sb = new StringBuilder(chunks * ChunkSize);
        for (int c = 0; c < chunks; c++)
            sb.Append(PlayerPrefs.GetString($"{SaveKeyPrefix}{slot}_{c}", ""));
        return sb.ToString();
    }

    public static void Clear(string slot)
    {
        Debug.Log("attempting to clear save in slot " + slot);
        int c = PlayerPrefs.GetInt($"{SaveKeyPrefix}{slot}_count", -1);
        if (c >= 0)
        {
            for (int i = 0; i < c; i++)
                PlayerPrefs.DeleteKey($"{SaveKeyPrefix}{slot}_{i}");
            PlayerPrefs.DeleteKey($"{SaveKeyPrefix}{slot}_count");
            PlayerPrefs.Save();
        }
        SaveCurrentNight(-1);
    }

    public static void ClearAll()
    {
        int possibleNights = 5;
        for (int i = 0; i < possibleNights; i++)
        {
            string slot = "Night_" + i;
            Debug.Log("attempting to clear save in slot " + slot);
            int c = PlayerPrefs.GetInt($"{SaveKeyPrefix}{slot}_count", -1);
            if (c >= 0)
            {
                for (int j = 0; j < c; j++)
                    PlayerPrefs.DeleteKey($"{SaveKeyPrefix}{slot}_{j}");
                PlayerPrefs.DeleteKey($"{SaveKeyPrefix}{slot}_count");
                PlayerPrefs.Save();
            }
        }
        SaveCurrentNight(-1);
    }
}