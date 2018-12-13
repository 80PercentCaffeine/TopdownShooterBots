using System;
using System.Reflection;
using SwinGameSDK;
using Color = System.Drawing.Color;
using System.Collections.Generic;

namespace MyGame
{
	public enum TileType
	{
		WALL,
		EMPTY,
		UNKNOWN,
	}

	public class Game
    {
        public const int x_width = 28;
        public const int y_height = 14;

        private Drawing _drawer;
		private Tile[,] _tiles;
		private Player _player1;
		private Player _player2;
		private List<Entity> _entities;
		private List<Player> _players;
		private bool _paused;

        public Game ()
		{
			_entities = new List<Entity>();
			_players = new List<Player>();
			_tiles = GenerateTiles();
			_paused = true;

			_player1 = new Player(90, 1, 100, 500, 1, 1, 5, 2, 5, 100, 30, Color.Red, _tiles, _entities);
			_player2 = new Player(90, 1, 100, 500, 1, 1, 5, 2, 5, 1000, 1050, Color.Blue,  _tiles, _entities);
			_player1.SetEnemy(_player2);
			_player2.SetEnemy(_player1);

			for(int i = 0; i < x_width; i++)
			{
				for(int j = 0; j < y_height; j++)
				{
					_entities.Add(_tiles[i,j]);
				}
			}

			_entities.Add(_player1);
			_entities.Add(_player2);

			_players.Add(_player1);
			_players.Add(_player2);

			_drawer = new Drawing(_player1, _player2, _entities);
		}

		// Grid will be 18X32. size of wall, 60X60
		private Tile[,] GenerateTiles()
		{

            Tile[,] temp = new Tile[x_width, y_height];

			for(int i = 0; i < x_width; i++)
			{
				temp[i,0] = new Tile(TileType.EMPTY, i*60, 0, 60);
			}
		
			for(int i = 0; i < x_width; i += 4)
			{
				for(int j = 1; j < y_height - 2; j += 4)
				{
					temp[i,  j] = new Tile(TileType.WALL, i*60, j*60, 60);
					temp[i+1,j] = new Tile(TileType.EMPTY, (i+1)*60, j*60, 60);
					temp[i+2,j] = new Tile(TileType.WALL, (i+2)*60, j*60, 60);
					temp[i+3,j] = new Tile(TileType.EMPTY, (i+3)*60, j*60, 60);
		
					temp[i,  j+1] = new Tile(TileType.EMPTY, i*60, (j+1)*60, 60);
					temp[i+1,j+1] = new Tile(TileType.EMPTY, (i+1)*60, (j+1)*60, 60);
					temp[i+2,j+1] = new Tile(TileType.WALL, (i+2)*60, (j+1)*60, 60);
					temp[i+3,j+1] = new Tile(TileType.EMPTY, (i+3)*60, (j+1)*60, 60);
		
					temp[i,  j+2] = new Tile(TileType.EMPTY, i*60, (j+2)*60, 60);
					temp[i+1,j+2] = new Tile(TileType.EMPTY, (i+1)*60, (j+2)*60, 60);
					temp[i+2,j+2] = new Tile(TileType.WALL, (i+2)*60, (j+2)*60, 60);
					temp[i+3,j+2] = new Tile(TileType.EMPTY, (i+3)*60, (j+2)*60, 60);
		
					temp[i,  j+3] = new Tile(TileType.EMPTY, i*60, (j+3)*60, 60);
					temp[i+1,j+3] = new Tile(TileType.EMPTY, (i+1)*60, (j+3)*60, 60);
					temp[i+2,j+3] = new Tile(TileType.WALL, (i+2)*60, (j+3)*60, 60);
					temp[i+3,j+3] = new Tile(TileType.EMPTY, (i+3)*60, (j+3)*60, 60);
				}
			}
		
			for(int i = 0; i < x_width; i++)
			{
				temp[i,y_height - 1] = new Tile(TileType.EMPTY, i*60, 1020, 60);
			}

			return temp;
		}

		public void UpdateTile(Point2D pt)
		{
			int x = (int)Math.Floor(pt.X/60);
			int y = (int)Math.Floor(pt.Y/60);

			if(_tiles[x,y].GetTileType() == TileType.EMPTY)
			{
				_entities.Remove(_tiles[x,y]);
				_entities.Remove(_player1);
				_entities.Remove(_player2);
				_tiles[x,y] = new Tile(TileType.WALL, x*60, y*60, 60);
				_entities.Add(_tiles[x,y]);
				_entities.Add(_player1);
				_entities.Add(_player2);
			}
			else
			{
				_entities.Remove(_player1);
				_entities.Remove(_player2);
				_entities.Remove(_tiles[x,y]);
				_tiles[x,y] = new Tile(TileType.EMPTY, x*60, y*60, 60);
				_entities.Add(_tiles[x,y]);
				_entities.Add(_player1);
				_entities.Add(_player2);
			}

			foreach(Player p in _players)
			{
				p.UpdateTile(x,y,_tiles[x,y]);
			}
		}

		public void main()
		{
			foreach(Entity e in _entities)
			{
				e.Main();
			}
			//Run the game loop
			while(!SwinGame.WindowCloseRequested() && !SwinGame.KeyTyped(KeyCode.vk_ESCAPE))
			{
				if(SwinGame.KeyTyped(KeyCode.vk_BACKSPACE))
				{
					SwinGame.ToggleFullScreen();
				}

				if(SwinGame.MouseClicked(MouseButton.LeftButton))
				{
					bool playerSelected = false;
					foreach( Player p in _players)
					{
						if(p.AtPoint(SwinGame.MousePosition()))
						{
							p.Selected = !p.Selected;
							playerSelected = true;
						}
					}

					if(!playerSelected)
					{
						if(_player1.Selected)
						{
							_player1.SetDest(SwinGame.MousePosition());
						}
						else if(!_player2.Selected)
						{
							UpdateTile(SwinGame.MousePosition());
						}
					}

				}
				else if (SwinGame.MouseClicked(MouseButton.RightButton) && _player2.Selected)
				{
					_player2.SetDest(SwinGame.MousePosition());
				}
				if(SwinGame.KeyTyped(KeyCode.vk_p))
				{
					_paused = !_paused;
				}
				if(!_paused)
				{
					List<Entity> toRemove = new List<Entity>();
					foreach(Player p in _players)
					{
						foreach(Entity e in p.GetToAdd())
						{
							_entities.Add(e);
						}
					}
					foreach(Entity e in _entities)
					{
						e.Main();
						if(e.GetToRemove())
						{
							toRemove.Add(e);
						}
					}
					foreach(Entity e in toRemove)
					{
						_entities.Remove(e);
					}
				}
				foreach(Player p in _players)
				{
					if(p.Selected)
					{
						p.AcceptInput();
					}
				}

				_drawer.Draw ();
			}
		}
	}
}

