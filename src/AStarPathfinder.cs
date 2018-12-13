using System;
using System.Reflection;
using SwinGameSDK;
using Color = System.Drawing.Color;
using System.Collections.Generic;

namespace MyGame
{
	public static class AStarPathfinder
	{
		private static bool CanSee(Tile[,] tiles, SwinGameSDK.Point2D toSee, int positionX, int positionY)
		{
			// takes the absoloute value of the angle between two vectors. The one between the player and what they're trying to see and the vector in the direction the player is facing.
			// It then checks if this value is less than half the player's fov. This will check if the object is within the player's cone of vision.
		
			//bool result = true;
			int minX = 0;
			int minY = 0;
			int maxX = 0;
			int maxY = 0;
		
			if(toSee.X < positionX)
			{
				maxX = positionX;
				minX = (int)toSee.X;
			}
			else
			{
				minX = positionX;
				maxX = (int)toSee.X;
			}
		
			if(toSee.Y < positionY)
			{
				maxY = positionY;
				minY = (int)toSee.Y;
			}
			else
			{
				minY = positionY;
				maxY = (int)toSee.Y;
			}
		
			for(int i = minX - 1; i < maxX + 1; i ++)
			{
				for(int j = minY - 1; j < maxY + 1; j ++)
				{
					if(i > 0 && j > 0 && i < Game.x_width && j < 18)
					{
						if(tiles[i,j].GetTileType() == TileType.WALL && !SwinGame.PointInRect(toSee.X*60, toSee.Y*60, tiles[i,j].GetHitBox()) && SwinGame.LineIntersectsRect(SwinGame.LineFrom(positionX*60, positionY*60, toSee.X*60, toSee.Y*60), tiles[i,j].GetHitBox()))
						{
							return false;
						}
					}
				}
				//return result;
			}
			return true;
		}

		private static void ScanTiles(int playerX, int playerY, Tile[,] toTraverse, PathfinderTile[,] pathTiles, int destX, int destY)
		{
			int activeXStart = playerX;
			int activeYStart = playerY;
			int activeXEnd = playerX;
			int activeYEnd = playerY;
			int currentValue = 0;

			pathTiles[playerX, playerY].Active = TileState.ACTIVE;
			pathTiles[playerX, playerY].Value = currentValue;

			while(pathTiles[destX, destY].Active == TileState.NOT_ACTIVATED)
			{
				//for(int i = activeXStart; i < activeXEnd; i++)
				//{
				//	for(int j = activeYStart; j < activeYEnd; j++)
				//	{
				for(int i = 0; i < Game.x_width; i ++)
				{
					for(int j = 0; j < Game.y_height; j ++)
					{
						if(pathTiles[i,j].Active == TileState.ACTIVE)
						{
							if(i-1 >= 0)
							{
								if(pathTiles[i-1, j].Active.Equals(TileState.NOT_ACTIVATED))
								{
									if(toTraverse[i-1, j].GetTileType() == TileType.WALL)
									{
										pathTiles[i-1, j].Active = TileState.NOT_TRAVERSABLE;
									}
									else
									{
										pathTiles[i-1, j].Active = TileState.ACTIVE;
										pathTiles[i-1, j].Value = pathTiles[i,j].Value + 1;

										if( i-1 < activeXStart)
										{
											activeXStart = i-1;
										}
									}
								}
							}
							if(i+1 <= Game.x_width - 1)
							{
								if(pathTiles[i+1, j].Active == TileState.NOT_ACTIVATED)
								{
									if(toTraverse[i+1, j].GetTileType() == TileType.WALL)
									{
										pathTiles[i+1, j].Active = TileState.NOT_TRAVERSABLE;
									}
									else
									{
										pathTiles[i+1, j].Active = TileState.ACTIVE;
										pathTiles[i+1, j].Value = pathTiles[i,j].Value + 1;

										if( i+1 > activeXEnd)
										{
											activeXEnd = i+1;
										}
									}
								}
							}
							if(j-1 >= 0)
							{
								if(pathTiles[i, j-1].Active == TileState.NOT_ACTIVATED)
								{
									if(toTraverse[i, j-1].GetTileType() == TileType.WALL)
									{
										pathTiles[i, j-1].Active = TileState.NOT_TRAVERSABLE;
									}
									else
									{
										pathTiles[i, j-1].Active = TileState.ACTIVE;
										pathTiles[i, j-1].Value = pathTiles[i,j].Value + 1;

										if( j-1 < activeYStart)
										{
											activeYStart = j-1;
										}
									}
								}
							}
							if(j+1 <= Game.y_height - 1)
							{
								if(pathTiles[i, j+1].Active == TileState.NOT_ACTIVATED)
								{
									if(toTraverse[i, j+1].GetTileType() == TileType.WALL)
									{
										pathTiles[i, j+1].Active = TileState.NOT_TRAVERSABLE;
									}
									else
									{
										pathTiles[i, j+1].Active = TileState.ACTIVE;
										pathTiles[i, j+1].Value = pathTiles[i,j].Value + 1;

										if( j+1 > activeYEnd)
										{
											activeYEnd = j+1;
										}
									}
								}
							}

							pathTiles[i,j].Active = TileState.INACTIVE;
						}
					}
				}
			}
		}

