using System;
using System.IO;
using System.Windows.Controls;

namespace TestApp;
/// <summary>
/// Interaction logic for ChangelogView.xaml
/// </summary>
public partial class ChangelogView : UserControl
{
    public ChangelogView()
    {
        InitializeComponent();

        webView2.Source = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "changelog.html"));
        webView2.NavigationCompleted += async (sender, args) => await webView2.CoreWebView2.ExecuteScriptAsync(@"
            (function() {
                var links = document.querySelectorAll('a[href^=""#""]');
                for (var i = 0; i < links.length; i++) {
                    links[i].onclick = function (event) {
                        event.preventDefault();
                        var targetId = this.getAttribute('href').substring(1);
                        var targetElement = document.getElementById(targetId);
                        if (targetElement) {
                            targetElement.scrollIntoView({ behavior: 'smooth' });
                        }
                    };
                }
            })();");
    }
}
