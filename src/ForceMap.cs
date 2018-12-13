using System;
using System.Reflection;
using SwinGameSDK;
using Color = System.Drawing.Color;

namespace MyGame
{
	public class ForceMap
	{
		SwinGameSDK.Vector[,] _forces;
		bool _toUpdate;
		private bool _velocity;
		private bool _inControl;

		public ForceMap()
		{
			_toUpdate = true;
			_velocity = true;
			_inControl = false;
		}

		public void NeedsUpdating()
		{
			_toUpdate = true;
		}

		public bool getVelocity()
		{
			return _velocity;
		}

		public bool getInControl()
		{
			return _inControl;
		}

		public void GetInput(bool selected)
		{
			if(SwinGame.KeyTyped(KeyCode.vk_v) && selected)
			{
				_velocity = !_velocity;
			}
			if(SwinGame.KeyTyped(KeyCode.vk_c) && selected)
			{
				_inControl = !_inControl;
				_toUpdate = true;
			}
		}

		public SwinGameSDK.Vector GetMovementVector(SwinGameSDK.Point2D playerPos, Tile[,] tileSet, SwinGameSDK.Vector currentMoveVector, Point2D destination, bool selected)
		{
			if(currentMoveVector.Magnitude > 8) 
			{
				currentMoveVector = SwinGame.LimitVector(currentMoveVector, 8);
			}

			if(!_velocity)
			{
				currentMoveVector = new SwinGameSDK.Vector();
			}

			currentMoveVector = SwinGame.AddVectors(currentMoveVector, GetPathVector(playerPos, tileSet, destination, selected));

			currentMoveVector = SwinGame.AddVectors(currentMoveVector, GetDistractionVector(playerPos, tileSet, currentMoveVector));

			if(SwinGame.PointPointDistance(playerPos, destination) < 120)
			{
				if(SwinGame.PointPointDistance(playerPos, destination) < 60)
				{
					if(currentMoveVector.Magnitude < 2)
					{
						return SwinGame.VectorFromAngle(currentMoveVector.Angle, 0);
					}
					else
					{
						return SwinGame.LimitVector(currentMoveVector, (float)(currentMoveVector.Magnitude * 0.7));
						//////////////////////////FIIIIIIIIIIIIIIIIX
						/// 
					}
				}
				// FIX FIX FIX FIX FIX
				return SwinGame.LimitVector(currentMoveVector, (float)(currentMoveVector.Magnitude * 0.9));
			}

			return currentMoveVector;
		}

		private SwinGameSDK.Vector CurrentForce( SwinGameSDK.Point2D playerPos, Tile[,] tileSet)
		{
			int toReturnX = (int) Math.Floor( playerPos.X / 60);
			int toReturnY = (int) Math.Floor( playerPos.Y / 60);

			if(toReturnX < 0 || toReturnX > Game.x_width - 1 || toReturnY < 0 || toReturnY > Game.y_height - 1)
			{
				return SwinGame.VectorFromAngle(0,0);
			}

			return _forces[toReturnX, toReturnY];
		}

