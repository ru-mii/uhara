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
		public void Create(dynamic[,] settings)
		{
			try
			{
				int rows = settings.GetLength(0);
				int cols = settings.GetLength(1);

				if (cols != 4)
				{
					TUtils.Print("Settings could not be parsed!");
					return;
				}

				for (int i = 0; i < rows; i++)
				{
                    MainShared._ScriptSettings.AddSetting(settings[i, 0], settings[i, 1], settings[i, 2], settings[i, 3]);
				}
			}
			catch { }
		}
	}
}