using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Net.Http;
using System.Windows.Forms;

namespace Neptune
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Initialize WebView2
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await webView21.EnsureCoreWebView2Async(null);
            webView21.CoreWebView2.Navigate("https://google.com"); // default homepage
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // stops ding sound
                btnGo.PerformClick();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (webView21.CanGoBack)
                webView21.GoBack();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (webView21.CanGoForward)
                webView21.GoForward();
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            webView21.Reload();
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.Navigate("https://google.com");
        }

        private async void btnGo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUrl.Text))
                return;

            if (webView21.CoreWebView2 == null)
            {
                await webView21.EnsureCoreWebView2Async();
            }

            NavigateTo(webView21, txtUrl.Text);
        }


        private void webView21_Click(object sender, EventArgs e)
        {
            // Optional: do nothing or handle if needed
        }

        private void txtUrl_TextChanged(object sender, EventArgs e)
        {

        }

        // Event handler (wired up by the Designer)
        private void AddTab_Click(object sender, EventArgs e)
        {
            AddTab();
        }


        private async void AddTab()
        {
            TabPage newTab = new TabPage("New Tab");

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,  // ✅ 3 rows: top search bar, bottom nav bar, browser
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // top search bar
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // bottom nav bar
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // browser

            // ── Top bar (search only) ──────────────────────────────
            RichTextBox topSearch = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Multiline = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            // ── Bottom bar (navigate + buttons) ───────────────────
            Panel bottomBar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(45, 45, 45)
            };

            RichTextBox bottomSearch = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Multiline = false,
                BackColor = System.Drawing.Color.FromArgb(45, 45, 45),
                ForeColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None
            };

            // Buttons: Go, <, >, Refresh, Home, +
            Button btnGo = MakeButton("Go");
            Button btnBack = MakeButton("<");
            Button btnForward = MakeButton(">");
            Button btnRefresh = MakeButton("Refresh");
            Button btnHome = MakeButton("Home");
            Button btnNewTab = MakeButton("+");

            // Layout the bottom bar using a TableLayoutPanel inside
            TableLayoutPanel bottomLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 1
            };
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // search fills space
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 45));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));

            bottomLayout.Controls.Add(bottomSearch, 0, 0);
            bottomLayout.Controls.Add(btnGo, 1, 0);
            bottomLayout.Controls.Add(btnBack, 2, 0);
            bottomLayout.Controls.Add(btnForward, 3, 0);
            bottomLayout.Controls.Add(btnRefresh, 4, 0);
            bottomLayout.Controls.Add(btnHome, 5, 0);
            bottomLayout.Controls.Add(btnNewTab, 6, 0);
            bottomBar.Controls.Add(bottomLayout);

            // ── WebView2 ───────────────────────────────────────────
            WebView2 browser = new WebView2 { Dock = DockStyle.Fill };

            // Add all rows to layout
            layout.Controls.Add(topSearch, 0, 0);
            layout.Controls.Add(bottomBar, 0, 1);
            layout.Controls.Add(browser, 0, 2);

            newTab.Controls.Add(layout);
            tabControl1.TabPages.Add(newTab);

            // ✅ Switch to new tab by index (more reliable than SelectedTab)
            tabControl1.SelectedIndex = tabControl1.TabPages.Count - 1;

            // ✅ Await WebView2 init before doing anything with it
            await browser.EnsureCoreWebView2Async();
            browser.CoreWebView2.Navigate("https://www.google.com");

            browser.CoreWebView2.NavigationCompleted += async (s, e) =>
            {
                try
                {
                    var uri = new Uri(browser.CoreWebView2.Source);
                    var faviconUrl = "https://www.google.com/s2/favicons?domain=" + uri.Host + "&sz=16";

                    using (var http = new HttpClient())
                    {
                        var bytes = await http.GetByteArrayAsync(faviconUrl);

                        using (var ms = new System.IO.MemoryStream(bytes))
                        {
                            var icon = new System.Drawing.Bitmap(ms);

                            Invoke((Action)(() =>
                            {
                                if (tabControl1.ImageList == null)
                                    tabControl1.ImageList = new ImageList
                                    {
                                        ImageSize = new System.Drawing.Size(16, 16)
                                    };

                                var key = uri.Host;
                                if (!tabControl1.ImageList.Images.ContainsKey(key))
                                    tabControl1.ImageList.Images.Add(key, icon);

                                newTab.ImageKey = key;
                            }));
                        }
                    }
                }
                catch { }
            };

            // ✅ Update tab title automatically
            browser.CoreWebView2.DocumentTitleChanged += (s, e) =>
                newTab.Text = browser.CoreWebView2.DocumentTitle;

            // ── Wire up top search bar (search only) ──────────────
            topSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                var query = Uri.EscapeDataString(topSearch.Text.Trim());
                browser.CoreWebView2.Navigate($"https://www.google.com/search?q={query}");
            };

            // ── Wire up bottom search bar (navigate + search) ─────
            bottomSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                NavigateTo(browser, bottomSearch.Text);
            };

            // ── Wire up buttons ───────────────────────────────────
            btnGo.Click += (s, e) => NavigateTo(browser, bottomSearch.Text);
            btnBack.Click += (s, e) => { if (browser.CoreWebView2.CanGoBack) browser.CoreWebView2.GoBack(); };
            btnForward.Click += (s, e) => { if (browser.CoreWebView2.CanGoForward) browser.CoreWebView2.GoForward(); };
            btnRefresh.Click += (s, e) => browser.CoreWebView2.Reload();
            btnHome.Click += (s, e) => browser.CoreWebView2.Navigate("https://www.google.com");

            // ✅ + button creates a new tab
            btnNewTab.Click += (s, e) => AddTab();

            // ✅ Keep bottom search bar in sync with current URL
            browser.CoreWebView2.NavigationCompleted += (s, e) =>
                bottomSearch.Text = browser.CoreWebView2.Source;
        }

        // ── Helper to make styled buttons ─────────────────────────
        private Button MakeButton(string text) => new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = System.Drawing.Color.FromArgb(45, 45, 45),
            ForeColor = System.Drawing.Color.White,
            Font = new System.Drawing.Font("Segoe UI", 8f)
        };


        private void NavigateTo(WebView2 browser, string input)
        {
            string url = input.Trim();

            // If it looks like a URL (has a dot + TLD), load it directly
            if (url.Contains(".") && !url.EndsWith("."))
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url; // auto add https
                }
            }
            else
            {
                // Otherwise, treat as search query
                string query = Uri.EscapeDataString(url);
                url = $"https://www.google.com/search?q={query}";
            }

            browser.CoreWebView2.Navigate(url);
        }



        private async void Editor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;

            var editor = (RichTextBox)sender;
            var browser = (WebView2)editor.Tag;

            // Make sure CoreWebView2 is ready (in case somehow it isn't)
            await browser.EnsureCoreWebView2Async();

            var input = editor.Text.Trim();

            // Detect if it's a URL or a search query
            if (Uri.TryCreate(input, UriKind.Absolute, out var uri) &&
                (uri.Scheme == "http" || uri.Scheme == "https"))
            {
                browser.CoreWebView2.Navigate(input);
            }
            else if (!input.Contains(" ") && input.Contains("."))
            {
                // Looks like a domain without scheme (e.g. "google.com")
                browser.CoreWebView2.Navigate("https://" + input);
            }
            else
            {
                // Treat as a search query
                browser.CoreWebView2.Navigate(
                    "https://www.google.com/search?q=" + Uri.EscapeDataString(input));
            }
        }

        private void process1_Exited(object sender, EventArgs e)
        {

        }

        private void btnClearDone_Click(object sender, EventArgs e)
        {

        }
    }
}
