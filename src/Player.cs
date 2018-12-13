using System;
using System.Reflection;
using SwinGameSDK;
using Color = System.Drawing.Color;
using System.Collections.Generic;

namespace MyGame
{
	//For the player's finite state machine.
	public enum State
	{
		AIMING,
		SHOOTING,
		SEEKING_HIDING,
		HIDING,
		RELOADING,
		PURSUING_ENEMY,
		ON_LAST_SEEN,
		SEEKING_ENEMY,
		FINDING_HIDING,
		DEAD
	}

	public class Player : Entity
	{
		private State _state;
		private Player _enemy;
		private float _angleFacing;
		private SwinGameSDK.Vector _movementVector;
		private int _fov;
		private int _reloadSpeed;
		private int _maxAmmo;
		private int _ammo;
		private int _health;
		private int _damage;
		private int _firerate;
		private int _bulletSpread;
		private int _speed;
		private int _rotateSpeed;
		private int _timer;
		private SwinGameSDK.Point2D _enLastSeen;
		private SwinGameSDK.Vector _enLastHeading;
		private SwinGameSDK.Point2D _destination;
		private Color _colour;
		private Tile[,] _knownTiles;
		private Tile[,] _allTiles;
		private ForceMap _movementManager;
		private List<Entity> _toAdd;
		private List<Entity> _refToEnts;
		private bool _selected;
		private bool _ignoreEnemy;
		private bool _predictMovement;

		public Player (int fov, int reloadSpeed, int maxAmmo, int health, int damage, int firerate, int bulletSpread, int speed, int rotateSpeed, int posX, int posY, Color colour, Tile[,] tiles, List<Entity> refToEnts) : base(posX, posY, 20)
		{
			_state = State.SEEKING_ENEMY;
			_enemy = null;
			_angleFacing = 0;
			_movementVector = new SwinGameSDK.Vector();
			_fov = fov;
			_reloadSpeed = reloadSpeed;
			_maxAmmo = maxAmmo;
			_ammo = maxAmmo;
			_health = health;
			_damage = damage;
			_firerate = firerate;
			_bulletSpread = bulletSpread;
			_speed = speed;
			_rotateSpeed = rotateSpeed;
			_enLastSeen.X = 700;
			_enLastSeen.Y = 700;
			_colour = colour;
			_destination.X = 700;
			_destination.Y = 700;
			_allTiles = tiles;
			_movementManager = new ForceMap();
			_ignoreEnemy = false;
			_selected = false;
			_toAdd = new List<Entity>();
			_refToEnts = refToEnts;
			_timer = 0;
			_predictMovement = true;

			_knownTiles = new Tile[Game.x_width,Game.y_height];
			for(int i = 0; i < Game.x_width; i ++)
			{
				for(int j = 0; j < Game.y_height; j ++)
				{
					_knownTiles[i,j] = new Tile(TileType.UNKNOWN, i*60, j*60, 60);
				}
			}
		}

		public void UpdateTile(int x, int y, Tile tile)
		{
			_knownTiles[x,y].ChangeTile(TileType.UNKNOWN);
			_movementManager.NeedsUpdating();
		}

		public void SetEnemy(Player enemy)
		{
			_enemy = enemy;
			_enLastSeen.X = -404;
			_enLastSeen.Y = -404;
			_enLastHeading = _enemy.GetCurrentVelocity();
		}

		public bool AtPoint(Point2D toCheck)
		{
			if(SwinGame.PointInCircle(toCheck, _position.X, _position.Y, 20))
			{
				return true;
			}
			return false;
		}

		public void AcceptInput()
		{
			if(SwinGame.KeyTyped(KeyCode.vk_i))
			{
				if(_selected)
				{
					_ignoreEnemy = !_ignoreEnemy;
				}
			}

			if(SwinGame.KeyTyped(KeyCode.vk_a))
			{
				if(_selected)
				{
					for(int i = 0; i < Game.x_width; i ++)
					{
						for(int j = 0; j < Game.y_height; j ++)
						{
							if(!_knownTiles[i,j].Equals(_allTiles[i,j]))
							{
								_knownTiles[i,j] = _allTiles[i,j];
								_knownTiles[i,j].AddColour(_colour);
							}
						}

					}
					_movementManager.NeedsUpdating();
				}

				for(int i = 0; i < Game.x_width; i ++)
				{
					for(int j = 0; j < Game.y_height; j ++)
					{
						_knownTiles[i,j].TakeColour(_colour);
						_knownTiles[i,j].AddColour(_colour);
					}
				}
			}

			if(SwinGame.KeyTyped(KeyCode.vk_f))
			{
				if(_selected)
				{
					for(int i = 0; i < Game.x_width; i ++)
					{
						for(int j = 0; j < Game.y_height; j ++)
						{
							_knownTiles[i,j].TakeColour(_colour);
							_knownTiles[i,j] = new Tile(TileType.UNKNOWN, i*60, j*60, 60);
						}
					}
				}
			}

			if(SwinGame.KeyTyped(KeyCode.vk_m))
			{
				_predictMovement = !_predictMovement;
			}

			_movementManager.GetInput(_selected);
		}

