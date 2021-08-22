using System;
using System.IO;
using System.Text;

namespace BracketsCheck
{
    class PluginBase
    {
        #region " Fields "
        internal static NppData nppData;
        internal static FuncItems _funcItems = new FuncItems();

        public static string IniFilePath
        {
            get;
            private set;
        }
        #endregion

        #region " Helper "
        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer)
        {
            SetCommand(index, commandName, functionPointer, new ShortcutKey(), false);
        }
        internal static void SetCommand(int index, NppFuncItemDelegate functionPointer)
        {
            SetCommand(index, string.Empty, functionPointer, new ShortcutKey(), false);
        }
        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut)
        {
            SetCommand(index, commandName, functionPointer, shortcut, false);
        }
        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, bool checkOnInit)
        {
            SetCommand(index, commandName, functionPointer, new ShortcutKey(), checkOnInit);
        }
        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, bool checkOnInit)
        {
            FuncItem funcItem = new FuncItem();
            funcItem._cmdID = index;
            funcItem._itemName = commandName;
            if (functionPointer != null)
                funcItem._pFunc = new NppFuncItemDelegate(functionPointer);
            if (shortcut._key != 0)
                funcItem._pShKey = shortcut;
            funcItem._init2Check = checkOnInit;
            _funcItems.Add(funcItem);
        }

        internal static void InitiINI()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            IniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(IniFilePath))
            {
                Directory.CreateDirectory(IniFilePath);
            }
            IniFilePath = Path.Combine(IniFilePath, Main.PluginName + ".ini");
        }

        internal static IntPtr GetCurrentScintilla()
        {
            int curScintilla;
            Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out curScintilla);
            return (curScintilla == 0) ? nppData._scintillaMainHandle : nppData._scintillaSecondHandle;
        }

        internal static void toggleCheckMenuItem(int funcItemID, bool isChecked)
        {
            IntPtr menu = Win32.GetMenu(nppData._nppHandle);
            int i = Win32.CheckMenuItem(menu, _funcItems.Items[funcItemID]._cmdID, Win32.MF_BYCOMMAND | (isChecked ? Win32.MF_CHECKED : Win32.MF_UNCHECKED));
        }

        internal static void nppn_ready()
        {
            IntPtr menu = Win32.GetMenu(nppData._nppHandle);
            if (menu != IntPtr.Zero)
            {
                foreach (FuncItem item in _funcItems.Items)
                {
                    Win32.CheckMenuItem(menu, item._cmdID, Win32.MF_BYCOMMAND | (item._init2Check ? Win32.MF_CHECKED : Win32.MF_UNCHECKED));
                }
            }
        }

        internal static void savePluginParams()
        {
            foreach (FuncItem item in _funcItems.Items)
            {
                if (item._itemName != null && item._itemName != string.Empty)
                {
                    Win32.WritePrivateProfileString(Main.PluginName, item._itemName, item._init2Check ? "1" : "0", IniFilePath);
                }
            }
        }

        #endregion
    }
}
