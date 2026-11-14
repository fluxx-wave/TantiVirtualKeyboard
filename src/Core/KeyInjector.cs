using System;
using System.Collections.Generic;

namespace HotVirtualKeyboard.Core
{
    public static class KeyInjector
    {
        public static void SendVirtualKey(ushort vkCode, bool extended = false)
        {
            var state = KeyboardState.Instance;
            ushort scan = (ushort)Win32.MapVirtualKeyEx((uint)vkCode, 0, IntPtr.Zero);
            uint flags = extended ? Win32.KEYEVENTF_EXTENDEDKEY : 0;

            bool hasModifiers = (state.IsCtrlDown && vkCode != Win32.VK_CONTROL) ||
                                (state.IsAltDown && vkCode != Win32.VK_MENU) ||
                                (state.IsWinDown && vkCode != Win32.VK_LWIN && vkCode != Win32.VK_RWIN) ||
                                (state.IsShiftActive && vkCode != Win32.VK_SHIFT && vkCode != Win32.VK_LSHIFT && vkCode != Win32.VK_RSHIFT);

            if (hasModifiers)
            {
                var modDown = new List<Win32.INPUT>();
                if (state.IsCtrlDown && vkCode != Win32.VK_CONTROL && vkCode != Win32.VK_LCONTROL && vkCode != Win32.VK_RCONTROL)
                    modDown.Add(CreateKeyInput(Win32.VK_CONTROL, (ushort)Win32.MapVirtualKeyEx(Win32.VK_CONTROL, 0, IntPtr.Zero), 0));

                if (state.IsAltDown && vkCode != Win32.VK_MENU && vkCode != Win32.VK_LMENU && vkCode != Win32.VK_RMENU)
                    modDown.Add(CreateKeyInput(Win32.VK_MENU, (ushort)Win32.MapVirtualKeyEx(Win32.VK_MENU, 0, IntPtr.Zero), 0));

                if (state.IsWinDown && vkCode != Win32.VK_LWIN && vkCode != Win32.VK_RWIN)
                    modDown.Add(CreateKeyInput(Win32.VK_LWIN, (ushort)Win32.MapVirtualKeyEx(Win32.VK_LWIN, 0, IntPtr.Zero), Win32.KEYEVENTF_EXTENDEDKEY));

                if (state.IsShiftActive && vkCode != Win32.VK_SHIFT && vkCode != Win32.VK_LSHIFT && vkCode != Win32.VK_RSHIFT)
                    modDown.Add(CreateKeyInput(Win32.VK_SHIFT, (ushort)Win32.MapVirtualKeyEx(Win32.VK_SHIFT, 0, IntPtr.Zero), 0));

                if (modDown.Count > 0)
                {
                    Win32.SendInput((uint)modDown.Count, modDown.ToArray(), Win32.INPUT.Size);
                    System.Threading.Thread.Sleep(8);
                }

                var keyInputs = new Win32.INPUT[2];
                keyInputs[0] = CreateKeyInput(vkCode, scan, flags);
                keyInputs[1] = CreateKeyInput(vkCode, scan, flags | Win32.KEYEVENTF_KEYUP);
                Win32.SendInput(2, keyInputs, Win32.INPUT.Size);

                System.Threading.Thread.Sleep(8);

                var modUp = new List<Win32.INPUT>();
                if (state.IsWinDown && vkCode != Win32.VK_LWIN && vkCode != Win32.VK_RWIN)
                    modUp.Add(CreateKeyInput(Win32.VK_LWIN, (ushort)Win32.MapVirtualKeyEx(Win32.VK_LWIN, 0, IntPtr.Zero), Win32.KEYEVENTF_EXTENDEDKEY | Win32.KEYEVENTF_KEYUP));

                if (state.IsAltDown && vkCode != Win32.VK_MENU && vkCode != Win32.VK_LMENU && vkCode != Win32.VK_RMENU)
                    modUp.Add(CreateKeyInput(Win32.VK_MENU, (ushort)Win32.MapVirtualKeyEx(Win32.VK_MENU, 0, IntPtr.Zero), Win32.KEYEVENTF_KEYUP));

                if (state.IsCtrlDown && vkCode != Win32.VK_CONTROL && vkCode != Win32.VK_LCONTROL && vkCode != Win32.VK_RCONTROL)
                    modUp.Add(CreateKeyInput(Win32.VK_CONTROL, (ushort)Win32.MapVirtualKeyEx(Win32.VK_CONTROL, 0, IntPtr.Zero), Win32.KEYEVENTF_KEYUP));

                if (state.IsShiftActive && vkCode != Win32.VK_SHIFT && vkCode != Win32.VK_LSHIFT && vkCode != Win32.VK_RSHIFT)
                    modUp.Add(CreateKeyInput(Win32.VK_SHIFT, (ushort)Win32.MapVirtualKeyEx(Win32.VK_SHIFT, 0, IntPtr.Zero), Win32.KEYEVENTF_KEYUP));

                if (modUp.Count > 0)
                {
                    Win32.SendInput((uint)modUp.Count, modUp.ToArray(), Win32.INPUT.Size);
                }
            }
            else
            {
                var inputs = new Win32.INPUT[2];
                inputs[0] = CreateKeyInput(vkCode, scan, flags);
                inputs[1] = CreateKeyInput(vkCode, scan, flags | Win32.KEYEVENTF_KEYUP);
                Win32.SendInput(2, inputs, Win32.INPUT.Size);
            }

            state.OnCharacterInjected();
        }

