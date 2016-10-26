using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;

namespace PinBall {
    public class Program : Application {

        private Color4 backgroundColor;
        private Brush brush;
        private Bar bar;
        private Ball ball;

        [STAThread]
        static void Main() {
            Program program = new Program();
            program.Run(new Configuration("PinBall Game"));
        }

        protected override void Initialize(Configuration config) {
            base.Initialize(config);

            backgroundColor = new Color4(0.1f,0.1f,0.1f,1.0f);
            brush = new SolidColorBrush(RenderTarget2D,Color.Green);
            bar = new Bar(Config.Width / 2 - 15,Config.Height - 150,50,10);
            ball = new Ball(Config.Width / 2 - 5,100,10,10);
            ball.Bar = bar;
        }

        protected override void Draw(Time time) {
            base.Draw(time);

            RenderTarget2D.Clear(backgroundColor);

            bar.Draw(this,brush,time);
            ball.Draw(this,brush,time);
        }

        protected override void MouseMove(MouseEventArgs e) {
            base.MouseMove(e);

            bar.MouseMove(this,e);
        }
    }
}
