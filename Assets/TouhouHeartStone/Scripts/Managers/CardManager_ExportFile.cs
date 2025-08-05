using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Controllers;
using MessagePack;
using MukioI18n;
using TouhouCardEngine.Shared;
using UnityEngine;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        /// <summary>
        /// 导出卡池
        /// </summary>
        /// <param name="cardPoolInfo"></param>
        /// <returns></returns>
        public async Task<string> exportCardPool(CardPoolInfo cardPoolInfo)
        {
            if (cardPoolInfo == null)
                return null;

            if (isBuiltinCardPool(cardPoolInfo.cardPoolId) || isInnateCardPool(cardPoolInfo))
            {
                game.ui.dialog.ShowConfirm(game.i18n._(StringTable.HINT_TITLE), game.i18n._(ERROR_BUILTIN_CARDPOOL));
                return null;
            }

            string cardPoolDefineFile = null;
            foreach (var kv in _localCardPoolInfoDict)
            {
                if (kv.Value.CompareCardPool(cardPoolInfo))
                {
                    cardPoolDefineFile = kv.Key;
                    break;
                }
            }

            var cardPool = _localCardPoolDict[cardPoolInfo.ToReference()];
            if (cardPoolDefineFile == null)
                return null;

            var cardPoolName = getCardPoolName(cardPoolInfo);
            var defaultFileName = $"{cardPoolName}.{CardPoolPackExt}";

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX 
            var exportPath = SFB.StandaloneFileBrowser.SaveFilePanel("", "", defaultFileName, CardPoolPackExt);
            if (string.IsNullOrEmpty(exportPath))
                return null;
            await exportCardPool(cardPoolInfo, cardPoolDefineFile, cardPool, exportPath);
            return exportPath;
#elif UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
            // 这个插件是先写出再复制到目标目录的
            var exportPath = Path.Combine(Application.temporaryCachePath, defaultFileName);
            await exportCardPool(cardPoolInfo, cardPoolDefineFile, cardPool, exportPath);

            var t = new TaskCompletionSource<string>();
            NativeFilePicker.ExportFile(exportPath, (bool success) =>
            {
                t.SetResult(success ? defaultFileName : null);
                File.Delete(exportPath); // 清理临时文件
            });
            return await t.Task;
#else
            var exportPath = Path.Combine(_persistentDataPath, defaultFileName);
            await exportCardPool(cardPoolInfo, cardPoolDefineFile, cardPool, exportPath);
            return exportPath;
#endif
        }
        public async Task<string> exportAnyFile(byte[] bytes, string defaultFileName, string extension)
        {
#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX 
            var exportPath = SFB.StandaloneFileBrowser.SaveFilePanel("", "", defaultFileName, extension);
            if (string.IsNullOrEmpty(exportPath))
                return null;
            await File.WriteAllBytesAsync(exportPath, bytes);
            return exportPath;
#elif UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
            // 这个插件是先写出再复制到目标目录的
            var exportPath = Path.Combine(Application.temporaryCachePath, string.Format("{0}.{1}", defaultFileName, extension));
            await File.WriteAllBytesAsync(exportPath, bytes);

            var t = new TaskCompletionSource<string>();
            NativeFilePicker.ExportFile(exportPath, (bool success) =>
            {
                t.SetResult(success ? defaultFileName : null);
                File.Delete(exportPath); // 清理临时文件
            });
            return await t.Task;
#else
            var exportPath = Path.Combine(_persistentDataPath, string.Format("{0}.{1}", defaultFileName, extension));
            await File.WriteAllBytesAsync(exportPath, bytes);
            return exportPath;
#endif
        }
        public async Task<bool> importCardPool()
        {
#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_EDITOR
            var selectedPathes = SFB.StandaloneFileBrowser.OpenFilePanel("", "", CardPoolPackExt, false);
            if (selectedPathes.Length == 0)
                return false;
            var importPath = selectedPathes[0];
            return await importCardPool(importPath);
#elif UNITY_IOS || UNITY_ANDROID
            var t = new TaskCompletionSource<bool>();
            NativeFilePicker.PickFile(async (string path) =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    t.SetResult(false);
                    return;
                }

                try
                {
                    var result = await importCardPool(path);
                    t.SetResult(result);
                }
                catch (Exception e)
                {
                    t.SetException(e);
                }
            }, "*/*");
            return await t.Task;
