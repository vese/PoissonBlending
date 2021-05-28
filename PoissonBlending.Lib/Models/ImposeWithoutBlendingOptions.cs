namespace PoissonBlending.Lib
{
    public class ImposeWithoutBlendingOptions
    {
        public string BaseImageFilename { get; set; }
        public string ImposingImageFilename { get; set; }
        public Point InsertPosition { get; set; }
        public Point[] SelectedAreaPoints { get; set; }
        public bool SaveResultImage { get; set; } = true;
        public string ResultImageFilename { get; set; } = PoissonBlendingSolver.DefaultResultFilename;
    }
}
