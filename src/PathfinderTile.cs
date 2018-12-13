using System;
using System.Reflection;
using SwinGameSDK;

namespace MyGame
{
	public enum TileState
	{
		ACTIVE,
		INACTIVE,
		NOT_ACTIVATED,
		NOT_TRAVERSABLE
	}

	public class PathfinderTile
	{
		private TileState _currentState;
		private int _value;
		private SwinGameSDK.Vector _direction;
		private bool _waypoint;
		private Point2D _pos;

		public PathfinderTile ()
		{
			_currentState = TileState.NOT_ACTIVATED;
			_value = 500;
			_direction = new Vector();
			_waypoint = false;
			_pos = new Point2D();
		}

		public void SetPos(float x, float y)
		{
			_pos.X = x;
			_pos.Y = y;
		}

		public Point2D getPos()
		{
			return _pos;
		}

		public TileState Active
		{
			set
			{
				_currentState = value;
			}
			get
			{
				return _currentState;
			}
		}

		public int Value
		{
			set
			{
				_value = value;
			}
			get
			{
				return _value;
			}
		}

		public void FaceTowards(Point2D pos, Point2D nextWaypoint)
		{
			_direction = SwinGame.VectorFromAngle( SwinGame.CalculateAngleBetween(pos, nextWaypoint), 8);
		}

		public SwinGameSDK.Vector Direction
		{
			get
			{
				return _direction;
			}
		}

		public bool Waypoint
		{
			get
			{
				return _waypoint;
			}
			set
			{
				_waypoint = value;
			}
		}
	}
}

