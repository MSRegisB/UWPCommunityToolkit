// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

#if WINDOWS_UWP
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
#else
using System.Windows.Input;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals
{
    internal static class KeyboardHelper
    {
        public static void GetMetaKeyState(out bool ctrl, out bool shift)
        {
#if WINDOWS_UWP
            ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) == CoreVirtualKeyStates.Down;
            shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down;
#else
            ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            shift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
#endif
        }

        public static void GetMetaKeyState(out bool ctrl, out bool shift, out bool alt)
        {
            GetMetaKeyState(out ctrl, out shift);
#if WINDOWS_UWP
            alt = Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu) == CoreVirtualKeyStates.Down;
#else
            alt = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
#endif
        }
    }
}
