using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ApkTool
{
    class Ini
    {
        [DllImport("kernel32.dll")]
        public static extern uint GetPrivateProfileInt(string section, string key, int def, string filePath);

        [DllImport("kernel32.dll")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, int nSize, string filePath);

        [DllImport("KERNEL32.DLL")]
        public static extern int GetPrivateProfileSection(string lpAppName, byte[] lpReturnedString, int nSize, string filePath);

        private string m_config;

        public Ini()
        {
            m_config = Application.StartupPath + "\\" + System.IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".ini";
        }

        public Ini(string inifile)
        {
            m_config = inifile;
        }

        public void Write(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, m_config);
        }

        public string Read(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, m_config);
            return temp.ToString();
        }

        public string Read(string Section, string Key, string def)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, def, temp, 255, m_config);
            return temp.ToString();
        }

        public uint ReadInt(string Section, string Key)
        {
            return GetPrivateProfileInt(Section, Key, 0, m_config);
        }

        public int GetAllSectionNames(out string[] sections)
        {
            int MAX_BUFFER = 32767;
            IntPtr pReturnedString = Marshal.AllocCoTaskMem(MAX_BUFFER);
            int bytesReturned = GetPrivateProfileSectionNames(pReturnedString, MAX_BUFFER, m_config);
            if (bytesReturned == 0)
            {
                sections = null;
                return -1;
            }
            string local = Marshal.PtrToStringAnsi(pReturnedString, (int)bytesReturned).ToString();
            Marshal.FreeCoTaskMem(pReturnedString);
            //use of Substring below removes terminating null for split
            sections = local.Substring(0, local.Length - 1).Split('\0');
            return 0;
        }

        public int GetAllKeyValues(string section, out string[] keys, out string[] values)
        {
            byte[] b = new byte[65535];
            GetPrivateProfileSection(section, b, b.Length, m_config);
            string s = System.Text.Encoding.Default.GetString(b);
            string[] tmp = s.Split((char)0);
            ArrayList result = new ArrayList();
            foreach (string r in tmp)
            {
                if (r != string.Empty)
                    result.Add(r);
            }
            keys = new string[result.Count];
            values = new string[result.Count];
            for (int i = 0; i < result.Count; i++)
            {
                string[] item = result[i].ToString().Split(new char[] { '=' });
                if (item.Length == 2)
                {
                    keys[i] = item[0].Trim();
                    values[i] = item[1].Trim();
                }
                else if (item.Length == 1)
                {
                    keys[i] = item[0].Trim();
                    values[i] = "";
                }
                else if (item.Length == 0)
                {
                    keys[i] = "";
                    values[i] = "";
                }
            }
            return 0;
        }
    }
}