namespace Yugen.Mosaic.Uwp.Models
{
    public class TileFound
    {
        public TileFound(Tile tile, int difference)
        {
            Tile = tile;
            Difference = difference;
        }

        public Tile Tile { get; set; }
        public int Difference { get; set; }
    }
}