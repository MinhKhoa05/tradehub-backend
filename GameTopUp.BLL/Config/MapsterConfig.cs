using Mapster;
using GameTopUp.BLL.DTOs.GamePackages;
using GameTopUp.BLL.Utils;
using GameTopUp.DAL.Entities;

namespace GameTopUp.BLL.Config
{
    public static class MapsterConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

            TypeAdapterConfig<UpdateGamePackageRequest, GamePackage>
                .NewConfig()
                .Map(dest => dest.NormalizedName, src => NormalizeName.Normalize(src.Name!), srcCond => srcCond.Name != null);
        }
    }
}