        public static void SendUnicodeChar(char ch)
        {
            var inputs = new Win32.INPUT[2];

            // Key Down Unicode
            inputs[0] = new Win32.INPUT
            {
                type = Win32.INPUT_KEYBOARD,
                U = new Win32.InputUnion
                {
                    ki = new Win32.KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = (ushort)ch,
                        dwFlags = Win32.KEYEVENTF_UNICODE,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };

            // Key Up Unicode
            inputs[1] = new Win32.INPUT
            {
                type = Win32.INPUT_KEYBOARD,
                U = new Win32.InputUnion
                {
                    ki = new Win32.KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = (ushort)ch,
                        dwFlags = Win32.KEYEVENTF_UNICODE | Win32.KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };

            Win32.SendInput(2, inputs, Win32.INPUT.Size);

            KeyboardState.Instance.OnCharacterInjected();
        }

        public static void SendText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            var inputs = new Win32.INPUT[text.Length * 2];
            for (int i = 0; i < text.Length; i++)
            {
                ushort ch = (ushort)text[i];
                inputs[i * 2] = new Win32.INPUT
                {
                    type = Win32.INPUT_KEYBOARD,
                    U = new Win32.InputUnion
                    {
                        ki = new Win32.KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = ch,
                            dwFlags = Win32.KEYEVENTF_UNICODE,
                            time = 0,
                            dwExtraInfo = UIntPtr.Zero
                        }
                    }
                };

                inputs[i * 2 + 1] = new Win32.INPUT
                {
                    type = Win32.INPUT_KEYBOARD,
                    U = new Win32.InputUnion
                    {
                        ki = new Win32.KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = ch,
                            dwFlags = Win32.KEYEVENTF_UNICODE | Win32.KEYEVENTF_KEYUP,
                            time = 0,
                            dwExtraInfo = UIntPtr.Zero
                        }
                    }
                };
            }

            Win32.SendInput((uint)inputs.Length, inputs, Win32.INPUT.Size);
            KeyboardState.Instance.OnCharacterInjected();
        }

        public static void SendHotkey(ushort modifierVk, ushort keyVk)
        {
            SendKeyCombination(modifierVk, keyVk);
        }

        public static void SendKeyCombination(params ushort[] vks)
        {
            if (vks == null || vks.Length == 0) return;

            var inputs = new List<Win32.INPUT>();

            // Press all down in order
            foreach (var vk in vks)
            {
                uint flags = (vk == Win32.VK_LWIN || vk == Win32.VK_RWIN) ? Win32.KEYEVENTF_EXTENDEDKEY : 0;
                inputs.Add(CreateKeyInput(vk, 0, flags));
            }

            // Release all up in reverse order
            for (int i = vks.Length - 1; i >= 0; i--)
            {
                var vk = vks[i];
                uint flags = ((vk == Win32.VK_LWIN || vk == Win32.VK_RWIN) ? Win32.KEYEVENTF_EXTENDEDKEY : 0) | Win32.KEYEVENTF_KEYUP;
                inputs.Add(CreateKeyInput(vk, 0, flags));
            }

            Win32.SendInput((uint)inputs.Count, inputs.ToArray(), Win32.INPUT.Size);
        }

        private static Win32.INPUT CreateKeyInput(ushort vk, ushort scan, uint flags)
        {
            if (scan == 0 && vk != 0)
            {
                scan = (ushort)Win32.MapVirtualKeyEx((uint)vk, 0, IntPtr.Zero);
            }

            return new Win32.INPUT
            {
                type = Win32.INPUT_KEYBOARD,
                U = new Win32.InputUnion
                {
                    ki = new Win32.KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = scan,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };
        }
    }
}
