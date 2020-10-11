using System.Threading.Tasks;

namespace Yugen.Mosaic.Uwp.Services
{
    public interface IWhatsNewDisplayService
    {
        Task ShowIfAppropriateAsync();
    }
}