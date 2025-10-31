using LiveSplit.ASL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class Main : MainShared
{
	public class ScriptSettings
	{
        public void CreateCustom(dynamic[,] settings, params int[] order)
        {
            try
            {
                Create(settings, order);
            }
            catch { }
        }

        public void Create(dynamic[,] settings, params int[] order)
		{
			try
			{
				int rows = settings.GetLength(0);
				int cols = settings.GetLength(1);

				if (cols != 4 && cols != 3 && cols != 5)
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

                if (cols == 3)
				{
                    for (int i = 0; i < rows; i++)
					{
                        _ScriptSettings.AddSetting(settings[i, _order[0] - 1], settings[i, _order[1] - 1], settings[i, _order[2] - 1], null);
                    }
                }
				else if (cols == 4)
				{
                    for (int i = 0; i < rows; i++)
                    {
                        _ScriptSettings.AddSetting(settings[i, _order[0] - 1], settings[i, _order[1] - 1], settings[i, _order[2] - 1], settings[i, _order[3] - 1]);
                    }
                }
                else if (cols == 5)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        _ScriptSettings.AddSetting(settings[i, _order[0] - 1], settings[i, _order[1] - 1], settings[i, _order[2] - 1], settings[i, _order[3] - 1]);

                        string newTooltip = settings[i, _order[4] - 1];
                        if (!string.IsNullOrEmpty(newTooltip))
                            _ScriptSettings.Settings[settings[i, _order[0] - 1]].ToolTip = newTooltip;
                    }
                }
            }
			catch { }
		}
	}
}