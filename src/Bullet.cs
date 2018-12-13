using System;
using System.Reflection;
using SwinGameSDK;
using Color = System.Drawing.Color;
using System.Collections.Generic;

namespace MyGame
{
	public class Bullet : Entity
	{
		private int _damage;
		private SwinGameSDK.Vector _direction;
		private Player _origin;
		private List<Entity> _collideAgainst;

		public Bullet (int x, int y, int damage, SwinGameSDK.Vector direction, Player origin, List<Entity> collideAgainst) : base(x, y, 1)
		{
			_collideAgainst = collideAgainst;
			_damage = damage;
			_direction = direction;
			_origin = origin;
		}

		public override void Main ()
		{
			_position.X += _direction.X;
			_position.Y += _direction.Y;
			if(_position.X < 0 || _position.Y < 0 || _position.X > SwinGame.ScreenWidth() || _position.Y > SwinGame.ScreenHeight())
			{
				_toRemove = true;
			}

			foreach(Entity e in _collideAgainst)
			{
				if(SwinGame.PointInRect(_position, e.GetHitBox()) && !e.Equals(_origin))
				{
					e.BulletCollision(this);
				}
			}
		}

		public int GetDamage()
		{
			return _damage;
		}

		public override void BulletCollision (Bullet b)
		{
			
		}

		public override void Draw ()
		{
			SwinGame.DrawLine(Color.Yellow, SwinGame.LineFromVector(_position.X, _position.Y, _direction));
			//SwinGame.DrawCircle(Color.Yellow, _position, 3);
		}
	}
}