		private static void AssignWaypoints(PathfinderTile[,] pathTiles, int destX, int destY, List<PathfinderTile> waypoints, Tile[,] toTraverse)
		{
			///////////////////////////////////////////////////////////////////////////////////////////
			////////////////////////////////////IS STILL BROKEN////////////////////////////////////////
			///////////////////////////////////////////////////////////////////////////////////////////

			pathTiles[destX, destY].SetPos(destX, destY);
			waypoints.Add(pathTiles[destX, destY]);

			int currentValue = pathTiles[destX, destY].Value;
			int currentX = destX;
			int currentY = destY;

			while(currentValue > 0)
			{
				if(currentX-1 >= 0 && pathTiles[currentX-1, currentY].Value < currentValue)
				{
					//if(!CanSee(toTraverse, waypoints[waypoints.Count - 1].getPos(), currentX - 1, currentY))
					//{
						waypoints.Add(pathTiles[currentX,currentY]);
						pathTiles[currentX, currentY].SetPos(currentX, currentY);
					//}

					currentX -= 1;
					currentValue = pathTiles[currentX, currentY].Value;
				}
				else if(currentX+1 <= 31 && pathTiles[currentX+1, currentY].Value < currentValue)
				{
					//if(!CanSee(toTraverse, waypoints[waypoints.Count - 1].getPos(), currentX + 1, currentY))
					//{
						waypoints.Add(pathTiles[currentX,currentY]);
						pathTiles[currentX, currentY].SetPos(currentX, currentY);
					//}

					currentX += 1;
					currentValue = pathTiles[currentX, currentY].Value;
				}
				else if(currentY-1 >= 0 && pathTiles[currentX, currentY - 1].Value < currentValue)
				{
					//if(!CanSee(toTraverse, waypoints[waypoints.Count - 1].getPos(), currentX, currentY - 1))
					//{
						waypoints.Add(pathTiles[currentX,currentY]);
						pathTiles[currentX, currentY].SetPos(currentX, currentY);
					//}

					currentY -= 1;
					currentValue = pathTiles[currentX, currentY].Value;
				}
				else if(currentY+1 <= 17 && pathTiles[currentX, currentY + 1].Value < currentValue)
				{
					//if(!CanSee(toTraverse, waypoints[waypoints.Count - 1].getPos(), currentX, currentY + 1))
					//{
						waypoints.Add(pathTiles[currentX,currentY]);
						pathTiles[currentX, currentY].SetPos(currentX, currentY);
					//}

					currentY += 1;
					currentValue = pathTiles[currentX, currentY].Value;
				}	
			}

			List<PathfinderTile> SetPoints = new List<PathfinderTile>();
			SetPoints.Add(waypoints[waypoints.Count - 1]);

			for(int i = waypoints.Count - 1; i > 0; i--)
			{
				if(CanSee(toTraverse, SetPoints[SetPoints.Count-1].getPos(), (int) Math.Floor( waypoints[i].getPos().X / 60 ), (int) Math.Floor( waypoints[i].getPos().Y ) / 60))
				{

				}
			}
		}

		private static void AssignForces(PathfinderTile[,] pathTiles, int destX, int destY, List<PathfinderTile> waypoints)
		{
			foreach(PathfinderTile way in waypoints)
			{
				for(int i = 0; i < Game.x_width; i ++)
				{
					for(int j = 0; j < Game.y_height; j ++)
					{
						if(pathTiles[i,j].Active != TileState.NOT_ACTIVATED && pathTiles[i,j].Active != TileState.NOT_TRAVERSABLE)
						{
							if (pathTiles[i,j].Value < way.Value)
							{
								Point2D pos = new Point2D();
								pos.X = i;
								pos.Y = j;
								pathTiles[i,j].FaceTowards(pos, way.getPos());
							}
						}
					}
				}
			}
		}

		public static SwinGameSDK.Vector[,] CalculateForces(Point2D current, Tile[,] toTraverse, Point2D destination)
		{
			int playerX = (int) Math.Floor( current.X / 60);
			int playerY = (int) Math.Floor( current.Y / 60);
			int destX = (int) Math.Floor( destination.X / 60);
			int destY = (int) Math.Floor( destination.Y / 60);

			PathfinderTile[,] tiles = new PathfinderTile[Game.x_width,Game.y_height];
			for(int i = 0; i < Game.x_width; i ++)
			{
				for(int j = 0; j < Game.y_height; j ++)
				{
					tiles[i,j] = new PathfinderTile();
				}
			}

			if(playerX < 0 || playerX > Game.x_width - 1 || playerY < 0 || playerY > Game.y_height - 1)
			{
				return new Vector[Game.x_width,Game.y_height];
			}
			if(destX < 0 || destX > Game.x_width - 1 || destY < 0 || destY > Game.y_height - 1)
			{
				return new Vector[Game.x_width,Game.y_height];
			}

			ScanTiles(playerX, playerY, toTraverse, tiles, destX, destY);

			List<PathfinderTile> waypoints = new List<PathfinderTile>();

			AssignWaypoints(tiles, destX, destY, waypoints, toTraverse);

			AssignForces(tiles, destX, destY, waypoints);

			SwinGameSDK.Vector[,] forceVectors = new Vector[Game.x_width,Game.y_height];

			for(int i = 0; i < Game.x_width; i ++)
			{
				for(int j = 0; j < Game.y_height; j ++)
				{
					forceVectors[i,j] = tiles[i,j].Direction;
				}
			}

			return forceVectors;
		}

		public static int GetCost(Point2D current, Tile[,] toTraverse, Point2D destination)
		{
			int playerX = (int) Math.Floor( current.X / 60);
			int playerY = (int) Math.Floor( current.Y / 60);
			int destX = (int) Math.Floor( destination.X / 60);
			int destY = (int) Math.Floor( destination.Y / 60);

			PathfinderTile[,] tiles = new PathfinderTile[Game.x_width,Game.y_height];
			for(int i = 0; i < Game.x_width; i ++)
			{
				for(int j = 0; j < Game.y_height; j ++)
				{
					tiles[i,j] = new PathfinderTile();
				}
			}

			ScanTiles(playerX, playerY, toTraverse, tiles, destX, destY);

			return tiles[destX, destY].Value;
		}
	}
}

