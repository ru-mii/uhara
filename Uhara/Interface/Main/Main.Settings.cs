using LiveSplit.ASL;
using LiveSplit.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

public partial class Main
{
	public class ScriptSettings
	{
        public void Create(Dictionary<string, string> settings, bool defaultValue = true, string defaultParent = null, params int[] order)
        {
            try
            {
                int count = settings.Count;
                dynamic[,] result = new dynamic[count, 2];

                int index = 0;
                foreach (var pair in settings)
                {
                    result[index, 0] = pair.Key;
                    result[index, 1] = pair.Value;
                    index++;
                }

                Create(result, defaultValue, defaultParent, order);
            }
            catch { }
        }

        public void CreateFromXml(string path)
        {
            try
            {
                do
                {
                    if (!File.Exists(path)) break;

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(path);

                    XmlNodeList nodes = xmlDoc.SelectNodes("//Setting");
                    if (nodes == null) break;

                    foreach (XmlNode node in nodes)
                    {
                        if (node is XmlElement setting)
                        {
                            string id = setting.GetAttribute("Id");
                            string label = setting.GetAttribute("Label");
                            bool state = bool.Parse(setting.GetAttribute("State"));
                            string tooltip = setting.GetAttribute("ToolTip");

                            string group = null;
                            if (node.ParentNode != null && node.ParentNode.Name == "Setting")
                            {
                                XmlElement parent = node.ParentNode as XmlElement;
                                group = parent.GetAttribute("Id");
                            }

                            _settings.AddSetting(id, state, label, group);
                            if (!string.IsNullOrEmpty(tooltip))
                                _settings.Settings[id].ToolTip = tooltip;
                        }
                    }
                }
                while (false);
            }
            catch { }
        }

        public void CreateCustom(dynamic[,] settings, params int[] order)
        {
            try
            {
                Create(settings, order);
            }
            catch { }
        }

        public void Create(dynamic[,] settings, bool defaultValue = true, string defaultParent = null, params int[] order)
        {
            try
            {
                int rows = settings.GetLength(0);
                int cols = settings.GetLength(1);

                if (cols < 1 || cols > 5)
                {
                    TUtils.Print("Settings could not be parsed");
                    return;
                }

                int[] _order = new int[] { 1, 2, 3, 4, 5 };
                if (order.Length != 0)
                {
                    if (order.Length != cols)
                    {
                        TUtils.Print("Order count did not match the settings columns");
                        return;
                    }

                    _order = order;
                }

                if (cols == 1)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        _settings.AddSetting(settings[i, _order[0] - 1], defaultValue, settings[i, _order[0] - 1], defaultParent);
                    }
                }
                else if (cols == 2)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        _settings.AddSetting(settings[i, _order[0] - 1], defaultValue, settings[i, _order[1] - 1], defaultParent);
                    }
                }
                else if (cols == 3)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        _settings.AddSetting(settings[i, _order[0] - 1], settings[i, _order[1] - 1], settings[i, _order[0] - 1], defaultParent);
                    }
                }
                else if (cols == 4)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        _settings.AddSetting(settings[i, _order[0] - 1], settings[i, _order[1] - 1], settings[i, _order[2] - 1], settings[i, _order[3] - 1]);
                    }
                }
                else if (cols == 5)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        _settings.AddSetting(settings[i, _order[0] - 1], settings[i, _order[1] - 1], settings[i, _order[2] - 1], settings[i, _order[3] - 1]);

                        string newTooltip = settings[i, _order[4] - 1];
                        if (!string.IsNullOrEmpty(newTooltip))
                            _settings.Settings[settings[i, _order[0] - 1]].ToolTip = newTooltip;
                    }
                }
            }
            catch { }
        }

        public void Create(dynamic[,] settings, params int[] order)
		{
            try
            {
                bool defaultValue = true;
                string defaultParent = null;

                int rows = settings.GetLength(0);
                int cols = settings.GetLength(1);

                if (cols < 1 || cols > 5)
                {
                    TUtils.Print("Settings could not be parsed");
                    return;
                }

                int[] _order = new int[] { 1, 2, 3, 4, 5 };
                if (order.Length != 0)
                {
                    if (order.Length != cols)
                    {
                        TUtils.Print("Order count did not match the settings columns");
                        return;
                    }

                    _order = order;
                }

                if (cols == 1)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        _settings.AddSetting(settings[i, _order[0] - 1], defaultValue, settings[i, _order[0] - 1], defaultParent);
                    }
                }
                else if (cols == 2)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        _settings.AddSetting(settings[i, _order[0] - 1], defaultValue, settings[i, _order[1] - 1], defaultParent);
                    }
                }
                else if (cols == 3)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        _settings.AddSetting(settings[i, _order[0] - 1], settings[i, _order[1] - 1], settings[i, _order[0] - 1], defaultParent);
                    }
                }
                else if (cols == 4)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        _settings.AddSetting(settings[i, _order[0] - 1], settings[i, _order[1] - 1], settings[i, _order[2] - 1], settings[i, _order[3] - 1]);
                    }
                }
                else if (cols == 5)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        _settings.AddSetting(settings[i, _order[0] - 1], settings[i, _order[1] - 1], settings[i, _order[2] - 1], settings[i, _order[3] - 1]);

                        string newTooltip = settings[i, _order[4] - 1];
                        if (!string.IsNullOrEmpty(newTooltip))
                            _settings.Settings[settings[i, _order[0] - 1]].ToolTip = newTooltip;
                    }
                }
            }
            catch { }
        }
	}
}