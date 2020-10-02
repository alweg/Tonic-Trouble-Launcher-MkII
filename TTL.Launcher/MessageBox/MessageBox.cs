using System.Windows;

namespace TTL.Launcher.MessageBox
{
    public static class MessageBox
    {
        public static bool ShowDialog(Window owner, string text, string caption, string button)
        {
            Windows.MessageBoxWindow messageBox = new Windows.MessageBoxWindow(owner, text, caption, button);
            messageBox.ShowDialog();

            if (messageBox.DialogResult == false) { return false; }
            else if (messageBox.DialogResult == true) { return true; }
            return false;
        }
    }
}