		public List<Entity> GetToAdd()
		{
			List<Entity> toReturn = new List<Entity>();
			foreach(Entity e in _toAdd)
			{
				toReturn.Add(e);
			}
			_toAdd = new List<Entity>();
			return toReturn;
		}

		public override void Main()
		{
			if(_state != State.DEAD)
				{
				_movementVector = _movementManager.GetMovementVector(_position, _knownTiles, _movementVector, _destination, _selected);
				
				_position.X += _movementVector.X;
				_position.Y += _movementVector.Y;
				
				if(_position.X < _size)
				{
					_position.X = 0 + _size;
				}
				else if(_position.X > SwinGame.ScreenWidth() - _size)
				{
					_position.X = SwinGame.ScreenWidth() - _size;
				}
				
				if(_position.Y < _size)
				{
					_position.Y = 0 + _size;
				}
				else if(_position.Y > SwinGame.ScreenHeight() - _size)
				{
					_position.Y = SwinGame.ScreenHeight() - _size;
				}
			}

			TileCheck();

			if(_enLastSeen.X != -404)
			{
				if(_enLastSeen.X + _enLastHeading.X > 0 && _enLastSeen.X + _enLastHeading.X < SwinGame.ScreenWidth())
				{
					if(_enLastSeen.Y + _enLastHeading.Y > 0 && _enLastSeen.Y + _enLastHeading.Y < SwinGame.ScreenHeight())
					{
						if( _allTiles[(int)Math.Floor((_enLastSeen.X + _enLastHeading.X) / 60), (int)Math.Floor((_enLastSeen.Y + _enLastHeading.Y) / 60)].GetTileType() != TileType.WALL)
						{
							_enLastSeen.X += _enLastHeading.X;
							_enLastSeen.Y += _enLastHeading.Y;
						}
					}
				}
			}

			if(!_ignoreEnemy)
			{
				if (_state == State.AIMING)
				{
					Aiming();
				}
				else if (_state == State.DEAD)
				{
					Dead();
				}
				else if (_state == State.HIDING)
				{
					Hiding();
				}
				else if (_state == State.ON_LAST_SEEN)
				{
					OnLastSeen();
				}
				else if (_state == State.PURSUING_ENEMY)
				{
					PursuingEnemy();
				}
				else if (_state == State.RELOADING)
				{
					Reloading();
				}
				else if (_state == State.SEEKING_ENEMY)
				{
					SeekingEnemy();
				}
				else if (_state == State.SEEKING_HIDING)
				{
					SeekingHiding();
				}
				else if (_state == State.FINDING_HIDING)
				{
					FindingHiding();
				}
				else if (_state == State.SHOOTING)
				{
					Shooting();
				}
			}

			if(_health <= 0)
			{
				_state = State.DEAD;
			}

			if(CanSee(_enemy.GetPos(), false))
			{
				_enLastSeen = _enemy.GetPos();
				_enLastHeading = _enemy.GetCurrentVelocity();
			}
		}

		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				_selected = value;
			}
		}
			
		private void TileCheck()
		{
			for(int i = 0; i < Game.x_width; i ++)
			{
				for(int j = 0; j < Game.y_height; j ++)
				{
					if(_knownTiles[i,j].GetTileType() == TileType.UNKNOWN)
					{
						Point2D tempX = _allTiles[i,j].GetPos();
						Point2D tempY = _allTiles[i,j].GetPos();

						if(_position.X < _allTiles[i,j].GetPos().X)
						{
							tempX.X -= 30;
						}
						else
						{
							tempX.X += 30;
						}

						if(_position.Y < _allTiles[i,j].GetPos().Y)
						{
							tempY.Y -= 30;
						}
						else
						{
							tempY.Y += 30;
						}

						if(CanSee(tempX, true) || CanSee(tempY, true))
						{
							_knownTiles[i,j] = _allTiles[i,j];
							_knownTiles[i,j].AddColour(_colour);
							_movementManager.NeedsUpdating();
						}
					}
				}
			}
		}
			
		////////////////////////////////////////////////////////////////////////////////////////////////////

		private void Aiming()
		{
			if(AimingAt(_enemy.GetX(), _enemy.GetY()))
			{
				_state = State.SHOOTING;
			}

			if(!CanSee(_enemy.GetPos(), false))
			{
				_state = State.PURSUING_ENEMY;
			}

		}

		private void Shooting()
		{
			AimingAt(_enemy.GetX(), _enemy.GetY());
			Random r = new Random();
			for(int i = 0; i < _firerate; i ++)
			{
				_toAdd.Add(new Bullet((int)_position.X, (int)_position.Y, 1, SwinGame.VectorFromAngle(_angleFacing + (r.Next(_bulletSpread) - _bulletSpread/2), 20), this, _refToEnts));
				_ammo--;
			}

			if(!CanSee(_enemy.GetPos(), false))
			{
				if( _ammo > _maxAmmo/4)
				{
					_state = State.PURSUING_ENEMY;
				}
				else _state = State.SEEKING_HIDING;
			}

			if (_ammo <= 0)
			{
				_state = State.SEEKING_HIDING;
			}
		}

		private void FindingHiding()
		{
			//find hiding spot
			//SetDest(hidingpt);
			_state = State.SEEKING_HIDING;
		}

		private void SeekingHiding()
		{
			AimingAt((int)(_position.X + _movementVector.X), (int)(_position.Y + _movementVector.Y));

			_state = State.HIDING;
		}

		private void Hiding()
		{
			_state = State.RELOADING;
		}

		private void Reloading()
		{
			_ammo += _reloadSpeed;
			if (_ammo >= _maxAmmo)
			{
				_ammo = _maxAmmo;
				_state = State.PURSUING_ENEMY;
			}
		}

		private void PursuingEnemy()
		{
			AimingAt((int)(_position.X + _movementVector.X), (int)(_position.Y + _movementVector.Y));

			SetDest(_enLastSeen);

			if ((_position.X <= _enLastSeen.X + 60) && (_position.X >= _enLastSeen.X - 60) && (_position.Y <= _enLastSeen.Y + 60) && (_position.Y >= _enLastSeen.Y - 60))
			{
				_timer = (int)Math.Floor( (double)(360 / _rotateSpeed) );
				_state = State.ON_LAST_SEEN;
			}
			if (CanSee(_enemy.GetPos(), false))
			{
				_state  = State.AIMING;
				_destination.X = _position.X;
				_destination.Y = _position.Y;
				_enLastHeading = _enemy.GetCurrentVelocity();
			}
			if (_enLastSeen.X == -404)
			{
				_state = State.SEEKING_ENEMY;
			}
		}

		private void OnLastSeen()
		{
			_timer --;

			if(_timer < (360/_rotateSpeed)/2)
			{
				AimingAt((int)(_position.X + 100), (int)_position.Y);
			}
			else
			{
				AimingAt((int)(_position.X - 100), (int)(_position.Y - 1));
			}

			if(CanSee(_enemy.GetPos(), false))
			{
				_state = State.AIMING;
			}

			if(_timer < 0)
			{
				_state = State.SEEKING_ENEMY;
			}
		}

		private void SeekingEnemy()
		{
			AimingAt((int)(_position.X + _movementVector.X), (int)(_position.Y + _movementVector.Y));
			if (CanSee(_enemy.GetPos(), false))
			{
				_state  = State.AIMING;
			}
		}

		private void Dead()
		{
			_movementVector.LimitToMagnitude(_movementVector.Magnitude/2);
			if(_colour.A > 0)
			{
				_colour = SwinGame.RGBAColor(_colour.R, _colour.G, _colour.B, (byte)(_colour.A - 1));
			}
		}


		////////////////////////////////////////////////////////////////////////////////////////////////////


		///////////////////////////////////////////// FRAME RATE HIT IS IN THE FOLLOWING/////////////////////////////////////////////////////////
		private bool CanSee(SwinGameSDK.Point2D toSee, bool heading)
		{
			// takes the absoloute value of the angle between two vectors. The one between the player and what they're trying to see and the vector in the direction the player is facing.
			// It then checks if this value is less than half the player's fov. This will check if the object is within the player's cone of vision.
			//////////////////////////////////// DROPPED FROM 50 TO 20 WHEN CHANGED FROM CALCANGLE(pt, pt) TO CALCANGLE(x,y,x,y)//////////////////////////////////////////////////////
			float temp = 0;
			if(heading)
			{
				temp = Math.Abs( SwinGame.CalculateAngle( SwinGame.VectorFromAngle(SwinGame.CalculateAngle(_position.X, _position.Y, toSee.X, toSee.Y), 2000), SwinGame.VectorFromAngle(_movementVector.Angle, 2000)));
			}
			else
			{
				temp = Math.Abs( SwinGame.CalculateAngle( SwinGame.VectorFromAngle(SwinGame.CalculateAngle(_position.X, _position.Y, toSee.X, toSee.Y), 2000), SwinGame.VectorFromAngle(_angleFacing, 2000)));
			}

			if (temp  < (_fov/2) )
			{
				bool result = true;
				int minX = 0;
				int minY = 0;
				int maxX = 0;
				int maxY = 0;

				if(toSee.X < _position.X)
				{
					maxX = (int)Math.Floor( _position.X/60 );
					minX = (int)Math.Floor( toSee.X/60 );
				}
				else
				{
					minX = (int)Math.Floor( _position.X/60 );
					maxX = (int)Math.Floor( toSee.X/60 );
				}

				if(toSee.Y < _position.Y)
				{
					maxY = (int)Math.Floor( _position.Y/60 );
					minY = (int)Math.Floor( toSee.Y/60 );
				}
				else
				{
					minY = (int)Math.Floor( _position.Y/60 );
					maxY = (int)Math.Floor( toSee.Y/60 );
				}
					
				for(int i = minX - 1; i < maxX + 1; i ++)
				{
					for(int j = minY - 1; j < maxY + 1; j ++)
					{
						if(i > 0 && j > 0 && i < Game.x_width && j < Game.y_height)
						{
							if(_allTiles[i,j].GetTileType() == TileType.WALL && !SwinGame.PointInRect(toSee, _allTiles[i,j].GetHitBox()) && SwinGame.LineIntersectsRect(SwinGame.LineFrom(_position, toSee), _allTiles[i,j].GetHitBox()))
							{
								result = false;
							}
						}
					}
				}
				return result;
			}
			return false;
		}

		private bool AimingAt(int x, int y)
		{
			if( Math.Abs(Math.Round(SwinGame.CalculateAngle( SwinGame.VectorFromAngle(SwinGame.CalculateAngle(_position.X, _position.Y, x, y), 2000), SwinGame.VectorFromAngle(_angleFacing, 2000) ))) < _rotateSpeed)
			{
				_angleFacing = SwinGame.CalculateAngle(_position.X, _position.Y, x, y);
				return true;
			}
			else if( SwinGame.CalculateAngle( SwinGame.VectorFromAngle(SwinGame.CalculateAngle(_position.X, _position.Y, x, y), 2000), SwinGame.VectorFromAngle(_angleFacing, 2000) ) > 0)
			{
				_angleFacing -= _rotateSpeed;
				//if( Math.Abs( SwinGame.CalculateAngle(_position.X, _position.Y, x, y) ) <= _rotateSpeed)
				//{
				//	_angleFacing = SwinGame.CalculateAngle(_position.X, _position.Y, x, y);
				//	return true;
				//}
			}
			else
			{
				_angleFacing += _rotateSpeed;
				//if( Math.Abs( SwinGame.CalculateAngle(_position.X, _position.Y, x, y) ) <= _rotateSpeed)
				//{
				//	_angleFacing = SwinGame.CalculateAngle(_position.X, _position.Y, x, y);
				//	return true;
				//}
			}

			if(_angleFacing < 0)
			{
				_angleFacing = _angleFacing + 360;
			}
			else if(_angleFacing > 360)
			{
				_angleFacing = _angleFacing - 360;
			}

			return false;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////////


		public override void Draw()
		{
			SwinGame.FillEllipse(_colour, _position.X - (_size/2), _position.Y - (_size/2), _size, _size);

			if(_selected)
			{
				SwinGame.DrawEllipse(Color.White, _position.X - (_size/2), _position.Y - (_size/2), _size, _size);
			}

			SwinGame.DrawLine(_colour, SwinGame.LineFromVector(_position.X, _position.Y, SwinGame.VectorFromAngle(_angleFacing, 100)));
			SwinGame.DrawLine(_colour, SwinGame.LineFromVector(_position.X, _position.Y, SwinGame.VectorFromAngle(_angleFacing + (_fov/2), 100)));
			SwinGame.DrawLine(_colour, SwinGame.LineFromVector(_position.X, _position.Y, SwinGame.VectorFromAngle(_angleFacing - (_fov/2), 100)));

			//SwinGame.DrawText(SwinGame.VectorAngle(_movementVector) + "", Color.White, _position.X, _position.Y - 50);
			//SwinGame.DrawText(_movementVector.Magnitude + "", Color.White, _position.X, _position.Y - 10);

			SwinGame.DrawLine(Color.White, _position.X, _position.Y, _movementVector.X + _position.X, _movementVector.Y + _position.Y);

			SwinGame.DrawLine(Color.White, _position.X, _position.Y, (_movementVector.X*7) + _position.X, (_movementVector.Y*7) + _position.Y);

			float normalX = (SwinGame.VectorNormal(_movementVector).X * (12) );
			float normalY = (SwinGame.VectorNormal(_movementVector).Y * (12) );
			SwinGame.DrawLine(Color.White, _position.X + normalX, _position.Y + normalY, (_movementVector.X*7) + _position.X + normalX, (_movementVector.Y*7) + _position.Y + normalY);
			SwinGame.DrawLine(Color.White, _position.X - normalX, _position.Y - normalY, (_movementVector.X*7) + _position.X - normalX, (_movementVector.Y*7) + _position.Y - normalY);

			SwinGame.DrawLine(_colour, _destination.X - 3, _destination.Y, _destination.X + 3, _destination.Y);
			SwinGame.DrawLine(_colour, _destination.X, _destination.Y - 3, _destination.X, _destination.Y + 3);

			Color temp = SwinGame.RGBAColor(_colour.R, _colour.G, _colour.B, _colour.A);
			SwinGame.DrawCircle(temp, _enLastSeen, _size);

			SwinGame.DrawText(_health + "", Color.White, _position);

			//_movementManager.Draw(_colour);
		}

		public List<string> GetDisplay()
		{
			List<string> toReturn = new List<string>();

			string temp = "State: ";

			if(_state == State.AIMING)
			{
				temp += "Aiming";
			}
			else if(_state == State.DEAD)
			{
				temp += "Dead";
			}
			else if(_state == State.HIDING)
			{
				temp += "Hiding";
			}
			else if(_state == State.ON_LAST_SEEN)
			{
				temp += "On last seen";
			}
			else if(_state == State.PURSUING_ENEMY)
			{
				temp += "Pursuing enemy";
			}
			else if(_state == State.RELOADING)
			{
				temp += "Reloading";
			}
			else if(_state == State.SEEKING_ENEMY)
			{
				temp += "Seeking enemy";
			}
			else if(_state == State.SEEKING_HIDING)
			{
				temp += "Seeking hiding";
			}
			else if(_state == State.SHOOTING)
			{
				temp += "Shooting";
			}
			toReturn.Add(temp);

			temp = "User controlled (c): ";
			if(_movementManager.getInControl())
			{
				temp += "True";
			}
			else
			{
				temp += "False";
			}
			toReturn.Add(temp);

			temp = "Maintains velocity (v): ";
			if(_movementManager.getVelocity())
			{
				temp += "True";
			}
			else
			{
				temp += "False";
			}
			toReturn.Add(temp);

			temp = "Ignoring enemy (i): ";
			if(_ignoreEnemy)
			{
				temp += "True";
			}
			else
			{
				temp += "False";
			}
			toReturn.Add(temp);

			return toReturn;
		}

		public State GetState()
		{
			return _state;
		}

		public SwinGameSDK.Vector GetCurrentVelocity()
		{
			return _movementVector;
		}

		public void SetDest(SwinGameSDK.Point2D dest)
		{
			if(_destination.X != dest.X || _destination.Y != dest.Y)
			{
				_destination = dest;
				_movementManager.NeedsUpdating();
			}
		}

		public override Rectangle GetHitBox()
		{
			_hitBox = SwinGame.RectangleFrom(_position.X - _size/2, _position.Y - _size/2, _size, _size);
			return _hitBox;
		}
			
		public override void BulletCollision (Bullet b)
		{
			b.SetToRemove();
			_health -= b.GetDamage();
			if(_health < 1)
			{
				_state = State.DEAD;
			}
			if(_state == State.SEEKING_ENEMY)
			{
				_state = State.PURSUING_ENEMY;
			}
			_enLastSeen = _enemy.GetPos();
			_enLastHeading = _enemy.GetCurrentVelocity();
		}

	}
}

