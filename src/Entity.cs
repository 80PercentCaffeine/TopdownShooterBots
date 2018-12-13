using System;
using System.Reflection;
using SwinGameSDK;
using Color = System.Drawing.Color;

namespace MyGame
{
	public abstract class Entity
	{
		protected SwinGameSDK.Point2D _position;
		protected int _size;
		protected SwinGameSDK.Rectangle _hitBox;
		protected bool _toRemove;

		public Entity (int x, int y, int size)
		{
			_position.X = x;
			_position.Y = y;
			_size = size;
			_toRemove = false;
			_hitBox = SwinGame.RectangleFrom(x,y,size,size);
		}
			
		public virtual int GetX()
		{
			return (int)_position.X;
		}

		public virtual int GetY()
		{
			return (int)_position.Y;
		}

		public virtual SwinGameSDK.Point2D GetPos()
		{
			return _position;
		}

		public int GetSize()
		{
			return _size;
		}

		public virtual Rectangle GetHitBox()
		{
			_hitBox = SwinGame.RectangleFrom(_position.X,_position.Y,_size,_size);
			return _hitBox;
		}

		public bool GetToRemove()
		{
			return _toRemove;
		}

		public void SetToRemove()
		{
			_toRemove = true;
		}

		public virtual void BulletCollision(Bullet b)
		{
			b.SetToRemove();
		}

		public virtual void Main()
		{}

		public abstract void Draw();
	}
}

