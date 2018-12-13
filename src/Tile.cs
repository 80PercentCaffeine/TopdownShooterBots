using System;
using System.Reflection;
using SwinGameSDK;
using Color = System.Drawing.Color;

namespace MyGame
{
	public class Tile:Entity
	{
		private TileType _type;
		private Color _colour;
		private Color _original_colour;

		public Tile (TileType type, int x, int y, int size) : base (x, y, size)
		{
			_type = type;
			if(_type == TileType.EMPTY)
			{
				_colour = Color.Black;
			}
			else
			{
				_colour = Color.Gray;
			}

			_original_colour = _colour;

			_hitBox = SwinGame.CreateRectangle((Single)_position.X, (Single)_position.Y, _size, _size);
		}

		public void ChangeTile(TileType Unknown)
		{
			if(Unknown == TileType.UNKNOWN)
			{
				_type = TileType.UNKNOWN;
			}
			else if(_type == TileType.EMPTY)
			{
				_type = TileType.WALL;
			}
			else
			{
				_type = TileType.EMPTY;
			}
		}

		public void TakeColour(Color toTake)
		{
			int r = _colour.R;
			int g = _colour.G;
			int b = _colour.B;

			if(toTake.R > 0)
			{
				r = _original_colour.R;
			}
			if(toTake.G > 0)
			{
				g = _original_colour.G;
			}
			if(toTake.B > 0)
			{
				b = _original_colour.B;
			}

			_colour = SwinGame.RGBColor((byte)r, (byte)g, (byte)b);
		}

		public void AddColour(Color toAdd)
		{
			if(_type != TileType.EMPTY)
			{
				int r = toAdd.R + _colour.R;
				int g = toAdd.G + _colour.G;
				int b = toAdd.B + _colour.B;

				if(r > 255)
				{
					r = 255;
				}
				else if(r < 0)
				{
					r = 0;
				}

				if(g > 255)
				{
					g = 255;
				}
				else if(g < 0)
				{
					g = 0;
				}

				if(b > 255)
				{
					b = 255;
				}
				else if(b < 0)
				{
					b = 0;
				}

				_colour = SwinGame.RGBAColor((byte)r,(byte)g,(byte)b,255);
			}
			if(_type == TileType.EMPTY)
			{
				//_colour = SwinGame.RGBColor(20,20,20);
			}
		}

		public TileType GetTileType()
		{
			return _type;
		}

		public override void Draw ()
		{
			SwinGame.FillRectangle(_colour, _position.X, _position.Y, _size, _size);
		}

		public override int GetX()
		{
			return (int)(_position.X + _size/2);
		}

		public override int GetY()
		{
			return (int)(_position.Y + _size/2);
		}

		public override SwinGameSDK.Point2D GetPos()
		{
			SwinGameSDK.Point2D toReturn = new Point2D();
			toReturn.X = _position.X + (_size/2);
			toReturn.Y = _position.Y + (_size/2);
			return toReturn;
		}

		public override void BulletCollision(Bullet b)
		{
			if(_type == TileType.WALL)
			{
				b.SetToRemove();
			}
		}
	}
}

