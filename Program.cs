// ✅ C# 7.3 proper class wrapper
using Neptune;
using System;
using System.Windows.Forms;

namespace Neptune
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
