using System.Windows.Forms;

namespace ApkTool
{
    class GLOBAL
	{
        private static readonly string _lib = Application.StartupPath + "\\lib\\";
        private static readonly string _section = "config";
        private static readonly Ini _ini = new Ini(_lib + "config.ini");

        public static readonly string apkparser = _lib + _ini.Read(_section, "apkparser", "apkparser.jar");
        public static readonly string apksigner = _lib + _ini.Read(_section, "apksigner", "apksigner.jar");
        public static readonly string apktool = _lib + _ini.Read(_section, "apktool", "apktool.jar");

        public static readonly string dex2jar = _lib + _ini.Read(_section, "dex2jar", "dex2jar.jar");
        public static readonly string jar2dex = _lib + _ini.Read(_section, "jar2dex", "jar2dex.jar");

        public static readonly string baksmali = _lib + _ini.Read(_section, "baksmali", "baksmali.jar");
        public static readonly string smali = _lib + _ini.Read(_section, "smali", "smali.jar");

        public static readonly string jadx = _lib + _ini.Read(_section, "jadx", "jadx-gui.bat");
        public static readonly string jd = _lib + _ini.Read(_section, "jd", "jd-gui.jar");

        public static readonly string zipalign = _lib + _ini.Read(_section, "zipalign", "zipalign.exe");
	}
}