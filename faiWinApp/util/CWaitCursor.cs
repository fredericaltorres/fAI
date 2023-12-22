using System;
using System.Windows.Forms;

namespace LogViewer.Net
{
    public class CWaitCursor : IDisposable
    {
        private Cursor cursor;
        private System.Windows.Forms.Form aForm;

        private void Init()
        {
            cursor = null;
            aForm = null;
        }
        public CWaitCursor(System.Windows.Forms.Form f)
        {
            Init();
            Wait(f);
        }
        public CWaitCursor()
        {
            Init();
            Wait(null);
        }
        private void Wait(System.Windows.Forms.Form f)
        {
            cursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            if (f != null)
            {
                aForm = f;
                aForm.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            }
            System.Windows.Forms.Application.DoEvents();
        }
        public void Dispose()
        {

            if (aForm != null)
            {
                aForm.Cursor = System.Windows.Forms.Cursors.Default;
                aForm = null;
            }
            if (cursor != null)
            {
                Cursor.Current = cursor;
                cursor = null;
            }
        }
    }


}



