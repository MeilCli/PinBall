namespace PinBall {
    public class Configuration {

        public string Title { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool WaitVerticalBlanking { get; set; }

        public Configuration() : this("Game") { }

        public Configuration(string title) : this(title,600,800) { }

        public Configuration(string title,int width,int height) {
            Title = title;
            Width = width;
            Height = height;
        }
    }
}
