namespace MultiSlideServer
{
    public class ImagesOption
    {
        public ImageOptionItem[] Images { get; set; }
    }

    public class ImageOptionItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
