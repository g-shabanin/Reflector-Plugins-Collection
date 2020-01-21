namespace Reflector.Review
{
	using System;
	using System.Windows.Forms;
	using Reflector.Review.Data;
	using Reflector.CodeModel;

    internal sealed class HistoryRichTextBox : RichTextBox
    {
        //protected override void OnLinkClicked(LinkClickedEventArgs e)
        //{
        //    if (e.LinkText.StartsWith("code://"))
        //    {
        //        CodeIdentifier identifier = new CodeIdentifier(e.LinkText);
        //        this.services.ActiveItem = identifier.Resolve(
        //            this.services.AssemblyManager,
        //            this.services.AssemblyCache
        //            );
        //    }
        //    else
        //        base.OnLinkClicked(e);
        //}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Control | Keys.P:	
	                if (this.CanPaste(DataFormats.GetFormat(DataFormats.Text)))
	                {
	                    this.Paste();
						return true;
					}
					break;
				
				case Keys.Control | Keys.C:
            		if (this.SelectionLength > 0)
                	{
                		this.Copy();
                		return true;
        			}
            		break;
				
				case Keys.Control | Keys.X:
	                if (this.SelectionLength > 0)
	                {
                    	this.Cut();
						return true;
					}
					break;
			}
			
			if (base.ProcessCmdKey(ref msg, keyData))
			{
				return true;
			}

			return false;

			/*
			RichTextBox excludes the following keys:
			0x2004c	// Ctrl+L
			0x20052	// Ctrl+R
			0x20045 // Ctrl+E
			0x2004a // Ctrl+J

			0x2005a // Ctrl+Z
			0x20043 // Ctrl+C
			0x20058 // Ctrl+X
			0x20056 // Ctrl+V
			0x20041 // Ctrl+A
			0x20059 // Ctrl+Y
			0x20008 // Ctrl+Tab
			0x2002e // Ctrl+Delete
			0x1002e // Shift+Delete
			0x1002d // Shift+Insert
			*/
		}
    }
}
