using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using rpaulo.toolbar;

namespace DropToolBarDemo
{
    /**
     * @modify cs
     * @email chenshi011@163.com
     */ 
    public partial class Form1 : Form
    {
        ToolBarManager _toolbarManager;
        public Form1()
        {
            InitializeComponent();
        }

        private ArrayList controls;
        private string configFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "config.xml");
        private void Form1_Load(object sender, EventArgs e)
        {
            controls = new ArrayList();
            _toolbarManager = new ToolBarManager(this, this);
            controls.Add(toolBar1);
            controls.Add(toolStrip1);
            controls.Add(toolStrip2);
            if (File.Exists(configFile))
            {
                if (!LoadFromXml())
                {
                    MessageBox.Show("读取配置文件出错！");
                }
            }
            else
            {
                _toolbarManager.AddControl(toolBar1, DockStyle.Top, new Point(0, 22));
                _toolbarManager.AddControl(toolStrip1, DockStyle.None, new Point(185, 43));
                _toolbarManager.AddControl(toolStrip2, DockStyle.Top, new Point(0, 0));
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveAsXml();
        }

        private bool LoadFromXml()
        {
            bool result = false;
            FileStream fs = new FileStream(configFile, FileMode.Open, FileAccess.Read);
            try
            {
                try
                {
                    result = LoadFromXml(fs);
                }
                catch { };
            }
            finally
            {
                fs.Close();
            }
            return result;
        }
        public bool LoadFromXml(Stream stream)
        {
            bool result = false;
            XmlDocument xmlDoc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;//忽略文档里面的注释
            XmlReader reader = XmlReader.Create(stream, settings);
            xmlDoc.Load(reader);

            foreach (XmlElement xmlElement in xmlDoc.DocumentElement)
            {
                if (xmlElement.Name == "ToolBar")
                {
                    string name = xmlElement.GetAttribute("name");
                    int x = Int32.Parse(xmlElement.GetAttribute("x"));
                    int y = Int32.Parse(xmlElement.GetAttribute("y"));
                    string type = xmlElement.GetAttribute("type");
                    DockStyle style = DockStyle.Top;
                    switch (type)
                    {
                        case "None":
                            style = DockStyle.None;
                            break;
                        case "Top":
                            style = DockStyle.Top;
                            break;
                        case "Bottom":
                            style = DockStyle.Bottom;
                            break;
                        case "Right":
                            style = DockStyle.Right;
                            break;
                        case "Fill":
                            style = DockStyle.Fill;
                            break;
                    }
                    addControlInManager(name, style, x, y);
                    result = true;
                }
            }
            return result;
        }

        private void addControlInManager(string name,DockStyle style,int x,int y)
        {
            if(toolStrip1.Text == name)
                _toolbarManager.AddControl(toolStrip1, style, new Point(x, y));
            else if (toolStrip2.Text == name)
                _toolbarManager.AddControl(toolStrip2, style, new Point(x, y));
            else
                _toolbarManager.AddControl(toolBar1, style, new Point(x, y));
        }

        public void SaveAsXml()
        {
            FileStream fs = new FileStream(configFile, FileMode.Create);
            try
            {
                try
                {
                    SaveAsXml(fs);
                }
                catch { };
            }
            finally
            {
                fs.Close();
            }
        }

        public void SaveAsXml(Stream stream)
        {
            XmlWriter xmlOut = XmlWriter.Create(stream, new XmlWriterSettings() { Encoding = Encoding.Unicode, Indent = true });
            xmlOut.WriteComment("测试");
            ArrayList list = _toolbarManager.GetControls();

            int count = list.Count;
            xmlOut.WriteStartElement("ToolBars");
            for (int i = 0; i < count; i++)
            {
                xmlOut.WriteStartElement("ToolBar");
                Control c = list[i] as Control;
                ToolBarDockHolder refHolder = _toolbarManager.GetHolder(c);
                string style = refHolder.DockStyle.ToString();
                Point point = new Point(0, 0);

                if (style.ToLower() != "none")
                {
                    point = refHolder.PreferredDockedLocation;
                }
                else
                {
                    point = refHolder.FloatForm.Location;
                }

                int width = refHolder.Width;
                int height = refHolder.Height;
               
                xmlOut.WriteAttributeString("name", c.Text);
                xmlOut.WriteAttributeString("x", point.X+"");
                if(style.ToLower()!= "none")
                    xmlOut.WriteAttributeString("y", point.Y / height * height + "");
                else
                    xmlOut.WriteAttributeString("y", point.Y + "");
                xmlOut.WriteAttributeString("type", style);
                xmlOut.WriteEndElement();
            }
            xmlOut.WriteEndElement();
            xmlOut.Close();
        }
    }
}
