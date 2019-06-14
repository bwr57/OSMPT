using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodSoft.Core.Tools
{
    public interface ISelectableListData
    {
        string[] GetNames();
        void SetSelectedIndex(int index);
        int GetSelectedIndex();
        int SelectedItemIndex { get; set; }
    }
}
