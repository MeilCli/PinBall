using SharpDX.Direct2D1;

namespace PinBall {
    public class Ball {

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Width { get; }
        public float Height { get; }
        public float MoveDeltaXPerSecound { get; set; } = 250;
        public float MoveDeltaYPerSecound { get; set; } = 250;
        public Bar Bar { get; set; }

        public Ball() : this(10,10) { }

        public Ball(int width,int height) : this(0,400,width,height) { }

        public Ball(int x,int y,int width,int height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public void Draw(Application application,Brush brush,Time time) {
            float deltaX = MoveDeltaXPerSecound * application.FrameDelta;
            float deltaY = MoveDeltaYPerSecound * application.FrameDelta;

            if(mustTurningX(application,deltaX)) {
                deltaX *= -1;
                MoveDeltaXPerSecound *= -1;
            }
            if(mustTurningY(application,deltaY)) {
                deltaY *= -1;
                MoveDeltaYPerSecound *= -1;
            }

            X += deltaX;
            Y += deltaY;

            application.RenderTarget2D.FillEllipse(new Ellipse(new SharpDX.Mathematics.Interop.RawVector2(X,Y),Width,Height),brush);
        }

        private bool mustTurningX(Application application,float deltaX) {
            if((X - Width / 2) + deltaX < 0) {
                return true;
            }
            if((X - Width / 2) + deltaX > application.RenderingSize.Width) {
                return true;
            }
            if(Bar != null) {
                if((Bar.X < (X - Width / 2) + deltaX && (X - Width / 2) + deltaX < Bar.X + Bar.Width) && (Bar.Y < Y && Y < Bar.Y + Bar.Height)) {
                    return true;
                }
            }
            return false;
        }

        private bool mustTurningY(Application application,float deltaY) {
            if((Y - Height / 2) + deltaY < 0) {
                return true;
            }
            if(Y + deltaY > application.Config.Height) {
                return true;
            }
            if(Bar != null) {
                if((Bar.Y < (Y + Height / 2) + deltaY && (Y + Height / 2) + deltaY < Bar.Y + Bar.Height) && (Bar.X < X && X < Bar.X + Bar.Width)) {
                    return true;
                }
            }
            return false;
        }
    }
}
