using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using YGO_MDP.Properties;

namespace YGO_MDP
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct OpenFileName
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public string lpstrFilter;
            public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public string lpstrFile;
            public int nMaxFile;
            public string lpstrFileTitle;
            public int nMaxFileTitle;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved;
            public int dwReserved;
            public int flagsEx;
        }

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetOpenFileName(ref OpenFileName ofn);

        private static string ShowDialog()
        {
            var ofn = new OpenFileName();
            ofn.lStructSize = Marshal.SizeOf(ofn);
            ofn.lpstrFilter = "MasterDuel.exe(*.exe)\0*.exe\0All Files(*.*)\0*.*\0\0";
            ofn.lpstrFile = new string(new char[256]);
            ofn.nMaxFile = ofn.lpstrFile.Length;
            ofn.lpstrFileTitle = new string(new char[64]);
            ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
            ofn.lpstrTitle = "Open File Dialog...";
            if (GetOpenFileName(ref ofn))
                return ofn.lpstrFile;
            return string.Empty;
        }

        static void Main(string[] args)
        {
            var str = @"
/*********************************************
** YGO汉化补丁1.4.1                         **
** 补丁作者：Timelic  2022.05.09            **
** 程序作者：CodeGize 2022.05.10            **
**                                          **
** 请确保游戏的语言为日本语                 **
**                                          **
** 请输入命令编号:                          **
** 1、开始汉化                              **
** 2、还原汉化                              **
** 3、打开游戏                              **
** 4、关闭游戏                              **
** 5、退出                                  **
*********************************************/
";
            Console.WriteLine(str);
            while (true)
            {
                var cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "1":
                        Patch();
                        break;
                    case "2":
                        Revert();
                        break;
                    case "3":
                        Process.Start("steam://rungameid/1449850");
                        break;
                    case "4":
                        var ps = Process.GetProcessesByName("masterduel");
                        foreach (var item in ps)
                        {
                            item.Kill();
                        }
                        break;
                    case "5":
                        return;
                }
            }
        }

        private static void Revert()
        {
            var dest = GetGamePath();
            if (string.IsNullOrEmpty(dest))
                return;
            var basedir = AppDomain.CurrentDomain.BaseDirectory;
            var dir = basedir + "backup";

            var destdir = Path.GetDirectoryName(dest);
            var file1 = dir + "\\data.unity3d";
            ProcessCopyFile(file1, destdir + "\\masterduel_Data\\data.unity3d");

            var dicts = Directory.GetDirectories(dir);
            foreach (var dict in dicts)
            {
                var destlocaldata = destdir + "\\LocalData";
                var tardir = dict.Replace(dir, destlocaldata);
                ProcessCopyFolder(dict, tardir);
            }
            Console.WriteLine("Patch已经还原");
        }

        private static void Patch()
        {
            var dest = GetGamePath();
            if (string.IsNullOrEmpty(dest))
                return;

            var v = "汉化补丁";
            ProcessCopyPatch(v, dest);
            Console.WriteLine("Patch完成：" + v);
        }

        private static string GetGamePath()
        {
            var dest = Settings.Default.GamePath;
            if (string.IsNullOrEmpty(dest))
            {
                string path = ShowDialog();

                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("路径错误:" + path);
                    return "";
                }
                if (!path.EndsWith("masterduel.exe", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("路径错误:" + path);
                    return "";
                }
                dest = path;
                Settings.Default.GamePath = dest;
                Settings.Default.Save();
                Console.WriteLine("定位路径成功:" + dest);
            }
            return dest;
        }

        private static void ProcessCopyPatch(string dir, string dest)
        {
            var basedir = AppDomain.CurrentDomain.BaseDirectory;
            dir = basedir + dir;

            var backupdir = basedir + "backup/";
            if (Directory.Exists(backupdir))
                Directory.Delete(backupdir, true);
            Directory.CreateDirectory(backupdir);

            var destdir = Path.GetDirectoryName(dest);
            var file1 = dir + "\\data.unity3d";
            ProcessCopyFile(file1, destdir + "\\masterduel_Data\\data.unity3d", backupdir + "data.unity3d");

            var srcdir = dir + "\\0000";

            var destfolder = destdir + "\\LocalData";
            var folders = Directory.GetDirectories(destfolder);
            foreach (var folder in folders)
            {
                var backfolder = folder.Replace(destfolder, backupdir);
                ProcessCopyFolder(srcdir, folder + "\\0000", backfolder);
            }
        }

        private static void ProcessCopyFolder(string srcdir, string destdir, string backupdir = "")
        {
            var files = Directory.GetFiles(srcdir, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var destfile = file.Replace(srcdir, destdir);
                var backupfile = "";
                if (!string.IsNullOrEmpty(backupdir))
                {
                    var destdir1 = Path.GetDirectoryName(destdir);
                    backupfile = destfile.Replace(destdir1, backupdir);
                }
                ProcessCopyFile(file, destfile, backupfile);
            }
        }

        private static void ProcessCopyFile(string file1, string destfile, string backup = "")
        {
#if DEBUG
            Console.WriteLine(string.Format("复制文件{0},{1},{2}", file1, destfile, backup));
#endif
            if (!string.IsNullOrEmpty(backup))
            {
                if (File.Exists(destfile))
                {
                    var dir = Path.GetDirectoryName(backup);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    File.Copy(destfile, backup, true);
                }
            }
            var destdir = Path.GetDirectoryName(destfile);
            if (!Directory.Exists(destdir))
                Directory.CreateDirectory(destdir);
            File.Copy(file1, destfile, true);
        }
    }
}
