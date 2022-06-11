using System.Threading.Tasks;

namespace Haravan.Service
{
    public interface IShinkoService
    {
        Task<string> CreateAllProducts();
        Task<string> DeleteAllProducts(int start, int end);
    }
}
