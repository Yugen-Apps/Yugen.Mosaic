// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Yugen.Mosaic.Uwp.Models
{
    public class MosaicType
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