#else
            var importPath = Path.Combine(_persistentDataPath, "import.dcpp");
            if (!File.Exists(importPath))
            {
                game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), game.i18n._(ERROR_FILE_NOT_FOUND, importPath));
                return false;
            }
            return await importCardPool(importPath);
#endif
        }
        #endregion

        #region 私有方法
        private async Task exportCardPool(CardPoolInfo cardPoolInfo, string cardPoolDefineFile, CardPool cardPool, string exportPath)
        {
            // 关联卡片资源列表
            var imageFiles = getCardPoolImages(cardPoolInfo, cardPool);
            var soundFiles = getCardPoolSounds(cardPool);
            byte[] defineHash;
            using (var file = File.OpenRead(cardPoolDefineFile))
            {
                var sha256 = SHA256.Create();
                defineHash = sha256.ComputeHash(file);
            }
            var manifest = new CardPoolPackManifest(cardPoolInfo, Convert.ToBase64String(defineHash));

            using (var ms = new MemoryStream())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    // 写出卡池定义文件
                    var defineEntry = await archive.CreateEntryFromFileAsync(cardPoolDefineFile, CardPoolPackDefine);

                    // 写卡池包清单，之后要校验
                    archive.CreateEntryFromData(Encoding.UTF8.GetBytes(manifest.toJson()), CardPoolPackManifest);

                    // 写出卡池图片文件
                    foreach (var file in imageFiles)
                    {
                        await archive.CreateEntryFromFileAsync(Path.Combine(_persistentDataPath, textureDirName, file), $"{textureDirName}/{file}");
                    }
                    // 写出卡池音频文件
                    foreach (var file in soundFiles)
                    {
                        await archive.CreateEntryFromFileAsync(Path.Combine(_persistentDataPath, soundDirName, file), $"{soundDirName}/{file}");
                    }
                }

                using (var fileStream = new FileStream(exportPath, FileMode.Create))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fileStream);
                }
            }

            Debug.Log($"卡池文件已打包至{exportPath}");
        }
        private async Task<bool> importCardPool(string importPath)
        {
            prepareCardpoolDir();

            using (var fs = File.OpenRead(importPath))
            {
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    // 读清单文件
                    var manifestEntry = archive.GetEntry(CardPoolPackManifest);
                    if (manifestEntry == null)
                    {
                        Debug.Log($"导入的Manifest不存在");
                        game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), game.i18n._(ERROR_WRONG_FORMAT));
                        return false;
                    }

                    var manifestStr = await manifestEntry.ReadStringAsync(Encoding.UTF8);
                    var manifest = JsonHelper.fromJson<CardPoolPackManifest>(manifestStr);

                    if (manifest.FileVersion != 1)
                    {
                        // 版本号不匹配
                        game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), game.i18n._(ERROR_WRONG_VERSION));
                        return false;
                    }

                    Debug.Log($"即将导入的卡池ID：{manifest.CardPoolID}");

                    // 判断卡池是否存在，若存在则不给导入
                    var oldPoolInfo = _localCardPoolInfoDict.Where(kv => kv.Value.cardPoolId == manifest.CardPoolID);
                    if (oldPoolInfo.Count() > 0)
                    {
                        // 已存在相同卡池的不同版本，不允许导入
                        var cardPoolName = getCardPoolName(oldPoolInfo.First().Value);
                        game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), game.i18n._(ERROR_ALREADY_EXISTS, manifest.Name, cardPoolName));
                        return false;
                    }

                    // 读取文件，验证hash
                    var defineEntry = archive.GetEntry(CardPoolPackDefine);
                    if (defineEntry == null)
                    {
                        Debug.Log($"导入的Define不存在");
                        game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), game.i18n._(ERROR_WRONG_FORMAT));
                        return false;
                    }

                    if (manifest.FileVersion == 1)
                    {
                        // 判断是否被修改，虽然没啥卵用但是先留着接口
                        var defineHash = defineEntry.GetSHA256();
                        var hashStr = Convert.ToBase64String(defineHash);
                        if (hashStr != manifest.Signature)
                        {
                            game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), game.i18n._(ERROR_SIGNATURE_NOT_MATCH));
                            return false;
                        }

                        // 将ID与清单文件对照，防止被修改
                        var defineBytes = defineEntry.Decompress();
                        var defineStream = new MemoryStream(defineBytes);
                        var (cardPoolInfo, cardPool) = parseCardPoolStream(defineStream);
                        if (cardPoolInfo.cardPoolId != manifest.CardPoolID || cardPoolInfo.editTime != manifest.Revision)
                        {
                            game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), game.i18n._(ERROR_SIGNATURE_NOT_MATCH));
                            return false;
                        }

                        // 检查卡图是否完全正常
                        var images = getCardPoolImages(cardPoolInfo, cardPool);
                        HashSet<ZipArchiveEntry> imageEntries = new HashSet<ZipArchiveEntry>();
                        foreach (var image in images)
                        {
                            var fileName = $"{textureDirName}/{image}";
                            var imgEntry = archive.GetEntry(fileName);

                            if (imgEntry == null)
                            {
                                Debug.Log($"导入的图片{fileName}不存在");
                                game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), game.i18n._(ERROR_WRONG_FORMAT));
                                return false;
                            }
                            imageEntries.Add(imgEntry);
                        }

                        // 检查音频是否完全正常
                        var soundFiles = getCardPoolSounds(cardPool);
                        HashSet<ZipArchiveEntry> soundEntries = new HashSet<ZipArchiveEntry>();
                        foreach (var sound in soundFiles)
                        {
                            var fileName = $"{soundDirName}/{sound}";
                            var sndEntry = archive.GetEntry(fileName);

                            if (sndEntry == null)
                            {
                                Debug.Log($"导入的音频{fileName}不存在");
                                game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), game.i18n._(ERROR_WRONG_FORMAT));
                                return false;
                            }
                            soundEntries.Add(sndEntry);
                        }

                        // 写出卡池文件。
                        var definePath = Path.Combine(_persistentDataPath, _cardPoolDirName, $"{manifest.CardPoolID}.{_cardPoolFilePostfix}");
                        await defineEntry.SaveToAsync(definePath);

                        // 写出所有图像。
                        foreach (var image in imageEntries)
                        {
                            var fileName = image.FullName;
                            await image.SaveToAsync(Path.Combine(_persistentDataPath, fileName));
                        }
                        // 写出所有音频。
                        foreach (var sound in soundEntries)
                        {
                            var fileName = sound.FullName;
                            await sound.SaveToAsync(Path.Combine(_persistentDataPath, fileName));
                        }

                        // 加载卡池
                        await loadLocalCardPool(definePath);
                    }
                }
            }
            return true;
        }
        #endregion

        #region 属性字段
        [TranslateMsg]
        const string ERROR_WRONG_FORMAT = "卡池格式错误或内部文件丢失";
        [TranslateMsg]
        const string ERROR_WRONG_VERSION = "无法识别的卡池版本，请尝试更新客户端";
        [TranslateMsg("{0}是即将导入的卡池，{1}是已存在的卡池名称")]
        const string ERROR_ALREADY_EXISTS = "导入的卡池{0}与{1}重复，请在导入前删除卡池{1}";
        [TranslateMsg]
        const string ERROR_SIGNATURE_NOT_MATCH = "卡池签名错误";
        [TranslateMsg]
        const string ERROR_BUILTIN_CARDPOOL = "无法导出经典卡池";
        [TranslateMsg("{0}是默认导入路径")]
        const string ERROR_FILE_NOT_FOUND = "无法找到需要导入的卡池。请将需要导入的卡池放置于{0}";
        const string CardPoolPackExt = "dcpp";
        const string CardPoolPackManifest = "manifest.json";
        string CardPoolPackDefine => $"cardpool.{_cardPoolFilePostfix}";
        #endregion
    }
    [MessagePackObject]
    public class CardPoolPackManifest
    {
        public CardPoolPackManifest(CardPoolInfo info, string signature)
        {
            CardPoolID = info.cardPoolId;
            Revision = info.editTime;
            FileVersion = 1;
            Signature = signature;
        }
        protected CardPoolPackManifest()
        {
        }
        /// <summary>
        /// 卡池ID
        /// </summary>
        [Key(0)]
        public long CardPoolID;
        /// <summary>
        /// 卡池名称
        /// </summary>
        [Key(1)]
        public string Name;
        /// <summary>
        /// 卡池版本
        /// </summary>
        [Key(2)]
        public long Revision;
        /// <summary>
        /// 文件包版本
        /// </summary>
        [Key(3)]
        public int FileVersion;
        /// <summary>
        /// 文件签名
        /// </summary>
        [Key(4)]
        public string Signature;
    }

    static class ZipStreamExtension
    {
        public static ZipArchiveEntry CreateEntryFromFile(this ZipArchive destination, String sourceFileName, String entryName)
        {
            var entry = destination.CreateEntry(entryName);
            using (var zipStream = entry.Open())
            {
                using (var fs = File.OpenRead(sourceFileName))
                {
                    fs.CopyTo(zipStream);
                }
            }
            return entry;
        }

        public static async Task<ZipArchiveEntry> CreateEntryFromFileAsync(this ZipArchive destination, String sourceFileName, String entryName)
        {
            var entry = destination.CreateEntry(entryName);
            using (var zipStream = entry.Open())
            {
                using (var fs = File.OpenRead(sourceFileName))
                {
                    await fs.CopyToAsync(zipStream);
                }
            }
            return entry;
        }

        public static ZipArchiveEntry CreateEntryFromData(this ZipArchive destination, byte[] content, string entryName)
        {
            var entry = destination.CreateEntry(entryName);
            using (var zipStream = entry.Open())
            {
                zipStream.Write(content, 0, content.Length);
            }
            return entry;
        }

        public static string ReadString(this ZipArchiveEntry entry, Encoding encoding)
        {
            using (var zipStream = entry.Open())
            {
                using (var reader = new StreamReader(zipStream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static async Task<string> ReadStringAsync(this ZipArchiveEntry entry, Encoding encoding)
        {
            using (var zipStream = entry.Open())
            {
                using (var reader = new StreamReader(zipStream, encoding))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        public static string[] ReadLines(this ZipArchiveEntry entry, Encoding encoding)
        {
            List<string> lines = new List<string>();
            using (var zipStream = entry.Open())
            {
                using (var reader = new StreamReader(zipStream, encoding))
                {
                    while (!reader.EndOfStream)
                    {
                        lines.Add(reader.ReadLine());
                    }
                }
            }
            return lines.ToArray();
        }

        public static async Task<string[]> ReadLinesAsync(this ZipArchiveEntry entry, Encoding encoding)
        {
            List<string> lines = new List<string>();
            using (var zipStream = entry.Open())
            {
                using (var reader = new StreamReader(zipStream, encoding))
                {
                    while (!reader.EndOfStream)
                    {
                        lines.Add(await reader.ReadLineAsync());
                    }
                }
            }
            return lines.ToArray();
        }
        public static byte[] Decompress(this ZipArchiveEntry entry)
        {
            using (var zipStream = entry.Open())
            {
                using (var stream = new MemoryStream())
                {
                    zipStream.CopyTo(stream);
                    return stream.ToArray();
                }
            }
        }

        public static byte[] GetSHA256(this ZipArchiveEntry entry)
        {
            using (var zipStream = entry.Open())
            {
                var sha256 = SHA256.Create();
                return sha256.ComputeHash(zipStream);
            }
        }

        public static void SaveTo(this ZipArchiveEntry entry, string path)
        {
            using (var zipStream = entry.Open())
            {
                using (var fs = File.OpenWrite(path))
                {
                    zipStream.CopyTo(fs);
                }
            }
        }

        public static async Task SaveToAsync(this ZipArchiveEntry entry, string path)
        {
            using (var zipStream = entry.Open())
            {
                using (var fs = File.OpenWrite(path))
                {
                    await zipStream.CopyToAsync(fs);
                }
            }
        }
    }
}
