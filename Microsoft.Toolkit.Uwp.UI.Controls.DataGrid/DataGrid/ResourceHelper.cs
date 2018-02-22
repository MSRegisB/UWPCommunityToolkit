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

using System;
using System.Collections.Generic;
using System.IO;
#if WINDOWS_UWP
using System.Reflection;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
#else
using System.Windows.Controls;
using System.Windows.Markup;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals
{
    internal sealed class ResourceHelper
    {
        private static Dictionary<string, string> _cache = new Dictionary<string, string>();

        private ResourceHelper()
        {
        }

        public static ControlTemplate GetControlTemplate<T>()
        {
            return GetControlTemplate(typeof(T));
        }

        public static ControlTemplate GetControlTemplate(Type type)
        {
            return GetControlTemplate(type, type.FullName);
        }

        public static ControlTemplate GetControlTemplate(Type type, string resourceName)
        {
            string xaml = GetTemplateXaml(type, resourceName);
            if (string.IsNullOrEmpty(xaml))
            {
                throw new Exception(type.Name + " XAML markup could not be loaded.");
            }
            else
            {
#if DEBUG
                try
                {
#endif
#if WINDOWS_UWP
                    return XamlReader.Load(xaml) as ControlTemplate;
#else
                    return XamlReader.Load(type.Assembly.GetManifestResourceStream(resourceName + ".xaml")) as ControlTemplate;
#endif
#if DEBUG
                }
                catch
                {
                    throw;
                }
#endif
            }
        }

        public static string GetTemplateXaml(Type type, string resourceName)
        {
            string template;

            if (!_cache.TryGetValue(resourceName, out template))
            {
#if WINDOWS_UWP
                Stream s = type.GetTypeInfo().Assembly.GetManifestResourceStream(resourceName + ".xaml");
#else
                Stream s = type.Assembly.GetManifestResourceStream(resourceName + ".xaml");
#endif
                if (s != null)
                {
                    template = new StreamReader(s).ReadToEnd();
                    _cache[resourceName] = template;
                }
            }

            return template;
        }
    }
}
