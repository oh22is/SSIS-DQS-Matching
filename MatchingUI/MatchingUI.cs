using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Design;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.Windows.Forms;

namespace oh22is.SqlServer.DQS
{
    class MatchingUI : IDtsComponentUI
    {
        #region Members

        IDTSComponentMetaData100 metaData;
        IServiceProvider serviceProvider;

        #endregion

        #region IDtsComponentUI Member

        bool IDtsComponentUI.Edit(IWin32Window parentWindow, Variables variables, Connections connections)
        {
            FrmMatchingUi editor = new FrmMatchingUi(metaData, serviceProvider, variables, connections);
            return (editor.ShowDialog(parentWindow) == DialogResult.OK ? true : false);
        }

        void IDtsComponentUI.Help(IWin32Window parentWindow) { }

        void IDtsComponentUI.Initialize(IDTSComponentMetaData100 ComponentMetadata, IServiceProvider serviceProvider)
        {
            this.metaData = ComponentMetadata;
            this.serviceProvider = serviceProvider;
        }

        void IDtsComponentUI.New(IWin32Window parentWindow) { }

        void IDtsComponentUI.Delete(IWin32Window parentWindow) { }

        #endregion

    }
}
