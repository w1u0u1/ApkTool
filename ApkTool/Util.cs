namespace ApkTool
{
    class Util
	{
		public static string GetBuildArg(string inputFolderName, string outputApk)
		{
			return string.Format("-jar \"{0}\" b \"{1}\" -o \"{2}\"", GLOBAL.apktool, inputFolderName, outputApk);
		}

		public static string GetBuildDex(string inputFolderName, string outputDex)
		{
			return string.Format("-jar \"{0}\" \"a\" \"{1}\" -o \"{2}\"", GLOBAL.smali, inputFolderName, outputDex);
		}

		public static string GetDecompilerArg(string inputApk, string outputFolderName)
		{
			return string.Format("-jar \"{0}\" d \"{1}\" -o \"{2}\"", GLOBAL.apktool, inputApk, outputFolderName);
		}

		public static string GetDecompilerArgWithoutRes(string inputApk, string outputFolderName)
		{
			return string.Format("-jar \"{0}\" d -r \"{1}\" -o \"{2}\"", GLOBAL.apktool, inputApk, outputFolderName);
		}

		public static string GetDecompilerDex(string inputDex, string outputFolderName)
		{
			return string.Format("-jar \"{0}\" \"d\"  \"{1}\" -o \"{2}\"", GLOBAL.baksmali, inputDex, outputFolderName);
		}

		public static string GetDex2JarArg(string inputDex, string outputJar)
		{
			return string.Format("/c \"\"{0}\" \"{1}\" -o \"{2}\"\" -f", GLOBAL.dex2jar, inputDex, outputJar);
		}

		public static string GetJar2DexArg(string inputDex, string outputJar)
		{
			return string.Format("/c \"\"{0}\" \"{1}\" -o \"{2}\"\" -f", GLOBAL.jar2dex, inputDex, outputJar);
		}

        public static string GetAppInfo(string inputApk)
		{
			return string.Format("-jar " + GLOBAL.apkparser + " " + inputApk, new object[0]);
		}
	}
}