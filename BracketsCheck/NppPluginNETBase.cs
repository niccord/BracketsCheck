using System;

namespace BracketsCheck
{
    class PluginBase
    {
        #region " Fields "
        internal static NppData nppData;
        internal static FuncItems _funcItems = new FuncItems();
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

        internal static IntPtr GetCurrentScintilla()
        {
            int curScintilla;
            Win32.SendMessage(nppData._nppHandle, NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out curScintilla);
            return (curScintilla == 0) ? nppData._scintillaMainHandle : nppData._scintillaSecondHandle;
        }

        internal static void toggleCheckMenuItem(int funcItemID, bool isChecked)
        {
            IntPtr menu = Win32.GetMenu(nppData._nppHandle);
            int ret = Win32.CheckMenuItem(menu, _funcItems.Items[funcItemID]._cmdID, Win32.MF_BYCOMMAND | (isChecked ? Win32.MF_CHECKED : Win32.MF_UNCHECKED));

            // Main.displayMessage("menu: " + menu + ", nppHandle: " + nppData._nppHandle + ", ret: " + ret);

            FuncItem itemToUpdate = new FuncItem();
            itemToUpdate._cmdID = _funcItems.Items[funcItemID]._cmdID;
            itemToUpdate._init2Check = isChecked;
            itemToUpdate._itemName = _funcItems.Items[funcItemID]._itemName;
            itemToUpdate._pFunc = _funcItems.Items[funcItemID]._pFunc;
            itemToUpdate._pShKey = _funcItems.Items[funcItemID]._pShKey;

            _funcItems.UpdateItem(itemToUpdate);
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

        #endregion
    }
}
