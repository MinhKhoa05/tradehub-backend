using Mapster;
using TradeHub.BLL.DTOs.GamePackages;
using TradeHub.BLL.Utils;
using TradeHub.DAL.Entities;

namespace TradeHub.BLL.Config
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