		private SwinGameSDK.Vector GetDistractionVector(SwinGameSDK.Point2D playerPos, Tile[,] tileSet, SwinGameSDK.Vector currentMoveVector)
		{
			SwinGameSDK.Vector distractionVector = new SwinGameSDK.Vector();
			SwinGameSDK.Point2D max = new Point2D();
			SwinGameSDK.Point2D min = new Point2D();


			if(currentMoveVector.Angle <= 0)
			{
				min.Y = playerPos.Y + (currentMoveVector.Y*7);
				max.Y = playerPos.Y;
			}
			else
			{
				max.Y = playerPos.Y - (currentMoveVector.Y*7);
				min.Y = playerPos.Y;
			}

			if(currentMoveVector.Angle < 90 && currentMoveVector.Angle > -90)
			{
				max.X = playerPos.X + (currentMoveVector.X*7);
				min.X = playerPos.X;
			}
			else
			{
				min.X = playerPos.X - (currentMoveVector.X*7);
				max.X = playerPos.X;
			}

			float normalX = SwinGame.VectorNormal(currentMoveVector).X * (12);
			float normalY = SwinGame.VectorNormal(currentMoveVector).Y * (12);

			int iMax = (int)(Math.Ceiling(max.X/60) + 1);
			int iMin = (int)(Math.Floor(min.X/60) - 1);
			int jMax = (int)(Math.Ceiling(max.Y/60) + 1);
			int jMin = (int)(Math.Floor(min.Y/60) - 1);

			if(iMax > Game.x_width - 1)
			{
				iMax = Game.x_width -1;
			}
			if(iMin < 0)
			{
				iMin = 0;
			}
			if(jMax > Game.y_height - 1)
			{
				jMax = Game.y_height - 1;
			}
			if(jMin < 0)
			{
				jMin = 0;
			}

			//loops through each square in the min/max of the rect fov.
			for(int i = iMin; i < iMax; i++)
			{
				for(int j = jMin; j < jMax; j++)
				{
					if(tileSet[i,j].GetTileType() == TileType.WALL)
					{
						// I'm so sorry. It's so ugly.
						if
							(
								SwinGame.LineIntersectsRect(SwinGame.LineFrom( playerPos.X + normalX, playerPos.Y + normalY, playerPos.X + (currentMoveVector.X * 7) + normalX, playerPos.Y + (currentMoveVector.Y * 7) + normalY), tileSet[i,j].GetHitBox())
								||
								SwinGame.LineIntersectsRect(SwinGame.LineFrom( playerPos.X - normalX, playerPos.Y - normalY, playerPos.X + (currentMoveVector.X * 7) - normalX, playerPos.Y + (currentMoveVector.Y * 7) - normalY), tileSet[i,j].GetHitBox())
							)
						{
							SwinGameSDK.Vector thisDistraction = new SwinGameSDK.Vector();

							float distance = SwinGame.PointPointDistance(playerPos, tileSet[i,j].GetPos());
							if(distance < 0)
							{
								distance = 0;
							}

							thisDistraction = SwinGame.VectorFromAngle( SwinGame.CalculateAngleBetween( playerPos, tileSet[i,j].GetPos() ), distance);

							distractionVector = SwinGame.AddVectors(distractionVector, SwinGame.InvertVector(thisDistraction));
						}
					}
				}
			}

			if(distractionVector.Magnitude > 3) 
			{
				distractionVector = SwinGame.LimitVector(distractionVector, 3);
			}

			return distractionVector;
		}

		private SwinGameSDK.Vector GetPathVector(SwinGameSDK.Point2D playerPos, Tile[,] tileSet, Point2D destination, bool selected)
		{
			SwinGameSDK.Vector pathVector = new SwinGameSDK.Vector();

			if(_inControl)
			{
				///////////////////////ALLOWS CONTROL OF PLAYERS////////////////////////////////
				if(SwinGame.KeyDown(KeyCode.vk_RIGHT) && selected)
				{
					pathVector = SwinGame.AddVectors(pathVector, SwinGame.CreateVectorFromAngle(0, 8));
				}
				if(SwinGame.KeyDown(KeyCode.vk_DOWN) && selected)
				{
					pathVector = SwinGame.AddVectors(pathVector, SwinGame.CreateVectorFromAngle(90, 8));
				}
				if(SwinGame.KeyDown(KeyCode.vk_LEFT) && selected)
				{
					pathVector = SwinGame.AddVectors(pathVector, SwinGame.CreateVectorFromAngle(180, 8));
				}
				if(SwinGame.KeyDown(KeyCode.vk_UP) && selected)
				{
					pathVector = SwinGame.AddVectors(pathVector, SwinGame.CreateVectorFromAngle(-90, 8));
				}
				///////////////////////ALLOWS CONTROL OF PLAYERS////////////////////////////////
			}
			else
			{
				if(_toUpdate)
				{
					_forces = AStarPathfinder.CalculateForces(playerPos, tileSet, destination);
					_toUpdate = false;
					pathVector = CurrentForce(playerPos, tileSet);
				}
				else
				{
					pathVector = CurrentForce(playerPos, tileSet);
				}
			}
				
			if(pathVector.Magnitude > 3) 
			{
				pathVector = SwinGame.LimitVector(pathVector, 3);
			}

			return pathVector;
		}



		public void Draw(Color colour)
		{
			if(!_inControl)
			{
				for(int i = 0; i < 32; i ++)
				{
					for(int j = 0; j < 18; j ++)
					{
						if(_forces[i,j].Magnitude != 0)
						{
							SwinGame.DrawCircle(colour, i*60 + 30, j*60 + 30, 2);
							SwinGame.DrawLine(colour, i*60 + 30, j*60 + 30, _forces[i,j].X + (i*60) + 30, _forces[i,j].Y + (j*60) + 30);
						}
					}
				}
			}
		}
	}
}