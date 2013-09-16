using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data;

//to use with VS IDE we need to implementent IDispatch
using System.Runtime.InteropServices;
using stdole;

namespace CodeNavigator
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    /// 
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None), System.Runtime.InteropServices.GuidAttribute("CDFE9B1B-40BA-4BC2-9CF4-AFA8C727533E")]
    public partial class ShortcutListWindow : UserControl, IDispatch
    {

        List<Shortcut> items = new List<Shortcut>();

        /// <summary>
        /// Create an empty dialog to list files in the curren project
        /// </summary>
        public ShortcutListWindow()
        {
            InitializeComponent();
            this.itemGrid.ItemsSource = items;
        }

        /// <summary>
        /// Add a shortcut to the dialog
        /// </summary>
        /// <param name="shortcut"></param>
        public void Add(Shortcut shortcut)
        {
            items.Add(shortcut);
        }

        void OnItemSelected(Shortcuts item)
        {
            MessageBox.Show(item.ToString());
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Shortcuts item = (Shortcuts)this.itemGrid.SelectedItems[0];
                OnItemSelected(item);
            }
        }

        private void FileList_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DocumentHandler handler = FastCode.Instance.GetDocumentHandler();
            if (handler == null)
                return;

            this.items = handler.GetAutoComplete().GetShortcuts().ShortcutList;
            this.itemGrid.ItemsSource = this.items;
        }
    }
}
