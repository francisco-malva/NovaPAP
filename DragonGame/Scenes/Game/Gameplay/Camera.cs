using DragonGame.Engine.Utilities;
using DragonGame.Engine.Wrappers.SDL2;

namespace DuckDuckJump.Scenes.Game.Gameplay
{
    internal class Camera
    {
        private Point _viewport;
        private Rectangle _limits;


        private Point _position;
        public Point Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position.X = Mathematics.Clamp(value.X, _limits.X, _limits.W);
                _position.Y = Mathematics.Clamp(value.Y, _limits.Y, _limits.H);
            }
        }

        public Camera(Point viewport, Rectangle limits)
        {
            _viewport = viewport;
            _limits = limits;
        }

        public bool OnScreen(Rectangle rectangle)
        {
            var viewport = new Rectangle(0, 0, _viewport.X, _viewport.Y);
            return rectangle.HasIntersection(ref viewport);
        }

        /// <summary>
        /// Transforms a point from the world space to the view space.
        /// </summary>
        public Point TransformPoint(Point point)
        {
            return new Point(point.X - Position.X, _viewport.Y - point.Y + Position.Y);
        }

    }
}