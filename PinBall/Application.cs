using System;
using System.Drawing;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;

namespace PinBall {
    public abstract class Application : IDisposable {

        private readonly Time time = new Time();
        private FormWindowState currentFormWindowState;
        private bool isDisposed;
        private Form form;
        private float frameAccumulator;
        private int frameCount;
        private Configuration configuration;

        protected IntPtr DisplayHandle => form.Handle;
        public Configuration Config => configuration;
        public float FrameDelta { get; private set; }
        public float FramePerSecond { get; private set; }
        public Size RenderingSize => form.ClientSize;

        private SharpDX.Direct3D11.Device device;
        private SwapChain swapChain;
        private Texture2D backBuffer;
        private RenderTargetView backBufferView;

        public SharpDX.Direct3D11.Device Device => device;
        public Texture2D BackBuffer => backBuffer;
        public RenderTargetView BackBufferView => backBufferView;

        public SharpDX.Direct2D1.Factory Factory2D { get; private set; }
        public SharpDX.DirectWrite.Factory FactoryDWrite { get; private set; }
        public RenderTarget RenderTarget2D { get; private set; }
        public SolidColorBrush SceneColorBrush { get; private set; }

        protected Form CreateForm(Configuration config) {
            return new RenderForm(config.Title) {
                Size = new Size(config.Width,config.Height)
            };
        }

        protected virtual void Initialize(Configuration config) {
            var desc = new SwapChainDescription() {
                BufferCount = 1,
                ModeDescription =new ModeDescription(config.Width,config.Height,new Rational(60,1),Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = DisplayHandle,
                SampleDescription = new SampleDescription(1,0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                DriverType.Hardware,DeviceCreationFlags.BgraSupport,
                new[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 },desc,out device,out swapChain);

            SharpDX.DXGI.Factory factory = swapChain.GetParent<SharpDX.DXGI.Factory>();
            factory.MakeWindowAssociation(DisplayHandle,WindowAssociationFlags.IgnoreAll);

            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain,0);
            backBufferView = new RenderTargetView(device,backBuffer);



            Factory2D = new SharpDX.Direct2D1.Factory();
            using(var surface = BackBuffer.QueryInterface<Surface>()) {
                RenderTarget2D = new RenderTarget(Factory2D,surface,
                                                  new RenderTargetProperties(new PixelFormat(Format.Unknown,AlphaMode.Premultiplied)));
            }
            RenderTarget2D.AntialiasMode = AntialiasMode.PerPrimitive;

            FactoryDWrite = new SharpDX.DirectWrite.Factory();
            SceneColorBrush = new SolidColorBrush(RenderTarget2D,SharpDX.Color.White);
        }

        protected virtual void LoadContent() { }

        protected virtual void UnloadContent() { }

        protected virtual void Update(Time time) { }

        protected virtual void Draw(Time time) { }

        protected virtual void BeginRun() { }

        protected virtual void EndRun() { }

        protected virtual void BeginDraw() {
            Device.ImmediateContext.Rasterizer.SetViewport(new Viewport(0,0,Config.Width,Config.Height));
            Device.ImmediateContext.OutputMerger.SetTargets(backBufferView);

            RenderTarget2D.BeginDraw();
        }

        protected virtual void EndDraw() {
            RenderTarget2D.EndDraw();

            swapChain.Present(Config.WaitVerticalBlanking ? 1 : 0,PresentFlags.None);
        }

        private void OnUpdate() {
            FrameDelta = (float)time.Update();
            Update(time);
        }

        private void Render() {
            frameAccumulator += FrameDelta;
            frameCount++;
            if(frameAccumulator >= 1.0f) {
                FramePerSecond = frameCount / frameAccumulator;

                form.Text = configuration.Title + " - FPS: " + FramePerSecond;
                frameAccumulator = 0.0f;
                frameCount = 0;
            }

            BeginDraw();
            Draw(time);
            EndDraw();
        }

        public void Run() {
            Run(new Configuration());
        }

        public void Run(Configuration config) {
            configuration = config ?? new Configuration();
            form = CreateForm(config);
            Initialize(configuration);

            bool isFormClosed = false;
            bool formIsResizing = false;

            form.MouseClick += HandleMouseClick;
            form.KeyDown += HandleKeyDown;
            form.KeyUp += HandleKeyUp;
            form.MouseMove += HandleMouseMove;
            form.Resize += (o,args) => {
                if(form.WindowState != currentFormWindowState) {
                    HandleResize(o,args);
                }

                currentFormWindowState = form.WindowState;
            };

            form.ResizeBegin += (o,args) => { formIsResizing = true; };
            form.ResizeEnd += (o,args) => {
                formIsResizing = false;
                HandleResize(o,args);
            };

            form.Closed += (o,args) => { isFormClosed = true; };

            LoadContent();

            time.Start();
            BeginRun();
            RenderLoop.Run(form,() => {
                if(isFormClosed) {
                    return;
                }

                OnUpdate();
                if(!formIsResizing) {
                    Render();
                }     
            });

            UnloadContent();
            EndRun();

            Dispose();
        }

        protected virtual void MouseClick(MouseEventArgs e) { }

        protected virtual void MouseMove(MouseEventArgs e) { }

        protected virtual void KeyDown(KeyEventArgs e) {
            if(e.KeyCode == Keys.Escape) {
                Exit();
            }
        }

        protected virtual void KeyUp(KeyEventArgs e) {}

        private void HandleMouseClick(object sender,MouseEventArgs e) {
            MouseClick(e);
        }

        private void HandleMouseMove(object sender,MouseEventArgs e) {
            MouseMove(e);
        }

        private void HandleKeyDown(object sender,KeyEventArgs e) {
            KeyDown(e);
        }

        private void HandleKeyUp(object sender,KeyEventArgs e) {
            KeyUp(e);
        }

        private void HandleResize(object sender,EventArgs e) {
            if(form.WindowState == FormWindowState.Minimized) {
                return;
            }
        }

        public void Exit() {
            form.Close();
        }

        ~Application() {
            if(!isDisposed) {
                Dispose(false);
                isDisposed = true;
            }
        }

        public void Dispose() {
            if(!isDisposed) {
                Dispose(true);
                isDisposed = true;
            }
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManagedResources) {
            if(disposeManagedResources) {
                form?.Dispose();
            }
        }
    }
}
