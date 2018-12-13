using System;
using System.Reflection;
using SwinGameSDK;
using Color = System.Drawing.Color;
using System.Collections.Generic;

namespace MyGame
{
	public class Drawing
	{
		private Player _player1;
		private Player _player2;
		private List<Entity> _entities;

		public Drawing (Player player1, Player player2, List<Entity> entities)
		{
			_player1 = player1;
			_player2 = player2;
			_entities = entities;
		}

		public void Draw()
		{
			//Fetch the next batch of UI interaction
			SwinGame.ProcessEvents();

			//Clear the screen and draw the framerate
			SwinGame.ClearScreen(Color.Black);
			//SwinGame.DrawFramerate(0,0);

			foreach(Entity e in _entities)
			{
				e.Draw();
			}

			DrawGUI();

			SwinGame.DrawFramerate(0,0);

			//Draw onto the screen
			SwinGame.RefreshScreen(30);
		}

		private void DrawGUI()
		{
			List<string> temp = _player1.GetDisplay();
			for(int i = 0; i < temp.Count; i++)
			{
				SwinGame.DrawText(temp[i], Color.Red, 10, 15 + (i*15));
			}

			temp = _player2.GetDisplay();
			for(int i = 0; i < temp.Count; i++)
			{
				SwinGame.DrawText(temp[i], Color.Blue, 1910 - SwinGame.TextWidth(SwinGame.FontNamed("GUI_text"), temp[i]), 15 + (i*15));
			}
		}
	}
}

