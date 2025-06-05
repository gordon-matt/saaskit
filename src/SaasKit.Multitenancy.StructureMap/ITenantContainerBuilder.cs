using System.Threading.Tasks;
using StructureMap;

namespace SaasKit.Multitenancy.StructureMap
{
    public interface ITenantContainerBuilder<TTenant>
    {
        Task<IContainer> BuildAsync(TTenant tenant);
    }
}