﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Reflection;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.PlatformUI;
using System.ComponentModel;

namespace GitHub.VisualStudio.Helpers
{
    public class SharedDictionaryManager : ResourceDictionary
    {
        static readonly string[] ourAssemblies =
        {
            "GitHub.Api",
            "GitHub.App",
            "GitHub.CredentialManagement",
            "GitHub.Exports",
            "GitHub.Exports.Reactive",
            "GitHub.Extensions",
            "GitHub.Extensions.Reactive",
            "GitHub.UI",
            "GitHub.UI.Reactive",
            "GitHub.VisualStudio"
        };

        static SharedDictionaryManager()
        {
            AppDomain.CurrentDomain.AssemblyResolve += LoadAssemblyFromRunDir;
        }

        public SharedDictionaryManager()
        {
            currentTheme = Colors.DetectTheme();
        }

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        static Assembly LoadAssemblyFromRunDir(object sender, ResolveEventArgs e)
        {
            try
            {
                var name = new AssemblyName(e.Name);
                if (!ourAssemblies.Contains(name.Name))
                    return null;
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var filename = Path.Combine(path, name.Name + ".dll");
                if (!File.Exists(filename))
                    return null;
                return Assembly.LoadFrom(filename);
            }
            catch (Exception ex)
            {
                var log = string.Format(CultureInfo.CurrentCulture, "Error occurred loading {0} from {1}.{2}{3}{4}", e.Name, Assembly.GetExecutingAssembly().Location, Environment.NewLine, ex, Environment.NewLine);
                VsOutputLogger.Write(log);
            }
            return null;
        }

#region ResourceDictionaryImplementation
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        string currentTheme;

#if !XAML_DESIGNER
        static readonly Dictionary<Uri, ResourceDictionary> resourceDicts = new Dictionary<Uri, ResourceDictionary>();
        static string baseThemeUri = "pack://application:,,,/GitHub.VisualStudio;component/Styles/";

        Uri sourceUri;
        bool themed = false;
        public new Uri Source
        {
            get { return sourceUri; }
            set
            {
                if (value.ToString() == "pack://application:,,,/GitHub.VisualStudio;component/Styles/ThemeDesignTime.xaml")
                {
                    if (!themed)
                    {
                        themed = true;
                        VSColorTheme.ThemeChanged += OnThemeChange;
                    }
                    value = new Uri(baseThemeUri + "Theme" + currentTheme + ".xaml");
                }

                sourceUri = value;
                ResourceDictionary ret;
                if (resourceDicts.TryGetValue(value, out ret))
                {
                    if (ret != this)
                    {
                        MergedDictionaries.Add(ret);
                        return;
                    }
                }
                base.Source = value;
                if (ret == null)
                    resourceDicts.Add(value, this);
            }
        }

        void OnThemeChange(ThemeChangedEventArgs e)
        {
            var uri = new Uri(baseThemeUri + "Theme" + currentTheme + ".xaml");
            ResourceDictionary ret;
            if (resourceDicts.TryGetValue(uri, out ret))
                MergedDictionaries.Remove(ret);
            currentTheme = Colors.DetectTheme();
            Source = uri;
        }
#endif
#endregion
    }
}