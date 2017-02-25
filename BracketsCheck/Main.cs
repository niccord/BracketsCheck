using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace BracketsCheck
{
    class Main
    {
        #region Fields
        internal const string PluginName = "BracketsCheck";
        static string iniFilePath = null;
        static bool someSetting = false;
        //static frmMyDlg frmMyDlg = null;
        //static int idMyDlg = -1;
        static Bitmap tbBmp = Properties.Resources.star;
        static Bitmap tbBmp_tbTab = Properties.Resources.star_bmp;
        //static Icon tbIcon = null;
        #endregion

        #region 

        private struct BCChar
        {
            public char charvalue;
            public int rownumber;
            public int charnumber;
        }

        #endregion

        #region StartUp/CleanUp

        internal static void CommandMenuInit()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath)) Directory.CreateDirectory(iniFilePath);
            iniFilePath = Path.Combine(iniFilePath, PluginName + ".ini");
            someSetting = (Win32.GetPrivateProfileInt("SomeSection", "SomeKey", 0, iniFilePath) != 0);

            //TODO: tutti i file aperti

            // file aperto
            PluginBase.SetCommand(0, "Check Brackets: All text", checkBracketsAll, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(1, "Check Brackets: Selected text", checkBracketsSelected, new ShortcutKey(false, false, false, Keys.None));

            //idMyDlg = 0;
        }

        internal static string GetAllText()
        {
            int length = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETLENGTH, 0, 0);
            IntPtr ptrToText = Marshal.AllocHGlobal(length + 1);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETTEXT, length + 1, ptrToText);
            string textAnsi = Marshal.PtrToStringAnsi(ptrToText);
            Marshal.FreeHGlobal(ptrToText);
            return textAnsi;
        }

        internal static string GetSelectedText()
        {
            int length = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETLENGTH, 0, 0);
            IntPtr ptrToText = Marshal.AllocHGlobal(length + 1);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETSELTEXT, length + 1, ptrToText);
            string textAnsi = Marshal.PtrToStringAnsi(ptrToText);
            Marshal.FreeHGlobal(ptrToText);
            return textAnsi;
        }

        internal static int GetSelectionStart()
        {
            int length = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETLENGTH, 0, 0);
            IntPtr ptrToText = Marshal.AllocHGlobal(length + 1);
            int selectionStart = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETSELECTIONSTART, 0, 0);
            Marshal.FreeHGlobal(ptrToText);
            return selectionStart;
        }

        //internal static void SetToolBarIcon()
        //{
        //    toolbarIcons tbIcons = new toolbarIcons();
        //    tbIcons.hToolbarBmp = tbBmp.GetHbitmap();
        //    IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
        //    Marshal.StructureToPtr(tbIcons, pTbIcons, false);
        //    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[idMyDlg]._cmdID, pTbIcons);
        //    Marshal.FreeHGlobal(pTbIcons);
        //}

        //internal static void PluginCleanUp()
        //{
        //    Win32.WritePrivateProfileString("SomeSection", "SomeKey", someSetting ? "1" : "0", iniFilePath);
        //}

        #endregion

        #region Menu functions

        internal static void checkBracketsSelected()
        {
            // checking selected text only
            string allText = GetAllText();
            string textToCheck = GetSelectedText();
            int selectionStart = GetSelectionStart();

            // ATTENTION: DO NOT TRIM
            string textBeforeSelection = allText.Substring(0, selectionStart);
            string[] rows = textBeforeSelection.Split('\n');
            int rowcount = rows.Length;
            int charcount = rows[rows.Length - 1].Length;

            bool isOk = checkBrackets(textToCheck, rowcount, charcount + 1);

            if (isOk)
            {
                MessageBox.Show("Selected text in your file have brackets balanced", "Brackets balanced!");
            }
        }

        internal static void checkBracketsAll()
        {
            // checking entire text in file
            string textToCheck = GetAllText();
            bool isOk = checkBrackets(textToCheck);

            if (isOk)
            {
                MessageBox.Show("All brackets in your file are balanced", "Brackets balanced!");
            }
        }

        internal static bool checkBrackets(string text)
        {
            return checkBrackets(text, 1, 1);
        }

        /// <summary>
        /// Check brackets balancing in text parameter. rownumber and charnumber are used for better error explaining
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <param name="rownumber"></param>
        /// <param name="charnumber"></param>
        /// <returns></returns>
        internal static bool checkBrackets(string text, int rownumber, int charnumber)
        {
            Stack stack = new Stack();

            // reading entire text, one character at a time
            for (int i = 0; i < text.Length; i++, charnumber++)
            {
                //TODO: parametrize brackets types
                char c = text[i];
                switch (c)
                {
                    // if it's open bracket: stack push
                    case '(':
                    case '[':
                    case '{':
                    case '<':
                        BCChar bcc = new BCChar();
                        bcc.charvalue = c;
                        bcc.rownumber = rownumber;
                        bcc.charnumber = charnumber;

                        stack.Push(bcc);
                        break;

                    // if it's a close bracket
                    case ')':
                    case ']':
                    case '}':
                    case '>':
                        if (stack.Count > 0)
                        {
                            // stack isn't empty: stack pop
                            BCChar bcc_pop = (BCChar)stack.Pop();
                            char opened = bcc_pop.charvalue;

                            // if brackets are not of the same type: error
                            if ((c == ')' && opened != '(') || (c == ']' && opened != '[') || (c == '}' && opened != '{') || (c == '>' && opened != '<'))
                            {
                                string error = string.Format("Brackets unbalanced at row {0} and character {1}", bcc_pop.rownumber, bcc_pop.charnumber);
                                MessageBox.Show(error, "Brackets unbalanced");
                                return false;
                            }
                        }
                        else
                        {
                            // stack is empty: error
                            string error = string.Format("Brackets unbalanced at row {0} and character {1}", rownumber, charnumber);
                            MessageBox.Show(error, "Brackets unbalanced");
                            return false;
                        }
                        break;

                    case '\n':
                        rownumber++;
                        charnumber = 0;
                        break;

                    default:
                        break;
                }
            }

            if (stack.Count > 0)
            {
                // stack isn't empty: error
                BCChar bcc_pop = (BCChar)stack.Pop();
                string error = string.Format("Brackets unbalanced at row {0} and character {1}", bcc_pop.rownumber, bcc_pop.charnumber);
                MessageBox.Show(error, "Brackets unbalanced");
                return false;
            }

            return true;
        }

        /* dockable dialog unused

        internal static void myDockableDialog()
        {
            if (frmMyDlg == null)
            {
                frmMyDlg = new frmMyDlg();

                using (Bitmap newBmp = new Bitmap(16, 16))
                {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.Fuchsia;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(tbBmp_tbTab, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    tbIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = frmMyDlg.Handle;
                _nppTbData.pszName = "My dockable dialog";
                _nppTbData.dlgID = idMyDlg;
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint)tbIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
            }
            else
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMSHOW, 0, frmMyDlg.Handle);
            }
        }
        */

        #endregion
    }
}