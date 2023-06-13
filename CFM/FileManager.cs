using System.Collections;
using System.Text;

namespace CFM;

public static class FileManager
{
    public static ArrayList ReadFile(string path)
    {
        var arrayList = new ArrayList();
        var s = 0;
        using var sr = new StreamReader(path, Encoding.UTF8);
        while (sr.ReadLine() is { } line)
        {
            if (s >= 3000) break;
            arrayList.Add(line);
            s++;
        }

        return arrayList;
    }

    public static void WriteFile(string path, List<string> text)
    {
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        foreach (var s in text) writer.WriteLine(s);
    }
}