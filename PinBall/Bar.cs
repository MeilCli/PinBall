using System.Windows.Forms;
using SharpDX.Direct2D1;

namespace PinBall {
    public class Bar {

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Width { get; }
        public float Height { get; }
        public float MoveSpeedPerSecound { get; set; } = 500;
        public float MoveTargetX { get; private set; }

        public Bar() : this(30,10) { }

        public Bar(int width,int height) : this(0,400,width,height) { }

        public Bar(int x,int y,int width,int height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public void Draw(Application application,Brush brush,Time time) {
            float canMoveX = MoveSpeedPerSecound * application.FrameDelta;
            if(MoveTargetX - X > canMoveX) {
                X += canMoveX;
            } else if(MoveTargetX - X > 0) {
                X = MoveTargetX;
            } else if(MoveTargetX - X < canMoveX) {
                X -= canMoveX;
            } else {
                X = MoveTargetX;
            }

            application.RenderTarget2D.DrawRectangle(new SharpDX.Mathematics.Interop.RawRectangleF(X,Y,X + Width,Y + Height),brush);
        }

        public void MouseMove(Application application,MouseEventArgs e) {
            MoveTargetX = e.X;
            if(MoveTargetX < 0) {
                MoveTargetX = 0;
            }
            if(MoveTargetX + Width/2 > application.RenderingSize.Width) {
                MoveTargetX = application.RenderingSize.Width - Width / 2;
            }
        }
    }
}
