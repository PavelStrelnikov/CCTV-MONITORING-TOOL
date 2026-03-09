using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace AppsDemo
{
    public partial class FormMenuContainer : DockContent
    {
        public FormMenuContainer()
        {
            InitializeComponent();
        }
        private List<string> m_listMenu = new List<string>();

        public delegate void MenuItemClickedHandler(string menuItem);
        public delegate void MenuItemDoubleClickedHandler(string menuItem);

        public event MenuItemClickedHandler MenuItemClicked;
        public event MenuItemDoubleClickedHandler MenuItemDoubleClicked;

        public List<string> MenuList
        {
            get
            {
                return m_listMenu;
            }
            set
            {
                m_listMenu = value;
                if (m_listMenu != null)
                {
                    if (string.IsNullOrEmpty(this.textBoxSearch.Text))
                    {
                        this.listBoxMenu.Items.Clear();
                        foreach (string strMenu in m_listMenu)
                        {
                            this.listBoxMenu.Items.Add(strMenu);
                        }
                    }
                    else
                    {
                        SearchMenu();
                    }
                }
                else
                {
                    this.listBoxMenu.Items.Clear();
                }
            }
        }

        private void SearchMenu()
        {
            if (MenuList == null)
            {
                return;
            }
            this.listBoxMenu.Items.Clear();
            string strKey = this.textBoxSearch.Text.ToLower();
            foreach (string subMenu in MenuList)
            {
                if (subMenu.ToLower().Contains(strKey))
                {
                    this.listBoxMenu.Items.Add(subMenu);
                }
            }
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            SearchMenu();
        }

        private void listBoxMenu_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.MenuItemClicked != null)
            {
                string menuItem = this.listBoxMenu.Text;
                this.MenuItemClicked(menuItem);
            }
            
        }

        private void listBoxMenu_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.MenuItemDoubleClicked != null)
            {
                string menuItem = this.listBoxMenu.Text;
                this.MenuItemDoubleClicked(menuItem);
            }
        }

        private void textBoxSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            //按下了回车
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                if (this.listBoxMenu.Items.Count > 0)
                {
                    this.listBoxMenu.SelectedIndex = 0;
                }
                this.listBoxMenu.Focus();
            }
        }

        private void listBoxMenu_KeyPress(object sender, KeyPressEventArgs e)
        {
            //按下了回车
            if (e.KeyChar == 13)
            {
                if (this.MenuItemDoubleClicked != null)
                {
                    string menuItem = this.listBoxMenu.Text;
                    this.MenuItemDoubleClicked(menuItem);
                }
            }
        }

        private void listBoxMenu_SelectedValueChanged(object sender, EventArgs e)
        {
            if (this.MenuItemClicked != null)
            {
                string menuItem = this.listBoxMenu.Text;
                this.MenuItemClicked(menuItem);
            }
        }
    }
}
