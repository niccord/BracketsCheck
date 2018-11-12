using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections;

namespace BracketsCheck
{
    class Main
    {
        #region Fields
        internal const string PluginName = "BracketsCheck";
        #endregion

        #region Parameters
        static bool checkRoundBrackets = true;
        static bool checkSquareBrackets = true;
        static bool checkCurlyBrackets = true;
        static bool checkAngleBrackets = true;
        #endregion

        #region BCChar

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
            PluginBase.InitiINI();
            PluginBase.SetCommand(0, "Check Brackets: All text", checkBracketsAll);
            PluginBase.SetCommand(1, "Check Brackets: Selected text", checkBracketsSelected);
            PluginBase.SetCommand(2, "", null); // ----------------------------------
            string checkRound = "Check round brackets";
            string checkSquare = "Check square brackets";
            string checkCurly = "Check curly brackets";
            string checkAngle = "Check angle brackets";
            PluginBase.SetCommand(3, checkRound, toggleCheckRoundBrackets, Win32.GetPrivateProfileInt(PluginName, checkRound, 1, PluginBase.IniFilePath) > 0);
            if (Win32.GetPrivateProfileInt(PluginName, checkRound, 1, PluginBase.IniFilePath) < 1) checkRoundBrackets = false;
            PluginBase.SetCommand(4, checkSquare, toggleCheckSquareBrackets, Win32.GetPrivateProfileInt(PluginName, checkSquare, 1, PluginBase.IniFilePath) > 0);
            if (Win32.GetPrivateProfileInt(PluginName, checkSquare, 1, PluginBase.IniFilePath) < 1) checkSquareBrackets = false;
            PluginBase.SetCommand(5, checkCurly, toggleCheckCurlyBrackets, Win32.GetPrivateProfileInt(PluginName, checkCurly, 1, PluginBase.IniFilePath) > 0);
            if (Win32.GetPrivateProfileInt(PluginName, checkCurly, 1, PluginBase.IniFilePath) < 1) checkCurlyBrackets = false;
            PluginBase.SetCommand(6, checkAngle, toggleCheckAngleBrackets, Win32.GetPrivateProfileInt(PluginName, checkAngle, 1, PluginBase.IniFilePath) > 0);
            if (Win32.GetPrivateProfileInt(PluginName, checkAngle, 1, PluginBase.IniFilePath) < 1) checkAngleBrackets = false;
        }

        #endregion

        #region Get Text

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

        #endregion

        #region Toggle

        internal static void toggleCheckRoundBrackets()
        {
            checkRoundBrackets = !checkRoundBrackets;
            PluginBase.toggleCheckMenuItem(3, checkRoundBrackets);
        }

        internal static void toggleCheckSquareBrackets()
        {
            checkSquareBrackets = !checkSquareBrackets;
            PluginBase.toggleCheckMenuItem(4, checkSquareBrackets);
        }

        internal static void toggleCheckCurlyBrackets()
        {
            checkCurlyBrackets = !checkCurlyBrackets;
            PluginBase.toggleCheckMenuItem(5, checkCurlyBrackets);
        }

        internal static void toggleCheckAngleBrackets()
        {
            checkAngleBrackets = !checkAngleBrackets;
            PluginBase.toggleCheckMenuItem(6, checkAngleBrackets);
        }

        #endregion

        #region Check Brackets

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
                char c = text[i];

                if ((c == '(' && checkRoundBrackets) ||
                    (c == '[' && checkSquareBrackets) ||
                    (c == '{' && checkCurlyBrackets) ||
                    (c == '<' && checkAngleBrackets))
                {
                    // it's an open bracket
                    stackPushBracket(stack, c, rownumber, charnumber);
                }
                else if ((c == ')' && checkRoundBrackets) ||
                    (c == ']' && checkSquareBrackets) ||
                    (c == '}' && checkCurlyBrackets) ||
                    (c == '>' && checkAngleBrackets))
                {
                    // it's a close bracket
                    if (stack.Count > 0)
                    {
                        // stack isn't empty: stack pop
                        BCChar bcc_pop = (BCChar)stack.Pop();
                        char opened = bcc_pop.charvalue;

                        // if brackets are not of the same type: error
                        if ((c == ')' && opened != '(') ||
                            (c == ']' && opened != '[') ||
                            (c == '}' && opened != '{') ||
                            (c == '>' && opened != '<'))
                        {
                            displayError(bcc_pop.rownumber, bcc_pop.charnumber);
                            return false;
                        }
                    }
                    else
                    {
                        // stack is empty: error
                        displayError(rownumber, charnumber);
                        return false;
                    }
                }
                else if (c == '\n')
                {
                    // new line
                    rownumber++;
                    charnumber = 0;
                }
            }

            if (stack.Count > 0)
            {
                // stack isn't empty: error
                BCChar bcc_pop = (BCChar)stack.Pop();
                displayError(bcc_pop.rownumber, bcc_pop.charnumber);
                return false;
            }

            return true;
        }

        internal static void stackPushBracket(Stack stack, char c, int rownumber, int charnumber)
        {
            BCChar bcc = new BCChar();
            bcc.charvalue = c;
            bcc.rownumber = rownumber;
            bcc.charnumber = charnumber;

            stack.Push(bcc);
        }

        internal static void displayError(int rownumber, int charnumber)
        {
            string error = string.Format("Brackets unbalanced at row {0} and character {1}", rownumber, charnumber);
            MessageBox.Show(error, "Brackets unbalanced");
        }

        internal static void displayMessage(String message)
        {
            MessageBox.Show(message);
        }

        #endregion
    }
}
