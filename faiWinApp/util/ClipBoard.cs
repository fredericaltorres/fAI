using System;
using System.Windows.Forms;

namespace faiWinApp
{
    public class ClipBoard
    {

        public bool Clear()
        {
            Clipboard.SetDataObject(new DataObject());
            return true;
        }
        public bool SetText(string strText)
        {
            this.Clear();
            System.Windows.Forms.Clipboard.SetDataObject(strText);
            return true;
        }
        public bool DataPresent()
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData != null)
            {
                return iData.GetDataPresent(DataFormats.Text);
            }
            return false;
        }
        public string GetText()
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Text))
            {
                return (String)iData.GetData(DataFormats.Text);
            }
            return null;
        }
    }
}
