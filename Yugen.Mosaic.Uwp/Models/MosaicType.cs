namespace Yugen.Mosaic.Uwp.Models
{
    public class MosaicType
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public override string ToString() => Title;
    }
}
